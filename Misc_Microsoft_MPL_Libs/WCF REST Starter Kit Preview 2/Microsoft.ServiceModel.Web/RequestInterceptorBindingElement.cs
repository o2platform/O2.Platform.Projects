//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Web;
using System.IdentityModel.Claims;
using System.ServiceModel.Security;
using System.Collections.ObjectModel;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;

namespace Microsoft.ServiceModel.Web
{
    public class RequestInterceptorBindingElement : BindingElement
    {
        Collection<RequestInterceptor> interceptors;

        public RequestInterceptorBindingElement(Collection<RequestInterceptor> interceptors)
        {
            this.interceptors = new Collection<RequestInterceptor>(interceptors);
        }

        RequestInterceptorBindingElement(RequestInterceptorBindingElement src)
        {
            this.interceptors = new Collection<RequestInterceptor>(src.interceptors);
        }

        public Collection<RequestInterceptor> Interceptors
        {
            get { return this.interceptors; }
        }

        public override BindingElement Clone()
        {
            return new RequestInterceptorBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return false;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (!CanBuildChannelListener<TChannel>(context))
            {
                throw new NotSupportedException();
            }
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                return (IChannelListener<TChannel>)(object)new RequestInterceptorReplyChannelListener(context, this.interceptors);
            }
            else
            {
                return (IChannelListener<TChannel>)(object)new RequestInterceptorReplySessionChannelListener(context, this.interceptors);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            throw new NotSupportedException();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return (context.CanBuildInnerChannelListener<IReplyChannel>()
                || (context.CanBuildInnerChannelListener<IReplySessionChannel>()));
        }
    }

    abstract class RequestInterceptorReplyChannelListenerBase<TChannel> : ChannelListenerBase<TChannel>
        where TChannel : class, IReplyChannel
    {
        IChannelListener<TChannel> inner;
        Collection<RequestInterceptor> interceptors;

        public RequestInterceptorReplyChannelListenerBase(IChannelListener<TChannel> inner, Collection<RequestInterceptor> interceptors)
        {
            this.inner = inner;
            this.interceptors = (interceptors != null) ? new Collection<RequestInterceptor>(interceptors) : new Collection<RequestInterceptor>();
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return WrapInner(inner.AcceptChannel(timeout));
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginAcceptChannel(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return WrapInner(inner.EndAcceptChannel(result));
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return inner.EndWaitForChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return inner.WaitForChannel(timeout);
        }

        TChannel WrapInner(TChannel inner)
        {
            if (inner == null) return null;
            return CreateChannel(inner, this.interceptors);
        }

        protected abstract TChannel CreateChannel(TChannel inner, Collection<RequestInterceptor> interceptors);

        public override Uri Uri
        {
            get { return inner.Uri; }
        }

        protected override void OnAbort()
        {
            inner.Abort();
        }

        public override T GetProperty<T>()
        {
            return inner.GetProperty<T>();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            inner.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            inner.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            inner.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            inner.Open(timeout);
        }
    }

    class RequestInterceptorReplyChannelListener : RequestInterceptorReplyChannelListenerBase<IReplyChannel>
    {
        public RequestInterceptorReplyChannelListener(BindingContext c, Collection<RequestInterceptor> interceptors)
            : base(c.BuildInnerChannelListener<IReplyChannel>(), new Collection<RequestInterceptor>(interceptors))
        {
        }

        protected override IReplyChannel CreateChannel(IReplyChannel inner, Collection<RequestInterceptor> interceptors)
        {
            return new RequestInterceptorReplyChannel(this, inner, interceptors);
        }
    }

    class RequestInterceptorReplySessionChannelListener : RequestInterceptorReplyChannelListenerBase<IReplySessionChannel>
    {
        public RequestInterceptorReplySessionChannelListener(BindingContext c, Collection<RequestInterceptor> interceptors)
            : base(c.BuildInnerChannelListener<IReplySessionChannel>(), new Collection<RequestInterceptor>(interceptors))
        {
        }

        protected override IReplySessionChannel CreateChannel(IReplySessionChannel inner, Collection<RequestInterceptor> interceptors)
        {
            return new RequestInterceptorReplySessionChannel(this, inner, interceptors);
        }
    }

    abstract class RequestInterceptorReplyChannelBase<TChannel> : ChannelBase, IReplyChannel
        where TChannel : IReplyChannel
    {
        TChannel inner;
        Collection<RequestInterceptor> interceptors;

        public RequestInterceptorReplyChannelBase(ChannelManagerBase channelManager, TChannel inner, Collection<RequestInterceptor> interceptors)
            : base(channelManager)
        {
            this.inner = inner;
            this.interceptors = interceptors;
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryReceiveRequestAsyncResult(timeout, this, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginWaitForRequest(timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            RequestContext context;
            if (!TryReceiveRequestAsyncResult.End(result, out context))
            {
                throw new TimeoutException();
            }
            return context;
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            return TryReceiveRequestAsyncResult.End(result, out context);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return this.inner.EndWaitForRequest(result);
        }

        public EndpointAddress LocalAddress
        {
            get { return inner.LocalAddress; }
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext result;
            if (!this.TryReceiveRequest(timeout, out result))
            {
                throw new TimeoutException();
            }
            return result;
        }

        RequestContext WrapInner(RequestContext requestContext)
        {
            if (requestContext == null) return null;
            for (int i = 0; i < this.interceptors.Count; ++i)
            {
                RequestContext original = requestContext;
                bool dispose = true;
                try
                {
                    this.interceptors[i].ProcessRequest(ref requestContext);
                    dispose = false;
                }
                finally
                {
                    if (dispose)
                    {
                        original.Abort();
                    }
                }
                if (requestContext == null)
                {
                    return null;
                }
            }
            return requestContext;
        }

        IAsyncResult BeginWrapInner(RequestContext requestContext, AsyncCallback callback, object state)
        {
            return new WrapInnerAsyncResult(requestContext, this.interceptors, callback, state);
        }

        RequestContext EndWrapInner(IAsyncResult result)
        {
            return WrapInnerAsyncResult.End(result);
        }

        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(this.DefaultReceiveTimeout);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            while (true)
            {
                if (inner.TryReceiveRequest(timeoutHelper.RemainingTime(), out context))
                {
                    if (context == null) return true;
                    context = WrapInner(context);
                    // if the message was discarded by the interceptors, read the next message
                    if (context == null) continue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return inner.WaitForRequest(timeout);
        }

        protected override void OnAbort()
        {
            inner.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inner.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            inner.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            inner.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            inner.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            inner.Open(timeout);
        }

        public override T GetProperty<T>()
        {
            return this.inner.GetProperty<T>();
        }

        class TryReceiveRequestAsyncResult : AsyncResult
        {
            TimeoutHelper timeoutHelper;
            RequestContext context;
            RequestInterceptorReplyChannelBase<TChannel> channel;
            bool timedout;

            public TryReceiveRequestAsyncResult(TimeSpan timeout, RequestInterceptorReplyChannelBase<TChannel> channel, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
                IAsyncResult result = this.channel.inner.BeginTryReceiveRequest(timeoutHelper.RemainingTime(), this.InnerTryReceiveCallback, null);
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                if (OnWrapInnerRequest(result))
                {
                    Complete(true);
                }
            }

            void InnerTryReceiveCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously) return;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    if (OnWrapInnerRequest(result))
                    {
                        completeSelf = true;
                    }
                }
                catch (Exception e)
                {
                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    Complete(false, completionException);
                }
            }

            void WrapInnerCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously) return;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    this.context = this.channel.EndWrapInner(result);
                    if (this.context != null)
                    {
                        completeSelf = true;
                    }
                    else
                    {
                        IAsyncResult newResult = this.channel.inner.BeginTryReceiveRequest(this.timeoutHelper.RemainingTime(), this.InnerTryReceiveCallback, null);
                        if (!newResult.CompletedSynchronously)
                        {
                            return;
                        }
                        completeSelf = this.OnWrapInnerRequest(newResult);
                    }
                }
                catch (Exception e)
                {
                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    Complete(false, completionException);
                }
            }

            bool OnWrapInnerRequest(IAsyncResult result)
            {
                while (true)
                {
                    RequestContext innerContext;
                    if (!this.channel.inner.EndTryReceiveRequest(result, out innerContext))
                    {
                        timedout = true;
                        return true;
                    }
                    if (innerContext == null)
                    {
                        this.context = null;
                        return true;
                    }
                    IAsyncResult wrapResult = this.channel.BeginWrapInner(innerContext, this.WrapInnerCallback, null);
                    if (!wrapResult.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.context = this.channel.EndWrapInner(wrapResult);
                    if (this.context != null)
                    {
                        return true;
                    }
                    result = this.channel.inner.BeginTryReceiveRequest(this.timeoutHelper.RemainingTime(), this.InnerTryReceiveCallback, null);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }
            }

            public static bool End(IAsyncResult result, out RequestContext context)
            {
                AsyncResult.End<TryReceiveRequestAsyncResult>(result);
                TryReceiveRequestAsyncResult typedResult = (TryReceiveRequestAsyncResult) result;
                context = typedResult.context;
                return !typedResult.timedout;
            }
        }

        class WrapInnerAsyncResult : AsyncResult
        {
            RequestContext context;
            Collection<RequestInterceptor> interceptors;
            int index;

            public WrapInnerAsyncResult(RequestContext context, Collection<RequestInterceptor> interceptors, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;
                this.interceptors = interceptors;
                if (this.RunInterceptors())
                {
                    Complete(true);
                }
            }

            bool RunInterceptors()
            {
                while (this.index < this.interceptors.Count)
                {
                    RequestContext original = this.context;
                    bool dispose = true;
                    RequestInterceptor interceptor = this.interceptors[this.index];
                    try
                    {
                        if (interceptor.IsSynchronous)
                        {
                            interceptor.ProcessRequest(ref this.context);
                            dispose = false;
                        }
                        else
                        {
                            IAsyncResult result = interceptor.BeginProcessRequest(this.context, this.ProcessRequestCallback, null);
                            if (!result.CompletedSynchronously)
                            {
                                dispose = false;
                                return false;
                            }
                            else
                            {
                                this.context = interceptor.EndProcessRequest(result);
                                dispose = false;
                            }
                        }
                    }
                    finally
                    {
                        if (dispose)
                        {
                            original.Abort();
                        }
                    }
                    if (this.context == null) return true;
                    ++this.index;
                }
                return true;
            }

            void ProcessRequestCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously) return;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    RequestContext original = this.context;
                    bool dispose = true;
                    try
                    {
                        this.context = this.interceptors[this.index].EndProcessRequest(result);
                        dispose = false;
                    }
                    finally
                    {
                        if (dispose)
                        {
                            original.Abort();
                        }
                    }
                    if (this.context == null)
                    {
                        completeSelf = true;
                    }
                    else
                    {
                        ++this.index;
                        completeSelf = this.RunInterceptors();
                    }
                }
                catch (Exception e)
                {
                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    Complete(false, completionException);
                }
            }

            public static RequestContext End(IAsyncResult result)
            {
                AsyncResult.End<WrapInnerAsyncResult>(result);
                WrapInnerAsyncResult typedResult = (WrapInnerAsyncResult)result;
                return typedResult.context;
            }
        }
    }

    class RequestInterceptorReplyChannel : RequestInterceptorReplyChannelBase<IReplyChannel>
    {
        public RequestInterceptorReplyChannel(ChannelManagerBase cmb, IReplyChannel inner, Collection<RequestInterceptor> interceptors)
            : base(cmb, inner, interceptors)
        {
        }
    }

    class RequestInterceptorReplySessionChannel : RequestInterceptorReplyChannelBase<IReplySessionChannel>, IReplySessionChannel
    {
        IReplySessionChannel inner;

        public RequestInterceptorReplySessionChannel(ChannelManagerBase cmb, IReplySessionChannel inner, Collection<RequestInterceptor> interceptors)
            : base(cmb, inner, interceptors)
        {
            this.inner = inner;
        }

        public IInputSession Session
        {
            get { return inner.Session; }
        }
    }

    struct TimeoutHelper
    {
        internal static TimeSpan Infinite { get { return TimeSpan.MaxValue; } }

        DateTime deadline;
        bool deadlineSet;

        TimeSpan originalTimeout;

        internal void SetDeadline()
        {
            this.deadline = DateTime.UtcNow + this.originalTimeout;
            this.deadlineSet = true;
        }

        internal TimeoutHelper(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("timeout", "Timeout is negative");
            }

            this.originalTimeout = timeout;
            this.deadline = DateTime.MaxValue;
            this.deadlineSet = (timeout == TimeSpan.MaxValue);
        }

        public TimeSpan OriginalTimeout
        {
            get { return this.originalTimeout; }
        }

        public TimeSpan RemainingTime()
        {
            if (!this.deadlineSet)
            {
                this.SetDeadline();
                return this.originalTimeout;
            }
            else if (this.deadline == DateTime.MaxValue)
            {
                return TimeSpan.MaxValue;
            }
            else
            {
                TimeSpan remaining = this.deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return remaining;
                }
            }
        }
    }
}
