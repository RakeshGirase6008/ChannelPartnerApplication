namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class ChannelPartnerMapping : BaseEntity
    {
        public int ChannelPartnerId { get; set; }
        public int ParentId { get; set; }
        public int LevelId { get; set; }
        public int CurrentCount { get; set; }
        public int TotalCount { get; set; }
    }
}
