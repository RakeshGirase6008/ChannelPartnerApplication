using System;

namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class ChannelPartnerProfileModel
    {
        public int ChannelPartnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string ImageUrl { get; set; }
        public string DOB { get; set; }
        public string ContactNo { get; set; }
        public string AlternateContact { get; set; }
        public string TeachingExperience { get; set; }
        public string Description { get; set; }
        public string ReferCode { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string Pincode { get; set; }
        public string CurrentLevel { get; set; }
        public int ParentId { get; set; }
        public string IntroducerName { get; set; }
    }
}
