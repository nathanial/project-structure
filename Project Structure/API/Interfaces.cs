using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectStructure.API {

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

    public interface IFolderNode : IProjectNode {
        event EventHandler<FolderDeletedEventArgs> Deleted;
        event EventHandler<FolderRenamedEventArgs> Renamed;
        event EventHandler<FolderSelectedEventArgs> Selected;
        event EventHandler<FolderDeselectedEventArgs> Deselected;
        event EventHandler<DirectoryRefreshedEventArgs> Refreshed;
        event EventHandler<FolderMovedEventArgs> Moved;

        void CreateSubFolder(string name);
        void CreateFile(string name, string content);
        void CreateFile(string name, byte[] content);
        
        void Select();
        void Deselect();

        void Refresh();
        void SoftRefresh();
        
        bool IsSelected { get; }
        bool IsDeleted { get; }
        bool IsRootNode { get; }
    }

    public class CouldNotOpenProjectException : Exception { }

    public enum FileChangeType {
        Modified,
        Deleted
    }

    public class DirectoryChangeEventArgs : EventArgs {
        public IList<string> AddedFiles { get; set; }

        public IList<string> AddedDirectories { get; set; }

        public bool Deleted { get; set;}
    
        public DirectoryChangeEventArgs() {
            AddedFiles = new List<string>();
            AddedDirectories = new List<string>();
        }
    }

    public delegate void DirectoryChangeHandler(DirectoryChangeEventArgs args);

    public class FileChangeEventArgs : EventArgs {
        public FileChangeType Type { get; set; }
        public string FilePath { get; set; }

        public FileChangeEventArgs(FileChangeType type, string filePath) {
            Type = type;
            FilePath = filePath;
        }
    }
    public delegate void FileChangeHandler(FileChangeEventArgs args);

    public interface IProjectIO {
        event EventHandler<ProjectIOFileLoadedEventArgs> FileLoaded;

        void Move(string oldPath, string newPath);
        void Delete(string file);
        string CachedReadText(string file);
        byte[] CachedReadRaw(string file);

        void WriteFile(string filePath, string content);
        void CreateDirectory(string path);

        IList<string> ListFiles(string dirpath = null);
        IList<string> ListDirectories(string dirpath = null);

        void WatchFile(object watcher, string file, FileChangeHandler action);
        void UnwatchFile(object watcher, string file);

        void WatchDirectory(object watcher, string dirpath, DirectoryChangeHandler action);
        void UnwatchDirectory(object watcher, string dirpath);
        DateTime FileCreationTime(string file);
        DateTime DirectoryCreationTime(string directory);

        void RunWatchers();
        DateTime DirectoryLastWriteTime(string dirpath);
        DateTime FileLastWriteTime(string filepath);
        bool FileExists(string filepath);
        string RootName { get; }

        void OpenInExplorer(IProjectNode node);
        void AddVirtualFolder(string path);
        void CreateFile(string filepath, string content);
        void CreateFile(string filepath, byte[] content);
        byte[] ReadBytes(string path);
        string GetAbsolutePath(string path);
    }

    public interface INodeFactory {
        IFolderNode CreateFolderNode(string dirpath);
        IFileNode CreateFileNode(string file);
    }

    public interface IProjectIOFactory {
        IProjectIO CreateIO(string projectPath);
    }


    public class FolderDeletedException : Exception { }

    public class RecursiveFolderException : Exception { }


}
