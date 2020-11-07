namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class CommissionHistory : BaseEntity
    {
        public int OrderId { get; set; }
        public string From { get; set; }
        public int ChannelPartnerId { get; set; }
        public int LevelId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string CommissionType { get; set; }
    }
}


