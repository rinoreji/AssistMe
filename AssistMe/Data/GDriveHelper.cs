using AssistMe.Data;
using AssistMe.Helpers;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Oauth2.v2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IO = System.IO;

namespace AssistMe
{
    public class GDriveHelper
    {
        public void GetUserDetails()
        {
            var service = HttpContext.Current.Session["Oauth2Service"] as Oauth2Service;
            var userinfo = service.Userinfo.Get().Execute();
            HttpContext.Current.Session["user"] = userinfo.Email;

            UserConfig.User_Id = userinfo.Email;
            UserConfig.DB_NAME = string.Format("{0}.json", UserConfig.User_Id);
        }

        public string LocalDbPath { get { return HttpContext.Current.Server.MapPath(string.Format("~/App_Data/{0}", UserConfig.DB_NAME)); } }

        public void InitBaseSystem()
        {
            GetUserDetails();
            PopulateLocalDbData();

            DriveService service = HttpContext.Current.Session["DriveService"] as DriveService;

            if (string.IsNullOrWhiteSpace(UserConfig.DB.AssistMeDrive.id))
            {
                UpdateLocaDbFromGDrive(service);
            }
        }

        private void UpdateLocaDbFromGDrive(DriveService service)
        {
            var rootDir = GetDriveFileMetadata(new GDirectory { title = AppConfig.APP_DRIVE_FOLDER }, service, CreateIfNotExist: true);
            if (rootDir != null)
            {
                UserConfig.DB.AssistMeDrive = rootDir.GetGFile<GDirectory>();
                var gFile = new GFile { title = UserConfig.DB_NAME, parentId = UserConfig.DB.AssistMeDrive.id };
                var dbFile = GetDriveFileMetadata(gFile, service);
                if (dbFile != null)
                {
                    //Let it be there, will think about it.
                    if (!IO.File.Exists(LocalDbPath))
                    {
                        var request = service.HttpClient.GetByteArrayAsync(dbFile.DownloadUrl);
                        var content = Encoding.ASCII.GetString(request.Result);
                        IO.File.WriteAllText(LocalDbPath, content);
                        UserConfig.DB = JsonConvert.DeserializeObject<AssistMeDb>(content);
                    }
                }
                else
                {
                    UserConfig.DB.AssistMeDrive.Children.Add(gFile);
                    byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(UserConfig.DB));
                    using (var stream = new IO.MemoryStream(byteArray))
                    {
                        var uploadedFile = CreateFile(gFile, service, stream);
                        UserConfig.DB.AssistMeDrive.Children.Remove(gFile);
                        UserConfig.DB.AssistMeDrive.Children.Add(uploadedFile.GetGFile<GFile>());
                    }

                    IO.File.WriteAllText(LocalDbPath, JsonConvert.SerializeObject(UserConfig.DB));
                }
            }
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = string.Empty;
            string ext = IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        void PopulateLocalDbData()
        {
            if (!IO.File.Exists(LocalDbPath))
                return;
            JsonSerializer serializer = new JsonSerializer();
            var db = serializer.Deserialize(IO.File.OpenText(LocalDbPath), typeof(AssistMeDb)) as AssistMeDb;
            if (db != null)
                UserConfig.DB = db;
        }

        public File GetDriveFileMetadata(GFile file, DriveService service, bool CreateIfNotExist = false)
        {
            File resultFile = null;
            var result = getFileMetadata(file, service).Result;
            if (result != null && result.Items.Count > 0)
            {
                resultFile = result.Items.First();
                if (resultFile == null && CreateIfNotExist)
                {
                    return CreateFile(file, service);
                }
            }
            return resultFile;
        }

        public File CreateFile(GFile file, DriveService service, IO.Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(file.mimeType))
                file.mimeType = GetMimeType(file.title);

            var body = new File
            {
                Title = file.title,
                MimeType = file.mimeType
            };

            if (!string.IsNullOrWhiteSpace(file.parentId))
            {
                if (body.Parents == null) body.Parents = new List<ParentReference>();
                body.Parents.Add(new ParentReference { Id = file.parentId });
            }

            if (stream == null)
                return service.Files.Insert(body).Execute();
            else
            {
                var request = service.Files.Insert(body, stream, "");
                request.Upload();
                return request.ResponseBody;
            }
        }

        private Task<FileList> getFileMetadata(GFile file, DriveService service)
        {
            if (service != null)
            {
                var request = service.Files.List();
                if (file.IsDirectory)
                {
                    request.Q = string.Format(
                        "mimeType='{0}' and trashed=false and title='{1}'",
                        file.mimeType, file.title);
                }
                else
                {
                    request.Q = string.Format("trashed=false and title='{0}'", file.title);
                }
                if (!string.IsNullOrWhiteSpace(file.parentId))
                    request.Q += string.Format(" and '{0}' in parents", file.parentId);

                return request.ExecuteAsync();
            }
            return null;
        }
    }
}