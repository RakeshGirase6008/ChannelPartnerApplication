using ChannelPartnerApplication.Domain.Common;
using ChannelPartnerApplication.Utility;

namespace ChannelPartnerApplication.Factory
{
    public class ChannelPartnerModelFactory
    {
        public object PrepareUserDetail(Users user)
        {
            return new
            {
                UserId = user.EntityId,
                Email = user.Email,
                AuthorizeTokenKey = user.AuthorizeTokenKey
            };
        }
        public string PrepareURL(string ImageUrl)
        {
            if (!string.IsNullOrEmpty(ImageUrl))
                return ChannelPartnerConstant.ClassbookWebSite_HostURL.ToString() + "/" + ImageUrl.Replace("\\", "/");
            return string.Empty;
        }
    }
}
