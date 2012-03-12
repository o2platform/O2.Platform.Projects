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

    sealed class HeaderStore
    {
        public static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        readonly List<Entry> entries = new List<Entry>();

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (var e in entries)
                {
                    if (e.Values.Count != 0 && HasCommaString(e))
                    {
                        yield return e.Name;
                    }
                }

                //return entries.Where((e) => e.Values.Count != 0 &&
                //    // !string.IsNullOrEmpty(CommaString(e))
                //    HasCommaString(e)
                //    ).Select((e) => e.Name);
            }
        }

        public string this[string name]
        {
            get
            {
                var temp = GetEntryOrNull(name);

                if (temp == null || temp.Values.Count == 0)
                {
                    throw new KeyNotFoundException(name);
                }

                return CommaString(temp);
            }

            set
            {
                var temp = GetEntryOrCreate(name);
                temp.BoundToCollection = null;
                temp.ResetValuesToEmptyList(); // detach from what's there
                temp.Values.Clear();
                temp.Values.Add(new HeaderString<object>(value, HeaderString.Source.Set));
            }
        }

        public static HeaderStore Parse(string lines)
        {
            var h = new HeaderStore();
            foreach (var line in GetHeaderLines(lines))
            {
                h.Add(line);
            }
            return h;
        }

        public static IEnumerable<string> GetHeaderLines(string lines)
        {
            using (var reader = new System.IO.StringReader(lines))
            {
                string line;
                var buffer = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    buffer.Append(line);
                    if (!line.EndsWithInvariant(","))
                    {
                        yield return buffer.ToString();
                        buffer.Length = 0;
                    }
                }
                if (buffer.Length != 0)
                {
                    yield return buffer.ToString();
                }
            }
        }

        public static Collection<string> ParseMultiValue(string value, char delimiter)
        {
            return ParseMultiValue(value, delimiter, '"', '"');
        }

        public static Collection<string> ParseMultiValue(string value, char delimiter, char open, char close)
        {
            var strings = new Collection<string>();
            bool isInQuote = false;
            int length = 0;
            char[] chArray = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == open || value[i] == close)
                {
                    if (open == close)
                    {
                        isInQuote = !isInQuote;
                    }
                    else
                    {
                        if (value[i] == open)
                        {
                            if (isInQuote)
                            {
                                throw new FormatException(value);
                            }

                            isInQuote = true;
                        }
                        else // if (value[i] == close)
                        {
                            if (!isInQuote)
                            {
                                throw new FormatException(value);
                            }
                            isInQuote = false;
                        }
                    }
                }
                else if ((value[i] == delimiter) && !isInQuote)
                {
                    string str = new string(chArray, 0, length);
                    strings.Add(str.Trim());
                    length = 0;
                    continue;
                }
                chArray[length++] = value[i];
            }
            if (length != 0)
            {
                strings.Add(new string(chArray, 0, length).Trim());
            }
            return strings;
        }

        public void Add(string name, string value)
        {
            var e = GetEntryOrCreate(name);
            e.Values.Add(new HeaderString<object>(value, HeaderString.Source.AddString));
        }

        public void Add<T>(string name, T value)
        {
            var e = GetEntryOrCreate(name);
            var view = new HeaderString<T>(value, HeaderString.Source.AddObject);
            view.ToString();
            e.Values.Add(view);
        }

        public void Add(string line)
        {
            var x = line.IndexOf(':');
            if (x == -1)
            {
                throw new FormatException(line);
            }
            var name = line.Substring(0, x);
            var value = line.Substring(x + 1).Trim();

            Add(name, value);
            //var temp = GetEntryOrCreate(name);
            //temp.Values.Add(new HeaderString<object>(value, HeaderString.Source.AddString));
        }

        public void Clear()
        {
            this.entries.Clear();
        }
        public HeaderValues<T> GetCollection<T>(string name)
        {
            return GetCollection<T>(name, ',', '"', '"');
        }
        public HeaderValues<T> GetCollection<T>(string name, char delimiter, char open, char close)
        {
            var e = GetEntryOrCreate(name);
            if (e.BoundToCollection == null)
            {
                List<string> elements = new List<string>(e.Values.Count);

                if (e.Values.Count == 1 && e.Values[0].Value.IndexOf(delimiter) != -1 && !Comparer.Equals(e.Name, "Set-Cookie"))
                {
                    elements.AddRange(ParseMultiValue(e.Values[0].Value, delimiter, open, close));
                }
                else
                {
                    foreach (var x in e.Values)
                    {
                        elements.Add(x.Value);
                    }
                }

                e.ResetValuesToEmptyList();
                foreach (var item in elements)
                {
                    e.Values.Add(new HeaderString<object>(item.Trim(), HeaderString.Source.GetCollection));
                }
                var c = new HeaderValues<T>(e.Values);
                c.Delimiter = delimiter;
                e.BoundToCollection = c;
            }
            var collection = (HeaderValues<T>)e.BoundToCollection;

            collection.Delimiter = delimiter;

            collection.ToArray();

            return collection;
        }

        public T GetOrNull<T>(string name)
            where T : class
        {
            Entry e = GetEntryOrNull(name);
            if (e == null || e.Values.Count == 0)
            {
                // e.Values.Add(new StringView<T>(default(T)));
                return null;
            }
            if (e.Values.Count == 1)
            {
                return e.Values[0].Get<T>();
            }

            throw new NotSupportedException(e.Values.Count + " values for " + name);
        }
        public T? GetOrNullable<T>(string name) where T : struct
        {
            Entry e = GetEntryOrNull(name);
            if (e == null || e.Values.Count == 0)
            {
                // e.Values.Add(new StringView<T>(default(T)));
                return null;
            }
            if (e.Values.Count == 1)
            {
                return e.Values[0].Get<T>();
            }

            throw new NotSupportedException(e.Values.Count + " values for " + name);
        }

        public void Remove(string name)
        {
            Entry toRemove = null;
            foreach (var e in entries)
            {
                if (HeaderStore.Comparer.Equals(name, e.Name))
                {
                    toRemove = e;
                }
            }

            if (toRemove != null)
            {
                entries.Remove(toRemove);
            }
        }

        public void SetCollection<T>(string name, HeaderValues<T> value)
        {
            var e = GetEntryOrCreate(name);
            if (value == null)
            {
                e.BoundToCollection = null;
                e.ResetValuesToEmptyList();
                return;
            }

            e.ReplaceValuesWith(value);
            e.BoundToCollection = value;
            return;
        }

        // don't need a generic parameter restriction because Nullable<T> will work correctly 
        public void SetOrNull<T>(string name, T value)
        {
            var e = GetEntryOrCreate(name);
            e.Values.Clear();
            if (value != null)
            {
                e.Values.Add(new HeaderString<T>(value, HeaderString.Source.SetObject));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in this.entries)
            {
                if (e.Values.Count == 0)
                {
                    continue;
                }

                if (e.Values.Any((s) => s.Value != null && s.Value.IndexOf(',') != -1))
                {
                    foreach (var h in e.Values)
                    {
                        var v = h.Value;
                        if (!string.IsNullOrEmpty(v))
                        {
                            if (sb.Length != 0)
                            {
                                sb.AppendLine();
                            }
                            sb.AppendFormat("{0}: {1}", e.Name, v);
                        }
                    }
                }
                else
                {
                    var f = CommaString(e);
                    if (!string.IsNullOrEmpty(f))
                    {
                        if (sb.Length != 0)
                        {
                            sb.AppendLine();
                        }
                        sb.AppendFormat("{0}: {1}", e.Name, f);
                    }
                }
            }
            return sb.ToString();
        }

        static bool HasCommaString(Entry e)
        {
            if (e.Values.Count == 0)
            {
                return false;
            }

            if (e.Values.Count == 1)
            {
                return e.Values[0].HasValue();
            }

            foreach (var h in e.Values)
            {
                if (!h.HasValue())
                {
                    return false;
                }
            }
            return true;
        }

        static string CommaString(Entry e)
        {
            if (e.Values.Count == 0)
            {
                return "";
            }
            if (e.Values.Count == 1)
            {
                return e.Values[0].Value ?? "";
            }
            StringBuilder sb = null;
            foreach (var h in e.Values)
            {
                if (string.IsNullOrEmpty(h.Value))
                {
                    continue;
                }
                if (sb == null)
                {
                    sb = new StringBuilder();
                }
                if (sb.Length != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(h.Value);
            }
            return sb == null ? null : sb.ToString();
            // string.Join(", ", e.Values.Select((v) => v.Value).ToArray());
        }

        Entry GetEntryOrCreate(string name)
        {
            return GetEntryWork(name, true);
        }

        Entry GetEntryOrNull(string name)
        {
            return GetEntryWork(name, false);
        }
        Entry GetEntryWork(string name, bool createIfNotThere)
        {
            Entry e = null;

            foreach (var x in entries)
            {
                if (Comparer.Equals(x.Name, name))
                {
                    e = x;
                    break;
                }
            }
            if (e == null && createIfNotThere)
            {
                e = new Entry(name);
                entries.Add(e);
            }
            return e;
        }
        static readonly string[] emptyValues = new string[0];
        public string[] GetValues(string key)
        {
            var e = GetEntryOrNull(key);
            if (e == null || e.Values.Count == 0)
            {
                return emptyValues;
            }
            var s = new string[e.Values.Count];
            for (int i = 0; i < e.Values.Count; ++i)
            {
                s[i] = e.Values[i].Value;
            }
            return s;
        }
        class Entry
        {
            public Entry(string name)
            {
                this.Name = name;
                this.Values = new List<HeaderString>();
            }

            public object BoundToCollection
            {
                get;
                set;
            }
            public string Name
            {
                get;
                private set;
            }

            public List<HeaderString> Values
            {
                get;
                private set;
            }

            public void ReplaceValuesWith<T>(HeaderValues<T> value)
            {
                this.Values = value.InternalValues;
            }

            public void ResetValuesToEmptyList()
            {
                this.Values = new List<HeaderString>();
            }
        }
    }
}
