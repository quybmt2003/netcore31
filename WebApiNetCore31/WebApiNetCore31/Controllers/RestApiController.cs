using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using WebApiNetCore31.Filter;
using System.IO;

namespace WebApiNetCore31.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestApiController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public RestApiController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/RestApi
        [HttpGet]
        //[CacheOutputFilter(Location = ResponseCacheLocation.Any)]
        public async Task<string> Get()
        {
            var fileInfo = new FileInfo("C:\t.txt");
            var latestEtag = fileInfo.LastWriteTime.Ticks;

            var etag = Request.Headers[HeaderNames.IfNoneMatch];
            //_cache.GetOrCreate
            Response.Headers.Add(HeaderNames.ETag, new[] { DateTime.Now.Ticks.ToString() });
            return DateTime.Now.ToString();
        }

        // GET: api/RestApi/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            var fileInfo = new FileInfo(@"C:\Data\t.txt");
            var latestEtag = fileInfo.CreationTime.Ticks;

            if (!Request.Headers.ContainsKey(HeaderNames.IfNoneMatch)) throw new Exception("Etag have to define");

            Response.Headers.Add(HeaderNames.AccessControlExposeHeaders, "etag") ;
            //Response.Headers.Add(HeaderNames.AccessControlAllowHeaders, "etag");
            Response.Headers.Add(HeaderNames.ETag, new[] { latestEtag.ToString() });

            var etag = Request.Headers[HeaderNames.IfNoneMatch][0];
            
            if (etag?.Equals(latestEtag.ToString()) == false
                || !_cache.TryGetValue($"{id}_{etag}", out object value))
            {
                _cache.Remove($"{id}_{etag}");
                var data = _cache.GetOrCreate($"{id}_{latestEtag}", entry =>
                {
                    //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10); access-control-expose-headers
                    return fileInfo.CreationTime.ToString();
                });
                return data;
            }
            else
            {
                return value?.ToString();
            }
        }

        // POST: api/RestApi
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/RestApi/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
