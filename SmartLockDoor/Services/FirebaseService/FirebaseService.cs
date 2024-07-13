using Firebase.Auth;
using Firebase.Storage;

namespace SmartLockDoor
{
    public class FirebaseService : IFirebaseService
    {
        private readonly string ApiKey = "AIzaSyApFPl6nm99dVKgjzVu-JsrrwSDlIgd-sQ";
        private readonly string Email = "trungkienttmh@gmail.com";
        private readonly string Password = "Maihoa2302!";
        private readonly string Bucket = "smart-doorbell-ffebe.appspot.com";
        private readonly TimeService _timeService;

        public FirebaseService(TimeService timeService)
        {
            _timeService = timeService;
        }

        public async Task<string> UploadImageAsync(FolderEnum folderEnum, string imageBase64Data)
        {
            byte[] imageData = Convert.FromBase64String(imageBase64Data);
            var stream = new MemoryStream(imageData);

            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(Email, Password);

            var folderName = string.Empty;
            if (folderEnum == FolderEnum.Account)
            {
                folderName = "Account";
            } else if (folderEnum == FolderEnum.Member)
            {
                folderName = "Member";
            } else if (folderEnum == FolderEnum.History)
            {
                folderName = "History";
            } else
            {
                folderName = _timeService.GetLocalTime().ToString("dd-MM-yyyy");
            }

            var cancellation = new CancellationTokenSource();
            var task = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("Smart Lock Door")
                .Child(folderName)
                .Child($"image_{_timeService.GetLocalTime():ddMMyyy_HHmmss}.jpg")
                .PutAsync(stream, cancellation.Token);
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            GetFolderAndFileName(imageUrl, out string folderName, out string fileName);

            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(Email, Password);

            var task = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("Smart Lock Door")
                .Child(folderName)
                .Child(fileName)
                .DeleteAsync();

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa ảnh trên firebase");
            }
        }

        private static void GetFolderAndFileName(string url, out string folderName, out string fileName)
        {
            Uri uri = new(url);

            string path = uri.GetLeftPart(UriPartial.Path);

            string[] segments = path.Split(new string[] { "%2F" }, StringSplitOptions.RemoveEmptyEntries);

            folderName = segments[segments.Length - 2];
            fileName = segments[segments.Length - 1];
        }
    }
}
