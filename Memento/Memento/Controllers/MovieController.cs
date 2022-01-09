using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Memento.Libs;
using Microsoft.AspNetCore.Mvc;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/movies")]
    public class MovieController : BaseApiController
    {
        [HttpGet]
        [Route("{id}.mp4")]
        public async Task<HttpResponseMessage> Get(int id)
        {
            var image = await MovieService.Get(id, CurrentUserId);
            if (image != null)
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Headers.Add("Accept-Ranges", "bytes");
                result.Content = new StreamContent(image);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

                return result;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }


        [HttpGet]
        [Route("{id}/thumb")]
        public async Task<HttpResponseMessage> Thumb(int id)
        {
            var image = await MovieService.GetThumb(id, CurrentUserId);
            if (image != null)
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(image);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

                return result;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}