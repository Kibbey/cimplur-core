using Memento.Web.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Microsoft.AspNetCore.Http;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/images")]
    public class ImageController : BaseApiController
    {

        [HttpGet]
        [Route("{id}")]
        public async Task<HttpResponseMessage> Get(int id)
        {
            Stream image = null;
            try
            {
                image = await ImageService.Get(id, CurrentUserId);
            }
            catch (Exception e)
            {
                // do nothing
            }
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

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> PostFormData(IFormFile formFile)
        {
            /*
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            */
            var file = formFile;
            // TODO - verify below - this may NOT be the way to pull this from the query / path
            var dropId = Int32.Parse(HttpContext.Request.Query["dropId"]);
            var commentIdParam = (String)HttpContext.Request.Query["commentId"];
            int? commentId = null;
            if (commentIdParam != null) {
                commentId = int.Parse(commentIdParam);
            }
            bool result = false;
            if (file != null)
            {
                if (file.ContentType.Split('/')[0] == "video")
                {
                    result = await MovieService.Add(file, CurrentUserId, dropId, commentId);
                }
                if (file.ContentType.Split('/')[0] == "image" || file.FileName.ToLower().Contains(".heic"))
                {
                    result = await ImageService.Add(file, CurrentUserId, dropId, commentId);
                }
            }
            return Ok(result);
        }
    }
}
