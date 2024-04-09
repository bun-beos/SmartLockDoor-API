namespace SmartLockDoor
{
    public interface IEmailService
    {
        void SendEmail(EmailDto emailDto);

        string GetVerifyTokenBody(string verifyUrl);

        string GetPasswordTokenBody(string passwordToken);
    }
}
