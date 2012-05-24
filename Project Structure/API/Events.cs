using System;

namespace ProjectStructure.API {
    public static class ProjectEvents {
        public const string Error = "topic://dchem/project/error";

        public static class Folder {
            public const string Added = "topic://dchem/project/directory/added";
            public const string Deleted = "topic://dchem/project/directory/deleted";
            public const string Renamed = "topic://dchem/project/directory/renamed";
            public const string Selected = "topic://dchem/project/directory/selected";
            public const string Deselected = "topic://dchem/project/directory/deselected";
            public const string Refreshed = "topic://dchem/project/directory/refreshed";
            public const string Moved = "topic://dchem/project/directory/moved";
        }

        public static class File {
            public const string DirtyTextChanged = "topic://dchem/project/file/dirty-text-changed";
            public const string Added = "topic://dchem/project/file/added";
            public const string Deleted = "topic://dchem/project/file/deleted";
            public const string Renamed = "topic://dchem/project/file/renamed";
            public const string Moved = "topic://dchem/project/file/moved";
            public const string Modified = "topic://dchem/project/file/modified";
            public const string Saved = "topic://dchem/project/file/saved";
        }

        public static class Project {
            public const string Closed = "topioc://dchem/project/closed";
            public const string LoadingStarted = "topic://dchem/project/loading-started";
            public const string LoadingFinished = "topic://dchem/project/loading-finished";
            public const string LoadingProgress = "topic://dchem/project/loading-progress";
        }

        public static class IO {
            public const string FileLoaded = "topic://dchem/project/io/file-loaded";
        }
    }

    public class ProjectIOFileLoadedEventArgs : EventArgs {
        public string FileName { get; private set; }
        public ProjectIOFileLoadedEventArgs(string filename) {
            FileName = filename;
        }
    }

    public class ProjectLoadingStartedEventArgs : EventArgs {
        public string ProjectName { get; private set; }
        public ProjectLoadingStartedEventArgs(string name) {
            ProjectName = name;
        }
    }
    public class ProjectLoadingFinishedEventArgs : EventArgs {
        public IProject Project { get; private set; }
        public ProjectLoadingFinishedEventArgs(IProject project) {
            Project = project;
        }
    }
    public class ProjectLoadingProgressEventArgs : EventArgs {
        public double Progress { get; private set; }
        public double Total { get; private set; }

        public ProjectLoadingProgressEventArgs(double progress, double total) {
            Progress = progress;
            Total = total;
        }
    }

    public class FolderDeletedEventArgs : EventArgs {
        public IFolderNode FolderNode { get; set; }
        public FolderDeletedEventArgs(IFolderNode dir) {
            FolderNode = dir;
        }
    }
    public class FolderRenamedEventArgs : EventArgs {
        public IFolderNode Directory { get; set; }
        public string OldPath { get; set; }
        public string NewPath { get; set; }

        public FolderRenamedEventArgs(IFolderNode directory, string oldPath, string newPath) {
            Directory = directory;
            OldPath = oldPath;
            NewPath = newPath;
        }
    }

    public class FolderSelectedEventArgs : EventArgs {
        public IFolderNode FolderNode { get; set; }
        public FolderSelectedEventArgs(IFolderNode dir) {
            FolderNode = dir;
        }
    }

    public class FolderDeselectedEventArgs : EventArgs {
        public IFolderNode FolderNode { get; set; }
        public FolderDeselectedEventArgs(IFolderNode dir) {
            FolderNode = dir;
        }
    }

    public class FolderMovedEventArgs : EventArgs {
        public IFolderNode FolderNode { get; private set; }
        public FolderMovedEventArgs(IFolderNode node) {
            FolderNode = node;
        }

    }

    public class DirectoryRefreshedEventArgs : EventArgs {
        public IFolderNode Directory { get; set; }
        public DirectoryRefreshedEventArgs(IFolderNode dir) {
            Directory = dir;
        }
    }

    public class FileSavedEventArgs : EventArgs { }
    public class FileDirtyTextChangedEventArgs : EventArgs {
        public string DirtyText { get; set; }
        public FileDirtyTextChangedEventArgs(string newText) {
            DirtyText = newText;
        }
    }

    public class FileModifiedEventArgs : EventArgs {
        public string NewText { get; private set; }
        public FileModifiedEventArgs(string newText) {
            NewText = newText;
        }
    }

    public class FileMovedEventArgs : EventArgs {
        public string NewPath { get; private set; }
        public FileMovedEventArgs(string newPath) {
            NewPath = newPath;
        }
    }

    public class FileRenamedEventArgs : EventArgs {
        public IFileNode FileNode { get; private set; }
        public string OldPath { get; private set; }
        public string NewPath { get; private set; }
        public FileRenamedEventArgs(IFileNode node, string oldpath, string newpath) {
            FileNode = node;
            OldPath = oldpath;
            NewPath = newpath;
        }
    }

    public class FileDeletedEventArgs : EventArgs {
        public IFileNode FileNode { get; private set; }
        public FileDeletedEventArgs(IFileNode node) {
            FileNode = node;
        }
    }

    public class ProjectClosedEventArgs : EventArgs {
        public IProject Project { get; private set; }
        public ProjectClosedEventArgs(IProject project) {
            Project = project;
        }
    }

}
