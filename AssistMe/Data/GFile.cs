
using System.Collections.Generic;
namespace AssistMe
{
    public class GFile
    {
        public string id { get; set; }
        public string title { get; set; }
        public string mimeType { get; set; }
        public bool IsDirectory { get { return mimeType == Constants.GFolderIdentifier; } }

        public string parentId { get; set; }

        private List<GFile> _children = new List<GFile>();
        public List<GFile> Children
        {
            get { return _children; }
            set { _children = value; }
        }

    }
}