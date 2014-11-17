
namespace AssistMe.Data
{
    public class AssistMeDb
    {
        private AFolderInfo _aFolder = new AFolderInfo();
        public AFolderInfo AssistMeDrive
        {
            get { return _aFolder; }
            set { _aFolder = value; }
        }

    }
}