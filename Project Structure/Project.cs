using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace ProjectStructure {
    public interface IProjectNode {
        event EventHandler<PreviewNodeDeletedEventArgs> PreviewDeleted;
        event EventHandler<PreviewNodeRenamedEventArgs> PreviewRenamed;
        event EventHandler<PreviewNodeMovedEventArgs> PreviewMoved;
        event EventHandler<PreviewNodeModifiedEventArgs> PreviewModified;

        event EventHandler<NodeDeletedEventArgs> Deleted;
        event EventHandler<NodeRenamedEventArgs> Renamed;
        event EventHandler<NodeMovedEventArgs> Moved;
        event EventHandler<NodeModifiedEventArgs> Modified;

        void Rename(string newName);
        void Move(string newPath);
        void Delete();

        string Name { get;  }
        string Path { get; }
        IProjectNode Parent { get; set; }
        ObservableCollection<IProjectNode> Children { get; }

        void OpenInExplorer();

        string AbsolutePath { get; }
    }


    public interface IProject : IFolderNode {
        void AddVirtualFolder(string path);
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

        public void Save() {
            File.WriteAllText(_projectFile, _doc.ToString());
        }

        protected virtual void ProcessProjectFileHook(XDocument doc) { }

        void InternalAddVirtualFolder(string path, bool addToDoc) {
            _io.AddVirtualFolder(path);

            var vnode = _nodeFactory.CreateFolderNode(System.IO.Path.GetFileName(path));
            Children.Insert(0, vnode);

            if (addToDoc) {
                _doc.Root.Add(new XElement("Virtual-Folder", path));
                Save();
            }
        }

    }
}