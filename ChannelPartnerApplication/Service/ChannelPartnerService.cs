using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Domain.ChannelPartner;
using ChannelPartnerApplication.Domain.Common;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        private readonly ChannelPartnerModelFactory _channelPartnerModelFactory;
        private readonly ChannelPartnerManagementContext _channelPartnerManagementContext;
        

        #endregion

        #region Ctor

        public ChannelPartnerService(
            FileService fileService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ChannelPartnerModelFactory channelPartnerModelFactory,
            ChannelPartnerManagementContext channelPartnerManagementContext)
        {
            this._fileService = fileService;
            this._configuration = configuration;
            this._env = env;
            this._httpContextAccessor = httpContextAccessor;
            this._channelPartnerModelFactory = channelPartnerModelFactory;
            this._channelPartnerManagementContext = channelPartnerManagementContext;
        }

        #endregion

        #region Common

        /// <summary>
        /// Ge the ConnectionString
        /// </summary>
        public string GetConnectionStringForClassbook()
        {
            return _configuration.GetConnectionString("ClassBookManagementeDatabase");
        }

        ///// <summary>
        ///// Save the Device Authorization Data
        ///// </summary>
        //public void SaveDeviceAuthorizationData(Users user, string DeviceId)
        //{
        //    if (!_context.AuthorizeDeviceData.Where(x => x.DeviceId == DeviceId && x.UserId == user.Id).AsNoTracking().Any())
        //    {
        //        var AuthorizeDeviceData = new AuthorizeDeviceData
        //        {
        //            UserId = user.Id,
        //            DeviceId = DeviceId
        //        };
        //        _context.AuthorizeDeviceData.Add(AuthorizeDeviceData);
        //        _context.SaveChanges();
        //    }
        //}
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

        ///// <summary>
        ///// SaveUserData
        ///// </summary>
        public Users SaveUserData(int userId, Module module, string userName, string email, string FCMId, string deviceId)
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
            //SaveDeviceAuthorizationData(user, deviceId);
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

        #endregion
    }

}
