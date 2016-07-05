﻿using System;
using System.Web;
using System.Web.Caching;
using BundleTransformer.Core;
using BundleTransformer.Core.Configuration;
using BundleTransformer.Core.FileSystem;
using CAF.Infrastructure.Core.Data;
using CAF.Infrastructure.Core;
using CAF.Infrastructure.Core.Collections;
namespace CAF.WebSite.Application.WebUI.Theming
{
	public class LessCssHttpHandler : BundleTransformer.Less.HttpHandlers.LessAssetHandlerBase 
    {
		
		public LessCssHttpHandler()
            : this(HttpContext.Current.Cache,
				BundleTransformerContext.Current.GetVirtualFileSystemWrapper(),
                BundleTransformerContext.Current.GetCoreConfiguration().AssetHandler)
        { }

        public LessCssHttpHandler(
            Cache cache,
            IVirtualFileSystemWrapper virtualFileSystemWrapper,
            AssetHandlerSettings assetHandlerConfig)
            : base(cache, virtualFileSystemWrapper, assetHandlerConfig)
        {
		}

        private bool IsThemeableRequest()
        {
			if (!DataSettings.DatabaseIsInstalled())
            {
                return false;
            }
            else
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();

                var requestUrl = webHelper.GetThisPageUrl(false);
                string themeUrl = string.Format("{0}themes", webHelper.GetSiteLocation());
                var isThemeableRequest = requestUrl.StartsWith(themeUrl + "/", StringComparison.InvariantCultureIgnoreCase);

                return isThemeableRequest;
            }
        }

        public override string GetCacheKey(string assetUrl)
        {
            string cacheKey = base.GetCacheKey(assetUrl);

            if (IsThemeableRequest())
            {
				var httpContext = HttpContext.Current;
				if (httpContext != null && httpContext.Request != null)
				{
					var qs = QueryString.Current;
					if (qs.Count > 0)
					{
						// required for Theme editing validation: See Admin.Controllers.ThemeController.ValidateLess()
						if (qs.Contains("theme"))
						{
							EngineContext.Current.Resolve<IThemeContext>().SetRequestTheme(qs["theme"]);
						}
						if (qs.Contains("siteId"))
						{
							EngineContext.Current.Resolve<ISiteContext>().SetRequestSite(qs["siteId"].ToInt());
						}
					}
				}
				
				cacheKey += "_" + EngineContext.Current.Resolve<IThemeContext>().CurrentTheme.ThemeName + "_" + EngineContext.Current.Resolve<ISiteContext>().CurrentSite.Id;
            }

            return cacheKey;
        }

    }
}