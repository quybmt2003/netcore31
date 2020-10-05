using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;

namespace WebApiNetCore31.Filter
{
#if false
    /// <summary>
    /// Specifies the parameters necessary for setting appropriate headers in response caching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ResponseCacheAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        // A nullable-int cannot be used as an Attribute parameter.
        // Hence this nullable-int is present to back the Duration property.
        // The same goes for nullable-ResponseCacheLocation and nullable-bool.
        private int? _duration;
        private ResponseCacheLocation? _location;
        private bool? _noStore;

        /// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// This sets "max-age" in "Cache-control" header.
        /// </summary>
        public int Duration
        {
            get => _duration ?? 0;
            set => _duration = value;
        }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// </summary>
        public ResponseCacheLocation Location
        {
            get => _location ?? ResponseCacheLocation.Any;
            set => _location = value;
        }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to <see langword="true"/>, it sets "Cache-control" header to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "duration" parameter.
        /// </summary>
        public bool NoStore
        {
            get => _noStore ?? false;
            set => _noStore = value;
        }

        /// <summary>
        /// Gets or sets the value for the Vary response header.
        /// </summary>
        public string VaryByHeader { get; set; }

        /// <summary>
        /// Gets or sets the query keys to vary by.
        /// </summary>
        /// <remarks>
        /// <see cref="VaryByQueryKeys"/> requires the response cache middleware.
        /// </remarks>
        public string[] VaryByQueryKeys { get; set; }

        /// <summary>
        /// Gets or sets the value of the cache profile name.
        /// </summary>
        public string CacheProfileName { get; set; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public bool IsReusable => true;

        /// <summary>
        /// Gets the <see cref="CacheProfile"/> for this attribute.
        /// </summary>
        /// <returns></returns>
        public CacheProfile GetCacheProfile(MvcOptions options)
        {
            CacheProfile selectedProfile = null;
            if (CacheProfileName != null)
            {
                options.CacheProfiles.TryGetValue(CacheProfileName, out selectedProfile);
                if (selectedProfile == null)
                {
                    throw new InvalidOperationException("cache is not found");
                }
            }

            // If the ResponseCacheAttribute parameters are set,
            // then it must override the values from the Cache Profile.
            // The below expression first checks if the duration is set by the attribute's parameter.
            // If absent, it checks the selected cache profile (Note: There can be no cache profile as well)
            // The same is the case for other properties.
            _duration = _duration ?? selectedProfile?.Duration;
            _noStore = _noStore ?? selectedProfile?.NoStore;
            _location = _location ?? selectedProfile?.Location;
            VaryByHeader = VaryByHeader ?? selectedProfile?.VaryByHeader;
            VaryByQueryKeys = VaryByQueryKeys ?? selectedProfile?.VaryByQueryKeys;

            return new CacheProfile
            {
                Duration = _duration,
                Location = _location,
                NoStore = _noStore,
                VaryByHeader = VaryByHeader,
                VaryByQueryKeys = VaryByQueryKeys,
            };
        }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var optionsAccessor = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();
            var cacheProfile = GetCacheProfile(optionsAccessor.Value);

            // ResponseCacheFilter cannot take any null values. Hence, if there are any null values,
            // the properties convert them to their defaults and are passed on.
            return new ResponseCacheFilter(cacheProfile, loggerFactory);
        }
    }

    /// <summary>
    /// An <see cref="IActionFilter"/> which sets the appropriate headers related to response caching.
    /// </summary>
    internal class ResponseCacheFilter : IActionFilter, IResponseCacheFilter
    {
        private readonly ResponseCacheFilterExecutor _executor;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of <see cref="ResponseCacheFilter"/>
        /// </summary>
        /// <param name="cacheProfile">The profile which contains the settings for
        /// <see cref="ResponseCacheFilter"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ResponseCacheFilter(CacheProfile cacheProfile, ILoggerFactory loggerFactory)
        {
            _executor = new ResponseCacheFilterExecutor(cacheProfile);
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// This is a required parameter.
        /// This sets "max-age" in "Cache-control" header.
        /// </summary>
        public int Duration
        {
            get => _executor.Duration;
            set => _executor.Duration = value;
        }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// </summary>
        public ResponseCacheLocation Location
        {
            get => _executor.Location;
            set => _executor.Location = value;
        }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to <see langword="true"/>, it sets "Cache-control" header to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "duration" parameter.
        /// </summary>
        public bool NoStore
        {
            get => _executor.NoStore;
            set => _executor.NoStore = value;
        }

        /// <summary>
        /// Gets or sets the value for the Vary response header.
        /// </summary>
        public string VaryByHeader
        {
            get => _executor.VaryByHeader;
            set => _executor.VaryByHeader = value;
        }

        /// <summary>
        /// Gets or sets the query keys to vary by.
        /// </summary>
        /// <remarks>
        /// <see cref="VaryByQueryKeys"/> requires the response cache middleware.
        /// </remarks>
        public string[] VaryByQueryKeys
        {
            get => _executor.VaryByQueryKeys;
            set => _executor.VaryByQueryKeys = value;
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // If there are more filters which can override the values written by this filter,
            // then skip execution of this filter.
            var effectivePolicy = context.FindEffectivePolicy<IResponseCacheFilter>();
            if (effectivePolicy != null && effectivePolicy != this)
            {
                _logger.NotMostEffectiveFilter(GetType(), effectivePolicy.GetType(), typeof(IResponseCacheFilter));
                return;
            }

            _executor.Execute(context);
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }

    internal class ResponseCacheFilterExecutor
    {
        private readonly CacheProfile _cacheProfile;
        private int? _cacheDuration;
        private ResponseCacheLocation? _cacheLocation;
        private bool? _cacheNoStore;
        private string _cacheVaryByHeader;
        private string[] _cacheVaryByQueryKeys;

        public ResponseCacheFilterExecutor(CacheProfile cacheProfile)
        {
            _cacheProfile = cacheProfile ?? throw new ArgumentNullException(nameof(cacheProfile));
        }

        public int Duration
        {
            get => _cacheDuration ?? _cacheProfile.Duration ?? 0;
            set => _cacheDuration = value;
        }

        public ResponseCacheLocation Location
        {
            get => _cacheLocation ?? _cacheProfile.Location ?? ResponseCacheLocation.Any;
            set => _cacheLocation = value;
        }

        public bool NoStore
        {
            get => _cacheNoStore ?? _cacheProfile.NoStore ?? false;
            set => _cacheNoStore = value;
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

        public void Execute(FilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!NoStore)
            {
                //// Duration MUST be set (either in the cache profile or in this filter) unless NoStore is true.
                //if (_cacheProfile.Duration == null && _cacheDuration == null)
                //{
                //    throw new InvalidOperationException(
                //        Resources.FormatResponseCache_SpecifyDuration(nameof(NoStore), nameof(Duration)));
                //}
            }

            var headers = context.HttpContext.Response.Headers;

            // Clear all headers
            headers.Remove(HeaderNames.Vary);
            headers.Remove(HeaderNames.CacheControl);
            headers.Remove(HeaderNames.Pragma);

            if (!string.IsNullOrEmpty(VaryByHeader))
            {
                headers[HeaderNames.Vary] = VaryByHeader;
            }

            if (VaryByQueryKeys != null)
            {
                var responseCachingFeature = context.HttpContext.Features.Get<IResponseCachingFeature>();
                //if (responseCachingFeature == null)
                //{
                //    throw new InvalidOperationException(
                //        Resources.FormatVaryByQueryKeys_Requires_ResponseCachingMiddleware(nameof(VaryByQueryKeys)));
                //}
                responseCachingFeature.VaryByQueryKeys = VaryByQueryKeys;
            }

            if (NoStore)
            {
                headers[HeaderNames.CacheControl] = "no-store";

                // Cache-control: no-store, no-cache is valid.
                if (Location == ResponseCacheLocation.None)
                {
                    headers.AppendCommaSeparatedValues(HeaderNames.CacheControl, "no-cache");
                    headers[HeaderNames.Pragma] = "no-cache";
                }
            }
            else
            {
                string cacheControlValue;
                switch (Location)
                {
                    case ResponseCacheLocation.Any:
                        cacheControlValue = "public,";
                        break;
                    case ResponseCacheLocation.Client:
                        cacheControlValue = "private,";
                        break;
                    case ResponseCacheLocation.None:
                        cacheControlValue = "no-cache,";
                        headers[HeaderNames.Pragma] = "no-cache";
                        break;
                    default:
                        cacheControlValue = null;
                        break;
                }

                cacheControlValue = $"{cacheControlValue}max-age={Duration}";
                headers[HeaderNames.CacheControl] = cacheControlValue;
            }
        }
    }
#endif
}
