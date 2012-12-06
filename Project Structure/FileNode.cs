using System;
using System.Collections.ObjectModel;
using NLog;

namespace ProjectStructure {
    public interface IFileNode : IProjectNode {
        byte[] Data { get; set; }
    }

    public class FileNode : IFileNode {
        public event EventHandler<PreviewNodeDeletedEventArgs> PreviewDeleted;
        public event EventHandler<PreviewNodeRenamedEventArgs> PreviewRenamed;
        public event EventHandler<PreviewNodeMovedEventArgs> PreviewMoved;
        public event EventHandler<PreviewNodeModifiedEventArgs> PreviewModified;

        public event EventHandler<NodeDeletedEventArgs> Deleted;
        public event EventHandler<NodeRenamedEventArgs> Renamed;
        public event EventHandler<NodeMovedEventArgs> Moved;
        public event EventHandler<NodeModifiedEventArgs> Modified;

        readonly IProjectIO _io;

        //This remains forever empty

        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FileNode(IProjectIO projectIO, string file) {
            _io = projectIO;
            FilePath = file;
            _logger.Trace("Created {0}: {1}", GetType().Name,file);
        }

        public override string ToString() {
            return FilePath;
        }

        protected string FilePath { get; set; }

        public string AbsolutePath {
            get {
                return _io.GetAbsolutePath(this.Path);
            }
        }

        public string Name {
            get {
                return System.IO.Path.GetFileName(FilePath);
            }
        }

        public byte[] Data {
            get {
                return _io.CachedReadRaw(FilePath);
            }
            set {
                var oldData = Data;
                PreviewModified.RaiseAndValidate(this, new PreviewNodeModifiedEventArgs(oldData, value));
                _io.WriteFile(FilePath,value);
                Modified.Raise(this, new NodeModifiedEventArgs(oldData,value));
            }
        }



        public void Rename(string newName) {
            if (newName == Name) return;
        
            var oldPath = FilePath;
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), newName);

            PreviewRenamed.RaiseAndValidate(this, new PreviewNodeRenamedEventArgs(oldPath,newPath));

            _io.Move(FilePath, newPath);
            FilePath = newPath;
            Renamed.Raise(this, new NodeRenamedEventArgs(oldPath, FilePath));
        }

        public void Move(string newPath) {
            var oldPath = FilePath;
            var ultimateNewPath = System.IO.Path.Combine(newPath, System.IO.Path.GetFileName(FilePath));
            PreviewMoved.RaiseAndValidate(this, new PreviewNodeMovedEventArgs(FilePath, ultimateNewPath));
            _io.Move(FilePath, ultimateNewPath);
            FilePath = ultimateNewPath;
            Moved.Raise(this, new NodeMovedEventArgs(oldPath,newPath));
        }


        public bool IsDeleted { get; private set; }

        public void Delete() {
            PreviewDeleted.RaiseAndValidate(this, new PreviewNodeDeletedEventArgs());
            _io.Delete(FilePath);
            IsDeleted = true;
            Deleted.Raise(this, new NodeDeletedEventArgs());
        }


        public string Path {
            get {
                return FilePath;
            }
        }

        public ObservableCollection<IProjectNode> Children {
            get {
                return null;
            }
        }

        public void OpenInExplorer() {
            _io.OpenInExplorer(this);
        }

        public IProjectNode Parent { get; set; }
    }

    public class FileNodeDeletedException : Exception { }
    public class NothingToSaveException : Exception { }
    public class FileUnsavedChangesException : Exception { }
    public class FileMoveException : Exception { }

}
