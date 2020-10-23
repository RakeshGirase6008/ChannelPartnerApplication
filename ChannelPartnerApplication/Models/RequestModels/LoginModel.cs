namespace ChannelPartnerApplication.Models.RequestModels
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string FCMId { get; set; }
    }

    public class ForgotPassword
    {
        public string Email { get; set; }
    }

    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
