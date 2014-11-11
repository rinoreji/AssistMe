using Google.Apis.Drive.v2.Data;
using System.Linq;

namespace AssistMe.Helpers
{
    public static class Extentions
    {
        public static T GetGFile<T>(this File file) where T : GFile
        {
            GFile gfile = null;
            if (typeof(T) == typeof(GDirectory))
                gfile = new GDirectory();
            else
                gfile = new GFile();

            gfile.id = file.Id;
            gfile.mimeType = file.MimeType;
            gfile.title = file.Title;

            if (file.Parents != null && file.Parents.Count > 0)
                gfile.parentId = file.Parents.First().Id;

            return (T)gfile;
        }
    }
}