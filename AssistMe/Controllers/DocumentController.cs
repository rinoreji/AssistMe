﻿using AssistMe.Data;
using AssistMe.Helpers;
using Google.Apis.Drive.v2;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    [CustomAuthorize]
    public class DocumentController : Controller
    {
        public ActionResult FolderTag()
        {
            var folders = UserConfig.DB.AssistMeDrive.Children
                .Where(f => f.IsFolder)
                .Select(f => f.DisplayName).ToArray();

            var result = Json(folders, JsonRequestBehavior.AllowGet);
            return result;
        }

        public ActionResult ListGDrive()
        {
            var service = HttpContext.Session["DriveService"] as DriveService;
            var gHelper = new GDriveHelper();
            var list = gHelper.GetAllFiles(service);
            var model = list.Items.Select(f => f.GetGFile<AFileInfo>());
            return PartialView("_ListGDrive", model);
        }

        //
        // GET: /Document/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Document/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            //try
            //{
            var fileInfo = new AFileInfo
            {
                Id = collection["Id"],
                Description = collection["Description"],
                DetailedInfo = collection["DetailedInfo"],
                DisplayName = collection["DisplayName"],
                FolderName = collection["FolderName"],
            };

            var folder = new AFolderInfo
            {
                ParentId = UserConfig.DB.AssistMeDrive.Id,
                FileName = fileInfo.FolderName,
                DisplayName = fileInfo.FolderName,
            };
            var service = HttpContext.Session["DriveService"] as DriveService;
            var ghelper = new GDriveHelper();
            var serverFolder = ghelper.GetDriveFileMetadata(folder, service, CreateIfNotExist: true);

            folder.Id = serverFolder.Id;

            //check folder already exist
            var existingFolder = UserConfig.DB.AssistMeDrive.GetChild(folder.Id);
            if (existingFolder == null)
            {
                var parentofFolder = UserConfig.DB.AssistMeDrive.GetChild(folder.ParentId);
                parentofFolder.Children.Add(folder);
            }
            //TODO: If folder changed while editing, Remove the file from older folder
            fileInfo.ParentId = folder.Id;

            Google.Apis.Drive.v2.Data.File driveFile = null;
            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                var file = Request.Files[0];
                fileInfo.FileName = file.FileName;
                driveFile = ghelper.CreateFile(fileInfo, service, file.InputStream);
            }
            else
            {
                driveFile = ghelper.GetDriveFileMetadata(fileInfo, service);
            }

            fileInfo.Id = driveFile.Id;
            fileInfo.DownloadUrl = driveFile.DownloadUrl;
            fileInfo.ThumbnailUrl = driveFile.ThumbnailLink;

            var existingFile = UserConfig.DB.AssistMeDrive.GetChild(fileInfo.Id);
            if (existingFile != null && existingFile.ParentId != null)
            {
                var folderOfFile = UserConfig.DB.AssistMeDrive.GetChild(existingFile.ParentId);
                folderOfFile.Children.Remove(folderOfFile.Children.First(f => f.Id == existingFile.Id));
            }

            var parentFolderInDb = UserConfig.DB.AssistMeDrive.GetChild(fileInfo.ParentId);
            parentFolderInDb.Children.Add(fileInfo);

            var dbFile = UserConfig.DB.AssistMeDrive.Children.First(f => f.FileName != null && f.FileName == UserConfig.DB_NAME);
            if (string.IsNullOrWhiteSpace(dbFile.Id))
            {
                var serverDbFile = ghelper.GetDriveFileMetadata(new AFileInfo
                {
                    ParentId = UserConfig.DB.AssistMeDrive.Id,
                    FileName = UserConfig.DB_NAME,
                }, service);
                dbFile.Id = serverDbFile.Id;
                dbFile.IsSystemFile = true;
            }

            ghelper.WriteDBDataToLocalDB();

            return RedirectToAction("Index", "Home");
            //}
            //catch
            //{
            //    return View();
            //}
        }

        //
        // GET: /Document/Edit/5

        public ActionResult Edit(string id)
        {
            var aFile = UserConfig.DB.AssistMeDrive.GetChild(id);
            return View(string.Format("Create", id), aFile);
        }

        //
        // POST: /Document/Edit/5

        public ActionResult Download(string id)
        {
            var aFile = UserConfig.DB.AssistMeDrive.GetChild(id);

            return File(DownloadFile(aFile.DownloadUrl),
                System.Net.Mime.MediaTypeNames.Application.Octet, aFile.FileName);
        }


        public static System.IO.Stream DownloadFile(string DownloadUrl)
        {
            if (!String.IsNullOrEmpty(DownloadUrl))
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                        new Uri(DownloadUrl));
                    //UserConfig.UserCredential.
                    //authenticator.ApplyAuthenticationToRequest(request);
                    request.Headers.Add("Authorization", "Bearer " + UserConfig.GoogleAccessToken);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return response.GetResponseStream();
                    }
                    else
                    {
                        Console.WriteLine(
                            "An error occurred: " + response.StatusDescription);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return null;
            }
        }
    }
}
