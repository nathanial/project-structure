using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;

namespace ProjectStructure {
    public interface IFileNode : IProjectNode {
        event EventHandler<FileDeletedEventArgs> Deleted;
        event EventHandler<FileRenamedEventArgs> Renamed;
        event EventHandler<FileMovedEventArgs> Moved;
        event EventHandler<FileModifiedEventArgs> Modified;
        event EventHandler<FileSavedEventArgs> Saved;
        event EventHandler<FileDirtyTextChangedEventArgs> DirtyTextChanged;

        string Text { get; }
        string DirtyText { get; set; }

        byte[] RawBytes { get; }

        bool IsDirty { get; }
        bool IsDeleted { get; }

        void Save();
        void Clean();
    }

    public class FileNode : IFileNode {
        public event EventHandler<FileDeletedEventArgs> Deleted;
        public event EventHandler<FileRenamedEventArgs> Renamed;
        public event EventHandler<FileMovedEventArgs> Moved;
        public event EventHandler<FileModifiedEventArgs> Modified;
        public event EventHandler<FileSavedEventArgs> Saved;

        public event EventHandler<FileDirtyTextChangedEventArgs> DirtyTextChanged;

        readonly IProjectIO _io;

        readonly ObservableCollection<IProjectNode> _children = new ObservableCollection<IProjectNode>();

        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FileNode(IProjectIO projectIO, string file) {
            _io = projectIO;
            FilePath = file;
            _io.WatchFile(this, FilePath, RaiseModified);
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
                CheckDeleted();
                return System.IO.Path.GetFileName(FilePath);
            }
            set {
                CheckDeleted();
                Rename(value);
                OnRename(value);
            }
        }

        protected virtual void OnRename(string newName) {
            
        }

        public string Text {
            get {
                CheckDeleted();
                return _io.CachedReadText(FilePath);
            }
        }

        public byte[] RawBytes {
            get {
                CheckDeleted();
                return _io.CachedReadRaw(FilePath);
            }
        }

        string _dirtyText;
        public string DirtyText {
            get {
                CheckDeleted();
                return IsDirty ? _dirtyText : Text;
            }
            set {
                CheckDeleted();
                _dirtyText = value;
                IsDirty = true;
                DirtyTextChanged.Raise(this, new FileDirtyTextChangedEventArgs(DirtyText));
            }
        }

        bool _isDirty;
        public bool IsDirty {
            get {
                CheckDeleted();
                return _isDirty;
            }
            protected set {
                _isDirty = value;
            }
        }

        public bool IsDeleted { get; private set; }

        public void Rename(string newName) {
            CheckDeleted();
            if (newName == Name) return;
            if (string.IsNullOrWhiteSpace(newName) ||
                newName.Contains("\\")) {
                throw new InvalidRenameException();
            }
            if (AcceptableFileExtensions != null) {
                if (AcceptableFileExtensions.Any(newName.EndsWith)) {
                    var extension = AcceptableFileExtensions.First(newName.EndsWith);
                    if (newName == extension) {
                        throw new InvalidRenameException();
                    }
                } else {
                    throw new InvalidRenameException();
                }
            }
            var oldPath = FilePath;
            _io.UnwatchFile(this, FilePath);
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), newName);
            _io.Move(FilePath, newPath);
            FilePath = newPath;
            _io.WatchFile(this, FilePath, RaiseModified);
            Renamed.Raise(this, new FileRenamedEventArgs(this, oldPath, FilePath));
        }

        public void Move(string newPath) {
            CheckDeleted();
            var ultimateNewPath = System.IO.Path.Combine(newPath, System.IO.Path.GetFileName(FilePath));
            _io.UnwatchFile(this, FilePath);
            _io.Move(FilePath, ultimateNewPath);
            FilePath = ultimateNewPath;
            _io.WatchFile(this, FilePath, RaiseModified);
            Moved.Raise(this, new FileMovedEventArgs(FilePath));
        }

        public void Delete() {
            CheckDeleted();
            if (IsDirty) {
                throw new FileUnsavedChangesException();
            }
            _io.UnwatchFile(this, FilePath);
            _io.Delete(FilePath);
            IsDeleted = true;
            Deleted.Raise(this, new FileDeletedEventArgs(this));
        }

        public void Save() {
            CheckDeleted();
            if (!IsDirty) return;
            _io.WriteFile(FilePath, DirtyText);
            IsDirty = false;
            Saved.Raise(this, new FileSavedEventArgs());
        }

        public string Path {
            get {
                CheckDeleted();
                return FilePath;
            }
        }

        void RaiseModified(FileChangeEventArgs args) {
            if (args != null && args.Type == FileChangeType.Deleted) {
                Deleted.Raise(this, new FileDeletedEventArgs(this));
            } else {
                Modified.Raise(this, new FileModifiedEventArgs(Text));
            }
        }

        protected void CheckDeleted() {
            if (IsDeleted) {
                throw new FileNodeDeletedException();
            }
        }

        public void Clean() {
            CheckDeleted();
            IsDirty = false;
            RaiseModified(null);
        }

        public ObservableCollection<IProjectNode> Children {
            get {
                CheckDeleted();
                return _children;
            }
        }

        public void OpenInExplorer() {
            _io.OpenInExplorer(this);
        }

        public IProjectNode Parent { get; set; }

        protected virtual IList<string> AcceptableFileExtensions {
            get {
                return null;
            }
        }
    }

    public class FileNodeDeletedException : Exception { }
    public class NothingToSaveException : Exception { }
    public class FileUnsavedChangesException : Exception { }
    public class FileMoveException : Exception { }

}
