using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProjectStructure {
    public interface IProjectIO {
        event EventHandler<ProjectIOFileLoadedEventArgs> FileLoaded;

        void Move(string oldPath, string newPath);
        void Delete(string file);
        string CachedReadText(string file);
        byte[] CachedReadRaw(string file);

        void WriteFile(string filePath, string content);
        void WriteFile(string filePath, byte[] data);
        void CreateDirectory(string path);

        IList<string> ListFiles(string dirpath = null);
        IList<string> ListDirectories(string dirpath = null);

        DateTime FileCreationTime(string file);
        DateTime DirectoryCreationTime(string directory);

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


    public class ProjectIO : IProjectIO {
        public event EventHandler<ProjectIOFileLoadedEventArgs> FileLoaded;

        readonly string _projectPath;

        readonly IDictionary<string, string> _cache = new Dictionary<string, string>();
        readonly IDictionary<string, string> _virtualFolders = new Dictionary<string, string>();

        public ProjectIO(string projectPath) {
            _projectPath = projectPath;
        }

        
        public string GetAbsolutePath(string path) {
            return ResolvePath(path);
        }


        public void AddVirtualFolder(string path) {
            var existingDirs = ListDirectories(null);
            if (existingDirs.Contains(Path.GetFileName(path))) {
                throw new VirtualFolderException();
            }
            if (!Path.IsPathRooted(path)) {
                path = Path.Combine(_projectPath, path);
            }
            _virtualFolders[Path.GetFileName(path)] = path;
        }

        public void Move(string oldPath, string newPath) {
            CheckPaths(oldPath, newPath);
            var actualOldPath = ResolvePath(oldPath);
            var actualNewPath = ResolvePath(newPath);

            //skip if origin doesn't exist (probably because file has already moved)
            if (File.Exists(actualOldPath)) {
                File.Move(actualOldPath, actualNewPath);
            } else if (Directory.Exists(actualOldPath)) {
                Directory.Move(actualOldPath, actualNewPath);
            }
        }

        public void Delete(string file) {
            CheckPaths(file);
            if (Directory.Exists(ResolvePath(file))) {
                DeleteDirectory(ResolvePath(file));
            } else {
                File.Delete(ResolvePath(file));
            }
        }

        public string CachedReadText(string file) {
            CheckPaths(file);
            FileLoaded.Raise(this, new ProjectIOFileLoadedEventArgs(file));
            return _cache.ContainsKey(file)
                       ? _cache[file]
                       : File.ReadAllText(ResolvePath(file));
        }

        public byte[] CachedReadRaw(string file) {
            CheckPaths(file);
            FileLoaded.Raise(this, new ProjectIOFileLoadedEventArgs(file));
            return File.ReadAllBytes(ResolvePath(file));
        }

        public void WriteFile(string filePath, string content) {
            CheckPaths(filePath);
            File.WriteAllText(ResolvePath(filePath), content);
        }

        public void WriteFile(string filePath, byte[] data) {
            CheckPaths(filePath);
            File.WriteAllBytes(ResolvePath(filePath), data);
        }

        public void CreateDirectory(string path) {
            CheckPaths(path);
            Directory.CreateDirectory(ResolvePath(path));
        }

        public IList<string> ListFiles(string dirpath) {
            var files = new List<string>();
            try {
                if (dirpath == null || dirpath == ".") {
                    files = Directory.GetFiles(_projectPath).ToList();
                    files = files.Select(Path.GetFileName).ToList();
                } else {
                    CheckPaths(dirpath);
                    var vpath = GetVPath(dirpath);
                    if (vpath != null) {
                        files = Directory.GetFiles(vpath).ToList();
                        files = files.Select(x => Path.Combine(dirpath, Path.GetFileName(x))).ToList();
                    } else {
                        files = Directory.GetFiles(Path.Combine(_projectPath, dirpath)).ToList();
                        files = files.Select(x => x.Remove(0, _projectPath.Length + 1))
                            .ToList();
                    }
                }
                files.Sort();
            } catch(DirectoryNotFoundException ex) {
                Debug.WriteLine("Unable to list files in directory: " + ex.Message);
            }
            return files;
        }


        public IList<string> ListDirectories(string dirpath) {
            var directories = new List<string>();
            try {
                if (dirpath == null || dirpath == ".") {
                    directories = Directory.GetDirectories(_projectPath).Concat(_virtualFolders.Values).ToList();
                    directories.Sort(new DirectoryCreatedComparison(this));
                    directories = directories.Select(Path.GetFileName).ToList();
                } else {
                    CheckPaths(dirpath);
                    var vpath = GetVPath(dirpath);
                    if (vpath != null) {
                        directories = Directory.GetDirectories(vpath).ToList();
                        directories.Sort(new DirectoryCreatedComparison(this));
                        directories = directories.Select(x => Path.Combine(dirpath, Path.GetFileName(x))).ToList();
                    } else {
                        directories = Directory.GetDirectories(Path.Combine(_projectPath, dirpath)).ToList();
                        directories.Sort(new DirectoryCreatedComparison(this));
                        directories = directories.Select(x => x.Remove(0, _projectPath.Length + 1)).ToList();
                    }
                }
                directories.Sort();
            } catch(DirectoryNotFoundException ex) {
                Debug.WriteLine("Unable to list directories in directory: " + ex.Message);
            }
            return directories;
        }

        public DateTime FileCreationTime(string file) {
            return File.GetCreationTime(ResolvePath(file));
        }

        public DateTime DirectoryCreationTime(string directory) {
            return Directory.GetCreationTime(ResolvePath(directory));
        }

        public DateTime DirectoryLastWriteTime(string dirpath) {
            return Directory.GetLastWriteTime(ResolvePath(dirpath));
        }

        public DateTime FileLastWriteTime(string filepath) {
            return File.GetLastWriteTime(ResolvePath(filepath));
        }

        public bool FileExists(string filepath) {
            var actualPath = ResolvePath(filepath);
            var exists = File.Exists(actualPath) || Directory.Exists(actualPath);
            return exists;
        }

        public void CreateFile(string path, string content) {
            File.WriteAllText(ResolvePath(path), content);
        }

        public void CreateFile(string path, byte[] content) {
            File.WriteAllBytes(ResolvePath(path), content);
        }

        public byte[] ReadBytes(string path) {
            return File.ReadAllBytes(ResolvePath(path));
        }

        public string RootName {
            get { return Path.GetFileName(_projectPath); }
        }

        public void OpenInExplorer(IProjectNode node) {
            Process.Start("explorer.exe", "\"" + ResolvePath(node.Path) + "\"");
        }

        static void CheckPaths(params string[] paths) {
            if (paths.Any(Path.IsPathRooted)) {
                throw new ProjectPathException("A path is rooted: " + string.Join(",",paths));
            }
            if (paths.Any(x => x.Contains(".."))) {
                throw new ProjectPathException("A path contains .. : " + string.Join(",",paths));
            }
        }

        static void DeleteDirectory(string target_dir) {
            var files = Directory.GetFiles(target_dir);
            var dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files) {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs) {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        string ResolvePath(string path) {
            var vpath = GetVPath(path);
            if (vpath != null) {
                return vpath;
            } else {
                return Path.Combine(_projectPath, path);
            }
        }

        string GetVPath(string dirpath) {
            var droot = "";
            var rest = "";
            if (dirpath.Contains(Path.DirectorySeparatorChar)) {
                droot = dirpath.Split(Path.DirectorySeparatorChar).First();
                rest = string.Join(Path.DirectorySeparatorChar.ToString(), dirpath.Split(Path.DirectorySeparatorChar).Skip(1));
            }
            string vpath = null;
            if (_virtualFolders.ContainsKey(dirpath)) {
                vpath = _virtualFolders[dirpath];
            } else if (_virtualFolders.ContainsKey(droot)) {
                vpath = Path.Combine(_virtualFolders[droot], rest);
            }
            return vpath;
        }
    }

    class DirectoryCreatedComparison : IComparer<string> {
        readonly IProjectIO _io;

        public DirectoryCreatedComparison(IProjectIO io) {
            _io = io;
        }

        public int Compare(string x, string y) {
            var xtime = _io.DirectoryCreationTime(x);
            var ytime = _io.DirectoryCreationTime(y);
            return xtime.CompareTo(ytime);
        }
    }

    class FileCreatedComparison : IComparer<string> {
        readonly IProjectIO _io;

        public FileCreatedComparison(IProjectIO io) {
            _io = io;
        }

        public int Compare(string x, string y) {
            var xtime = _io.FileCreationTime(x);
            var ytime = _io.FileCreationTime(y);

            return xtime.CompareTo(ytime);
        }
    }

    public class ProjectPathException : Exception {
        public ProjectPathException(string msg) : base(msg){}
    }

    public class VirtualFolderException : Exception { }

    public class FolderDeletedException : Exception { }

    public class RecursiveFolderException : Exception { }


}
