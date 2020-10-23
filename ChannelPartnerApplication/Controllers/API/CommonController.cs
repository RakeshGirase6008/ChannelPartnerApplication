using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Service;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ChannelPartnerApplication.Controllers.API
{
    [ApiVersion("1")]
    [ApiVersion("2")]
    public class CommonController : MainApiController
    {
        #region Fields

        private readonly ChannelPartnerManagementContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ChannelPartnerService _channelPartnerService;
        private readonly ChannelPartnerModelFactory _channelPartnerModelFactory;


        #endregion

        #region Ctor

        public CommonController(ChannelPartnerManagementContext context,
            IHttpContextAccessor httpContextAccessor,
            ChannelPartnerService channelPartnerService,
            ChannelPartnerModelFactory channelPartnerModelFactory)
        {
            this._context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._channelPartnerService = channelPartnerService;
            this._channelPartnerModelFactory = channelPartnerModelFactory;
        }
        #endregion

        #region Common

        // GET api/Common/GetStates
        [HttpGet("GetStates")]
        public string GetStates()
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/common/getStates");
            return response.Content;
        }

        // GET api/Common/GetCities
        [HttpGet("GetCities")]
        public string GetCities()
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/Common/GetCities");
            return response.Content;
        }

        // GET api/Common/GetCitiesByStateId/6
        [HttpGet("GetCitiesByStateId/{id:int}")]
        public string GetCities(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("api/Common/GetCitiesByStateId/" + id);
            return response.Content;
        }

        // GET api/Common/GetPincodes
        [HttpGet("GetPincodes")]
        public string GetPincodes()
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/Common/GetPincodes");
            return response.Content;
        }

        // GET api/Common/GetPincodeByCityId/6
        [HttpGet("GetPincodeByCityId/{id:int}")]
        public string GetPincodeByCityId(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("api/Common/GetPincodeByCityId/" + id);
            return response.Content;
        }

        #endregion

        #region User API

        // POST api/Common/Login
        [HttpPost("Login")]
        public IActionResult Login([FromForm] LoginModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (model != null)
            {
                var singleUser = _context.Users.Where(x => x.Email == model.Email && x.Password == model.Password).AsNoTracking();
                if (singleUser.Any())
                {
                    // Update UserData
                    var user = singleUser.FirstOrDefault();
                    user.AuthorizeTokenKey = _channelPartnerService.GenerateAuthorizeTokenKey();
                    user.FCMId = model.FCMId;
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    responseModel.Message = ChannelPartnerConstantString.Login_Success.ToString();
                    responseModel.Data = _channelPartnerModelFactory.PrepareUserDetail(user);
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    responseModel.Message = "Email & Password not matching for specified data";
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);

                }
            }
            return Ok();

        }

        // POST api/Common/ForgotPassword
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword([FromForm] ForgotPassword model)
        {
            ResponseModel responseModel = new ResponseModel();
            var singleUser = _context.Users.Where(x => x.Email == model.Email).AsNoTracking();
            if (singleUser.Any())
            {
                var user = singleUser.FirstOrDefault();
                _channelPartnerService.SendVerificationLinkEmail(user.Email, user.Password, "Forgot Password");
                responseModel.Message = "Please check your email Id for password";
                return StatusCode((int)HttpStatusCode.OK, responseModel);
            }
            else
            {
                responseModel.Message = "Email Id is not exist";
                return StatusCode((int)HttpStatusCode.NotFound, responseModel);
            }

        }

        // POST api/Common/ChangePassword
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromForm] ChangePassword model)
        {
            ResponseModel responseModel = new ResponseModel();
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey && x.Password == model.OldPassword).AsNoTracking();
            if (singleUser.Any())
            {
                var user = singleUser.FirstOrDefault();
                user.Password = model.NewPassword;
                _context.Users.Update(user);
                _context.SaveChanges();
                responseModel.Message = "Password change Successfully";
                return StatusCode((int)HttpStatusCode.OK, responseModel);
            }
            else
            {
                responseModel.Message = "Old Password is not matching";
                return StatusCode((int)HttpStatusCode.NotFound, responseModel);
            }
        }
        #endregion
    }
}
