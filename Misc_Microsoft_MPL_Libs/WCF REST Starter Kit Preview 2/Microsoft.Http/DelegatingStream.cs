//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.IO;

    abstract class DelegatingStream : Stream
    {
        readonly Stream stream;
        protected DelegatingStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.stream = stream;
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.stream.Position;
            }
            set
            {
                this.stream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.stream.ReadTimeout;
            }
            set
            {
                this.stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.stream.WriteTimeout;
            }
            set
            {
                this.stream.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfNotCanWrite();
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            if (this.stream != null)
            {
                this.stream.Close();
            }
        }

        public override int EndRead(IAsyncResult result)
        {
            return this.stream.EndRead(result);
        }

        public override void EndWrite(IAsyncResult result)
        {
            ThrowIfNotCanWrite();
            this.stream.EndWrite(result);
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfNotCanWrite();
            this.stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfNotCanWrite();
            this.stream.WriteByte(value);
        }
        protected void ThrowIfNotCanWrite()
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }
        }
    }
}
