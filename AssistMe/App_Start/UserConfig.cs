using AssistMe.Data;
using System.Web;

namespace AssistMe
{
    public static class SessionData
    {
        public static string GetUserId
        {
            get
            {
                var userConfig = HttpContext.Current.Session["UserConfig"] as UserConfig;
                if (userConfig == null)
                    return "---";
                else
                    return string.IsNullOrWhiteSpace(userConfig.User_Id) ? "---" : userConfig.User_Id;
            }
        }
    }

    public class UserConfig
    {
        public AssistMeDb DB = new AssistMeDb();
        public string User_Id = string.Empty;
        public string DB_NAME = string.Empty;
        public string GoogleAccessToken = string.Empty;

        //public UserConfig()
        //{
        //    ResetConfig();
        //}

        //public void ResetConfig()
        //{
        //    GoogleAccessToken = string.Empty;
        //    User_Id = string.Empty;
        //    DB_NAME = string.Empty;
        //    DB = new AssistMeDb();
        //}
    }
}