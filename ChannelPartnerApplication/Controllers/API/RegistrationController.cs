using ChannelPartnerApplication.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace ChannelPartnerApplication.Controllers.API
{
    [ApiVersion("1")]
    public class RegistrationController : MainApiController
    {
        #region Fields


        #endregion

        #region Ctor

        public RegistrationController()
        {
        }

        #endregion

        #region Register User

        // POST api/Registration/StudentRegister
        [HttpPost("Register")]
        public string StudentRegister([FromForm] CommonRegistrationModel model)
        {
            var client = new RestClient("http://localhost:57299/api/v1/student/register");
            client.Timeout = -1;
            //var json = JsonConvert.SerializeObject(model.Data);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Secret_Key", "ClassBook-y18PJltrUUfTYFfgvUpNkIs7YBLfHRA1");
            request.AddHeader("AuthorizeTokenKey", "Default");
            //request.AddFile("file", model.File.FileName, model.File.ContentType);
            request.AddParameter("data", model.Data.ToString());
            request.AddParameter("DeviceId", model.DeviceId);
            request.AddParameter("FCMId", model.FCMId);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        #endregion
    }
}