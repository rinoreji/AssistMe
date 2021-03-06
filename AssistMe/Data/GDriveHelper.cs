﻿using AssistMe.Data;
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
        private UserConfig userConfig
        {
            get
            {
                return (UserConfig)HttpContext.Current.Session["UserConfig"];
            }
        }

        private void GetUserDetails()
        {
            var service = HttpContext.Current.Session["Oauth2Service"] as Oauth2Service;
            var userinfo = service.Userinfo.Get().Execute();
            HttpContext.Current.Session["user"] = userinfo.Email;

            userConfig.User_Id = userinfo.Email;
            userConfig.DB_NAME = string.Format("{0}.json", userConfig.User_Id);
            HttpContext.Current.Session["UserConfig"] = userConfig;
        }

        public string LocalDbPath { get { return HttpContext.Current.Server.MapPath(string.Format("~/App_Data/{0}", userConfig.DB_NAME)); } }

        public void InitBaseSystem()
        {
            GetUserDetails();
            PopulateLocalDbData();

            DriveService service = HttpContext.Current.Session["DriveService"] as DriveService;

            if (string.IsNullOrWhiteSpace(userConfig.DB.AssistMeDrive.Id))
            {
                UpdateLocaDbFromGDrive(service);
            }
        }

        private void UpdateLocaDbFromGDrive(DriveService service)
        {
            var rootDir = GetDriveFileMetadata(new AFolderInfo { FileName = AppConfig.APP_DRIVE_FOLDER }, service, CreateIfNotExist: true);
            if (rootDir != null)
            {
                var dir = rootDir.GetGFile<AFolderInfo>();
                dir.DisplayName = "Drive";
                userConfig.DB.AssistMeDrive = dir;
                HttpContext.Current.Session["UserConfig"] = userConfig;
                var gFile = new AFileInfo { FileName = userConfig.DB_NAME, ParentId = userConfig.DB.AssistMeDrive.Id };
                var dbFile = GetDriveFileMetadata(gFile, service);
                if (dbFile != null)//db file exists in GDrive
                {
                    if (!IO.File.Exists(LocalDbPath))
                    {
                        var request = service.HttpClient.GetByteArrayAsync(dbFile.DownloadUrl);
                        var content = Encoding.ASCII.GetString(request.Result);
                        IO.File.WriteAllText(LocalDbPath, content);
                        userConfig.DB = JsonConvert.DeserializeObject<AssistMeDb>(content);
                        HttpContext.Current.Session["UserConfig"] = userConfig;
                    }
                    else//Local file exist, remove file exist
                    {
                        //Let it be there, will think about it. 
                        //have to consider the one with recent changes
                    }
                }
                else if (IO.File.Exists(LocalDbPath))//db file not in GDrive but exist localy, upload local to Drive
                {

                }
                else//File not available anywhere, create new and upload to gdrive as well as local
                {
                    userConfig.DB.AssistMeDrive.Children.Add(gFile);
                    byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(userConfig.DB));
                    using (var stream = new IO.MemoryStream(byteArray))
                    {
                        var uploadedFile = CreateFile(gFile, service, stream);
                        userConfig.DB.AssistMeDrive.Children.Remove(gFile);
                        var aUploadedFile = uploadedFile.GetGFile<AFileInfo>();
                        aUploadedFile.IsSystemFile = true;
                        aUploadedFile.DisplayName = "DB";
                        userConfig.DB.AssistMeDrive.Children.Add(aUploadedFile);
                    }
                    HttpContext.Current.Session["UserConfig"] = userConfig;

                    WriteDBDataToLocalDB();
                }
            }
        }

        public void WriteDBDataToLocalDB()
        {
            IO.File.WriteAllText(LocalDbPath, JsonConvert.SerializeObject(userConfig.DB));
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

        private void PopulateLocalDbData()
        {
            if (!IO.File.Exists(LocalDbPath))
                return;
            var db = GetDbDataFromLocalDbPath();
            if (db != null)
            {
                userConfig.DB = db;
                HttpContext.Current.Session["UserConfig"] = userConfig;
            }
        }

        public AssistMeDb GetDbDataFromLocalDbPath()
        {
            var serializer = new JsonSerializer();
            return serializer.Deserialize(IO.File.OpenText(LocalDbPath), typeof(AssistMeDb)) as AssistMeDb;
        }

        public FileList GetAllFiles(DriveService service)
        {
            return getFileMetadata(null, service).Result;
        }

        public File GetDriveFileMetadata(AFileInfo file, DriveService service, bool CreateIfNotExist = false)
        {
            if (file != null && !string.IsNullOrWhiteSpace(file.Id))
            {
                return service.Files.Get(file.Id).ExecuteAsync().Result;
            }
            var result = getFileMetadata(file, service).Result;
            if (result != null && result.Items.Count > 0)
            {
                return result.Items.First();
            }
            else if (CreateIfNotExist)
            {
                return CreateFile(file, service);
            }

            return null;
        }

        public File CreateFile(AFileInfo file, DriveService service, IO.Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(file.MimeType))
                file.MimeType = GetMimeType(file.FileName);

            var body = new File
            {
                Title = file.FileName,
                MimeType = file.MimeType
            };

            if (!string.IsNullOrWhiteSpace(file.ParentId))
            {
                if (body.Parents == null) body.Parents = new List<ParentReference>();
                body.Parents.Add(new ParentReference { Id = file.ParentId });
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


        private Task<FileList> getFileMetadata(AFileInfo file, DriveService service)
        {
            if (service != null)
            {
                var request = service.Files.List();
                if (file == null)
                {
                    request.Q = string.Format(
                        "mimeType!='{0}' and trashed=false",
                        Constants.GFolderIdentifier);
                    return request.ExecuteAsync();
                }
                if (file.IsFolder)
                {
                    request.Q = string.Format(
                        "mimeType='{0}' and trashed=false and title='{1}'",
                        file.MimeType, file.FileName);
                }
                else
                {
                    request.Q = string.Format("trashed=false and title='{0}'", file.FileName);
                }
                if (!string.IsNullOrWhiteSpace(file.ParentId))
                    request.Q += string.Format(" and '{0}' in parents", file.ParentId);

                return request.ExecuteAsync();
            }
            return null;
        }
    }
}