using System.Xml.Linq;
using ProjectStructure.API;

namespace ProjectStructure.Impl {
    public abstract class Project : FolderNode, IProject {
        readonly IProjectIO _io;
        readonly INodeFactory _nodeFactory;
        XDocument _doc;
        readonly string _projectFile;

        protected Project(string projectFile, IProjectIO projectIO, INodeFactory nodeFactory)
            : base(projectIO, nodeFactory, ".", true) {
            _projectFile = projectFile;
            _io = projectIO;
            _nodeFactory = nodeFactory;

            ProcessProjectFile(projectFile);
        }

        void ProcessProjectFile(string projectFile) {
            _doc = XDocument.Parse(_io.CachedReadText(projectFile));
            var virtualFolders = _doc.Descendants("Virtual-Folder");
            foreach (var vfolder in virtualFolders) {
                InternalAddVirtualFolder(vfolder.Value, false);
            }
            ProcessProjectFileHook(_doc);
        }

        public void AddVirtualFolder(string path) {
            InternalAddVirtualFolder(path, true);
        }

        public void CheckFilesystem() {
            _io.RunWatchers();
        }

        public void Save() {
            _io.WriteFile(_projectFile,_doc.ToString());
        }

        protected virtual void ProcessProjectFileHook(XDocument doc) { }

        void InternalAddVirtualFolder(string path, bool addToDoc) {
            _io.AddVirtualFolder(path);

            var vnode = _nodeFactory.CreateFolderNode(System.IO.Path.GetFileName(path));
            Children.Insert(0, vnode);

            if (addToDoc) {
                _doc.Element("Test-Project").Add(new XElement("Virtual-Folder", path));
                Save();
            }
        }

    }

}