//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Http.Headers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;


    public abstract partial class HttpHeaders : IEnumerable<KeyValuePair<string, string[]>>
    {
        public bool ContainsKey(string key)
        {
            return store.Keys.Contains(key, HeaderStore.Comparer);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return store.Keys;
            }
        }

        public override string ToString()
        {
            return store.ToString();
        }
        public void Remove(string name)
        {
            store.Remove(name);
        }

        public void Add(string line)
        {
            store.Add(line);
        }

        public void Add(string name, DateTime date)
        {
            store.Add(name, date);
        }

        public void Add(string name, string value)
        {
            store.Add(name, value);
        }
        public void Add(string name, string[] values)
        {
            foreach (var v in values)
            {
                store.Add(name, v);
            }
        }

        public void Clear()
        {
            store.Clear();
        }

        public string this[string name]
        {
            get
            {
                return store[name];
            }
            set
            {
                store[name] = value;
            }
        }
        internal readonly HeaderStore store;
        internal HttpHeaders(HeaderStore store)
        {
            this.store = store;
        }

        public IEnumerable<string> GetValues(string key)
        {
            return store.GetValues(key);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        IEnumerator<KeyValuePair<string, string[]>> GetEnumeratorCore()
        {
            foreach (var k in store.Keys)
            {
                yield return new KeyValuePair<string, string[]>(k, store.GetValues(k));
            }
        }
    }
    // these are the entity headers
    public partial class HttpHeaders
    {
        public HeaderValues<string> Allow
        {
            get
            {
                return store.GetCollection<string>("Allow");
            }
            set
            {
                store.SetCollection("Allow", value);
            }
        }

        public HeaderValues<ContentCoding> ContentEncoding
        {
            get
            {
                return store.GetCollection<ContentCoding>("Content-Encoding");
            }
            set
            {
                store.SetCollection("Content-Encoding", value);
            }
        }
        public HeaderValues<string> ContentLanguage
        {
            get
            {
                return store.GetCollection<string>("Content-Language");
            }
            set
            {
                store.SetCollection("Content-Language", value);
            }
        }

        public long? ContentLength
        {
            get
            {
                return store.GetOrNullable<long>("Content-Length");
            }
            set
            {
                store.SetOrNull("Content-Length", value);
            }
        }

        public Uri ContentLocation
        {
            get
            {
                return store.GetOrNull<Uri>("Content-Location");
            }
            set
            {
                store.SetOrNull("Content-Location", value);
            }
        }

        public byte[] ContentMD5
        {
            get
            {
                return store.GetOrNull<byte[]>("Content-MD5");
            }
            set
            {
                store.SetOrNull("Content-MD5", value);
            }
        }

        public ContentRange ContentRange
        {
            get
            {
                return store.GetOrNull<ContentRange>("Content-Range");
            }
            set
            {
                store.SetOrNull("Content-Range", value);
            }
        }

        public string ContentType
        {
            get
            {
                return store.GetOrNull<string>("Content-Type");
            }
            set
            {
                store.SetOrNull("Content-Type", value);
            }
        }
        public DateTime? Expires
        {
            get
            {
                var e = store.GetOrNull<HeaderFormatter.Expires>("Expires");
                if (e == null)
                {
                    return null;
                }
                return e.DateTime;
            }
            set
            {
                if (value == null)
                {
                    store.SetOrNull<HeaderFormatter.Expires>("Expires", null);
                }
                else
                {
                    store.SetOrNull<HeaderFormatter.Expires>("Expires", new HeaderFormatter.Expires(value.Value, HeaderFormatter.Default.ToString(value.Value)));
                }
            }
        }
        public DateTime? LastModified
        {
            get
            {
                return store.GetOrNullable<DateTime>("Last-Modified");
            }
            set
            {
                store.SetOrNull("Last-Modified", value);
            }
        }
    }

    // these are the general headers
    public partial class HttpHeaders
    {
        public CacheControl CacheControl
        {
            get
            {
                return store.GetOrNull<CacheControl>("Cache-Control");
            }
            set
            {
                store.SetOrNull("Cache-Control", value);
            }
        }
        public Connection Connection
        {
            get
            {
                return store.GetOrNull<Connection>("Connection");
            }
            set
            {
                store.SetOrNull("Connection", value);
            }
        }

        public DateTime? Date
        {
            get
            {
                return store.GetOrNullable<DateTime>("Date");
            }
            set
            {
                store.SetOrNull("Date", value);
            }
        }

        public HeaderValues<string> Pragma
        {
            get
            {
                return store.GetCollection<string>("Pragma");
            }
            set
            {
                store.SetCollection("Pragma", value);
            }
        }

        public HeaderValues<string> Trailer
        {
            get
            {
                return store.GetCollection<string>("Trailer");
            }
            set
            {
                store.SetCollection("Trailer", value);
            }
        }
        public HeaderValues<TransferCoding> TransferEncoding
        {
            get
            {
                return store.GetCollection<TransferCoding>("Transfer-Encoding");
            }
            set
            {
                store.SetCollection("Transfer-Encoding", value);
            }
        }

        public HeaderValues<Product> Upgrade
        {
            get
            {
                return store.GetCollection<Product>("Upgrade");
            }
            set
            {
                store.SetCollection("Upgrade", value);
            }
        }
        public HeaderValues<Via> Via
        {
            get
            {
                return store.GetCollection<Via>("Via");
            }
            set
            {
                store.SetCollection("Via", value);
            }
        }
        public HeaderValues<Warning> Warning
        {
            get
            {
                return store.GetCollection<Warning>("Warning");
            }
            set
            {
                store.SetCollection("Warning", value);
            }
        }
    }
}