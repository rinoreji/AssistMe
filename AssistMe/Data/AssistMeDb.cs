
namespace AssistMe.Data
{
    public class AssistMeDb
    {
        private GDirectory _aDrive = new GDirectory();
        public GDirectory AssistMeDrive
        {
            get { return _aDrive; }
            set { _aDrive = value; }
        }
    }
}