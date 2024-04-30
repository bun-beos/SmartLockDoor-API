using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SmartLockDoor
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService()
        {
            Account account = new(
                "ttkiencloud",
                "764552439513318",
                "QImaCimMomeRI1TFBpXDvFC6RNU"
                );

            _cloudinary = new Cloudinary(account);

        }

        public int DeleteImage(string imageUrl)
        {
            var publicId = GetPublicIdFromUrl(imageUrl);

            if (string.IsNullOrEmpty(publicId))
            {
                return 0;
            }

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var deletionResult = _cloudinary.Destroy(deleteParams);

            if (deletionResult.Result == "ok")
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //public string UploadImage(string imageBase64)
        //{
            //var uploadParams = new ImageUploadParams()
            //{
            //    File = new FileDescription(imageBase64)
            //};

            //try
            //{
            //    var uploadResult = _cloudinary.Upload(uploadParams);
                
            //    return uploadResult.SecureUri.AbsoluteUri;
            //}
            //catch
            //{
            //    return string.Empty;
            //}
        //}

        private static string GetPublicIdFromUrl(string imageUri)
        {
            var publicIdIndex = imageUri.LastIndexOf("/") + 1;
            var extensionIndex = imageUri.LastIndexOf(".");

            if (publicIdIndex >= 0 && extensionIndex > publicIdIndex)
            {
                return imageUri.Substring(publicIdIndex, extensionIndex - publicIdIndex);
            }
            else
                return string.Empty;
        }
    }
}
