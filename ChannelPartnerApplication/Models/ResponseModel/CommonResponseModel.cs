using System.Collections.Generic;

namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class CommonResponseModel
    {
        public CommonResponseModel()
        {
            ValidationMessage = new List<string>();
            ErrorMessage = new List<string>();
        }
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> ValidationMessage { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
    public class ResponseModel
    {
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
