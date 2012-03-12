//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.ComponentModel;
    using Microsoft.Http.Headers;

    sealed class HttpStageProcessingAsyncResult : AsyncResult<HttpStageProcessingAsyncState>
    {
        public static HttpStageProcessingAsyncState End(IAsyncResult result, bool throwException)
        {
            var stage = AsyncResult.End<HttpStageProcessingAsyncResult>(result, throwException);
            return stage.state;
        }
        void Complete()
        {
            this.Complete(state.StayedSynchronous, state);
        }
        void Complete(HttpStage stage, Exception e)
        {
            Debug.WriteLine("Exception: " + e.GetType() + ": " + e.Message);
            Debug.WriteLine("Thrown at: " + Environment.StackTrace.Trim());

            if (state.Cancelled && !(e is OperationCanceledException))
            {
                e = new OperationCanceledException("cancel with exception", e);
            }

            var ex = e as HttpProcessingException;
            if (ex == null)
            {
                ex = new HttpStageProcessingException(e.Message, e, stage, state.request, state.GetResponseOrNull());
            }

            state.SetException(ex);
            if (state.StayedSynchronous && state.ForceSynchronous)
            {
                throw ex;
            }
            this.Complete(state.StayedSynchronous, ex);
        }

        readonly HttpStageProcessingAsyncState state;

        public void MarkCancelled()
        {
            state.MarkCancelled();
        }

        public HttpStageProcessingAsyncResult(HttpStageProcessingAsyncState state, AsyncCallback callback, object user)
            : base(callback, user)
        {
            this.state = state;
            NextRequest(this);
        }

        public HttpStageProcessingAsyncState HttpAsyncState
        {
            get
            {
                return this.state;
            }
        }

        static void NextRequest(HttpStageProcessingAsyncResult self)
        {
            var state = self.state;
            HttpStage previous = null;
            HttpResponseMessage response = null;
            while (state.states.Count < state.stages.Count)
            {
                var current = state.stages[state.states.Count];
                try
                {
                    var async = current as HttpAsyncStage;
                    object stageState;

                    if (state.Cancelled)
                    {
                        response = null;
                        Trace(state, "request cancel previous: ", previous);
                        Trace(state, "request cancel current: ", current);
                        self.Complete(previous, new OperationCanceledException());
                        return;
                    }

                    if (async != null && state.AllowAsync)
                    {
                        Trace(state, "request async", current);

                        var r = async.BeginProcessRequestAndTryGetResponse(state.request, FinishRequestCallback, self);
                        if (r.CompletedSynchronously)
                        {
                            Trace(state, "request async", current, "completed sync");
                            async.EndProcessRequestAndTryGetResponse(r, out response, out stageState);
                        }
                        else
                        {
                            Trace(state, "request async", current, " is running");
                            return;
                        }
                    }
                    else
                    {
                        Trace(state, "request sync", current);
                        current.ProcessRequestAndTryGetResponse(state.request, out response, out stageState);
                    }
                    state.states.Push(stageState);
                    if (response != null)
                    {
                        state.SetResponse(response);
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (IsFatal(e))
                    {
                        throw;
                    }
                    Trace(state, "NextRequest", current, "exception in processing: " + e.GetType() + " " + e.Message);

                    self.Complete(current, e);
                    return;
                }
                previous = current;
            }

            if (response != null)
            {
                NextResponse(self);
            }
            else
            {
                self.Complete(null, new InvalidOperationException("HttpResponseMessage not available"));
            }
        }

        static readonly AsyncCallback FinishRequestCallback = FinishRequest;
        static void FinishRequest(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            var self = (HttpStageProcessingAsyncResult)result.AsyncState;
            var state = self.state;
            state.MarkAsAsync();
            var current = (HttpAsyncStage)state.stages[state.states.Count];
            Trace(state, "request async", current, "in async callback");
            HttpResponseMessage response;
            object stageState;
            try
            {
                current.EndProcessRequestAndTryGetResponse(result, out response, out stageState);
            }
            catch (Exception e)
            {
                if (IsFatal(e))
                {
                    throw;
                }
                Trace(state, "request async", current, "exception in async callback: " + e.GetType() + " " + e.Message);
                self.Complete(current, e);
                return;
            }

            Trace(state, "request async", current, "completed in async callback");
            state.states.Push(stageState);
            if (response != null)
            {
                state.SetResponse(response);
                NextResponse(self);
                return;
            }
            NextRequest(self);
        }

        static void NextResponse(HttpStageProcessingAsyncResult self)
        {
            var state = self.state;
            HttpStage previous = null;
            while (state.states.Count != 0)
            {
                var current = (HttpStage)state.stages[state.states.Count - 1];
                try
                {
                    if (state.Cancelled)
                    {
                        Trace(state, "response cancel current", current);
                        Trace(state, "response cancel previous", previous);
                        self.Complete(previous, new OperationCanceledException());
                        return;
                    }

                    var stageState = state.states.Pop();
                    var async = current as HttpAsyncStage;
                    if (async != null && state.AllowAsync)
                    {
                        Trace(state, "response async", current);

                        var r = async.BeginProcessResponse(state.Response, stageState, FinishResponseCallback, self);
                        if (r.CompletedSynchronously)
                        {
                            Trace(state, "response async", current, "completed sync");
                            async.EndProcessResponse(r);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        Trace(state, "response sync", current);
                        current.ProcessResponse(state.Response, stageState);
                    }
                }
                catch (Exception e)
                {
                    if (IsFatal(e))
                    {
                        throw;
                    }
                    Trace(state, "NextResponse", current, "exception in processing: " + e.GetType() + " " + e.Message);

                    self.Complete(current, e);
                    return;
                }
                previous = current;
            }
            self.Complete();
        }
        static readonly AsyncCallback FinishResponseCallback = FinishResponse;
        static void FinishResponse(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var self = (HttpStageProcessingAsyncResult)result.AsyncState;
            var state = self.state;
            state.MarkAsAsync();
            // i've already popped the state so state.states.Count is 0 == first stage
            var current = (HttpAsyncStage)state.stages[state.states.Count];
            Trace(state, "response async", current, "in async callback");

            try
            {
                current.EndProcessResponse(result);
                Trace(state, "response async", current, "completed");

            }
            catch (Exception e)
            {
                if (IsFatal(e))
                {
                    throw;
                }
                Trace(state, "response async", current, "exception in async callback: " + e.GetType() + " " + e.Message);
                self.Complete(current, e);
                return;
            }

            NextResponse(self);
        }

        [Conditional("TRACE"), Conditional("DEBUG")]
        public static void Trace(HttpStageProcessingAsyncState state, string location, HttpStage current)
        {
            Trace(state, location, current, null);
        }

        [Conditional("TRACE"), Conditional("DEBUG")]
        public static void Trace(HttpStageProcessingAsyncState state, string location, HttpStage current, string detail)
        {
#if DEBUG && TRACE
            var t = DateTime.UtcNow - state.started;
            var s = string.Format("{0,-20} {1} {2} {3}", t, location, current, detail);
            Debug.WriteLine(s);
#endif
        }
    }
}
