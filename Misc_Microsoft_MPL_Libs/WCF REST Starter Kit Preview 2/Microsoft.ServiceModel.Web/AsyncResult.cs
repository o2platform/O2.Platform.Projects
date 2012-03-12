//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System;


    abstract class AsyncResult : IAsyncResult
    {
        AsyncCallback callback;
        object state;
        bool completedSynchronously;
        bool endCalled;
        Exception exception;
        bool isCompleted;
        ManualResetEvent manualResetEvent;
        object thisLock;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (manualResetEvent != null)
                {
                    return manualResetEvent;
                }

                lock (ThisLock)
                {
                    if (manualResetEvent == null)
                    {
                        manualResetEvent = new ManualResetEvent(isCompleted);
                    }
                }

                return manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return completedSynchronously;
            }
        }

        public bool HasCallback
        {
            get
            {
                return this.callback != null;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        protected void Complete(bool completedSynchronously)
        {
            if (isCompleted)
            {
                // It's a bug to call Complete twice.
                Debug.Assert(false, "AsyncResult complete called twice for the same operation.");
                throw new SystemException("operation is already completed");
            }

            this.completedSynchronously = completedSynchronously;

            if (completedSynchronously)
            {
                // If we completedSynchronously, then there's no chance that the manualResetEvent was created so
                // we don't need to worry about a race
                Debug.Assert(this.manualResetEvent == null, "No ManualResetEvent should be created for a synchronous AsyncResult.");
                this.isCompleted = true;
            }
            else
            {
                lock (ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }

            if (callback != null)
            {
                try
                {
                    callback(this);
                }
                catch (Exception e)
                {
                    throw new SystemException("Callback threw an exception", e);
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            Complete(completedSynchronously);
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
            where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            TAsyncResult asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw new ArgumentException("result", "bad async result");
            }

            if (asyncResult.endCalled)
            {
                throw new InvalidOperationException("Async operation has already ended");
            }
            asyncResult.endCalled = true;

            if (!asyncResult.isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult.manualResetEvent != null)
            {
                asyncResult.manualResetEvent.Close();
            }

            if (asyncResult.exception != null)
            {
                throw asyncResult.exception;
            }

            return asyncResult;
        }
    }
}
