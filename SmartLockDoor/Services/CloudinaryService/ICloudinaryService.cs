namespace SmartLockDoor
{
    public interface ICloudinaryService
    {
        int DeleteImage(string imageUrl);

        string UploadImage(string imageBase64);
    }
}
