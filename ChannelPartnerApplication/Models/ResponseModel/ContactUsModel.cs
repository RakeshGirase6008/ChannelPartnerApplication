using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class ContactUsModel
    {
        public ContactUsModel()
        {
            QueryType = new List<SelectListItem>();
        }
        public string EmailId { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public List<SelectListItem> QueryType { get; set; }
        public int QueryTypeId { get; set; }
        public string Message { get; set; }
    }
}
