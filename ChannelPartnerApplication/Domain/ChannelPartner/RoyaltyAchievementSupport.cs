namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class RoyaltyAchievementSupport : BaseEntity
    {
        public int RoyaltyMappingId { get; set; }
        public int ChannelPartnerId { get; set; }
        public int SupportCount { get; set; }
    }
}
