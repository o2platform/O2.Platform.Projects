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


    public class ProductOrComment
    {

        string comment;

        public ProductOrComment()
        {
        }

        public ProductOrComment(string comment)
        {
            this.Comment = comment;
        }

        public ProductOrComment(Product product)
        {
            this.Product = product;
        }
        public string Comment
        {
            get
            {
                return comment;
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                    if (value.StartsWithInvariant("("))
                    {
                        if (!value.EndsWithInvariant(")"))
                        {
                            throw new FormatException(value);
                        }
                        value = value.Substring(1, value.Length - 2);
                    }
                }
                comment = value;
            }
        }
        public bool IsComment
        {
            get
            {
                return this.Comment != null;
            }
        }

        public Product Product
        {
            get;
            set;
        }

        public static ProductOrComment Parse(string value)
        {
            value = value.Trim();
            if (value.StartsWithInvariant("("))
            {
                return new ProductOrComment(value);
            }
            else
            {
                return new ProductOrComment(Product.Parse(value));
            }
        }

        public override string ToString()
        {
            if (this.IsComment)
            {
                return "(" + this.Comment + ")";
            }
            else
            {
                if (this.Product == null)
                {
                    return "";
                }
                return this.Product.ToString();
            }
        }
    }
}
