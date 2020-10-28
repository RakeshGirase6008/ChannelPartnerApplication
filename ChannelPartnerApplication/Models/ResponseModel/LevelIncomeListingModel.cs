namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class LevelIncomeListingModel
    {
        public int ChannelPartnerId { get; set; }
        public string Type { get; set; }
        public string CurrentLevel { get; set; }
        public string PromotionLevel { get; set; }
        public string Target { get; set; }
        public string IncomePercentage { get; set; }
    }
}
