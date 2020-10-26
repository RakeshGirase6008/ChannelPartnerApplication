namespace ChannelPartnerApplication.Domain.Common
{
    public class City : BaseEntity
    {
        public string Name { get; set; }
        public int StateId { get; set; }
        public bool Active { get; set; }
    }
}