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

    public sealed class HeaderValues<T> : IEnumerable<T>
    {
        internal char Delimiter = ',';

        readonly List<HeaderString> values;

        public HeaderValues()
            : this(null)
        {
        }

        internal HeaderValues(List<HeaderString> values)
        {
            this.values = values ?? new List<HeaderString>();
        }

        public int Count
        {
            get
            {
                return this.values.Count;
            }
        }

        internal List<HeaderString> InternalValues
        {
            get
            {
                return this.values;
            }
        }

        public static implicit operator HeaderValues<T>(T single)
        {
            var h = new HeaderValues<T>();
            h.Add(single);
            return h;
        }

        public T this[int index]
        {
            get
            {
                return this.values[index].Get<T>();
            }
            set
            {
                this.values[index].Set(value);
            }
        }

        public void Add(T item)
        {
            values.Add(new HeaderString<T>(item, HeaderString.Source.AddObjectToCollection));
        }

        public void AddString(string value)
        {
            values.Add(new HeaderString<T>(value, HeaderString.Source.AddString));
        }

        public void Clear()
        {
            values.Clear();
        }

        IEnumerable<T> GetEnumeratorCore()
        {
            return this.values.Select((x) => x.Get<T>());
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumeratorCore().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorCore().GetEnumerator();
        }

        public override string ToString()
        {
            if (this.Delimiter == ' ')
            {
                return string.Join(" ", InternalValues.Select((v) => v.Value).ToArray());
            }
            else
            {
                return string.Join(this.Delimiter + " ", InternalValues.Select((v) => v.Value).ToArray());
            }
        }

#if ICOLLECTION
        void ICollection<T>.Clear()
        {
            this.InternalValues.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return this.values.Any((hs) => object.Equals(hs.Get<T>(), item));
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < this.InternalValues.Count; ++i)
            {
                array[i + arrayIndex] = this.InternalValues[i].Get<T>();
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            var index = this.values.FindIndex((hs) => object.Equals(hs.Get<T>(), item));
            if (index != -1)
            {
                this.values.RemoveAt(index);
            }
            return index != -1;
        }
#endif
    }
}
