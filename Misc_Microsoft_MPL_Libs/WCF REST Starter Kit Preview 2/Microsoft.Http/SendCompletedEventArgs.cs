//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class SendCompletedEventArgs : AsyncCompletedEventArgs
    {
        readonly HttpRequestMessage request;
        readonly HttpResponseMessage response;
        public SendCompletedEventArgs(HttpRequestMessage request, HttpResponseMessage response, Exception exception, bool cancelled, object userState)
            : base(exception, cancelled, userState)
        {
            this.request = request;
            this.response = response;
        }

        bool prevent;
        public bool PreventAutomaticDispose
        {
            get
            {
                return prevent;
            }
            set
            {
                // if one says false and one says true = true
                prevent = prevent || value;
            }
        }
        public HttpRequestMessage Request
        {
            get
            {
                return this.request;
            }
        }

        public HttpResponseMessage Response
        {
            get
            {
                if (this.Error != null)
                {
                    throw this.Error;
                }
                if (this.Cancelled)
                {
                    throw new InvalidOperationException("Cancelled");
                }
                return this.response;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SendCompletedEventArgs(Request = {0}, Response = {1}, Cancelled = {2}, Error = {3})",
                this.request, this.response, this.Cancelled,
                this.Error == null ? "(null)" : this.Error.GetType() + ": " + this.Error.Message);
        }
    }
}