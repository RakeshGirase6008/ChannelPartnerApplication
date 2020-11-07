using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Domain.ChannelPartner;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Service;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChannelPartnerApplication.Controllers.API
{
    [ApiVersion("1")]
    public class ChannelPartnerController : MainApiController
    {
        #region Fields

        private readonly ChannelPartnerManagementContext _channelPartnerManagementContext;
        private readonly ChannelPartnerModelFactory _channelPartnerModelFactory;
        private readonly ChannelPartnerService _channelPartnerService;


        #endregion

        #region Ctor

        public ChannelPartnerController(ChannelPartnerManagementContext channelPartnerManagementContext,
            ChannelPartnerModelFactory channelPartnerModelFactory,
            ChannelPartnerService channelPartnerService)
        {
            this._channelPartnerManagementContext = channelPartnerManagementContext;
            this._channelPartnerModelFactory = channelPartnerModelFactory;
            this._channelPartnerService = channelPartnerService;
        }

        #endregion

        #region Register ChannelPartner

        // POST api/ChannelPartner/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                ChannelPartner ChannelPartnerData = JsonConvert.DeserializeObject<ChannelPartner>(model.Data.ToString());
                if (ChannelPartnerData != null)
                {
                    var singleUser = _channelPartnerManagementContext.Users.Where(x => x.Email == ChannelPartnerData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int channelPartnerId, string uniqueNo) = _channelPartnerService.SaveChannelPartner(ChannelPartnerData, model.Files);
                        string UserName = ChannelPartnerData.FirstName + uniqueNo;
                        var user = _channelPartnerService.SaveUserData(channelPartnerId, Module.ChannelPartner, UserName, ChannelPartnerData.Email, model.FCMId);
                        await Task.Run(() => _channelPartnerService.SendVerificationLinkEmail(ChannelPartnerData.Email, user.Password, Module.ChannelPartner.ToString()));
                        responseModel.Message = ChannelPartnerConstantString.Register_ChannelPartner_Success.ToString();
                        responseModel.Data = _channelPartnerModelFactory.PrepareUserDetail(user);
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                    else
                    {
                        responseModel.Message = ChannelPartnerConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }

        }

        // POST api/ChannelPartner/EditCareerExpert
        [HttpPost("EditChannelPartner")]
        public IActionResult EditChannelPartner([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                ChannelPartner ChannelPartnerData = JsonConvert.DeserializeObject<ChannelPartner>(model.Data.ToString());
                if (ChannelPartnerData != null)
                {
                    if (_channelPartnerManagementContext.Users.Count(x => x.Email == ChannelPartnerData.Email && x.EntityId != ChannelPartnerData.Id) > 0)
                    {
                        responseModel.Message = ChannelPartnerConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleChannelPartner = _channelPartnerManagementContext.ChannelPartner.Where(x => x.Id == ChannelPartnerData.Id).AsNoTracking().FirstOrDefault();
                        int channelPartnerId = _channelPartnerService.UpdateChannelPartner(ChannelPartnerData, singleChannelPartner, model.Files);
                        responseModel.Message = ChannelPartnerConstantString.Edit_ChannelPartner_Success.ToString();
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }

        }

        #endregion

        #region GetChannelPartnerDetails

        // GET api/ChannelPartner/GetChannelPartnersList
        [HttpPost("GetChannelPartnersList")]
        public IEnumerable<ChannelPartnerListingModel> GetChannelPartnersList([FromForm] ListingSearchModel model)
        {
            return _channelPartnerService.GetChannelPartnerListing(1, model.searchKeyword, model.levelId, model.generationId);
        }

        //// GET api/ChannelPartner/GetChannelPartnerById/5
        [HttpGet("GetChannelPartnerById/{id:int}")]
        public object GetChannelPartnerById(int id)
        {
            var query = from channelPartner in _channelPartnerManagementContext.ChannelPartner
                        join state in _channelPartnerManagementContext.States on channelPartner.StateId equals state.Id
                        join city in _channelPartnerManagementContext.City on channelPartner.CityId equals city.Id
                        join pincode in _channelPartnerManagementContext.Pincode on channelPartner.Pincode equals pincode.Id
                        join mapping in _channelPartnerManagementContext.ChannelPartnerMapping on channelPartner.Id equals mapping.ChannelPartnerId
                        join promotion in _channelPartnerManagementContext.PromotionalCycle on mapping.LevelId equals promotion.LevelId
                        where channelPartner.Id == id && channelPartner.Active == true
                        orderby channelPartner.Id
                        select new ChannelPartnerProfileModel
                        {
                            ChannelPartnerId = channelPartner.Id,
                            FirstName = channelPartner.FirstName,
                            LastName = channelPartner.LastName,
                            Address = channelPartner.Address,
                            Email = channelPartner.Email,
                            Gender = channelPartner.Gender,
                            ImageUrl = _channelPartnerModelFactory.PrepareURL(channelPartner.ProfilePictureUrl),
                            DOB = channelPartner.DOB.ToString(),
                            ContactNo = channelPartner.ContactNo,
                            AlternateContact = channelPartner.AlternateContact,
                            TeachingExperience = channelPartner.TeachingExperience,
                            Description = channelPartner.Description,
                            ReferCode = channelPartner.ReferCode,
                            StateName = state.Name,
                            CityName = city.Name,
                            Pincode = pincode.Name,
                            CurrentLevel = promotion.Title,
                            ParentId = mapping.ParentId
                        };
            var channelPartnerData = query.FirstOrDefault();
            var cpData = _channelPartnerService.GetChannelPartnerById(channelPartnerData.ParentId);
            channelPartnerData.IntroducerName = cpData != null ? cpData.FirstName + " " + cpData.LastName : string.Empty;
            return channelPartnerData;
        }

        //// GET api/ChannelPartner/GetChannelPartnerPromotion/5
        [HttpGet("GetChannelPartnerPromotion/{id:int}")]
        public object GetChannelPartnerPromotion(int id)
        {
            return _channelPartnerService.GetChannelPartnerPromotion(id);
        }

        //// GET api/ChannelPartner/GetClassBookInformations/5
        [HttpGet("GetClassBookInformations/{id:int}")]
        public object GetClassBookInformations(int id)
        {
            return _channelPartnerService.GetClassBookInformations(id);
        }

        //// GET api/ChannelPartner/MyStatusInformation/5
        [HttpGet("MyStatusInformation/{id:int}")]
        public object MyStatusInformation(int id)
        {
            return _channelPartnerService.MyStatusInformation(id);
        }

        
        #endregion
    }
}