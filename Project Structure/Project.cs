using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            File.WriteAllText(_projectFile, _doc.ToString());
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
        readonly IList<IFileProvider> _fileProviders = new List<IFileProvider>();
        readonly IList<IProjectProvider> _projectProviders = new List<IProjectProvider>();

        public IProject Build(string path) {
            var io = new ProjectIO(Path.GetDirectoryName(path));
            var nfac = new NodeFactory(io) { IgnoreUnknownFiles = IgnoreUnknownFiles};
            foreach (var p in _fileProviders) nfac.Register(p);

            var pprovider = FindProvider(path);
            return pprovider.Create(path, io, nfac);
        }

        public void RegisterFileType(IFileProvider provider) {
            _fileProviders.Add(provider);
        }

        public void RegisterProjectType(IProjectProvider provider) {
            _projectProviders.Add(provider);
        }

        public bool IgnoreUnknownFiles { get; set; }

        IProjectProvider FindProvider(string file) {
            return _projectProviders.FirstOrDefault(x => x.Extensions.Any(file.EndsWith));
        }
    }

}