namespace ChannelPartnerApplication.Domain.Common
{
    public class Queries : BaseEntity
    {
        public int TypeId { get; set; }
        public int CpId { get; set; }
        public string Message { get; set; }
        public bool Active { get; set; }
    }
}


