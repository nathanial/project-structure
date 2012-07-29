using System;
using System.Collections.Generic;

namespace ProjectStructure {
    public class ProjectIOFileLoadedEventArgs : EventArgs {
        public string FileName { get; private set; }
        public ProjectIOFileLoadedEventArgs(string filename) {
            FileName = filename;
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

    public class DirectoryChangeEventArgs : EventArgs {
        public IList<string> AddedFiles { get; set; }

        public IList<string> AddedDirectories { get; set; }

        public bool Deleted { get; set; }

        public DirectoryChangeEventArgs() {
            AddedFiles = new List<string>();
            AddedDirectories = new List<string>();
        }
    }

    public class FileChangeEventArgs : EventArgs {
        public FileChangeType Type { get; set; }
        public string FilePath { get; set; }

        public FileChangeEventArgs(FileChangeType type, string filePath) {
            Type = type;
            FilePath = filePath;
        }
    }

    public class CouldNotOpenProjectException : Exception { }

    public enum FileChangeType {
        Modified,
        Deleted
    }


    public delegate void DirectoryChangeHandler(DirectoryChangeEventArgs args);


    public delegate void FileChangeHandler(FileChangeEventArgs args);


}
