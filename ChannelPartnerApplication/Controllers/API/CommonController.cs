using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Service;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IEnumerable<object> GetStates()
        {
            var States = _context.States.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return States;
        }

        // GET api/Common/GetCities
        [HttpGet("GetCities")]
        public IEnumerable<object> GetCities()
        {
            var cities = _context.City.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return cities;
        }

        // GET api/Common/GetCitiesByStateId/6
        [HttpGet("GetCitiesByStateId/{id:int}")]
        public IEnumerable<object> GetCities(int id)
        {
            var cityData = from city in _context.City
                           where city.StateId == id && city.Active == true
                           select new { city.Name, city.Id };
            return cityData;
        }

        // GET api/Common/GetPincodes
        [HttpGet("GetPincodes")]
        public IEnumerable<object> GetPincodes()
        {
            var pincodes = _context.Pincode.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return pincodes;
        }

        // GET api/Common/GetPincodeByCityId/6
        [HttpGet("GetPincodeByCityId/{id:int}")]
        public IEnumerable<object> GetPincodeByCityId(int id)
        {
            var cityData = from pincode in _context.Pincode
                           where pincode.CityId == id && pincode.Active == true
                           select new { pincode.Name, pincode.Id };
            return cityData;
        }

        // GET api/Common/GetGenerations
        [HttpGet("GetGenerations")]
        public IEnumerable<object> GetGenerations()
        {
            var levelIds = _context.PromotionalCycle.Where(x => x.Id > 1).Select(x =>
                            new { x.LevelId, x.Id }).ToList();
            List<CommonDropDownModel> levels = new List<CommonDropDownModel>();
            foreach (var item in levelIds)
            {
                levels.Add(new CommonDropDownModel()
                {
                    Id = item.LevelId.ToString(),
                    Name = _channelPartnerService.NumberToWords(item.LevelId)
                });
            }
            return levels;
        }

        // GET api/Common/GetLevels
        [HttpGet("GetLevels")]
        public IEnumerable<object> GetLevels()
        {
            var levels = _context.PromotionalCycle.Where(x => x.Id > 1).Select(x => new { x.LevelId, x.Id });
            return levels;
        }


        //// GET api/Common/GetStates
        //[HttpGet("GetStates")]
        //public string GetStates()
        //{
        //    IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/common/getStates");
        //    return response.Content;
        //}

        // GET api/Common/GetLevelChartInformations
        [HttpGet("GetLevelChartInformations")]
        public IEnumerable<object> GetLevelChartInformations()
        {
            return _channelPartnerService.GetLevelChart();
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