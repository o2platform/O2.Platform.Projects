//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.ServiceModel;
using AsyncResult = Microsoft.ServiceModel.Web.AsyncResult;
using System.Text;

namespace System.IO
{
    public sealed class AdapterStream : Stream
    {
        static AsyncCallback onWriterInvokeCompleted = null;

        Action<Stream> writer;
        object thisLock = new object();
        PendingIOQueue readQueue;
        PendingIOQueue writeQueue;
        bool noMoreData;
        bool wasWriterInvoked;
        Exception writerException;

        public AdapterStream(Action<Stream> writer)
            : base()
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.writer = writer;
            this.readQueue = new PendingIOQueue();
            this.writeQueue = new PendingIOQueue();
        }

        public AdapterStream(Action<TextWriter> writer, Encoding encoding)
            : this(new TextWriterAction(writer, encoding).WriteToStream)
        {
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override long Position 
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            ReadAsyncResult ar = new ReadAsyncResult(callback, state);
            bool shouldInvokeWriter;
            if (!TryCompleteReadRequest(buffer, offset, count, ar, out shouldInvokeWriter))
            {
                if (shouldInvokeWriter)
                {
                    InvokeWriter();
                }
            }
            return ar;
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    lock (this.thisLock)
                    {
                        this.readQueue.Clear();
                        this.writeQueue.Clear();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            return ReadAsyncResult.End(asyncResult);
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            ReadSyncResult readResult = new ReadSyncResult();
            bool shouldInvokeWriter;
            if (!TryCompleteReadRequest(buffer, offset, count, readResult, out shouldInvokeWriter))
            {
                if (shouldInvokeWriter)
                {
                    InvokeWriter();
                }
            }
            return readResult.WaitFor();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        bool TryCompleteReadRequest(byte[] buffer, int offset, int count, IIORequestEventsHandler handler,
            out bool shouldInvokeWriter)
        {
            int numOfBytesRead = 0;
            shouldInvokeWriter = false;

            lock (this.thisLock)
            {
                while ((count > 0) && this.writeQueue.HasPendingRequests)
                {
                    if (this.writeQueue.Head.Buffer == null)
                    {
                        // This is a pending flush request, we just need to complete it
                        this.writeQueue.Dequeue().Complete(0);
                    }
                    else if (this.writeQueue.Head.Count <= count)
                    {
                        int bytesToCopy = this.writeQueue.Head.Count;
                        Buffer.BlockCopy(this.writeQueue.Head.Buffer, this.writeQueue.Head.Offset,
                            buffer, offset, bytesToCopy);
                        numOfBytesRead += bytesToCopy;
                        offset += bytesToCopy;
                        count -= bytesToCopy;
                        this.writeQueue.Dequeue().Complete(bytesToCopy);
                    }
                    else
                    {
                        Buffer.BlockCopy(this.writeQueue.Head.Buffer, this.writeQueue.Head.Offset,
                            buffer, offset, count);
                        numOfBytesRead += count;
                        this.writeQueue.Head.Progress(count);
                        handler.OnCompletion(true, numOfBytesRead);
                        return true;
                    }
                }

                if ((this.noMoreData) || (numOfBytesRead > 0))
                {
                    if (this.writerException != null)
                    {
                        handler.OnException(this.writerException);
                        this.writerException = null;
                    }
                    else
                    {
                        // Clearing the queue from leading flush request :
                        while (this.writeQueue.HasPendingRequests && (this.writeQueue.Head.Buffer == null))
                        {
                            this.writeQueue.Dequeue().Complete(0);
                        }

                        handler.OnCompletion(true, numOfBytesRead);
                    }
                    return true;
                }
                else
                {
                    this.readQueue.Enqueue(buffer, offset, count, handler);
                    if (!this.wasWriterInvoked)
                    {
                        this.wasWriterInvoked = true;
                        shouldInvokeWriter = true;
                    }
                    return false;
                }
            }
        }
        bool TryCompleteWriteRequest(byte[] buffer, int offset, int count, IIORequestEventsHandler handler)
        {
            int numOfBytesWritten = 0;

            lock (this.thisLock)
            {
                while ((count > 0) && this.readQueue.HasPendingRequests)
                {
                    if (this.readQueue.Head.Count < count)
                    {
                        int bytesToCopy = this.readQueue.Head.Count;
                        Buffer.BlockCopy(buffer, offset, this.readQueue.Head.Buffer, this.readQueue.Head.Offset, 
                            bytesToCopy);
                        numOfBytesWritten += bytesToCopy;
                        offset += bytesToCopy;
                        count -= bytesToCopy;
                        this.readQueue.Dequeue().Complete(bytesToCopy);
                    }
                    else
                    {
                        Buffer.BlockCopy(buffer, offset, this.readQueue.Head.Buffer, this.readQueue.Head.Offset, 
                            count);
                        this.readQueue.Dequeue().Complete(count);
                        handler.OnCompletion(true, numOfBytesWritten + count);
                        return true;
                    }
                }

                if (count > 0)
                {
                    this.writeQueue.Enqueue(buffer, offset, count, handler);
                    return false;
                }
                else
                {
                    handler.OnCompletion(true, numOfBytesWritten);
                    return true;
                }
            }
        }
        bool TryCompleteFlushRequest(IIORequestEventsHandler handler)
        {
            lock (this.thisLock)
            {
                if (this.writeQueue.HasPendingRequests)
                {
                    this.writeQueue.Enqueue(null, 0, 0, handler);
                    return false;
                }
                else
                {
                    handler.OnCompletion(true, 0);
                    return true;
                }
            }
        }
        void InvokeWriter()
        {
            bool writerWasInvoked = false;
            try
            {
                SourceStream srcStream = new SourceStream(this);
                if (onWriterInvokeCompleted == null)
                {
                    onWriterInvokeCompleted = new AsyncCallback(OnWriterInvokeCompleted);
                }
                IAsyncResult invokeResult = this.writer.BeginInvoke(srcStream,
                    onWriterInvokeCompleted, srcStream);
                if (invokeResult.CompletedSynchronously)
                {
                    this.writer.EndInvoke(invokeResult);
                    CompleteWriterInvocation(srcStream);
                }

                writerWasInvoked = true;
            }
            finally
            {
                // If something went wrong, let's reset the state :
                if (!writerWasInvoked)
                    this.wasWriterInvoked = false;
            }
        }

        void CompleteWriterInvocation(SourceStream srcStream)
        {
            srcStream.Close();
            lock (this.thisLock)
            {
                this.noMoreData = true;
                while (this.readQueue.HasPendingRequests)
                {
                    if (this.writerException != null)
                    {
                        this.readQueue.Dequeue().CompleteWithException(srcStream.Owner.writerException);
                        this.writerException = null;
                    }
                    else
                    {
                        this.readQueue.Dequeue().Complete(0);
                    }
                }
            }

        }

        static void OnWriterInvokeCompleted(IAsyncResult ar)
        {
            if (ar.CompletedSynchronously)
            {
                return;
            }

            SourceStream srcStream = (SourceStream)ar.AsyncState;

            try
            {
                srcStream.Owner.writer.EndInvoke(ar);
            }
            catch (Exception exception)
            {
                srcStream.Owner.writerException = exception;
            }
            srcStream.Owner.CompleteWriterInvocation(srcStream);
        }

        interface IIORequestEventsHandler
        {
            void OnEnquing();
            void OnCompletion(bool completedSynchronously, int numOfBytesTransferred);
            void OnException(Exception exception);
        }

        class PendingIORequest
        {
            byte[] buffer;
            int offset;
            int count;
            int numOfBytesTransferred;
            IIORequestEventsHandler handler;

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }
            public int Offset
            {
                get
                {
                    return this.offset;
                }
            }
            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public PendingIORequest(byte[] buffer, int offset, int count, IIORequestEventsHandler handler)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
                this.numOfBytesTransferred = 0;
                this.handler = handler;
            }

            public void Progress(int numOfBytesTransferred)
            {
                Debug.Assert(numOfBytesTransferred <= this.count, "Can't progress more then expected by count");
                this.count -= numOfBytesTransferred;
                this.offset += numOfBytesTransferred;
                this.numOfBytesTransferred += numOfBytesTransferred;
            }
            public void Complete(int numOfBytesTransferred)
            {
                Debug.Assert(numOfBytesTransferred <= this.count, "Can't progress more then expected by count");
                this.numOfBytesTransferred += numOfBytesTransferred;
                this.handler.OnCompletion(false, this.numOfBytesTransferred);
            }
            public void CompleteWithException(Exception exception)
            {
                this.handler.OnException(exception);
            }
        }
        class PendingIOQueue
        {
            bool hasPendingRequests;
            PendingIORequest nextRequest;
            Queue<PendingIORequest> pendingRequests;

            public bool HasPendingRequests
            {
                get
                {
                    return this.hasPendingRequests;
                }
            }
            public PendingIORequest Head
            {
                get
                {
                    Debug.Assert(this.hasPendingRequests, "Can't peek into an empty queue");
                    return this.nextRequest;
                }
            }

            public void Enqueue(byte[] buffer, int offset, int count, IIORequestEventsHandler eventsHandler)
            {
                eventsHandler.OnEnquing();

                if (this.hasPendingRequests)
                {
                    if (this.pendingRequests == null)
                    {
                        this.pendingRequests = new Queue<PendingIORequest>();
                    }
                    this.pendingRequests.Enqueue(new PendingIORequest(buffer, offset, count, eventsHandler));
                }
                else
                {
                    this.nextRequest = new PendingIORequest(buffer, offset, count, eventsHandler);
                    this.hasPendingRequests = true;
                }
            }
            public PendingIORequest Dequeue()
            {
                Debug.Assert(this.hasPendingRequests, "Can't dequeue from an empty queue");

                PendingIORequest result = this.nextRequest;
                if ((this.pendingRequests != null) && (this.pendingRequests.Count > 0))
                {
                    this.nextRequest = this.pendingRequests.Dequeue();
                }
                else
                {
                    this.hasPendingRequests = false;
                }
                return result;
            }
            public void Clear()
            {
                if (this.hasPendingRequests)
                {
                    this.nextRequest.Complete(0);
                    this.hasPendingRequests = false;
                }
                if (this.pendingRequests != null)
                {
                    while (this.pendingRequests.Count > 0)
                    {
                        this.pendingRequests.Dequeue().Complete(0);
                    }
                }
            }
        }

        class ReadSyncResult : IIORequestEventsHandler, IDisposable
        {
            Exception exception;
            int numOfBytesRead;
            ManualResetEvent completionEvent;

            public void OnEnquing()
            {
                this.completionEvent = new ManualResetEvent(false);
            }
            public void OnCompletion(bool completedSynchronously, int numOfBytesTransferred)
            {
                this.numOfBytesRead = numOfBytesTransferred;
                if (this.completionEvent != null)
                {
                    this.completionEvent.Set();
                }
            }
            public void OnException(Exception exception)
            {
                this.exception = exception;
                if (this.completionEvent != null)
                {
                    this.completionEvent.Set();
                }
            }

            public int WaitFor()
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.WaitOne();
                }
                if (this.exception != null)
                {
                    throw this.exception;
                }
                return this.numOfBytesRead;
            }

            public void Dispose()
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.Close();
                }
            }
        }
        class ReadAsyncResult : AsyncResult, IIORequestEventsHandler
        {
            int numOfBytesRead;

            public ReadAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public static int End(IAsyncResult result)
            {
                return AsyncResult.End<ReadAsyncResult>(result).numOfBytesRead;
            }

            public void OnEnquing()
            {
            }
            public void OnCompletion(bool completedSynchronously, int numOfBytesTransferred)
            {
                this.numOfBytesRead = numOfBytesTransferred;
                base.Complete(completedSynchronously);
            }
            public void OnException(Exception exception)
            {
                base.Complete(false, exception);
            }
        }
        class WriteSyncResult : IIORequestEventsHandler, IDisposable
        {
            Exception exception;
            ManualResetEvent completionEvent;

            public void OnEnquing()
            {
                this.completionEvent = new ManualResetEvent(false);
            }
            public void OnCompletion(bool completedSynchronously, int numOfBytesTransferred)
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.Set();
                }
            }
            public void OnException(Exception exception)
            {
                this.exception = exception;
                if (this.completionEvent != null)
                {
                    this.completionEvent.Set();
                }
            }

            public void WaitFor()
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.WaitOne();
                }
                if (this.exception != null)
                {
                    throw this.exception;
                }
            }

            public void Dispose()
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.Close();
                }
            }
        }
        class WriteAsyncResult : AsyncResult, IIORequestEventsHandler
        {
            public WriteAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteAsyncResult>(result);
            }

            public void OnEnquing()
            {
            }
            public void OnCompletion(bool completedSynchronously, int numOfBytesTransferred)
            {
                base.Complete(completedSynchronously);
            }
            public void OnException(Exception exception)
            {
                base.Complete(false, exception);
            }
        }

        class SourceStream : Stream
        {
            AdapterStream owner;

            public SourceStream(AdapterStream owner)
                : base()
            {
                this.owner = owner;
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }
            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }
            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }
            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }
            public override long Position 
            {
                get
                {
                    throw new NotSupportedException();
                    
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            internal AdapterStream Owner
            {
                get
                {
                    return this.owner;
                }
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (offset >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (offset + count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("count");
                }

                WriteAsyncResult ar = new WriteAsyncResult(callback, state);
                this.owner.TryCompleteWriteRequest(buffer, offset, count, ar);
                return ar;
            }
           
            public override int EndRead(IAsyncResult asyncResult)
            {
                throw new NotSupportedException();
            }
            public override void EndWrite(IAsyncResult asyncResult)
            {
                WriteAsyncResult.End(asyncResult);
            }
            public override void Flush()
            {
                WriteSyncResult writeResult = new WriteSyncResult();
                this.owner.TryCompleteFlushRequest(writeResult);
                writeResult.WaitFor();
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (offset >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (offset + count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("count");
                }

                WriteSyncResult writeResult = new WriteSyncResult();
                this.owner.TryCompleteWriteRequest(buffer, offset, count, writeResult);
                writeResult.WaitFor();
            }
        }

        class TextWriterAction
        {
            Action<TextWriter> writer;
            Encoding encoding;
            
            public TextWriterAction(Action<TextWriter> writer, Encoding encoding)
            {
                this.writer = writer;
                this.encoding = encoding;
            }

            public void WriteToStream(Stream s)
            {
                if (this.writer != null)
                {
                    using (TextWriter textWriter = new StreamWriter(s, encoding))
                    {
                        this.writer(textWriter);
                    }
                }
            }
        }
    }
}
