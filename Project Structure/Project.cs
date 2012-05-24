using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace ProjectStructure {
    public interface IProjectNode {
        void Rename(string newName);
        void Move(string newPath);
        void Delete();

        string Name { get; set; }
        string Path { get; }
        IProjectNode Parent { get; set; }
        ObservableCollection<IProjectNode> Children { get; }

        void OpenInExplorer();

        string AbsolutePath { get; }
    }


    public interface IProject : IFolderNode {
        void AddVirtualFolder(string path);
        void CheckFilesystem();
        void Save();
    }

    public class Project : FolderNode, IProject {
        readonly IProjectIO _io;
        readonly INodeFactory _nodeFactory;
        XDocument _doc;
        readonly string _projectFile;

        public Project(string projectFile, IProjectIO projectIO, INodeFactory nodeFactory)
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
                _doc.Element("Project").Add(new XElement("Virtual-Folder", path));
                Save();
            }
        }

    }

    public class ProjectBuilder {
        readonly IList<IFileProvider> _providers = new List<IFileProvider>();

        public IProject Build(string path) {
            var io = new ProjectIO(Path.GetDirectoryName(path));
            var nfac = new NodeFactory(io) { IgnoreUnknownFiles = IgnoreUnknownFiles};
            foreach (var p in _providers) nfac.Register(p);
            return new Project(path, io, nfac);            
        }

        public void Register(IFileProvider provider) {
            _providers.Add(provider);
        }

        public bool IgnoreUnknownFiles { get; set; }
    }

}