using System;

namespace ChannelPartnerApplication.Domain.Common
{
    public class Logs : BaseEntity
    {
        public string ModuleName { get; set; }

        public string ShortMessage { get; set; }

        public string FullMessage { get; set; }

        public string APIName { get; set; }

        public int UserId { get; set; }

        public DateTime? CreatedOnDate { get; set; }
    }
}
