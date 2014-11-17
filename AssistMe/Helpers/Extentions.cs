using AssistMe.Data;
using Google.Apis.Drive.v2.Data;
using System.Linq;

namespace AssistMe.Helpers
{
    public static class Extentions
    {
        public static T GetGFile<T>(this File file) where T : AFileInfo
        {
            AFileInfo gfile = null;
            if (typeof(T) == typeof(AFolderInfo))
                gfile = new AFolderInfo();
            else
                gfile = new AFileInfo();

            gfile.Id = file.Id;
            gfile.MimeType = file.MimeType;
            gfile.FileName = file.Title;
            gfile.DisplayName = file.Title;
            gfile.Description = file.Title;

            if (file.Parents != null && file.Parents.Count > 0)
                gfile.ParentId = file.Parents.First().Id;

            return (T)gfile;
        }
    }
}