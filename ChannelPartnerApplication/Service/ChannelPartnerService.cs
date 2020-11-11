using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Domain.ChannelPartner;
using ChannelPartnerApplication.Domain.Common;
using ChannelPartnerApplication.Extension;
using ChannelPartnerApplication.Models.RequestModels;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ChannelPartnerApplication.Service
{
    public class ChannelPartnerService
    {
        #region Fields

        private readonly FileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ChannelPartnerManagementContext _channelPartnerManagementContext;
        private readonly ClassBookManagementContext _classBookManagementContext;


        #endregion

        #region Ctor

        public ChannelPartnerService(
            FileService fileService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ChannelPartnerManagementContext channelPartnerManagementContext,
            ClassBookManagementContext classBookManagementContext)
        {
            this._fileService = fileService;
            this._configuration = configuration;
            this._env = env;
            this._httpContextAccessor = httpContextAccessor;
            this._channelPartnerManagementContext = channelPartnerManagementContext;
            this._classBookManagementContext = classBookManagementContext;
        }

        #endregion

        #region Common

        /// <summary>
        /// Ge the ConnectionString
        /// </summary>
        public string GetConnectionStringForChannelPartner()
        {
            return _configuration.GetConnectionString("ChannelPartnerDatabase");
        }

        /// <summary>
        /// Generate Random Token Key
        /// </summary>
        public string GenerateAuthorizeTokenKey()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789`@#$%^&*";
            var stringChars = new char[50];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        /// <summary>
        /// Generate UniqueNo based on Some parameters
        /// </summary>
        public string GenerateUniqueNo(string uniqueNo, string firstWord, string secondWord)
        {
            if (!string.IsNullOrEmpty(firstWord) && !string.IsNullOrEmpty(secondWord))
            {
                int sno;
                if (uniqueNo == null)
                {
                    sno = 10001;
                }
                else
                {
                    string tt = uniqueNo;
                    int result = 0;
                    bool success = int.TryParse(new string(tt
                                         .SkipWhile(x => !char.IsDigit(x))
                                         .TakeWhile(x => char.IsDigit(x))
                                         .ToArray()), out result);
                    sno = result + 1;
                }
                string cpf = firstWord.Substring(0, 1).ToString().ToUpper();
                string cpl = secondWord.Substring(0, 1).ToString().ToUpper();
                uniqueNo = cpf + cpl + sno.ToString();
                return uniqueNo;
            }
            return string.Empty;

        }

        /// <summary>
        /// Generate password
        /// </summary>
        public string GeneratePassword(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, bool includeSpaces, int lengthOfPassword)
        {
            const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX)
            {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (includeLowercase)
            {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase)
            {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric)
            {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial)
            {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces)
            {
                characterSet += SPACE_CHARACTER;
            }

            char[] password = new char[lengthOfPassword];
            int characterSetLength = characterSet.Length;

            System.Random random = new System.Random();
            for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                bool moreThanTwoIdenticalInARow =
                    characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }

        /// <summary>
        /// Create Body From Parametes
        /// </summary>
        private string CreateBody(string username, string password, string link, string mypageName)
        {
            string webRootPath = _env.WebRootPath;
            string body = string.Empty;
            var target = Path.Combine(webRootPath + "/Content/HtmlTemplates/" + mypageName + ".html");
            using (StreamReader reader = new StreamReader(target.ToString()))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{username}", username);
            body = body.Replace("{password}", password);
            body = body.Replace("{link}", link);
            return body;
        }

        /// <summary>
        /// Create Body From Parametes
        /// </summary>
        public void SendVerificationLinkEmail(string ToEmailId, string GeneratedPassword, string title)
        {
            //var emailBody = CreateBody(ToEmailId, GeneratedPassword, string.Empty, "ActivateMyAccount");
            //title = title.ToString() + " Register";
            //SendEmail(ToEmailId, emailBody, title);
        }

        private bool SendEmail(string EmailTo, string EmailBody, string Subject)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("servicesautohub@gmail.com");
                mail.To.Add(EmailTo);
                mail.Subject = Subject;
                mail.Body = EmailBody;
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment("C:\\file.zip"));
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("servicesautohub@gmail.com", "@Ganapati20");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            return true;
        }
        public string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";
            string words = "";
            if (number > 0)
            {
                var unitsMap = new[] { "zero", "First", "Second", "Third", "Fourth", "Fifth", "Sixth" };
                words = unitsMap[number];
            }
            return words + " Generations";
        }

        ///// <summary>
        ///// SaveUserData
        ///// </summary>
        public Users SaveUserData(int userId, Module module, string userName, string email, string FCMId)
        {
            var password = GeneratePassword(true, true, true, false, false, 16);
            Users user = new Users();
            user.EntityId = userId;
            user.ModuleId = (int)module;
            user.UserName = userName;
            user.Email = email;
            user.Password = password;
            user.AuthorizeTokenKey = GenerateAuthorizeTokenKey();
            user.CreatedDate = DateTime.Now;
            user.Active = true;
            user.Deleted = false;
            user.FCMId = FCMId;
            _channelPartnerManagementContext.Users.Add(user);
            _channelPartnerManagementContext.SaveChanges();
            return user;
        }


        #endregion

        #region ChannelPartner

        /// <summary>
        /// Save the ChannelPartner Record
        /// </summary>
        public (int ChannelPartnerId, string UniqueNo) SaveChannelPartner(ChannelPartner ChannelPartnerData, List<IFormFile> files)
        {
            ChannelPartner ChannelPartner = new ChannelPartner();
            ChannelPartner.FirstName = ChannelPartnerData.FirstName;
            ChannelPartner.LastName = ChannelPartnerData.LastName;
            ChannelPartner.Email = ChannelPartnerData.Email;
            ChannelPartner.ContactNo = ChannelPartnerData.ContactNo;
            ChannelPartner.AlternateContact = ChannelPartnerData.AlternateContact;
            ChannelPartner.Gender = ChannelPartnerData.Gender;
            if (files?.Count > 0)
                ChannelPartner.ProfilePictureUrl = _fileService.SaveFile(files, ChannelPartnerConstant.ImagePath_ChannelPartner);
            ChannelPartner.DOB = ChannelPartnerData.DOB;
            ChannelPartner.Address = ChannelPartnerData.Address;
            ChannelPartner.StateId = ChannelPartnerData.StateId;
            ChannelPartner.CityId = ChannelPartnerData.StateId;
            ChannelPartner.Pincode = ChannelPartnerData.Pincode;
            ChannelPartner.ApproveStatus = ChannelPartnerData.ApproveStatus;
            ChannelPartner.ApprovalDate = ChannelPartnerData.ApprovalDate;
            ChannelPartner.TeachingExperience = ChannelPartnerData.TeachingExperience;
            ChannelPartner.Description = ChannelPartnerData.Description;
            ChannelPartner.ReferCode = GenerateReferecode();
            var previousUnique = _channelPartnerManagementContext.ChannelPartner.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            ChannelPartner.UniqueNo = GenerateUniqueNo(previousUnique, ChannelPartnerData.FirstName, ChannelPartnerData.Email);
            ChannelPartner.RegistrationFromTypeId = ChannelPartnerData.RegistrationFromTypeId;
            ChannelPartner.RegistrationByTypeId = ChannelPartnerData.RegistrationByTypeId;
            ChannelPartner.CreatedDate = DateTime.Now;
            ChannelPartner.CreatedBy = 0;
            ChannelPartner.Active = true;
            ChannelPartner.Deleted = false;
            _channelPartnerManagementContext.ChannelPartner.Add(ChannelPartner);
            _channelPartnerManagementContext.SaveChanges();

            // Save Channel Partner Mapping
            ChannelPartnerMapping channelPartnerMapping = new ChannelPartnerMapping();
            channelPartnerMapping.ChannelPartnerId = ChannelPartner.Id;
            channelPartnerMapping.LevelId = 0;
            channelPartnerMapping.CurrentCount = 0;
            channelPartnerMapping.TotalCount = 0;
            if (!string.IsNullOrEmpty(ChannelPartnerData.ReferCode))
            {
                var channelPartners = _channelPartnerManagementContext.ChannelPartner.Where(x => x.ReferCode == ChannelPartnerData.ReferCode);
                if (channelPartners.Any())
                    channelPartnerMapping.ParentId = channelPartners.FirstOrDefault().Id;
                else
                    channelPartnerMapping.ParentId = 0;
            }
            _channelPartnerManagementContext.ChannelPartnerMapping.Add(channelPartnerMapping);
            _channelPartnerManagementContext.SaveChanges();

            return (ChannelPartner.Id, ChannelPartner.UniqueNo);
        }

        private string GenerateReferecode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        /// <summary>
        /// Update the ChannelPartner Record
        /// </summary>
        public int UpdateChannelPartner(ChannelPartner ChannelPartnerData, ChannelPartner ChannelPartner, List<IFormFile> files)
        {
            ChannelPartner.FirstName = ChannelPartnerData.FirstName;
            ChannelPartner.LastName = ChannelPartnerData.LastName;
            ChannelPartner.Email = ChannelPartnerData.Email;
            ChannelPartner.ContactNo = ChannelPartnerData.ContactNo;
            ChannelPartner.AlternateContact = ChannelPartnerData.AlternateContact;
            ChannelPartner.Gender = ChannelPartnerData.Gender;
            if (files?.Count > 0)
            {
                ChannelPartner.ProfilePictureUrl = _fileService.SaveFile(files, ChannelPartnerConstant.ImagePath_ChannelPartner);
            }
            ChannelPartner.DOB = ChannelPartnerData.DOB;
            ChannelPartner.Address = ChannelPartnerData.Address;
            ChannelPartner.StateId = ChannelPartnerData.StateId;
            ChannelPartner.CityId = ChannelPartnerData.StateId;
            ChannelPartner.Pincode = ChannelPartnerData.Pincode;
            ChannelPartner.ApproveStatus = ChannelPartnerData.ApproveStatus;
            ChannelPartner.ApprovalDate = ChannelPartnerData.ApprovalDate;
            ChannelPartner.TeachingExperience = ChannelPartnerData.TeachingExperience;
            ChannelPartner.Description = ChannelPartnerData.Description;
            ChannelPartner.ReferCode = ChannelPartnerData.ReferCode;
            ChannelPartner.UpdatedDate = DateTime.Now;
            ChannelPartner.UpdatedBy = 0;
            _channelPartnerManagementContext.ChannelPartner.Update(ChannelPartner);
            _channelPartnerManagementContext.SaveChanges();
            return ChannelPartner.Id;
        }

        public ChannelPartner GetChannelPartnerById(int id)
        {
            return _channelPartnerManagementContext.ChannelPartner.Where(x => x.Id == id).FirstOrDefault();
        }

        #endregion

        #region SendRegister

        public IRestResponse RegisterMethod(CommonRegistrationModel model, string ApiName)
        {
            var secretKey = _classBookManagementContext.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            var client = new RestClient(ChannelPartnerConstant.ClassbookWebSite_HostURL.ToString() + ApiName.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Secret_Key", secretKey.Value.ToString());
            request.AddHeader("AuthorizeTokenKey", "Default");
            //request.AddFile("file", model.File.FileName, model.File.ContentType);
            request.AddParameter("data", model.Data);
            request.AddParameter("DeviceId", model.DeviceId);
            request.AddParameter("FCMId", model.FCMId);
            IRestResponse response = client.Execute(request);
            return response;
        }

        public IRestResponse GetCommonFromClassBook(string ApiName)
        {
            var secretKey = _classBookManagementContext.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            var client = new RestClient(ChannelPartnerConstant.ClassbookWebSite_HostURL.ToString() + ApiName.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Secret_Key", secretKey.Value.ToString());
            request.AddHeader("AuthorizeTokenKey", _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"]);
            IRestResponse response = client.Execute(request);
            return response;
        }
        #endregion

        #region Common


        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<LevelIncomeListingModel> GetLevelChart()
        {
            IList<LevelIncomeListingModel> listingModels = new List<LevelIncomeListingModel>();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_ChannelPartner_GetLevelChart.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LevelIncomeListingModel ISP = new LevelIncomeListingModel()
                        {
                            Type = reader.GetValue<string>("Type"),
                            CurrentLevel = reader.GetValue<string>("CurrentLevel"),
                            PromotionLevel = reader.GetValue<string>("PromotionLevel"),
                            Target = reader.GetValue<string>("Target"),
                            IncomePercentage = reader.GetValue<string>("IncomePercentage"),
                        };
                        listingModels.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        public IList<PromotionListingModel> GetChannelPartnerPromotion(int id)
        {
            IList<PromotionListingModel> listingModels = new List<PromotionListingModel>();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_ChannelPartner_GetPromotionLevel.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ChannelPartnerId", SqlDbType.Int).Value = id;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PromotionListingModel ISP = new PromotionListingModel()
                        {
                            ChannelPartnerId = reader.GetValue<int>("ChannelPartnerId"),
                            CurrentLevel = reader.GetValue<string>("CurrentLevel"),
                            NextLevel = reader.GetValue<string>("NextLevel"),
                            Target = reader.GetValue<int>("Target"),
                            Achieved = reader.GetValue<int>("Achieved"),
                            Pending = reader.GetValue<int>("Pending"),
                        };
                        listingModels.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        public IList<int> GetAllChildPartners(int cpId)
        {
            IList<int> listingModels = new List<int>();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_ChannelPartner_GetAllChilds.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ChannelPartnerId", SqlDbType.Int).Value = cpId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        listingModels.Add(reader.GetValue<int>("ChildId"));
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<ChannelPartnerListingModel> GetChannelPartnerListing(int CpId, string searchKeyword = "", int levelId = 0, int generationId = 0)
        {
            IList<ChannelPartnerListingModel> listingModels = new List<ChannelPartnerListingModel>();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_ChannelPartner_GetChannelPartnersList.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ChannelPartnerId", SqlDbType.Int).Value = CpId;
                cmd.Parameters.Add("@Searchkeyword", SqlDbType.VarChar).Value = searchKeyword;
                cmd.Parameters.Add("@LevelId", SqlDbType.Int).Value = levelId;
                cmd.Parameters.Add("@GenerationId", SqlDbType.Int).Value = generationId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ChannelPartnerListingModel ISP = new ChannelPartnerListingModel()
                        {
                            Id = reader.GetValue<int>("Id"),
                            FullName = reader.GetValue<string>("FullName"),
                            CityName = reader.GetValue<string>("CityName"),
                            IntroducerName = reader.GetValue<string>("IntroducerName"),
                            ProfilePictureURL = reader.GetValue<string>("ProfilePictureURL"),
                            UniqueNo = reader.GetValue<string>("UniqueNo"),
                            RegistrationDate = reader.GetValue<DateTime>("RegistrationDate"),
                        };
                        listingModels.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        /// <summary>
        /// Get the referCode by ChannelPartnerId
        /// </summary>
        public string GetReferCode(int channelpartnerId)
        {
            return _channelPartnerManagementContext.ChannelPartner.Where(x => x.Id == channelpartnerId).FirstOrDefault().ReferCode;
        }
        #endregion

        #region ClassBook Management

        /// <summary>
        /// Get the Class Book Informations
        /// </summary>
        public IList<ClassBookInformations> GetClassBookInformations(int id)
        {
            IList<ClassBookInformations> listingModels = new List<ClassBookInformations>();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_ChannelPartner_GetClassBookInformation.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ChannelPartnerId", SqlDbType.Int).Value = id;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ClassBookInformations ISP = new ClassBookInformations()
                        {
                            TypeId = reader.GetValue<int>("TypeId"),
                            Type = reader.GetValue<string>("MyType"),
                            Name = reader.GetValue<string>("Name"),
                            IntroducerName = reader.GetValue<string>("IntroducerName"),
                            Direct = reader.GetValue<bool>("Direct"),
                            UniqueNo = reader.GetValue<string>("UniqueNo"),
                        };
                        listingModels.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        /// <summary>
        /// ChannelPartner Status Level & Class book Count Information
        /// </summary>
        public MyStatusInformation MyStatusInformation(int id)
        {
            MyStatusInformation listingModels = new MyStatusInformation();
            SqlConnection connection = new SqlConnection(GetConnectionStringForChannelPartner());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ChannelPartnerConstant.SP_Classbook_GetMyStatusInformation.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ChannelPartnerId", SqlDbType.Int).Value = id;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        listingModels.CurrentLevel = reader.GetValue<string>("CurrentLevel");
                        listingModels.NextLevel = reader.GetValue<string>("NextLevel");
                        listingModels.Target = reader.GetValue<int>("Target");
                        listingModels.Achieved = reader.GetValue<int>("Achieved");
                        listingModels.Pending = reader.GetValue<int>("Pending");
                        listingModels.Student = reader.GetValue<int>("Student");
                        listingModels.Classes = reader.GetValue<int>("Classes");
                        listingModels.Teacher = reader.GetValue<int>("Teacher");
                        listingModels.CareerExpert = reader.GetValue<int>("CareerExpert");
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }

        public void SaveQueries(ContactUsModel model, int cpId)
        {
            Queries q = new Queries();
            q.Message = model.Message;
            q.TypeId = model.QueryTypeId;
            q.CpId = cpId;
            q.Active = true;
            _channelPartnerManagementContext.Queries.Add(q);
            _channelPartnerManagementContext.SaveChanges();
        }

        #endregion

    }
}