using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace ProjectStructure {
    public interface IFolderNode : IProjectNode {
        event EventHandler<FolderDeletedEventArgs> Deleted;
        event EventHandler<FolderRenamedEventArgs> Renamed;
        event EventHandler<DirectoryRefreshedEventArgs> Refreshed;
        event EventHandler<FolderMovedEventArgs> Moved;

        IFolderNode CreateSubFolder(string name);
        IFileNode CreateFile(string name, string content);
        IFileNode CreateFile(string name, byte[] content);

        void Refresh();
        void SoftRefresh();

        bool IsDeleted { get; }
        bool IsRootNode { get; }
    }

    public class FolderNode : IFolderNode {
        public event EventHandler<FolderDeletedEventArgs> Deleted;
        public event EventHandler<FolderRenamedEventArgs> Renamed;
        public event EventHandler<FolderMovedEventArgs> Moved;
        public event EventHandler<DirectoryRefreshedEventArgs> Refreshed;

        readonly IProjectIO _io;
        readonly INodeFactory _nodeFactory;
        readonly bool _isRoot;

        string _dirpath;

        readonly ObservableCollection<IProjectNode> _children = new ObservableCollection<IProjectNode>();

        public FolderNode(IProjectIO projectIO, INodeFactory nodeFactory, string dirpath, bool isRoot = false) {
            _io = projectIO;
            _nodeFactory = nodeFactory;
            _dirpath = dirpath;
            _isRoot = isRoot;

            _children.CollectionChanged += OnCollectionChanged;

            _io.WatchDirectory(this, dirpath, CheckDirectory);

            LoadFilesAndDirectories();
        }

        public override string ToString() {
            return string.Format("{0}", _dirpath);
        }

        public string AbsolutePath {
            get {
                return _io.GetAbsolutePath(this.Path);
            }
        }

        public string Name {
            get {
                CheckDeleted();
                return _isRoot ? _io.RootName : System.IO.Path.GetFileName(_dirpath);
            }
            set {
                CheckDeleted();
                Rename(value);
            }
        }

        public IFolderNode CreateSubFolder(string name) {
            CheckDeleted();
            var dirpath = System.IO.Path.Combine(_dirpath, name);
            _io.CreateDirectory(dirpath);
            return AddDirectory(dirpath);
        }

        public IFileNode CreateFile(string name, string content) {
            CheckDeleted();
            var filepath = System.IO.Path.Combine(_dirpath, name);
            _io.CreateFile(filepath, content);
            return AddFile(filepath);
        }

        public IFileNode CreateFile(string name, byte[] content) {
            CheckDeleted();
            var filepath = System.IO.Path.Combine(_dirpath, name);
            _io.CreateFile(filepath, content);
            return AddFile(filepath);
        }

        public void Delete() {
            CheckDeleted();
            _io.Delete(_dirpath);
            _io.UnwatchDirectory(this, _dirpath);
            IsDeleted = true;
            Deleted.Raise(this, new FolderDeletedEventArgs(this));
        }

        public void Rename(string newName) {
            CheckDeleted();
            if (string.IsNullOrWhiteSpace(newName)) {
                throw new InvalidRenameException();
            }
            if (newName.Contains("\\") || newName.Contains("/")) {
                throw new InvalidRenameException();
            }
            if (newName == Name) return;
            var oldpath = _dirpath;
            var rpath = RenamePath(newName);
            _io.Move(_dirpath, rpath);
            _dirpath = rpath;

            foreach (var child in _children) {
                TakeOwnership(child);
            }

            WatchNewDirectory(oldpath, _dirpath);
            Renamed.Raise(this, new FolderRenamedEventArgs(this, oldpath, _dirpath));
        }

        public void Move(string newPath) {
            CheckDeleted();
            var oldpath = _dirpath;
            var ultimateNewPath = System.IO.Path.Combine(newPath, System.IO.Path.GetFileName(_dirpath));
            _io.Move(_dirpath, ultimateNewPath);

            _dirpath = ultimateNewPath;

            foreach (var child in _children) {
                TakeOwnership(child);
            }

            WatchNewDirectory(oldpath, _dirpath);
            Moved.Raise(this, new FolderMovedEventArgs(this));
        }

        public void Refresh() {
            CheckDeleted();
            LoadFilesAndDirectories();
            Refreshed.Raise(this, new DirectoryRefreshedEventArgs(this));
        }

        public void SoftRefresh() {
            CheckDeleted();
            _io.RunWatchers();
        }

        public string Path {
            get { return _dirpath; }
        }

        public bool IsDeleted { get; private set; }

        public bool IsRootNode {
            get { return _isRoot; }
        }

        public ObservableCollection<IProjectNode> Children { get { return _children; } }

        IProjectNode _project;
        public IProjectNode Parent {
            get {
                return _project;
            }
            set {
                if (_isRoot) throw new Exception("Project cannot have parent");
                _project = value;
            }
        }

        public void OpenInExplorer() {
            _io.OpenInExplorer(this);
        }

        string RenamePath(string newName) {
            var dname = System.IO.Path.GetDirectoryName(_dirpath);
            return dname == null ? newName : System.IO.Path.Combine(dname, newName);
        }

        void WatchNewDirectory(string oldpath, string newdir) {
            _io.UnwatchDirectory(this, oldpath);
            _io.WatchDirectory(this, newdir, CheckDirectory);
        }

        void CheckDirectory(DirectoryChangeEventArgs args) {
            if (args.Deleted) {
                _io.UnwatchDirectory(this, _dirpath);
                IsDeleted = true;
                Deleted.Raise(this, new FolderDeletedEventArgs(this));
            } else {
                foreach (var file in args.AddedFiles) {
                    AddFile(file);
                }
                foreach (var directory in args.AddedDirectories) {
                    AddDirectory(directory);
                }
            }
        }


        IFileNode AddFile(string filePath) {
            if (Children.Any(x => SamePath(x.Path, filePath))) throw new ProjectPathException();
            if (Children.Any(x => x.Name == System.IO.Path.GetFileName(filePath))) throw new ProjectPathException();
            var node = _nodeFactory.CreateFileNode(filePath);
            if (node != null) {
                _children.Add(node);
            }
            return node;
        }

        IFolderNode AddDirectory(string directory) {
            if (Children.Any(x => SamePath(x.Path, directory))) throw new ProjectPathException();
            if (Children.Any(x => x.Name == System.IO.Path.GetFileName(directory))) throw new ProjectPathException();
            var node = _nodeFactory.CreateFolderNode(directory);
            _children.Add(node);
            return node;
        }

        void RaiseFileDeleted(object sender, FileDeletedEventArgs e) {
            _children.Remove(e.FileNode);
        }

        void RaiseFolderDeleted(object sender, FolderDeletedEventArgs e) {
            _children.Remove(e.FolderNode);
        }

        void CheckDeleted() {
            if (IsDeleted) {
                throw new FolderDeletedException();
            }
        }

        bool SamePath(string p1, string p2) {
            return p1 == p2 || ".\\" + p1 == p2 || p1 == p2 + ".\\";
        }

        void LoadFilesAndDirectories() {
            foreach (var subfolder in _io.ListDirectories(_dirpath)) {
                var child = Children.OfType<IFolderNode>().FirstOrDefault(x => x.Path == _dirpath);
                if (child != null) {
                    child.Refresh();
                } else {
                    AddDirectory(subfolder);
                }
            }

            foreach (var file in _io.ListFiles(_dirpath)) {
                if (Children.OfType<IFileNode>().Any(x => x.Path == file)) {
                    continue;
                }
                try {
                    AddFile(file);
                } catch {
                    Debug.WriteLine("Could not load: " + file);
                }
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems) {
                        if (ReferenceEquals(this, item)) {
                            throw new RecursiveFolderException();
                        }
                        if (item == null) {
                            throw new NullReferenceException("cannot add 'null' to folder children");
                        }
                        if (item is IFileNode) {
                            AddFileNode((IFileNode)item);
                        } else if (item is IFolderNode) {
                            AddFolderNode((IFolderNode)item);
                        } else {
                            throw new Exception("Not type not recognized");
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems) {
                        if (item is IFileNode) {
                            RemoveFileNode((IFileNode)item);
                        } else if (item is IFolderNode) {
                            RemoveFolderNode((IFolderNode)item);
                        } else {
                            throw new Exception("Not type not recognized");
                        }
                    }
                    break;
                default:
                    throw new Exception("Unsupported collection event");
            }
        }

        void AddFileNode(IFileNode node) {
            TakeOwnership(node);
            node.Parent = this;
            node.Deleted += RaiseFileDeleted;
        }

        void RemoveFileNode(IFileNode node) {
            node.Parent = null;
            node.Deleted -= RaiseFileDeleted;
        }

        void AddFolderNode(IFolderNode node) {
            TakeOwnership(node);
            node.Parent = this;
            node.Deleted += RaiseFolderDeleted;
        }

        void RemoveFolderNode(IFolderNode node) {
            node.Parent = this;
            node.Deleted -= RaiseFolderDeleted;
        }

        void TakeOwnership(IProjectNode node) {
            if (!BelongsToThisFolder(node)) {
                MoveToThisFolder(node);
            }
        }


        void CheckExisting(IProjectNode node) {
            if (Children.Count(x => x.Name == node.Name) > 1) {
                var match = Children.First(x => x.Name == node.Name && !object.ReferenceEquals(node, x));
                if (match != null) {
                    Children.Remove(match);
                }
            }
        }


        bool BelongsToThisFolder(IProjectNode node) {
            if (IsRootNode) {
                return node.Path.Split(System.IO.Path.DirectorySeparatorChar).Length == 1 || node.Path.StartsWith(".\\");
            }
            return System.IO.Path.GetDirectoryName(node.Path) == _dirpath;
        }

        void MoveToThisFolder(IProjectNode node) {
            node.Move(_dirpath);
        }

    }

    public class InvalidRenameException : Exception { }
}