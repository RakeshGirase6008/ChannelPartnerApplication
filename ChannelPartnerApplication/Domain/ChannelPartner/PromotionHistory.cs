using System;

namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class PromotionHistory : BaseEntity
    {
        public int ChannelPartnerId { get; set; }
        public int LevelId { get; set; }
        public string Status { get; set; }
        public DateTime PromotionDate { get; set; }
    }
}
