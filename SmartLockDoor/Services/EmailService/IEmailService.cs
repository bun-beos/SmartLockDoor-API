namespace SmartLockDoor
{
    public interface IEmailService
    {
        void SendEmail(EmailDto emailDto);

        string GetVerifyBodyEmail(string verifyUrl);
    }
}
