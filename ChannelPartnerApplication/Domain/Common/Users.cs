using System;

namespace ChannelPartnerApplication.Domain.Common
{
    public class Users : BaseEntity
    {
        public int EntityId { get; set; }
        public int ModuleId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int NoOfAttempt { get; set; }
        public string AuthorizeTokenKey { get; set; }
        public string FCMId { get; set; }
        public string EmailVerificationCode { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
