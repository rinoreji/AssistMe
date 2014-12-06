using AssistMe.Data;

namespace AssistMe
{
    public static class UserConfig
    {
        public static AssistMeDb DB = new AssistMeDb();
        public static string User_Id = string.Empty;
        public static string DB_NAME = string.Empty;
        public static string GoogleAccessToken = string.Empty;

        public static void ResetConfig()
        {
            GoogleAccessToken = string.Empty;
            User_Id = string.Empty;
            DB_NAME = string.Empty;
            DB = new AssistMeDb();
        }
    }
}