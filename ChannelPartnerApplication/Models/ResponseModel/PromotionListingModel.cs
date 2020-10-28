namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class PromotionListingModel
    {
        public int ChannelPartnerId { get; set; }
        public string CurrentLevel { get; set; }
        public string NextLevel { get; set; }
        public int Target { get; set; }
        public int Achieved { get; set; }
        public int Pending { get; set; }
    }
}
