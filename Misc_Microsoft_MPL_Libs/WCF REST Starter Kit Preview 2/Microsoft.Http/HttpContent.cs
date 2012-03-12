//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Diagnostics;

    public sealed class HttpContent : IDisposable
    {
        static readonly EmptyContent empty = new EmptyContent();
        readonly string contentType;

        byte[] buffer;
        Content content;
        long? contentLength;
        ContentState state;

        HttpContent(Content content, string contentType, long? contentLength)
        {
            this.content = content;
            this.contentType = contentType;
            this.contentLength = contentLength;
            this.state = ContentState.Created;
        }

        enum ContentState
        {
            Created,
            Buffered,
            ConsumedRead,
            ConsumedWrite,
            Disposed
        }

        public string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public static HttpContent Create(Stream source)
        {
            long? length = source.CanSeek ? source.Length : (long?)null;
            return Create(source, null, length);
        }

        public static HttpContent Create(Stream source, string contentType, long? length)
        {
            return new HttpContent(new StreamContent(source), contentType, length);
        }

        public static HttpContent Create(Action<Stream> writer)
        {
            return Create(writer, null);
        }
        public static HttpContent Create(Action<Stream> writer, string contentType)
        {
            return Create(writer, contentType, null);
        }
        public static HttpContent Create(Action<Stream> writer, string contentType, long? length)
        {
            return new HttpContent(new ActionContent(writer, length), contentType, length);
        }
        public static HttpContent Create(byte[] value)
        {
            return Create(value, null);
        }
        public static HttpContent Create(byte[] value, string contentType)
        {
            return new HttpContent(new BytesContent(value), contentType, value.LongLength);
        }

        public static HttpContent Create(string value)
        {
            return Create(value, HttpTextEncodingHelpers.DefaultHttpEncoding);
        }
        public static HttpContent Create(string value, string contentType)
        {
            var charsetEncoding = HttpTextEncodingHelpers.ExtractEncodingOrDefaultHttp(contentType);

            return Create(value, charsetEncoding, contentType);
        }

        public static HttpContent Create(string value, Encoding encoding)
        {
            return Create(value, encoding, null);
        }
        public static HttpContent Create(string value, Encoding encoding, string contentType)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (contentType != null)
            {
                HttpTextEncodingHelpers.EnsureContentTypeMatches(encoding, contentType);
            }

            var bytes = encoding.GetBytes(value);
            return new HttpContent(new StringContent(value, encoding, bytes), contentType, bytes.LongLength);
        }
        public static HttpContent Create(Func<Stream> deferred)
        {
            return Create(deferred, null);
        }
        public static HttpContent Create(Func<Stream> deferred, string contentType)
        {
            return Create(deferred, contentType, null);
        }
        public static HttpContent Create(Func<Stream> deferred, string contentType, long? length)
        {
            return new HttpContent(new DeferredStreamContent(deferred, length), contentType, length);
        }
        public static HttpContent Create(FileInfo info, string contentType)
        {
            return HttpContent.Create(() => new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read), contentType, info.Length);
        }

        public static HttpContent Create(ICreateHttpContent creator)
        {
            return creator.CreateHttpContent();
        }
        public static HttpContent CreateEmpty()
        {
            return new HttpContent(empty, null, 0);
        }

        public static bool IsNullOrEmpty(HttpContent content)
        {
            if (content == null)
            {
                return true;
            }
            if (content.content is EmptyContent || (content.HasLength() && content.GetLength() == 0))
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (this.state == ContentState.Disposed)
            {
                return;
            }
            this.state = ContentState.Disposed;

            this.content.Close();
            this.content = null;
            this.buffer = null;
        }

        // this is a method because it's not stateless
        public long GetLength()
        {
            if (!HasLength())
            {
                throw new InvalidOperationException();
            }
            return contentLength.Value;
        }

        public bool HasLength()
        {
            return this.contentLength != null;
        }

        public void LoadIntoBuffer()
        {
            ThrowIfDisposedOrConsumed();

            if (state == ContentState.Buffered)
            {
                return;
            }
            this.state = ContentState.Buffered;
            Debug.Assert(buffer == null);
            var temp = content.ReadAsBytes(this.contentLength);
            this.buffer = temp;
            this.contentLength = temp.Length;
        }

        public byte[] ReadAsByteArray()
        {
            ThrowIfDisposedOrConsumed();
            if (this.state == ContentState.Buffered)
            {
                return this.buffer;
            }
            this.state = ContentState.ConsumedRead;
            return this.content.ReadAsBytes(this.contentLength);
        }

        public Stream ReadAsStream()
        {
            ThrowIfDisposedOrConsumed();
            if (state == ContentState.Buffered)
            {
                return CreateReadOnlyMemoryStream(this.buffer);
            }
            this.state = ContentState.ConsumedRead;
            return content.ReadAsStream();
        }

        public string ReadAsString()
        {
            HttpContent content = this;
            var encoding = HttpTextEncodingHelpers.ExtractEncodingOrDefaultHttp(content.ContentType);
            if (content.HasLength())
            {
                var preamble = encoding.GetPreamble();
                var bytes = content.ReadAsByteArray();
                string s;
                if (preamble.SequenceEqual(bytes.Take(preamble.Length)))
                {
                    s = encoding.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
                }
                else
                {
                    s = encoding.GetString(bytes);
                }
                return s;
            }

            using (var stream = content.ReadAsStream())
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HttpContent(");

            if (this.contentType != null)
            {
                sb.Append("ContentType = ");
                sb.Append("\"" + this.contentType + "\"");
                sb.Append(", ");
            }

            if (this.contentLength != null)
            {
                sb.Append("GetLength() = ");
                sb.Append(this.contentLength.Value.ToStringInvariant());
                sb.Append(", ");
            }
            //sb.Append("Buffered = ");
            //sb.Append(this.buffer != null);
            //sb.Append(", ");

            sb.Append(this.content);

            sb.Append(')');
            return sb.ToString();
        }

        public void WriteTo(Stream stream)
        {
            ThrowIfDisposedOrConsumed();

            if (this.state == ContentState.Buffered)
            {
                stream.Write(this.buffer, 0, this.buffer.Length);
                return;
            }

            this.state = ContentState.ConsumedWrite;
            this.content.WriteTo(stream, this.contentLength);
        }

        static MemoryStream CreateReadOnlyMemoryStream(byte[] values)
        {
            return new MemoryStream(values, 0, values.Length, false, false);
        }

        void ThrowIfDisposedOrConsumed()
        {
            if (state == ContentState.Disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
            if (state == ContentState.ConsumedRead)
            {
                throw new InvalidOperationException(this.ToString() + " was already read; you must call LoadIntoBuffer() to allow multiple read or write calls");
            }
            if (state == ContentState.ConsumedWrite)
            {
                throw new InvalidOperationException(this.ToString() + " was already written; you must call LoadIntoBuffer() to allow multiple read or write calls");
            }
        }

        class ActionContent : Content
        {
            long? length;
            Action<Stream> writer;
            public ActionContent(Action<Stream> writer, long? length)
            {
                this.writer = writer;
                this.length = length;
            }

            public override Stream ReadAsStream()
            {
                return CreateMemoryStreamWithWriteTo(this.length);
            }
            public override string ToString()
            {
                return writer + "";
            }

            protected override void WriteTo(Stream other)
            {
                writer(other);
            }
        }
        class BytesContent : Content
        {
            readonly byte[] bytes;
            public BytesContent(byte[] bytes)
            {
                if (bytes.LongLength > bytes.Length)
                {
                    throw new NotImplementedException();
                }
                this.bytes = bytes;
            }

            public sealed override byte[] ReadAsBytes(long? length)
            {
                Debug.Assert(length.HasValue && length.Value == this.bytes.LongLength);
                return this.bytes;
            }

            public sealed override Stream ReadAsStream()
            {
                return CreateReadOnlyMemoryStream(this.bytes);
            }

            public override string ToString()
            {
                return "... bytes ...";
            }

            protected sealed override void WriteTo(Stream other)
            {
                other.Write(this.bytes, 0, bytes.Length);
            }
        }

        abstract class Content
        {
            public virtual void Close()
            {
            }

            public virtual byte[] ReadAsBytes(long? length)
            {
                MemoryStream temp = CreateMemoryStreamWithWriteTo(length);
                return temp.ToArray();
            }
            public abstract Stream ReadAsStream();
            public abstract override string ToString();
            protected abstract void WriteTo(Stream other);
            public virtual void WriteTo(Stream other, long? length)
            {
                WriteTo(other);
            }

            protected MemoryStream CreateMemoryStreamWithWriteTo(long? length)
            {
                MemoryStream temp;
                if (length.HasValue)
                {
                    temp = new MemoryStream(checked((int)length.Value));
                }
                else
                {
                    temp = new MemoryStream();
                }
                var ignore = new IgnoreCloseStream(temp);
                this.WriteTo(ignore, length);
                ignore.Flush();
                temp.Position = 0;
                return temp;
            }
        }
        class DeferredStreamContent : Content
        {
            readonly Func<Stream> func;
            StreamContent value;
            long? length;
            public DeferredStreamContent(Func<Stream> stream, long? length)
            {
                this.func = stream;
                this.length = length;
            }

            public override void Close()
            {
                if (value != null)
                {
                    value.Close();
                }
            }
            public override byte[] ReadAsBytes(long? length)
            {
                return EnsureValue().ReadAsBytes(length);
            }
            public override Stream ReadAsStream()
            {
                return EnsureValue().ReadAsStream();
            }
            public override string ToString()
            {
                return this.func.ToString();
            }
            protected override void WriteTo(Stream other)
            {
                this.WriteTo(other, this.length);
            }
            public override void WriteTo(Stream other, long? length)
            {
                var v = EnsureValue();
                var lengthToUse = length ?? this.length;
                v.WriteTo(other, lengthToUse);
            }
            StreamContent EnsureValue()
            {
                if (value == null)
                {
                    var s = func();
                    if (this.length == null && s.CanSeek)
                    {
                        this.length = s.Length;
                    }
                    value = new StreamContent(s);
                }
                return value;
            }
        }
        sealed class EmptyContent : Content
        {
            static readonly byte[] emptyArray = new byte[0];
            public override byte[] ReadAsBytes(long? length)
            {
                return emptyArray;
            }
            public override Stream ReadAsStream()
            {
                return CreateReadOnlyMemoryStream(emptyArray);
            }
            public override string ToString()
            {
                return "Empty";
            }
            protected override void WriteTo(Stream other)
            {
            }
        }

        class IgnoreCloseStream : DelegatingStream
        {
            string s;
            public IgnoreCloseStream(Stream stream)
                : base(stream)
            {
                s = "IgnoreClose(" + stream + ")";
            }

            public override void Close()
            {
            }

            public override string ToString()
            {
                return s;
            }
        }
        class StreamContent : Content
        {
            bool closed;
            Stream stream;
            public StreamContent(Stream s)
            {
                this.stream = s;
            }
            public override void Close()
            {
                if (!closed)
                {
                    this.stream.Close();
                    this.stream.Dispose();
                }
                stream = null;
            }
            public override Stream ReadAsStream()
            {
                // closed = true?
                return stream;
            }
            public override string ToString()
            {
                return "... streamed data ...";
            }
            public override void WriteTo(Stream other, long? length)
            {
                using (this.stream)
                {
                    closed = true;
                    StreamExtensions.CopyAndCloseSource(this.stream, other, length);
                }
            }
            protected override void WriteTo(Stream other)
            {
                WriteTo(other, null);
            }
        }

        class StringContent : BytesContent
        {
            readonly Encoding encoding;
            readonly string value;

            public StringContent(string value, Encoding encoding, byte[] bytes)
                : base(bytes)
            {
                this.value = value;
                this.encoding = encoding;
            }

            public override string ToString()
            {
                return "Encoding = " + this.encoding.EncodingName + ", Value = \"" + this.value + "\"";
            }
        }

        static class HttpTextEncodingHelpers
        {
            static Encoding defaultHttp;
            public static Encoding DefaultHttpEncoding
            {
                get
                {
                    if (defaultHttp == null)
                    {
                        defaultHttp = Encoding.GetEncoding("iso-8859-1");
                    }
                    return defaultHttp;
                }
            }
            public static void EnsureContentTypeMatches(Encoding encoding, string contentType)
            {
                var cs = ExtractEncoding(contentType, null);
                if (cs != null && !cs.WebName.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentOutOfRangeException("contentType", "contentType had charset of " + cs.EncodingName + " but encoding was given as " + encoding.WebName);
                }
            }

            public static Encoding ExtractEncoding(string contentType, Encoding fallback)
            {
                if (string.IsNullOrEmpty(contentType))
                {
                    return fallback;
                }
                var mime = new System.Net.Mime.ContentType(contentType);
                var cs = mime.Parameters["charset"];
                return string.IsNullOrEmpty(cs) ? fallback : Encoding.GetEncoding(cs);
            }


            public static Encoding ExtractEncodingOrDefaultHttp(string contentType)
            {
                return ExtractEncoding(contentType, DefaultHttpEncoding);
            }
        }
    }
}
