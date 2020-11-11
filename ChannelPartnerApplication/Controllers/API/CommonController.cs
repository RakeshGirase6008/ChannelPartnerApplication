using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Service;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET api/Common/GetLevelChartInformations
        [HttpGet("GetLevelChartInformations")]
        public IEnumerable<object> GetLevelChartInformations()
        {
            return _channelPartnerService.GetLevelChart();
        }

        #endregion

        #region Static  HTML Page

        // GET api/Common/GetAboutUsPage
        [HttpGet("GetAboutUsPage")]
        public string GetAboutUsPage()
        {
            return ChannelPartnerConstant.ClassbookWebSite_HostURL + "HtmlPage/AboutUs.html";
        }

        // GET api/Common/GetNeedHelpPage
        [HttpGet("GetNeedHelpPage")]
        public string GetNeedHelpPage()
        {
            return ChannelPartnerConstant.ClassbookWebSite_HostURL + "HtmlPage/NeedHelp.html";
        }

        // GET api/Common/GetTermsAndConditionsPage
        [HttpGet("GetTermsAndConditionsPage")]
        public string GetTermsAndConditionsPage()
        {
            return ChannelPartnerConstant.ClassbookWebSite_HostURL + "HtmlPage/TermsAndConditions.html";
        }

        // GET api/Common/GetContactUsPage
        [HttpGet("GetContactUsPage")]
        public object GetContactUsPage()
        {
            var queryType = from q in _context.QueryType
                            where q.Active == true
                            select new SelectListItem
                            {
                                Text = q.Type.ToString(),
                                Value = q.Id.ToString()
                            };

            return new ContactUsModel()
            {
                EmailId = "otgsServices@gmail.com",
                PhoneNo = "1234567890",
                Address = "Pune, Maharastara",
                Website = "www.otgs.com",
                Message = "",
                QueryType = queryType.ToList()
            };
        }

        // GET api/Common/PostContactUsPage
        [HttpPost("PostContactUsPage")]
        public object PostContactUsPage([FromForm] ContactUsModel model)
        {
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).AsNoTracking();
            if (singleUser.Any())
            {
                _channelPartnerService.SaveQueries(model, singleUser.FirstOrDefault().Id);
                return Ok();
            }
            return Ok();
        }

        // GET api/Common/GetFAQs
        [HttpPost("GetFAQs")]
        public object GetFAQs()
        {
            var faq = from q in _context.FAQ
                      where q.Active == true
                      select q;
            return faq.ToList();
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

        #region EditProfile

        // GET api/Common/GetClassIdById/5
        [HttpGet("GetClassIdById/{id:int}")]
        public object EditProfileForClass(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/Classes/EditProfileForClass/" + id.ToString() + "");
            return response.Content;
        }

        // GET api/Common/EditProfileForClass/5
        [HttpGet("GetCareerExpertById/{id:int}")]
        public object GetCareerExpertById(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/CareerExpert/GetCareerExpertById/" + id.ToString() + "");
            return response.Content;
        }

        // GET api/Common/GetStudentById/5
        [HttpGet("GetStudentById/{id:int}")]
        public object GetStudentById(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/Student/GetStudentById/" + id.ToString() + "");
            return response.Content;
        }

        // GET api/Common/GetSchoolById/5
        [HttpGet("GetSchoolById/{id:int}")]
        public object GetSchoolById(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/School/GetSchoolById/" + id.ToString() + "");
            return response.Content;
        }

        // GET api/Common/EditProfileForClass/5
        [HttpGet("GetTeacherById/{id:int}")]
        public object GetTeacherById(int id)
        {
            IRestResponse response = _channelPartnerService.GetCommonFromClassBook("/api/v1/Teacher/EditProfileForTeacher/" + id.ToString() + "");
            return response.Content;
        }
        #endregion
    }
}