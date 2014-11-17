using AssistMe.Data;
using AssistMe.Helpers;
using Google.Apis.Drive.v2;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    [CustomAuthorize]
    public class DocumentController : Controller
    {

        public ActionResult FolderTag()
        {
            string[] tags = { "rino", "reji", "rrc", "npa", "mmo" };
            var result = Json(tags, JsonRequestBehavior.AllowGet);
            return result;
        }
        //
        // GET: /Document/

        public ActionResult Index()
        {
            return View(new List<AFileInfo>());
        }

        //
        // GET: /Document/Details/5

        public ActionResult Details(int id)
        {
            return View();
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
            try
            {
                var fileInfo = new AFileInfo
                {
                    Description = collection["Description"],
                    DetailedInfo = collection["DetailedInfo"],
                    DisplayName = collection["DisplayName"],
                };

                var foldername = collection["foldername"];

                //get the id of this folder by searching filetype=folder, trashed=false, title=foldername, parentid = id of the drive folder
                //later we can think of storing the id of all folder created by us in the db itself

                //ok, now if the folder exist, get id. else create it and get id
                //set this id to the file parent
                //upload this file.
                //get the uploaded files details, create AFileInfo from the uploaded data
                //Add this file to the ADrive variable, serialize to local and upload to server.
                var folder = new AFolderInfo
                {
                    ParentId = UserConfig.DB.AssistMeDrive.Id,
                    FileName = foldername,
                    DisplayName = foldername,
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

                fileInfo.ParentId = folder.Id;

                var file = Request.Files[0];
                fileInfo.FileName = file.FileName;

                var uploadedFile = ghelper.CreateFile(fileInfo, service, file.InputStream);
                fileInfo.Id = uploadedFile.Id;
                fileInfo.DownloadUrl = uploadedFile.DownloadUrl;
                fileInfo.ThumbnailUrl = uploadedFile.ThumbnailLink;

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
                //upload db to gdrive


                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Document/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Document/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Document/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Document/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
