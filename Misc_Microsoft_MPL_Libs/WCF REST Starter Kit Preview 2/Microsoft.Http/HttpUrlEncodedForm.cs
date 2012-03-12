//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;


    public sealed class HttpUrlEncodedForm : Collection<HttpFormValue>, ICreateHttpContent
    {
        public void Add(string name, string value)
        {
            this.Add(new HttpFormValue()
            {
                Name = name,
                Value = value
            });
        }

        public HttpUrlEncodedForm()
        {
        }
        public HttpUrlEncodedForm(IDictionary<string, string> values)
        {
            foreach (var pair in values)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        public HttpContent CreateHttpContent()
        {
            var url = new UrlEncodedFormContent(this);
            return HttpContent.Create(url.WriteTo, UrlEncodedFormContent.UrlEncodedContentType);
        }

        sealed class UrlEncodedFormContent
        {
            public const string UrlEncodedContentType = "application/x-www-form-urlencoded";
            readonly HttpUrlEncodedForm form;
            public UrlEncodedFormContent(HttpUrlEncodedForm form)
            {
                this.form = form;
            }
            public void WriteTo(Stream stream)
            {
                StreamWriter writer = new StreamWriter(stream); // no using: we don't want to close the stream
                var values = this.form;
                for (int i = 0; i < values.Count; ++i)
                {
                    var pair = values[i];
                    if (i != 0)
                    {
                        writer.Write('&');
                    }
                    writer.Write(UrlEncodeWithUppercasePlus(pair.Name));
                    writer.Write('=');
                    writer.Write(UrlEncodeWithUppercasePlus(pair.Value));
                }
                writer.Flush();
            }

            static string UrlEncodeWithUppercasePlus(string input)
            {
                // this pulls in System.Web.dll
                byte[] output = System.Web.HttpUtility.UrlEncodeToBytes(input);

                // Transform output: all hexas %hh to %HH
                for (int i = 0; i < output.Length; i++)
                {
                    if ((i >= 2 && output[i - 2] == '%') ||
                        (i >= 1 && output[i - 1] == '%'))
                    {
                        if (output[i] >= 'a' && output[i] <= 'z')
                        {
                            output[i] -= 32; // Convert to upper case
                        }
                    }
                }
                string result = Encoding.UTF8.GetString(output, 0, output.Length);
                return result;
            }
        }
    }
}