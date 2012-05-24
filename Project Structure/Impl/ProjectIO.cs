using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ProjectStructure.API;

namespace ProjectStructure.Impl {

    class WatcherKey : Tuple<object, string> {
        public object Object { get; set; }
        public string FilePath { get; set; }

        public WatcherKey(object watcher, string filepath)
            : base(watcher, filepath) {
            Object = watcher;
            FilePath = filepath;
        }

    }

    interface IWatcher {
        void Check();
    }

    class DirectoryWatcher : IWatcher {
        readonly string _dirpath;
        readonly DirectoryChangeHandler _handler;
        readonly IProjectIO _io;

        List<string> _files = new List<string>();
        List<string> _directories = new List<string>();

        DateTime _lastChanged;

        public DirectoryWatcher(IProjectIO io, string dirpath, DirectoryChangeHandler handler) {
            _io = io;
            _dirpath = dirpath;
            _handler = handler;
            _lastChanged = _io.DirectoryLastWriteTime(dirpath);
            _files = _io.ListFiles(dirpath).ToList();
            _files.Sort();
            _directories = _io.ListDirectories(dirpath).ToList();
            _directories.Sort();

        }

        public void Check() {
            if (!_io.FileExists(_dirpath)) {
                _handler(new DirectoryChangeEventArgs { Deleted = true });
                return;
            }
            var latest = _io.DirectoryLastWriteTime(_dirpath);
            if (latest > _lastChanged) {
                _lastChanged = latest;
                var newFiles = _io.ListFiles(_dirpath).ToList();
                var newDirectories = _io.ListDirectories(_dirpath).ToList();

                newFiles.Sort();
                newDirectories.Sort();

                var addedFiles = AddedFiles(newFiles);
                var addedDirectories = AddedDirectories(newDirectories);

                _files = newFiles;
                _directories = newDirectories;
                _handler(new DirectoryChangeEventArgs {
                    AddedFiles = addedFiles,
                    AddedDirectories = addedDirectories
                });
            }
        }

        IList<string> AddedFiles(IEnumerable<string> newFiles) {
            return newFiles.Where(newFile => !_files.Contains(newFile)).ToList();
        }

        IList<string> AddedDirectories(IEnumerable<string> newDirectories) {
            return newDirectories.Where(newDir => !_directories.Contains(newDir)).ToList();
        }
    }

    class FileWatcher : IWatcher {
        readonly IProjectIO _io;
        readonly string _filepath;
        readonly FileChangeHandler _handler;

        DateTime _lastChanged;

        public FileWatcher(IProjectIO io, string filepath, FileChangeHandler handler) {
            _io = io;
            _filepath = filepath;
            _handler = handler;
            _lastChanged = _io.FileLastWriteTime(filepath);
        }

        public void Check() {
            if (!_io.FileExists(_filepath)) {
                _handler(new FileChangeEventArgs(FileChangeType.Deleted, _filepath));
            } else {
                var latest = _io.FileLastWriteTime(_filepath);
                if (latest > _lastChanged) {
                    _lastChanged = latest;
                    _handler(new FileChangeEventArgs(FileChangeType.Modified, _filepath));
                }
            }
        }
    }


    public class ProjectIO : IProjectIO {
        public event EventHandler<ProjectIOFileLoadedEventArgs> FileLoaded;

        readonly string _projectPath;

        readonly IDictionary<string, string> _cache = new Dictionary<string, string>();
        readonly IDictionary<WatcherKey, IWatcher> _watchers = new Dictionary<WatcherKey, IWatcher>();
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

        public void WatchFile(object watcher, string file, FileChangeHandler action) {
            _watchers.Add(new WatcherKey(watcher, file), new FileWatcher(this, file, action));
        }

        public void UnwatchFile(object watcher, string file) {
            if(!_watchers.Remove(new WatcherKey(watcher, file))) {
                throw new Exception("Unable to remove file watcher");
            }
        }

        public void WatchDirectory(object watcher, string dirpath, DirectoryChangeHandler action) {
            _watchers.Add(new WatcherKey(watcher, dirpath), new DirectoryWatcher(this, dirpath, action));
        }

        public void UnwatchDirectory(object watcher, string dirpath) {
            if(!_watchers.Remove(new WatcherKey(watcher, dirpath))) {
                throw new Exception("Unable to remove directory watcher");
            }
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

        public void RunWatchers() {
            foreach (var watcher in _watchers.Values.ToArray()) {
                watcher.Check();
            }
        }

        public void OpenInExplorer(IProjectNode node) {
            Process.Start("explorer.exe", "\"" + ResolvePath(node.Path) + "\"");
        }

        static void CheckPaths(params string[] paths) {
            if (paths.Any(Path.IsPathRooted)) {
                throw new ProjectPathException();
            }
            if (paths.Any(x => x.Contains(".."))) {
                throw new ProjectPathException();
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

    public class ProjectPathException : Exception { }

    public class VirtualFolderException : Exception { }


}
