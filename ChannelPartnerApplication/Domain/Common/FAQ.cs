namespace ChannelPartnerApplication.Domain.Common
{
    public class FAQ : BaseEntity
    {
        public string Type { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool Active { get; set; }
    }
}
