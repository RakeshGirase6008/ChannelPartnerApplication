using System;

namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class ChannelPartner : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string AlternateContact { get; set; }
        public string Gender { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime? DOB { get; set; }
        public string Address { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public int Pincode { get; set; }
        public bool ApproveStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string TeachingExperience { get; set; }
        public string Description { get; set; }
        public string ReferCode { get; set; }
        public string UniqueNo { get; set; }
        public string QRcodeImageURL { get; set; }
        public int RegistrationFromTypeId { get; set; }
        public int RegistrationByTypeId { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
