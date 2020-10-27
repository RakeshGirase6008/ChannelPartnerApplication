using System;

namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class ChannelPartnerListingModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string CityName { get; set; }
        public string IntroducerName { get; set; }
        public string ProfilePictureURL { get; set; }
        public string UniqueNo { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
