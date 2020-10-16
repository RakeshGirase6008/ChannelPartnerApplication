using ChannelPartnerApplication.Factory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
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

        #endregion

        #region Ctor

        public ChannelPartnerService(
            FileService fileService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ChannelPartnerModelFactory channelPartnerModelFactory)
        {
            this._fileService = fileService;
            this._configuration = configuration;
            this._env = env;
            this._httpContextAccessor = httpContextAccessor;
            this._channelPartnerModelFactory = channelPartnerModelFactory;
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
        //public Users SaveUserData(int userId, Module module, string userName, string email, string FCMId, string deviceId)
        //{
        //    var password = GeneratePassword(true, true, true, false, false, 16);
        //    Users user = new Users();
        //    user.EntityId = userId;
        //    user.ModuleId = (int)module;
        //    user.UserName = userName;
        //    user.Email = email;
        //    user.Password = password;
        //    user.AuthorizeTokenKey = GenerateAuthorizeTokenKey();
        //    user.CreatedDate = DateTime.Now;
        //    user.Active = true;
        //    user.Deleted = false;
        //    user.FCMId = FCMId;
        //    _context.Users.Add(user);
        //    _context.SaveChanges();
        //    SaveDeviceAuthorizationData(user, deviceId);
        //    return user;
        //}


        #endregion
    }
}
