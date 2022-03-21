using System;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;
using log4net;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Domain.Exceptions;
using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ExifTag = SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag;

namespace Domain.Repository
{
    public class ImageService : BaseService
    {
        private ILog log = LogManager.GetLogger(nameof(ImageService));
        private PermissionService permissionService;

        public ImageService(PermissionService permissionService) {
            this.permissionService = permissionService;
        }

        public async Task<bool> Test(string path, string outPath)
        {// Open the file automatically detecting the file type to decode it.
         // Our image is now in an uncompressed, file format agnostic, structure in-memory as
         // a series of pixels.
         // You can also specify the pixel format using a type parameter (e.g. Image<Rgba32> image = Image.Load<Rgba32>("foo.jpg"))
            using (Image image = await Image.LoadAsync(path))
            {
                // Resize the image in place and return it for chaining.
                // 'x' signifies the current image processing context.
                //image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                //ReSizeImageAsync(image);

                RotateImage(image);
                var bitmap = ReSizeImage(image, 2048);
                // The library automatically picks an encoder based on the file extension then
                // encodes and write the data to disk.
                // You can optionally set the encoder to choose.
                await bitmap.SaveAsJpegAsync(outPath);
            } // Dispose - releasing memory into a memory pool ready for the next image you wish to process.
            return true;
        }

        public async Task<bool> Add(IFormFile file, int userId, int dropId, int? commentId)
        {

            string imageId = this.DropImageId(dropId, userId, commentId);
            if (imageId == null)
            {
                return false;
            }
            string name = GetName(dropId, imageId, userId);

            Stream stream;
            try {
                stream = await ReSizeImageAsync(file);
                // Create S3 service client.
                //var creds = new AWSCredentials("", "");
                using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                {                 // Setup request for putting an object in S3.                 
                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = BucketName,
                        Key = name,
                        InputStream = stream,
                        ContentType = "image/jpeg",
                    };
                    PutObjectResponse response = await s3Client.PutObjectAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        this.RemoveImageId(imageId);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                this.RemoveImageId(imageId);
                throw e;
            }
            stream.Dispose();
            
            return true;
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
            var image = Context.ImageDrops.Include(i => i.Comment).FirstOrDefault(x => x.ImageDropId == imageId);
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
                        return getObjectResponse.ResponseStream;
                    }
                }
            }

            return null;
        }

        public async Task Delete(int dropId, string imageId, int userId)
        {
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                // Setup request for putting an object in S3.                 
                DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
                multiObjectDeleteRequest.BucketName = BucketName;
                var key = GetName(dropId, imageId, userId);
                multiObjectDeleteRequest.AddKey(key, null);

                try
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest)
                        .ConfigureAwait(false);
                }
                catch (DeleteObjectsException e)
                {
                    log.Error("Delete", e);
                }
            }
        }

        private async Task<Stream> ReSizeImageAsync(IFormFile file)
        {
            Image image = null;
            if (file.FileName != null && file.FileName.ToLower().Contains(".heic"))
            {
                image = await ConvertToJPG(file);
            }
            else {
                image = await Image.LoadAsync(file.OpenReadStream());
            }
            RotateImage(image);
            var bitmap = ReSizeImage(image, 2048);
            var stream = new MemoryStream();
            await bitmap.SaveAsJpegAsync(stream);
            image.Dispose();
            return stream;
        }

        
        public async Task<Image> ConvertToJPG(IFormFile file) {
            //Convert HEIC/HEIF to JPF
            using (var image = new MagickImage(file.OpenReadStream()))
            {
                image.Format = MagickFormat.Jpeg;
                var ms = new MemoryStream();
                image.Write(ms);
                return await Image.LoadAsync(ms);
            }
        }

        private Image RotateImage(Image image) {
            var tags = image.Metadata.ExifProfile.Values;
            var orientation = image.Metadata.ExifProfile.GetValue(ExifTag.Orientation);
            if (orientation != null)
            {
                switch (orientation.Value.ToString())
                {
                    case "1":
                        // No rotation required.
                        break;
                    case "2": //2
                        image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Horizontal));
                        //image.Mutate(x => x.Rotate(0f));
                        //image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case "3":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate180, FlipMode.None));
                        //image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case "4":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate180,FlipMode.Horizontal));
                        //image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case "5":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate90, FlipMode.Horizontal));
                        //image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case "6":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate90, FlipMode.None));
                        //image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case "7":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate270, FlipMode.Horizontal));
                        //image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case "8":
                        image.Mutate(x => x.RotateFlip(RotateMode.Rotate270, FlipMode.None));
                        //image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                image.Metadata.ExifProfile.RemoveValue(ExifTag.Orientation); // 274
                image.Metadata.ExifProfile.RemoveValue(ExifTag.ImageWidth); // 256 & 257
                image.Metadata.ExifProfile.RemoveValue(ExifTag.ImageLength);
            }
            return image;
        }

        private Image ReSizeImage(Image image, int maxWidth)
        {
            float width = image.Width;
            float height = image.Height;
            maxWidth = height > width ? maxWidth / 2 : maxWidth;
            if (width > maxWidth)
            {
                var aspectRatio = height / width;
                width = maxWidth;
                height = (float)(aspectRatio * width);
            }
            if (width == image.Width) {
                return image;
            }

            image.Mutate(x => x.Resize((int)width, (int)height));
            return image;
        }

        public static string GetName(int dropId, string imageId, int userId) 
        {
            if (InProduction)
            {
                return string.Format("{0}/{1}/{2}", userId, dropId.ToString(), imageId);
            }
            else {
                return string.Format("test/{0}/{1}/{2}", userId, dropId.ToString(), imageId);
            }
        }

        public string DropImageId(int dropId, int userId, int? commentId)
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
            var image = new ImageDrop { CommentId = commentId };
            drop.Images.Add(image);
            Context.SaveChanges();
            return image.ImageDropId.ToString();
        }

        public void RemoveImageId(string imageId)
        {
            // We do NOT do a security check here - this needs done higher up the stack!
            int id = int.Parse(imageId);
            var image = Context.ImageDrops
                .FirstOrDefault(x => x.ImageDropId == id);
            if (image != null)
            {
                Context.ImageDrops.Remove(image);
                Context.SaveChanges();
            }
        }

    }
}
