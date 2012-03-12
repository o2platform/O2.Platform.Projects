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
using System.ServiceModel.Description;
using System.Web.UI;
using System.Web.Configuration;
using System.ServiceModel.Web;
using System.Web.Caching;

namespace Microsoft.ServiceModel.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebCacheAttribute : Attribute, IOperationBehavior
    {
        public WebCacheAttribute()
        {
            this.Location = OutputCacheLocation.Any;
        }

        public int Duration { get; set; }

        public OutputCacheLocation Location { get; set; }

        public bool NoStore { get; set; }

        public string SqlDependency { get; set; }

        public string VaryByHeader { get; set; }

        public string VaryByParam { get; set; }

        public string VaryByCustom { get; set; }

        public string CacheProfileName { get; set; }

        internal OutputCacheProfile CacheProfile { get; set; }

        
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            throw new NotSupportedException();
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (this.CacheProfile == null)
            {
                this.CacheProfile = new OutputCacheProfile("temp");
                this.CacheProfile.Duration = this.Duration;
                this.CacheProfile.NoStore = this.NoStore;
                this.CacheProfile.SqlDependency = this.SqlDependency;
                this.CacheProfile.Location = this.Location;
                this.CacheProfile.VaryByCustom = this.VaryByCustom;
                this.CacheProfile.VaryByHeader = this.VaryByHeader;
                this.CacheProfile.VaryByParam = this.VaryByParam;
            }
            dispatchOperation.ParameterInspectors.Add(new CachingParameterInspector(this.CacheProfile));
        }

        public void Validate(OperationDescription operationDescription)
        {
            if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                throw new NotSupportedException("WebCacheAttribute is supported only in AspNetCompatibility mode.");
            }
            if (operationDescription.Behaviors.Find<WebGetAttribute>() == null)
            {
                throw new InvalidOperationException("The WebCacheAttribute can only be used with GET operations.");
            }
            if (!string.IsNullOrEmpty(this.CacheProfileName))
            {
                OutputCacheProfile cacheProfile = null;
                OutputCacheSettingsSection cacheSettings = (OutputCacheSettingsSection)WebConfigurationManager.GetSection("system.web/caching/outputCacheSettings");
                if (cacheSettings == null)
                {
                    throw new InvalidOperationException(String.Format("Cache profile with name '{0}' is not configured.", this.CacheProfileName));
                }
                cacheProfile = cacheSettings.OutputCacheProfiles[this.CacheProfileName];
                if (cacheProfile == null)
                {
                    throw new InvalidOperationException(String.Format("Cache profile with name '{0}' is not configured.", this.CacheProfileName));
                }
                if (!cacheProfile.Enabled)
                {
                    throw new InvalidOperationException(String.Format("Cache profile with name '{0}' is disabled.", this.CacheProfileName));
                }
                this.CacheProfile = cacheProfile;
            }
            if (string.Equals(this.SqlDependency, "CommandNotification", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("CommandNotification is not supported as a valid sql dependency currently");
            }
            // validate that the dependency has been properly configured in sql
            if (!string.IsNullOrEmpty(this.SqlDependency))
            {
                foreach (SqlCacheDependency dependency in CachingParameterInspector.CreateSqlDependencies(this.SqlDependency))
                {
                    dependency.Dispose();
                }
            }
        }
    }
}
