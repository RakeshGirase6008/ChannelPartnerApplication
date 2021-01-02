using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Service;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace ChannelPartnerApplication.Controllers.API
{
    [ApiVersion("1")]
    public class RegistrationController : MainApiController
    {
        #region Fields

        private readonly ChannelPartnerService _channelPartnerService;

        #endregion

        #region Ctor

        public RegistrationController(ChannelPartnerService channelPartnerService)
        {
            this._channelPartnerService = channelPartnerService;
        }

        #endregion

        #region Register User

        // POST api/Registration/StudentRegister
        [HttpPost("StudentRegister")]
        public ActionResult StudentRegister([FromForm] CommonRegistrationModel model)
        {
            IRestResponse response = _channelPartnerService.RegisterMethod(model, "/api/v1/student/register");
            return StatusCode((int)response.StatusCode, response.Content);
        }

        // POST api/Registration/TeacherRegister
        [HttpPost("TeacherRegister")]
        public ActionResult TeacherRegister([FromForm] CommonRegistrationModel model)
        {
            IRestResponse response = _channelPartnerService.RegisterMethod(model, "/api/v1/teacher/register");
            return StatusCode((int)response.StatusCode, response.Content);
        }

        // POST api/Registration/ClassRegister
        [HttpPost("ClassesRegister")]
        public ActionResult ClassesRegister([FromForm] CommonRegistrationModel model)
        {
            IRestResponse response = _channelPartnerService.RegisterMethod(model, "/api/v1/classes/register");
            return StatusCode((int)response.StatusCode, response.Content);
        }

        // POST api/Registration/SchoolRegister
        [HttpPost("SchoolRegister")]
        public ActionResult SchoolRegister([FromForm] CommonRegistrationModel model)
        {

            IRestResponse response = _channelPartnerService.RegisterMethod(model, "/api/v1/school/register");
            return StatusCode((int)response.StatusCode, response.Content);
        }


        // POST api/Registration/CareerExpertRegister
        [HttpPost("CareerExpertRegister")]
        public ActionResult CareerExpertRegister([FromForm] CommonRegistrationModel model)
        {
            IRestResponse response = _channelPartnerService.RegisterMethod(model, "/api/v1/CareerExpert/register");
            return StatusCode((int)response.StatusCode, response.Content);
        }

        #endregion

        
    }
}