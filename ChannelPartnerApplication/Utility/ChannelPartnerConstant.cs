namespace ChannelPartnerApplication.Utility
{
    public class ChannelPartnerConstant
    {
        #region Common Constant

        public const string ClassbookWebSite_HostURL = "http://localhost:57299";
        //public const string ClassbookWebSite_HostURL = "https://classbookapplication.appspot.com";

        #endregion

        #region ImagePath

        public const string ImagePath_Student = "image\\Student";
        public const string ImagePath_Teacher = "image\\Teacher";
        public const string ImagePath_Classes = "image\\Classes";
        public const string ImagePath_CareerExpert = "image\\CareerExpert";
        public const string ImagePath_School = "image\\School";
        public const string ImagePath_ChannelPartner = "image\\CareerExpert";
        public const string ImagePath_Topic = "image\\Topic";
        public const string ImagePath_Courses = "image\\Courses";
        public const string ImagePath_CourseCategory = "image\\CourseCategory";
        public const string ImagePath_AdvertisementBanner = "image\\AdvertisementBanner";

        #endregion

        #region VideoPath

        public const string VideoPath_Topic = "video\\Topic";
        public const string VideoPath_Teacher = "video\\Teacher";
        public const string VideoPath_Classes = "video\\Classes";

        #endregion

        #region LogLevel Module

        public const string LogLevelModule_Student = "Student";
        public const string LogLevelModule_Teacher = "Teacher";
        public const string LogLevelModule_Classes = "Classes";
        public const string LogLevelModule_CareerExpert = "CareerExpert";
        public const string LogLevelModule_School = "School";


        public const string LogLevelModule_Login = "Login";
        public const string LogLevelModule_ForgotPassword = "ForgotPassword";
        public const string LogLevelModule_ChangePassword = "ChangePassword";
        public const string LogLevelModule_Topic = "Topic";

        #endregion

        #region Module

        public const string Module_Student = "Student";
        public const string Module_Teacher = "Teacher";
        public const string Module_Classes = "Classes";
        public const string Module_CareerExpert = "CareerExpert";
        public const string Module_School = "School";

        #endregion

        #region StoredProcedure

        public const string SP_ClassBook_GetModuleDataByModuleId = "ClassBook_GetModuleDataByModuleId";
        public const string SP_ClassBook_GetCartDetailByUserId = "ClassBook_GetCartDetailByUserId";
        public const string SP_ClassBook_GetSubscrptionDetailByUserId = "ClassBook_GetSubscrptionDetailByUserId";
        public const string SP_ClassBook_GetTranscationDetailByUserId = "ClassBook_GetTranscationDetailByUserId";
        public const string SP_ClassBook_GetDetailById = "ClassBook_GetDetailById";
        public const string SP_ClassBook_GetSubjects = "ClassBook_GetSubjects";
        public const string SP_ClassBook_GetCourses = "ClassBook_GetCourses";
        public const string SP_ClassBook_OrderPaid = "ClassBook_OrderPaid";
        public const string SP_ClassBook_GetFavourites = "ClassBook_GetFavourites";

        #endregion

        #region TypeOfMapping

        public const string Mapping_Subject = "Subject";
        public const string Mapping_Course = "Course";
        public const string Mapping_Expertise = "Expertise";

        #endregion

        #region TypeOfLearning

        public const string LearningType_Distance = "Distance";
        public const string LearningType_Physical = "Physical";

        #endregion
    }
}
