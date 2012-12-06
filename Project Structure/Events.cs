using System;
using System.Collections.Generic;

namespace ProjectStructure {
    public class ProjectIOFileLoadedEventArgs : EventArgs {
        public string FileName { get; private set; }
        public ProjectIOFileLoadedEventArgs(string filename) {
            FileName = filename;
        }
    }

    public class DirectoryRefreshedEventArgs : EventArgs {
        public IFolderNode Directory { get; set; }
        public DirectoryRefreshedEventArgs(IFolderNode dir) {
            Directory = dir;
        }
    }

    public class NodeModifiedEventArgs : EventArgs {
        public byte[] OldData { get; private set; }
        public byte[] NewData { get; private set; }
        public NodeModifiedEventArgs(byte[] oldData, byte[] newData) {
            OldData = oldData;
            NewData = newData;
        }
    }

    public class NodeMovedEventArgs : EventArgs {
        public string OldPath { get; private set; }
        public string NewPath { get; private set; }
        public NodeMovedEventArgs(string oldPath, string newPath) {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }

    public class NodeRenamedEventArgs : EventArgs {
        public string OldPath { get; private set; }
        public string NewPath { get; private set; }
        public NodeRenamedEventArgs(string oldpath, string newpath) {
            OldPath = oldpath;
            NewPath = newpath;
        }
    }

    public class NodeDeletedEventArgs : EventArgs {
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

    public class CouldNotOpenProjectException : Exception { }

    public delegate void DirectoryChangeHandler(DirectoryChangeEventArgs args);


    public class PreviewFileEventArgs : EventArgs {
        public Exception Error { get; set; }

    }

    public class PreviewNodeDeletedEventArgs : PreviewFileEventArgs{}
    public class PreviewNodeRenamedEventArgs : PreviewFileEventArgs {
        public string OldPath { get; set; }
        public string NewPath { get; set; }

        public PreviewNodeRenamedEventArgs(string oldPath, string newPath) {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }
    public class PreviewNodeMovedEventArgs : PreviewFileEventArgs {
        public string OldPath { get; set; }
        public string NewPath { get; set; }
        public PreviewNodeMovedEventArgs(string oldPath, string newPath) {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }

    public class PreviewNodeModifiedEventArgs : PreviewFileEventArgs {
        public byte[] OldText { get; set; }
        public byte[] NewText { get; set; }
        public PreviewNodeModifiedEventArgs(byte[] oldText, byte[] newText) {
            OldText = oldText;
            NewText = newText;
        }
    }

    public static class PreviewExtensions {
        public static void RaiseAndValidate<T>(this EventHandler<T> handler, object sender, T args) where T : PreviewFileEventArgs {
            handler.Raise(sender, args);
            if (args.Error != null) {
                throw args.Error;
            }
        }
    }

}
