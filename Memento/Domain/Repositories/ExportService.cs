using Amazon;
using Amazon.S3;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System.Text;
using Domain.Entities;
using Amazon.SQS.Model;
using System.Linq;

namespace Domain.Repository
{
    public class ExportService : BaseService
    {
        private DropsService dropService;
        public ExportService(DropsService dropsService) {
            dropService = dropsService;
        }

        public async Task<bool> ExportAlbum(int currentUserId, int albumId)
        {
            var results = await dropService.GetAlbumDrops(currentUserId, albumId, 0, true, 500);
            var export = new AlbumExport { 
                AlbumId = albumId,
                AlbumPublicId = Guid.NewGuid().ToString()
            };
            Context.AlbumExports.Add(export);
            /*
            results.Drops.ForEach(drop => {
                drop.ImageLinks = drop.Images.ToDictionary(k => k, v =>
                    ImageService.GetName(drop.DropId, v.ToString(), drop.CreatedById)
                );
                drop.MovieLinks = drop.Movies.ToDictionary(k => k, v =>
                    MovieService.GetName(drop.DropId, v.ToString(), drop.CreatedById)
                );
                drop.Comments.ToList().ForEach(comment => { 
                    comment.ImageLinks = comment.Images.ToDictionary(k => k, v =>
                        ImageService.GetName(drop.DropId, v.ToString(), comment.OwnerId)
                    );
                    comment.MovieLinks = comment.Movies.ToDictionary(k => k, v =>
                        MovieService.GetName(drop.DropId, v.ToString(), comment.OwnerId)
                    );
                });
            });*/

            // permissions ??? hash json and compare - maybe use as key?
            // manipulate routes for images and movies
            // export result to S3
            // drop SQS request to move images and movies, etc
            var key = GetKey(currentUserId, albumId, export.AlbumPublicId);
            try
            {
                var resultJson = JsonConvert.SerializeObject(results);
                // Create S3 service client.             
                using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                {                 // Setup request for putting an object in S3.
                    key = InProduction ? key : "test/" + key;
                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = BucketName,
                        Key = key + "/album.json",
                        InputStream = new MemoryStream(Encoding.UTF8.GetBytes(resultJson)),
                        ContentType = "application/json",
                        //CannedACL = S3CannedACL.PublicRead,// ?
                    };
                    PutObjectResponse response = await s3Client.PutObjectAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return false;
                    }
                }
                using (IAmazonSQS sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1)) {
                    var sendRequest = new SendMessageRequest();
                    var requestBody = new { key, bucketName = BucketName };
                    //sendRequest.QueueUrl = "http://localhost:9324/prod-albumToProcess";
                    sendRequest.QueueUrl = "https://sqs.us-east-1.amazonaws.com/116134826460/prod-albumToProcess";
                    sendRequest.MessageBody = JsonConvert.SerializeObject(requestBody);          
                    var result = await sqsClient.SendMessageAsync(sendRequest);
                }
            }
            catch (Exception e)
            {
                //log & false?
                throw e;
            }
            await Context.SaveChangesAsync();
            return true;
        }

        private string GetKey(int userId, int albumId, string name)
        {
            return string.Format("{0}/exports/{1}/{2}", userId, albumId, name);
        }


    }
}
