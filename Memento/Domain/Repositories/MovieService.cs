using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using log4net;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Domain.Entities;

namespace Domain.Repository
{
    public class MovieService : BaseService
    {
        private ILog log = LogManager.GetLogger(nameof(MovieService));
        private PermissionService permissionService;
        public MovieService(PermissionService permissionService) {
            this.permissionService = permissionService;
        }
        public async Task<bool> Add(IFormFile file, int userId, int dropId, int? commentId)
        {
            
            if (!permissionService.CanView(userId, dropId))
            {
                throw new NotAuthorizedException("You do not have acces to this memory.");
            }
            string movieId = this.DropMovieId(dropId, userId, commentId);
            if (movieId == null)
            {
                return false;
            }
            string name = GetName(dropId, movieId, userId);

            Stream stream = file.OpenReadStream();

            try
            {
                // Create S3 service client.             
                using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                {

                    // Setup request for putting an object in S3.                 
                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = BucketName + "/temp",
                        Key = name,
                        InputStream = stream,
                        ContentType = file.ContentType,
                    };
                    PutObjectResponse response = await s3Client.PutObjectAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        this.RemoveMovieId(movieId);
                        return false;
                    }

                }

                await Transcode(name);
            }
            catch (Exception e)
            {
                this.RemoveMovieId(movieId);
                throw e;
            }
            
            return true;
        }


        public async Task<Stream> GetThumb(int imageId, int userId)
        {
            //todo: security check - parse name and check with userId...
            try
            {
                var image = await Context.MovieDrops.Include(i => i.Comment).FirstOrDefaultAsync(x => x.MovieDropId == imageId);
                if (image != null)
                {
                    int dropId = image.DropId;
                    int imageUserId = image.CommentId.HasValue ? image.Comment.UserId : image.Drop.CreatedBy.UserId;
                    if (permissionService.CanView(userId,dropId))
                    {
                        using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                        {
                            GetObjectRequest getObjectRequest = new GetObjectRequest
                            {
                                BucketName = BucketNameThumb,
                                Key = ThumbName(GetName(dropId, imageId.ToString(), imageUserId)) + thumbAppend
                            };

                            GetObjectResponse getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest);
                            var thumb = getObjectResponse.ResponseStream;
                            return thumb;
                        }
                    }
                }
            }
            catch (Exception e) {
                log.Error("get thumb", e);
            }
            
            return null;
        }

        public String GetThumbLink(int imageId, int imageOwnerUserId, int dropId) {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                GetPreSignedUrlRequest getObjectRequest = new GetPreSignedUrlRequest
                {
                    BucketName = BucketNameThumb,
                    Key = ThumbName(GetName(dropId, imageId.ToString(), imageOwnerUserId)) + thumbAppend,
                    Expires = DateTime.Now.AddHours(3)
                };

                return s3Client.GetPreSignedURL(getObjectRequest);
            }
        }

        public String GetLink(int imageId, int imageOwnerUserId, int dropId)
        {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {

                GetPreSignedUrlRequest getObjectRequest = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = GetName(dropId, imageId.ToString(), imageOwnerUserId),
                    Expires = DateTime.Now.AddHours(3)
                };

                return s3Client.GetPreSignedURL(getObjectRequest);
            }
        }
        
        public async Task<Stream> Get(int imageId, int userId)
        {
            var image = await Context.MovieDrops.Include(i => i.Comment).FirstOrDefaultAsync(x => x.MovieDropId == imageId);
            if (image != null)
            {
                int dropId = image.DropId;
                int imageUserId = image.CommentId.HasValue ? image.Comment.UserId : image.Drop.CreatedBy.UserId;
                if (permissionService.CanView(userId, dropId))
                {
                    using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                    {
                        GetObjectRequest getObjectRequest = new GetObjectRequest
                        {
                            BucketName = BucketName,
                            Key = GetName(dropId, imageId.ToString(), imageUserId)
                        };

                        GetObjectResponse getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest);
                        Stream movie = getObjectResponse.ResponseStream;

                        var byteArray = GetByteArray(movie);
                        return new MemoryStream(byteArray);
                    }
                }
            }

            return null;
        }

        public async Task Delete(int dropId, string imageId, int userId)
        {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {                 // Setup request for putting an object in S3.                 
                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
                multiObjectDeleteRequest.BucketName = BucketName;
                var key = GetName(dropId, imageId, userId);
                multiObjectDeleteRequest.AddKey(key, null); // version ID is null.

                try
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest)
                        .ConfigureAwait(false);
                }
                catch (DeleteObjectsException e)
                {
                    log.Error("Delete 1", e);
                }
            }

            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {                 // Setup request for putting an object in S3.                 
                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
                multiObjectDeleteRequest.BucketName = BucketNameThumb;
                var key = ThumbName((GetName(dropId, imageId, userId))) + thumbAppend;
                multiObjectDeleteRequest.AddKey(key, null); // version ID is null.

                try
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest)
                        .ConfigureAwait(false);
                }
                catch (DeleteObjectsException e)
                {
                    log.Error("Delete 2", e);
                }
            }
        }

        private async Task DeleteFullSize(string key) {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {                 // Setup request for putting an object in S3.                 
                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
                multiObjectDeleteRequest.BucketName = BucketName;
                multiObjectDeleteRequest.AddKey(key, null); // version ID is null.

                try
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest);
                }
                catch (DeleteObjectsException e)
                {
                    log.Error("Delete full size", e);
                }
            }

        }

        private async Task Transcode(string name)
        {
            var key = string.Format("temp/{0}", name);
            using (IAmazonElasticTranscoder tClient = new AmazonElasticTranscoderClient(RegionEndpoint.USEast1))
            {

                var request = new CreateJobRequest
                {
                    Input = new JobInput
                    {
                        Key = key,
                    },

                    Output = new CreateJobOutput
                    {
                        Key = name,
                        //PresetId = "1351620000001-000030",
                        //PresetId = "1562905432459-ygwxzk",
                        PresetId = "1616252839026-ptk8oj",
                        ThumbnailPattern = ThumbName(name) + "_{count}"
                    },
                    PipelineId = "1477544179677-j0ddby",
                };


                var response = await tClient.CreateJobAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    //do something
                }

            }

            using (IAmazonSQS sClient = new AmazonSQSClient(RegionEndpoint.USEast1))
            {
                var url = "https://sqs.us-east-1.amazonaws.com/116134826460/transcode_complete";
                var request = await sClient.ReceiveMessageAsync(url);
                var message = request.Messages.FirstOrDefault();
                if (message != null)
                {
                    await sClient.DeleteMessageAsync(new DeleteMessageRequest
                    {
                        QueueUrl = url,
                        ReceiptHandle = message.ReceiptHandle
                    });

                    var body = JsonSerializer.Deserialize<MessageBody>(message.Body);
                    var bodyTyped = JsonSerializer.Deserialize<Models.Message>(body.Message);
                    key = bodyTyped.input.key;
                    await DeleteFullSize(key);
                }
            }


        }

        public static string GetName(int dropId, string imageId, int userId)
        {
            return string.Format("{0}/{1}/m/{2}", userId, dropId, imageId);
        }

        private static string ThumbName(string name)
        {
            return name.Replace('/', '_');
        }

        private string thumbAppend = "_00001.png";


        private async Task DeleteInFolder(int id)
        {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
                multiObjectDeleteRequest.BucketName = BucketName + "in";
                var key = id.ToString();
                multiObjectDeleteRequest.AddKey(key, null);

                try
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest);
                }
                catch (DeleteObjectsException e)
                {
                    log.Error("Delete folder", e);
                }
            }
        }

        public void RemoveMovieId(string imageId)
        {
            int id = int.Parse(imageId);
            var movie = Context.MovieDrops.FirstOrDefault(x => x.MovieDropId == id);
            if (movie != null)
            {
                Context.MovieDrops.Remove(movie);
                Context.SaveChanges();
            }
        }

        private string DropMovieId(int dropId, int userId, int? commentId)
        {
            if (!permissionService.CanView(userId, dropId))
            {
                throw new NotAuthorizedException("You do not have acces to this memory.");
            }
            var drop = Context.Drops.FirstOrDefault(x => x.DropId == dropId);
            if (drop == null)
            {
                return null;
            }
            //grab imageId = imageId;
            //insert next
            var movie = new MovieDrop { CommentId = commentId };
            drop.Movies.Add(movie);
            Context.SaveChanges();
            return movie.MovieDropId.ToString();
        }

        private static byte[] GetByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


    }
}
