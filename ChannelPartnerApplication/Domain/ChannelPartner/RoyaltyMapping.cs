using System;

namespace ChannelPartnerApplication.Domain.ChannelPartner
{
    public class RoyaltyMapping : BaseEntity
    {
        public int ChannelPartnerId { get; set; }
        public string Status { get; set; }
        public DateTime RoyaltyDate { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
