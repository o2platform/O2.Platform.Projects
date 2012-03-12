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

    class HeaderFormatter
    {
        public class Expires
        {
            static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public static readonly Expires Zero = new Expires(epochStart.Subtract(TimeSpan.FromDays(1)), "0");
            public static readonly Expires Minus1 = new Expires(epochStart.Subtract(TimeSpan.FromDays(2)), "-1");
            public static readonly DateTime InvalidTime = epochStart.Subtract(TimeSpan.FromDays(3));

            public readonly DateTime DateTime;
            public readonly string Value;
            public Expires(DateTime d, string s)
            {
                this.DateTime = d;
                this.Value = s;
            }

            public override string ToString()
            {
                return this.Value;
            }
        }

        static HeaderFormatter defaultFormatter;

        readonly Dictionary<Type, Format> lookup = new Dictionary<Type, Format>();

        HeaderFormatter()
        {
        }

        public static HeaderFormatter Default
        {
            get
            {
                if (defaultFormatter == null)
                {
                    var temp = new HeaderFormatter();
                    temp.Add(new StringFormat());
                    temp.Add(new DateTimeFormat());
                    temp.Add(new ExpiresFormat());
                    temp.Add(new UriFormat());
                    temp.Add(new Int32Format());
                    temp.Add(new Int64Format());
                    temp.Add(new ByteRangeSpecifierFormat());
                    temp.Add(new StringWithQualityFormat());
                    temp.Add(new EntityTagFormat());
                    temp.Add(new EnumFormat<ContentCoding>()
                        {
                            UseLowerCase = true
                        });
                    temp.Add(new EnumFormat<TransferCoding>()
                        {
                            UseLowerCase = true
                        });
                    temp.Add(new Base64Format());
                    temp.Add(new ContentRangeFormat());
                    temp.Add(new ConnectionHeaderFormat());
                    temp.Add(new ExpectHeaderFormat());
                    temp.Add(new HttpProductFormat());
                    temp.Add(new HttpHostFormat());
                    temp.Add(new HttpViaFormat());
                    temp.Add(new HttpCookieFormat());
                    temp.Add(new HttpWarningFormat());
                    temp.Add(new CacheControlFormat());
                    temp.Add(new HttpProductOrCommentFormat());
                    temp.Add(new HttpDateOrDeltaSecondsFormat());
                    temp.Add(new HttpDateOrEntityTagFormat());
                    temp.Add(new DeltaSecondsFormat());
                    temp.Add(new HttpCredentialFormat());
                    temp.Add(new HttpChallengeFormat());
                    defaultFormatter = temp;
                }
                return defaultFormatter;
            }
        }

        public T FromString<T>(string s)
        {
            var formatter = GetFormat<T>();
            return formatter.FromString(s);
        }

        public string ToString(object value, Type t)
        {
            var formatter = GetFormat(t);
            return formatter.ObjectToString(value);
        }

        public string ToString<T>(T value)
        {
            var formatter = GetFormat<T>();
            return formatter.ToString(value);
        }

        void Add<T>(Format<T> format)
        {
            this.lookup.Add(typeof(T), format);
        }
        Format GetFormat(Type t)
        {
            Format format;
            if (!this.lookup.TryGetValue(t, out format))
            {
                throw new KeyNotFoundException(t.ToString());
            }
            return format;
        }

        Format<T> GetFormat<T>()
        {
            return (Format<T>)GetFormat(typeof(T));
        }

        class Base64Format : Format<byte[]>
        {
            public override byte[] FromString(string s)
            {
                return Convert.FromBase64String(s);
            }

            public override string ToString(byte[] value)
            {
                return Convert.ToBase64String(value);
            }
        }
        class ByteRangeSpecifierFormat : Format<Range>
        {
            public override Range FromString(string s)
            {
                return Range.Parse(s);
            }
        }
        class CacheControlFormat : Format<CacheControl>
        {
            public override CacheControl FromString(string s)
            {
                return CacheControl.Parse(s);
            }
        }
        class ConnectionHeaderFormat : Format<Connection>
        {
            public override Connection FromString(string s)
            {
                return Connection.Parse(s);
            }
        }
        class ContentRangeFormat : Format<ContentRange>
        {
            public override ContentRange FromString(string s)
            {
                return ContentRange.Parse(s);
            }
        }
        class ExpiresFormat : Format<Expires>
        {
            public override Expires FromString(string s)
            {
                if (s == null)
                {
                    return null;
                }
                if (s == "0")
                {
                    return Expires.Zero;
                }
                if (s == "-1")
                {
                    return Expires.Minus1;
                }

                DateTime d;
                bool parsed = DateTime.TryParse(s, DateTimeFormat.info, DateTimeStyles.None, out d);
                if (!parsed)
                {
                    return new Expires(Expires.InvalidTime, s);
                }
                return new Expires(d.ToUniversalTime(), s);
            }

            public override string ToString(Expires value)
            {
                return base.ToString(value);
            }
        }
        class DateTimeFormat : Format<DateTime>
        {
            public static readonly DateTimeFormatInfo info = new DateTimeFormatInfo();
            public override DateTime FromString(string value)
            {
                var d = DateTime.Parse(value, info);
                return d.ToUniversalTime();
            }
            public override string ToString(DateTime value)
            {
                var v = value;
                v = v.ToUniversalTime();
                return v.ToString("R", info);
            }
        }

        class DeltaSecondsFormat : Format<TimeSpan>
        {
            public override TimeSpan FromString(string s)
            {
                double d = double.Parse(s, CultureInfo.InvariantCulture);
                return TimeSpan.FromSeconds(d);
            }
            public override string ToString(TimeSpan value)
            {
                double x = value.TotalSeconds;
                return Math.Floor(x).ToString(CultureInfo.InvariantCulture);
            }
        }
        class EntityTagFormat : Format<EntityTag>
        {
            public override EntityTag FromString(string s)
            {
                return EntityTag.Parse(s);
            }
        }
        class EnumFormat<T> : Format<T>
        {
            public bool UseLowerCase
            {
                get;
                set;
            }
            public override T FromString(string s)
            {
                return (T)Enum.Parse(typeof(T), s, true);
            }
            public override string ToString(T value)
            {
                var s = Enum.GetName(typeof(T), value);

                if (UseLowerCase)
                {
                    s = s.ToLower(CultureInfo.InvariantCulture);
                }

                return s;
            }
        }
        class ExpectHeaderFormat : Format<Expect>
        {
            public override Expect FromString(string s)
            {
                return Expect.Parse(s);
            }
        }

        abstract class Format
        {
            public abstract string ObjectToString(object input);
        }

        abstract class Format<T> : Format
        {
            bool defaultImplementation;
            public abstract T FromString(string s);
            public sealed override string ObjectToString(object input)
            {
                defaultImplementation = false;
                var logic = ToString((T)input);
                if (logic == "" + input && !defaultImplementation)
                {
                    throw new InvalidOperationException(this.GetType().ToString());
                }
                return logic;
            }
            public virtual string ToString(T value)
            {
                defaultImplementation = true;
                return value + "";
            }
        }
        class HttpChallengeFormat : Format<Challenge>
        {
            public override Challenge FromString(string s)
            {
                return Challenge.Parse(s);
            }
        }

        class HttpCookieFormat : Format<Cookie>
        {
            public override Cookie FromString(string s)
            {
                return Cookie.Parse(s);
            }
        }

        class HttpCredentialFormat : Format<Credential>
        {
            public override Credential FromString(string s)
            {
                return Credential.Parse(s);
            }
        }
        class HttpDateOrDeltaSecondsFormat : Format<DateOrDeltaSeconds>
        {
            public override DateOrDeltaSeconds FromString(string s)
            {
                return DateOrDeltaSeconds.Parse(s);
            }
        }

        class HttpDateOrEntityTagFormat : Format<DateOrEntityTag>
        {
            public override DateOrEntityTag FromString(string s)
            {
                return DateOrEntityTag.Parse(s);
            }
        }

        class HttpHostFormat : Format<Host>
        {
            public override Host FromString(string s)
            {
                return Host.Parse(s);
            }
        }

        class HttpProductFormat : Format<Product>
        {
            public override Product FromString(string s)
            {
                return Product.Parse(s);
            }
        }
        class HttpProductOrCommentFormat : Format<ProductOrComment>
        {
            public override ProductOrComment FromString(string s)
            {
                return ProductOrComment.Parse(s);
            }
        }
        class HttpViaFormat : Format<Via>
        {
            public override Via FromString(string s)
            {
                return Via.Parse(s);
            }
        }
        class HttpWarningFormat : Format<Warning>
        {
            public override Warning FromString(string s)
            {
                return Warning.Parse(s);
            }
        }
        class Int32Format : Format<Int32>
        {
            public override int FromString(string s)
            {
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
        }
        class Int64Format : Format<Int64>
        {
            public override long FromString(string s)
            {
                return long.Parse(s, CultureInfo.InvariantCulture);
            }
        }
        class StringFormat : Format<string>
        {
            public override string FromString(string s)
            {
                return s;
            }
        }
        class StringWithQualityFormat : Format<StringWithOptionalQuality>
        {
            public override StringWithOptionalQuality FromString(string s)
            {
                return StringWithOptionalQuality.Parse(s);
            }
        }
        class UriFormat : Format<Uri>
        {
            public override Uri FromString(string s)
            {
                return new Uri(s, UriKind.RelativeOrAbsolute);
            }
        }
    }
}
