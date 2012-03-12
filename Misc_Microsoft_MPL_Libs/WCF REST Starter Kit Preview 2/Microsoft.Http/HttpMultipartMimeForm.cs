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


    public sealed class HttpMultipartMimeForm : System.Collections.IEnumerable, ICreateHttpContent
    {
        public HttpMultipartMimeForm(IDictionary<string, string> values)
            : this()
        {
            foreach (var pair in values)
            {
                this.Add(pair.Key, pair.Value);
            }
        }
        public HttpMultipartMimeForm()
        {
            this.Values = new Collection<HttpFormValue>();
            this.Files = new Collection<HttpFormFile>();
        }

        public Collection<HttpFormFile> Files
        {
            get;
            private set;
        }

        // needs to be a list in case of select multiple or checkboxes: x.html?list=dog&list=cat 
        public Collection<HttpFormValue> Values
        {
            get;
            private set;
        }

        public void Add(string key, string value)
        {
            this.Values.Add(new HttpFormValue()
            {
                Name = key,
                Value = value
            });
        }

        public void Add(string name, string fileName, HttpContent content)
        {
            if (string.IsNullOrEmpty(content.ContentType))
            {
                throw new ArgumentOutOfRangeException("content", "content.ContentType is null or empty");
            }

            this.Files.Add(new HttpFormFile()
                {
                    Name = name,
                    FileName = fileName,
                    Content = content
                });
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Values.Cast<object>().Concat(this.Files.Cast<object>()).GetEnumerator();
        }

        sealed class MultipartMimeFormContent
        {
            static string GetNonceAsHexDigitString(int lengthInBytes)
            {
                var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                var bytes = new byte[lengthInBytes];
                rng.GetBytes(bytes);
                return ToHexDigitString(bytes);
            }
            static string ToHexDigitString(byte[] hash)
            {
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                var h = sb.ToString();
                return h;
            }

            readonly string boundary;

            public MultipartMimeFormContent(HttpMultipartMimeForm form)
            {
                this.Form = form;
                string raw = GetNonceAsHexDigitString(12);
                this.ContentType = "multipart/form-data; boundary=" + raw;
                boundary = "--" + raw;
            }

            public string ContentType
            {
                get;
                private set;
            }

            public HttpMultipartMimeForm Form
            {
                get;
                private set;
            }

            public void WriteTo(Stream stream)
            {
                const string CRLF = "\r\n";
                StreamWriter writer = new StreamWriter(stream);
                foreach (var x in this.Form.Values)
                {
                    if (!string.IsNullOrEmpty(x.Value))
                    {
                        writer.Write(boundary + CRLF);
                        writer.Write("Content-Disposition: form-data; name=\"" + x.Name + "\"" + CRLF);
                        writer.Write(CRLF);
                        writer.Write(x.Value + CRLF);
                    }
                }

                foreach (var pair in this.Form.Files.GroupBy((e) => e.Name))
                {
                    if (pair.Count() != 1)
                    {
                        throw new NotImplementedException();
                    }
                    foreach (var x in pair)
                    {
                        writer.Write(boundary + CRLF);
                        writer.Write(string.Format(CultureInfo.InvariantCulture, "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", x.Name, x.FileName) + CRLF);
                        writer.Write("Content-Type: " + x.Content.ContentType + CRLF);
                        writer.Write(CRLF);

                        writer.Flush();
                        stream.Flush();
                        x.Content.WriteTo(stream);
                        stream.Flush();
                        writer.Write(CRLF);
                    }
                }
                writer.Write(boundary);
                writer.Write("--");
                writer.Write(CRLF);
                writer.Flush();
            }
        }

        public HttpContent CreateHttpContent()
        {
            var mime = new MultipartMimeFormContent(this);
            return HttpContent.Create(mime.WriteTo, mime.ContentType);
        }
    }
}