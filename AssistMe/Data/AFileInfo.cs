
using System.Collections.Generic;
namespace AssistMe.Data
{
    //public interface IDocumentInfo
    //{
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //}

    public class AFileInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DetailedInfo { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DownloadUrl { get; set; }
        public AFolderInfo ParentFolder { get; set; }
        private List<AFileInfo> _childern = new List<AFileInfo>();
        public List<AFileInfo> Children { get { return _childern; } set { _childern = value; } }
        public bool IsFolder { get; set; }
    }

    public class AFolderInfo : AFileInfo
    {
        public AFolderInfo() { IsFolder = true; }
        public AFolderInfo(string name, string description, AFolderInfo parent)
        {
            Name = name; Description = description; ParentFolder = parent;
            IsFolder = true;
        }
    }

    public static class DummyData
    {
        public static AFolderInfo GetDummyStructure()
        {
            var rootFolder = new AFolderInfo("Root", "Doc root", null);
            var certificateFolder = new AFolderInfo("Certificates", "root of all certificates", rootFolder);
            var degreeCertificateFolder = new AFolderInfo("Degree certidicates", "degree certificates", certificateFolder);
            var degreeCertificate = new AFileInfo()
            {
                Description = "Final certificate",
                Name = "Consolidated",
                ParentFolder = degreeCertificateFolder
            };
            var degreeCertificate2 = new AFileInfo()
            {
                Description = "2nd year certificate",
                Name = "2nd Year",
                ParentFolder = degreeCertificateFolder
            };
            var sslcCertificate = new AFileInfo()
            {
                Description = "SSLC",
                Name = "SSLC",
                ParentFolder = degreeCertificateFolder
            };

            degreeCertificateFolder.Children.Add(degreeCertificate);
            degreeCertificateFolder.Children.Add(degreeCertificate2);
            certificateFolder.Children.Add(sslcCertificate);
            certificateFolder.Children.Add(degreeCertificateFolder);


            var ebooksFolder = new AFolderInfo("ebooks", "All ebboks", null);
            var mvcbook = new AFileInfo
            {
                Description = "MVC Tutorial",
                Name = "MVC doc",
            };
            ebooksFolder.Children.Add(mvcbook);
            degreeCertificateFolder.Children.Add(ebooksFolder);


            rootFolder.Children.Add(certificateFolder);
            rootFolder.Children.Add(ebooksFolder);
            rootFolder.Children.Add(certificateFolder);
            rootFolder.Children.Add(ebooksFolder);
            rootFolder.Children.Add(certificateFolder);

            return rootFolder;
        }

    }
}