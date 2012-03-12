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


    public class Product
    {

        public Product()
        {
        }

        public Product(string product)
            : this(product, null)
        {
        }
        public Product(string product, string version)
        {
            this.Name = product;
            this.Version = version;
        }
        public string Name
        {
            get;
            set;
        }
        public string Version
        {
            get;
            set;
        }

        public static Product Parse(string value)
        {
            var parts = value.Split('/');
            if (parts.Length == 1)
            {
                return new Product(value);
            }
            else if (parts.Length == 2)
            {
                return new Product(parts[0], parts[1]);
            }
            else
            {
                throw new FormatException(value);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return "";
            }
            if (string.IsNullOrEmpty(this.Version))
            {
                return this.Name;
            }
            return this.Name + "/" + this.Version;
        }
    }
}
