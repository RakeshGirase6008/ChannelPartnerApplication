namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class PromotionalCycle : BaseEntity
    {
        public int LevelId { get; set; }
        public int Percentage { get; set; }
        public int AchievementCount { get; set; }
        public string Title { get; set; }
    }
}
