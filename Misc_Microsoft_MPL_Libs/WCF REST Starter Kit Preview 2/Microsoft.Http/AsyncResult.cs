//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Reflection;
    using System.Diagnostics;

    abstract class AsyncResult : IAsyncResult
    {
        AsyncCallback callback;
        bool completedSynchronously;
        bool endCalled;
        Exception exception;
        bool isCompleted;
        ManualResetEvent manualResetEvent;
        object state;
        object thisLock;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();

            CallbackChecker.Check(callback);
        }

        // Properties
        public object AsyncState
        {
            get
            {
                return this.state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.manualResetEvent == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.manualResetEvent == null)
                        {
                            this.manualResetEvent = new ManualResetEvent(this.isCompleted);
                        }
                    }
                }
                return this.manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
        {
            return End<TAsyncResult>(result, true);
        }
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result, bool throwException) where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }
            TAsyncResult local = (TAsyncResult)result;
            if (local.endCalled)
            {
                throw new InvalidOperationException("AsyncObjectAlreadyEnded");
            }
            local.endCalled = true;
            if (!local.isCompleted)
            {
                local.AsyncWaitHandle.WaitOne();
            }
            if (local.manualResetEvent != null)
            {
                local.manualResetEvent.Close();
            }
            if (local.exception != null && throwException)
            {
                System.Diagnostics.Debug.WriteLine(local.exception);
                throw local.exception;
            }
            return local;
        }


#if DEBUG
        string completionStack;
#endif

        protected void Complete(bool completedSynchronously)
        {
            if (this.isCompleted)
            {
#if DEBUG
                Debug.WriteLine("///////////////////// original stack /////////////////\n" + completionStack + "\n///////////////////// second completion /////////////////\n" + Environment.StackTrace);
                Debug.Assert(false);
#endif
                throw new InvalidOperationException("completed");
            }
#if DEBUG
            this.completionStack = Environment.StackTrace;
#endif
            this.completedSynchronously = completedSynchronously;
            if (completedSynchronously)
            {
                this.isCompleted = true;
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }
            if (this.callback != null)
            {
                try
                {
                    this.callback(this);
                }
                catch (Exception exception)
                {
                    if (IsFatal(exception))
                    {
                        throw;
                    }
                    throw new CallbackException(this.callback.Method.ToString(), exception);
                }
            }
        }

        [Serializable]
        internal sealed class CallbackException : SystemException
        {
            public CallbackException()
            {
            }

            //public CallbackException(string message)
            //    : base(message)
            //{
            //}
            public CallbackException(string message, Exception inner)
                : base(message, inner)
            {
            }
            CallbackException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
        }

        public static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                if (
(exception is OutOfMemoryException && !(exception is InsufficientMemoryException)) ||
 exception is CallbackException ||
                    exception is ThreadAbortException ||
                    exception is AccessViolationException ||
                    exception is System.Runtime.InteropServices.SEHException)
                {
                    return true;
                }

                // These exceptions aren't themselves fatal, but since the CLR uses them to wrap other exceptions,
                // we want to check to see whether they've been used to wrap a fatal exception.  If so, then they
                // count as fatal.
                if (exception is TypeInitializationException ||
                    exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                else
                {
                    break;
                }
            }

            return false;
        }
        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Complete(completedSynchronously);
        }

        static class CallbackChecker
        {
            static readonly Dictionary<MethodInfo, Delegate> seen = new Dictionary<MethodInfo, Delegate>();

            [Conditional("DEBUG")]
            public static void Check(Delegate callback)
            {
                if (callback != null)
                {
                    if (seen.ContainsKey(callback.Method))
                    {
                        if (!object.ReferenceEquals(seen[callback.Method], callback) &&
                            callback.Method.DeclaringType.Assembly == typeof(CallbackChecker).Assembly)
                        {
                            throw new InvalidOperationException("callback to " + callback.Method + " was not cached");
                        }
                    }
                    else
                    {
                        seen.Add(callback.Method, callback);
                    }
                }
            }

        }
    }

    abstract class AsyncResult<T> : AsyncResult
    {
        T data;
        public AsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public static T End(IAsyncResult result)
        {
            return AsyncResult.End<AsyncResult<T>>(result).data;
        }

        protected void Complete(bool completedSynchronously, T data)
        {
            this.data = data;
            base.Complete(completedSynchronously);
        }
    }
}
