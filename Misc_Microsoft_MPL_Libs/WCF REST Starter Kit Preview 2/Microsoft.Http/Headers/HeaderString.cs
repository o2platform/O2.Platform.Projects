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


    abstract class HeaderString
    {
        public enum Last
        {
            None,
            String,
            Object
        }

        public enum Source : byte
        {
            Unknown,
            AddString,
            AddObject,
            AddObjectToCollection,
            Set,
            SetObject,
            GetOrCreate,
            GetCollection,
        }

        public Source Origin
        {
            get;
            protected set;
        }
        public abstract string Value
        {
            get;
            set;
        }

        public abstract T2 Get<T2>();

        public abstract void Set<T2>(T2 value);

        public abstract bool HasValue();
    }

    sealed class HeaderString<T> : HeaderString
    {
       
        static readonly HeaderFormatter formatter = HeaderFormatter.Default;

        Last last;
        T obj;
        string str;
        public HeaderString()
        {
            this.Origin = Source.Unknown;
        }
        public HeaderString(string value, Source source)
        {
            this.Value = value;
            this.Origin = source;
        }
        public HeaderString(T value, Source source)
        {
            this.last = Last.Object;
            this.SetObject(value);
            this.Origin = source;
        }

        public override bool HasValue()
        {
            return this.obj != null || (this.last == Last.String && !string.IsNullOrEmpty(this.str));
        }

        public override string Value
        {
            get
            {
                ThrowIfNone();
                if (last == Last.Object)
                {
                    if (this.obj == null) // always true?
                    {
                        this.str = null;
                    }
                    else
                    {
                        this.str = formatter.ToString(this.obj, this.obj.GetType());
                    }
                }
                return this.str;
            }
            set
            {
                this.last = Last.String;
                this.str = value;
            }
        }
        public override T2 Get<T2>()
        {
            if (typeof(T2) == typeof(T) || this.obj is T2)
            {
                return (T2)(object)this.obj;
            }
            var o = formatter.FromString<T2>(this.Value);
            this.obj = (T)(object)o;
            return o;
        }

        public override void Set<T2>(T2 value)
        {
            if (value is T)
            {
                this.SetObject((T)(object)value);
                return;
            }

            throw new ArgumentOutOfRangeException("value", "value of type " + typeof(T2) + " cannot be stored in " + typeof(T));
        }

        public void SetObject(T value)
        {
            this.obj = value;
            last = Last.Object;
        }

        public override string ToString()
        {
            return typeof(T) + " " + this.last + " [" + this.obj + "] [" + this.str + "]";
        }

        void ThrowIfNone()
        {
            if (this.last == Last.None)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
