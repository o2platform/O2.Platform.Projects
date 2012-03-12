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
    using System.Timers;
    using System.ComponentModel;
    using Microsoft.Http.Headers;
    using System.Net;
    using System.Threading;
    using Timer = System.Timers.Timer;

    public class PollingAgent : IDisposable
    {
        static readonly TimeSpan defaultPollingInterval = TimeSpan.FromMinutes(5);

        Timer timer;
        EntityTag etag;
        Uri uri;
        bool isBusy;
        bool disposed;
        DateTime? expires;
        DateTime? lastModifiedTime;
        object thisLock = new object();
        SynchronizationContext syncContext;

        public PollingAgent()
        {
            this.PollingInterval = defaultPollingInterval;
        }

        public HttpClient HttpClient { get; set; }

        // Frequency of issuing conditional GET
        public TimeSpan PollingInterval { get; set; }

        // Fires if Conditional GET returns a status OK instead of NotModified
        public event EventHandler<ConditionalGetEventArgs> ResourceChanged;

        // Whether failures in sending the conditional GET should be ignored
        public bool IgnoreSendErrors { get; set; }

        // Whether response status codes other than NotModified and OK should be ignored
        public bool IgnoreNonOKStatusCodes { get; set; }

        // Whether the polling agent should ignore any Expires header sent by the service and
        // poll anyway
        public bool IgnoreExpiresHeader { get; set; }

        public void StartPolling()
        {
            this.StartPolling(null);
        }

        public void StartPolling(Uri uri)
        {
            this.StartPolling(uri, null, null);
        }

        public void StartPolling(Uri uri, EntityTag etag, DateTime? lastModifiedTime)
        {
            if (this.HttpClient == null)
            {
                throw new InvalidOperationException("The http client to use for polling is not specified.");
            }
            lock (thisLock)
            {
                if (this.isBusy)
                {
                    throw new InvalidOperationException("Polling has already started");
                }
                this.isBusy = true;
            }
            this.uri = uri ?? this.HttpClient.BaseAddress;
            this.etag = etag;
            this.lastModifiedTime = lastModifiedTime;
            this.timer = new Timer(this.PollingInterval.TotalMilliseconds);
            this.timer.AutoReset = true;
            this.syncContext = SynchronizationContext.Current;
            this.timer.Elapsed += this.TimerElapsed;
            this.timer.Enabled = true;
            this.timer.Start();
        }

        public void StopPolling()
        {
            bool dispose;
            lock (thisLock)
            {
                dispose = this.isBusy;
                if (this.isBusy)
                {
                    this.isBusy = false;
                }
            }
            if (dispose && this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.isBusy)
            {
                return;
            }
            if (!this.IgnoreExpiresHeader)
            {
                // Since Expires is a static guess by the server, use Expires only when the server did
                // not send back any Etag or LastModifiedTime, which makes conditional GET impossible
                if (this.etag == null && this.lastModifiedTime == null && this.expires != null && (DateTime.UtcNow < this.expires.Value.ToUniversalTime()))
                {
                    return;
                }
            }
            this.expires = null;
            HttpRequestMessage request = new HttpRequestMessage("GET", this.uri);
            if (this.etag != null)
            {
                var ifNoneMatch = new HeaderValues<EntityTag>();
                ifNoneMatch.Add(this.etag);
                request.Headers.IfNoneMatch = ifNoneMatch;
            }
            request.Headers.IfModifiedSince = this.lastModifiedTime;
            bool stopTimer = false;
            try
            {
                HttpResponseMessage response = null;
                
                try
                {
                    response = this.HttpClient.Send(request);
                }
                catch (Exception ex)
                {
                    if (!this.IgnoreSendErrors)
                    {
                        stopTimer = InvokeHandler(ex);
                    }
                    if (response != null)
                    {
                        response.Dispose();
                    }
                    return;
                }

                using (response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotModified:
                            // the resource has not been modified
                            response.Dispose();
                            break;
                        case HttpStatusCode.OK:
                            // the resource has been modified. Fire the event, along with the response message
                            this.etag = response.Headers.ETag;
                            this.expires = response.Headers.Expires;
                            this.lastModifiedTime = response.Headers.LastModified;
                            try
                            {
                                stopTimer = InvokeHandler(response);
                            }
                            finally
                            {
                                response.Dispose();
                            }
                            break;
                        default:
                            // this is an unexpected error. Fire the event, if errors are not to be suppressed
                            try
                            {
                                if (!this.IgnoreNonOKStatusCodes)
                                {
                                    stopTimer = InvokeHandler(response);
                                }
                            }
                            finally
                            {
                                response.Dispose();
                            }
                            break;
                    }
                }
            }
            finally
            {
                if (stopTimer)
                {
                    StopPolling();
                }
            }
        }

        bool InvokeHandler(HttpResponseMessage response)
        {
            ConditionalGetEventArgs args = new ConditionalGetEventArgs() { Response = response };
            return InvokeHandler(args);
        }

        bool InvokeHandler(Exception error)
        {
            ConditionalGetEventArgs args = new ConditionalGetEventArgs() { SendError = error };
            return InvokeHandler(args);
        }

        bool InvokeHandler(ConditionalGetEventArgs args)
        {
            if (this.syncContext != null)
            {
                this.syncContext.Send(new SendOrPostCallback(this.InvokeHandlerCore), args);
            }
            else
            {
                this.InvokeHandlerCore(args);
            }
            return args.StopPolling;
        }

        void InvokeHandlerCore(object state)
        {
            ConditionalGetEventArgs args = (ConditionalGetEventArgs) state;
            var handler = this.ResourceChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public void Dispose()
        {
            bool disposing;
            lock (thisLock)
            {
                disposing = !this.disposed;
                this.disposed = true;
            }
            if (disposing)
            {
                StopPolling();
            }
        }
    }

    public class ConditionalGetEventArgs : EventArgs
    {
        public HttpResponseMessage Response { get; set; }

        public Exception SendError { get; set; }

        public bool StopPolling { get; set; }
    }
}
