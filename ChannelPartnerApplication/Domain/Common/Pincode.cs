namespace ChannelPartnerApplication.Domain.Common
{
    public class Pincode : BaseEntity
    {
        public string Name { get; set; }
        public int CityId { get; set; }
        public bool Active { get; set; }
    }
}