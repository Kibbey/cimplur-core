using System;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using log4net;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Drawing;

namespace Domain.Repository
{
    public class ImageService : BaseService
    {
        private ILog log = LogManager.GetLogger(nameof(ImageService));

        public async Task<bool> Add(IFormFile file, int userId, int dropId, int? commentId)
        {
            using (DropsService dropService = new DropsService())
            {
                string imageId = dropService.DropImageId(dropId, userId, commentId);
                if (imageId == null)
                {
                    return false;
                }
                string name = GetName(dropId, imageId, userId);

                Stream stream;
                try {
                    stream = ReSizeImage(file);
                    // Create S3 service client.             
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
                            dropService.RemoveImageId(imageId);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    dropService.RemoveImageId(imageId);
                    throw e;
                }
                stream.Dispose();
            }
            return true;
        }


        public async Task<Stream> Get(int imageId, int userId)
        {
            var image = Context.ImageDrops.Include(i => i.Comment).FirstOrDefault(x => x.ImageDropId == imageId);
            if (image != null)
            {
                int dropId = image.DropId;
                int imageUserId = image.CommentId.HasValue ? image.Comment.UserId : image.Drop.CreatedBy.UserId;
                var dropService = new DropsService();
                if (dropService.CanView(userId, dropId))
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

        private Stream ReSizeImage(IFormFile file)
        {
            Image image = null;
            if (file.FileName != null && file.FileName.ToLower().Contains(".heic"))
            {
                image = ConvertToJPG(file);
            }
            else {
                image = Image.FromStream(file.OpenReadStream());
            }
            RotateImage(image);
            var bitmap = ReSizeImage(image, 2048);
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            image.Dispose();
            return stream;
        }

        public Image ConvertToJPG(IFormFile file) {
            //Convert HEIC/HEIF to JPF
            using (var image = new MagickImage(file.OpenReadStream()))
            {
                image.Format = MagickFormat.Jpeg;
                var ms = new MemoryStream();
                image.Write(ms);
                return Image.FromStream(ms);
            }
        }

        private Image RotateImage(Image image) {
            
            if (Array.IndexOf(image.PropertyIdList, 274) > -1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                image.RemovePropertyItem(274); // orientation
                if (Array.IndexOf(image.PropertyIdList, 256) > -1)
                    image.RemovePropertyItem(256); // width
                if (Array.IndexOf(image.PropertyIdList, 257) > -1)
                    image.RemovePropertyItem(257); // length
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
                //if (height > width)
                //{
                //    height = 768;
                //    width = (float)(height / aspectRatio);
                //}
                //else
                //{
                    width = maxWidth;
                    height = (float)(aspectRatio * width);
                //}
            }
            if (width == image.Width) {
                return image;
            }
            var destRect = new Rectangle(0, 0, (int)width, (int)height);
            var destImage = new Bitmap((int)width, (int)height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.Default;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            //foreach (var item in image.PropertyItems)
            //{
            //    //destImage.SetPropertyItem(item);
            //}
            return destImage;
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

    }
}
