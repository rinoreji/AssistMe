using System.Collections.Generic;
using System.Linq;
namespace AssistMe.Data
{
    public class AFileInfo
    {
        private string _displayName;
        public string Id { get; set; }
        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace(_displayName) ? FileName : _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string DetailedInfo { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string ParentId { get; set; }
        public string FolderName { get; set; }
        private List<AFileInfo> _childern = new List<AFileInfo>();
        public List<AFileInfo> Children { get { return _childern; } set { _childern = value; } }
        public bool IsFolder { get { return MimeType == Constants.GFolderIdentifier; } }
        public string MimeType { get; set; }
        public bool IsSystemFile { get; set; }

        public AFileInfo GetChild(string id)
        {
            return FindChildRecursive(id, this);
        }
        public override string ToString()
        {
            return string.Format("{0} - {1}", FileName, IsFolder ? "Folder" : "File");
        }
        public List<AFileInfo> GetAllChildren(AFileInfo parent = null, bool folderOnly = false)
        {
            if (parent == null)
                parent = this;
            var list = new List<AFileInfo>();
            IEnumerable<AFileInfo> children = new List<AFileInfo>();
            if (folderOnly)
                children = parent.Children.Where(f => f.IsFolder);
            else
                children = parent.Children;
            foreach (var child in children)
            {
                list.Add(child);
                list.AddRange(GetAllChildren(child, folderOnly));
            }
            return list;
        }

        private AFileInfo FindChildRecursive(string id, AFileInfo aFile)
        {
            if (aFile.Id == id)
                return aFile;
            else if (aFile.IsFolder && aFile.Children.Any(f => f.Id == id))
            {
                return aFile.Children.First(f => f.Id == id);
            }
            else
            {
                if (aFile.IsFolder && aFile.Children.Any(f => f.IsFolder))
                {
                    foreach (var child in aFile.Children.Where(f => f.IsFolder))
                    {
                        var data = FindChildRecursive(id, child);
                        if (data != null)
                            return data;
                    }
                }
            }
            return null;
        }
    }

    public class AFolderInfo : AFileInfo
    {
        public AFolderInfo() { MimeType = Constants.GFolderIdentifier; }
    }
}