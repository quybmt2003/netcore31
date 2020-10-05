using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApiNetCore31.Filter
{
    // prevents the action filter methods to be invoked twice
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CacheOutputFilter : ActionFilterAttribute
    {
        private readonly CacheProfile _cacheProfile;
        private string _cacheVaryByHeader;
        private string[] _cacheVaryByQueryKeys;
        private ResponseCacheLocation? _cacheLocation;

        private string Etag = "f2fad61c-4ffa-4def-9b08-a9b45a1c8ff0";

        public CacheOutputFilter()
        {
            _cacheProfile = new CacheProfile()
            {
                Duration = 60 * 60 // 1 hour
            };
        }

        public string VaryByHeader
        {
            get => _cacheVaryByHeader ?? _cacheProfile.VaryByHeader;
            set => _cacheVaryByHeader = value;
        }

        public string[] VaryByQueryKeys
        {
            get => _cacheVaryByQueryKeys ?? _cacheProfile.VaryByQueryKeys;
            set => _cacheVaryByQueryKeys = value;
        }

        public int Duration => 3600;

        public ResponseCacheLocation Location
        {
            get => _cacheLocation ?? _cacheProfile.Location ?? ResponseCacheLocation.Any;
            set => _cacheLocation = value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var headers = context.HttpContext.Response.Headers;
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            // Clear all headers
            headers.Remove(HeaderNames.Vary);
            headers.Remove(HeaderNames.CacheControl);
            headers.Remove(HeaderNames.Pragma);

            //if (!string.IsNullOrEmpty(VaryByHeader))
            //{
            //    headers[HeaderNames.Vary] = VaryByHeader;
            //}

            //if (VaryByQueryKeys != null)
            //{
            //    var responseCachingFeature = context.HttpContext.Features.Get<IResponseCachingFeature>();
            //    //if (responseCachingFeature == null)
            //    //{
            //    //    throw new InvalidOperationException(
            //    //        Resources.FormatVaryByQueryKeys_Requires_ResponseCachingMiddleware(nameof(VaryByQueryKeys)));
            //    //}
            //    responseCachingFeature.VaryByQueryKeys = VaryByQueryKeys;
            //}

            ////fetch etag from the incoming request header
            //if (request.Headers.Keys.Contains(HeaderNames.IfNoneMatch)
            //    && request.Headers[HeaderNames.IfNoneMatch]
            //                      .ToString().Equals(Etag))
            //{
            //    string cacheControlValue;
            //    switch (Location)
            //    {
            //        case ResponseCacheLocation.Any:
            //            cacheControlValue = "public,";
            //            break;
            //        case ResponseCacheLocation.Client:
            //            cacheControlValue = "private,";
            //            break;
            //        case ResponseCacheLocation.None:
            //            cacheControlValue = "no-cache,";
            //            headers[HeaderNames.Pragma] = "no-cache";
            //            break;
            //        default:
            //            cacheControlValue = null;
            //            break;
            //    }

            //    cacheControlValue = $"{cacheControlValue}max-age={Duration}";
            //    headers[HeaderNames.CacheControl] = cacheControlValue;
            //}
            //else
            //{
            //    headers[HeaderNames.CacheControl] = "no-store";

            //    // Cache-control: no-store, no-cache is valid.
            //    if (Location == ResponseCacheLocation.None)
            //    {
            //        headers.AppendCommaSeparatedValues(HeaderNames.CacheControl, "no-cache");
            //        headers[HeaderNames.Pragma] = "no-cache";
            //    }
            //}

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            // add ETag response header 
            response.Headers.Add(HeaderNames.ETag, new[] { Etag });

            base.OnActionExecuted(context);
        }
    }
}
