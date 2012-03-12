//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Web;
using System.IdentityModel.Claims;
using System.ServiceModel.Security;
using System.Collections.ObjectModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.Caching;

namespace Microsoft.ServiceModel.Web
{
    class CachingParameterInspector : IParameterInspector
    {
        OutputCacheProfile cacheProfile;

        public CachingParameterInspector(OutputCacheProfile cacheProfile)
        {
            this.cacheProfile = cacheProfile;
        }

        bool ShouldCacheOnServer(OutputCacheLocation location)
        {
            return (location == OutputCacheLocation.Any || location == OutputCacheLocation.Server || location == OutputCacheLocation.ServerAndClient);
        }

        internal static SqlCacheDependency[] CreateSqlDependencies(string sqlDependency)
        {
            string[] dbTablePairs = sqlDependency.Split(',');
            List<SqlCacheDependency> dependencies = new List<SqlCacheDependency>();
            foreach (string dbTablePair in dbTablePairs)
            {
                string db = null;
                string table = null;
                int separator = dbTablePair.IndexOf(':');
                if (separator > 0)
                {
                    db = dbTablePair.Substring(0, separator);
                    table = dbTablePair.Substring(separator + 1);
                }
                else
                {
                    db = null;
                    table = dbTablePair;
                }
                dependencies.Add(new SqlCacheDependency(db, table));
            }
            return dependencies.ToArray();
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            switch (this.cacheProfile.Location)
            {
                case OutputCacheLocation.Any:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case OutputCacheLocation.Client:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Private);
                    break;
                case OutputCacheLocation.Downstream:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case OutputCacheLocation.None:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    break;
                case OutputCacheLocation.Server:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    break;
                case OutputCacheLocation.ServerAndClient:
                    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    break;
                default:
                    throw new NotSupportedException();
            }
            if (cacheProfile.Duration > 0)
            {
                TimeSpan age = TimeSpan.FromSeconds(cacheProfile.Duration);
                HttpContext.Current.Response.Cache.SetExpires(DateTime.Now + age);
                if (ShouldCacheOnServer(this.cacheProfile.Location))
                {
                    HttpContext.Current.Response.Cache.SetMaxAge(age);
                    HttpContext.Current.Response.Cache.SetValidUntilExpires(true);
                }
            }
            if (cacheProfile.NoStore)
            {
                HttpContext.Current.Response.Cache.SetNoStore();
            }
            if (ShouldCacheOnServer(this.cacheProfile.Location) && !string.IsNullOrEmpty(cacheProfile.SqlDependency))
            {
                if (string.Equals(cacheProfile.SqlDependency, "CommandNotification", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException("CommandNotification is not supported as a valid sql dependency currently");
                }
                else
                {
                    HttpContext.Current.Response.AddCacheDependency(CreateSqlDependencies(cacheProfile.SqlDependency));
                    HttpContext.Current.Response.Cache.SetValidUntilExpires(true);
                }
            }
            if (!string.IsNullOrEmpty(cacheProfile.VaryByParam))
            {
                string[] parameters = cacheProfile.VaryByParam.Split(',');
                foreach (string parameter in parameters)
                {
                    HttpContext.Current.Response.Cache.VaryByParams[parameter] = true;
                }
            }
            if (!string.IsNullOrEmpty(cacheProfile.VaryByHeader))
            {
                string[] headers = cacheProfile.VaryByHeader.Split(',');
                foreach (string header in headers)
                {
                    HttpContext.Current.Response.Cache.VaryByHeaders[header] = true;
                }
            }
            if (!string.IsNullOrEmpty(cacheProfile.VaryByCustom))
            {
                HttpContext.Current.Response.Cache.SetVaryByCustom(cacheProfile.VaryByCustom);
            }
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            return null;
        }
    }
}
