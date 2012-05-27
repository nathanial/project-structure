using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectStructure {
    public interface INodeFactory {
        IFolderNode CreateFolderNode(string dirpath);
        IFileNode CreateFileNode(string file);
    }


    public interface IFileProvider {
        string[] Extensions { get; }
        IFileNode Create(string path, IProjectIO io);
    }

    public interface IProjectProvider {
        string[] Extensions { get; }
        IProject Create(string path, IProjectIO io, INodeFactory nfac);
    }

    public class NodeFactory : INodeFactory {
        readonly IProjectIO _io;
        readonly IList<IFileProvider> _providers = new List<IFileProvider>();

        public NodeFactory(IProjectIO io) {
            _io = io;
        }

        public IFolderNode CreateFolderNode(string dirpath) {
            return new FolderNode(_io, this, dirpath);
        }

        public IFileNode CreateFileNode(string file) {
            var p = FindProvider(file);
            if(p != null) {
                return p.Create(file,_io);
            }
            if(IgnoreUnknownFiles) {
                return null;
            }
            throw new Exception("Unknown file");
        }

        public void Register(IFileProvider provider) {
            _providers.Add(provider);
        }

        IFileProvider FindProvider(string file) {
            return _providers.FirstOrDefault(x => x.Extensions.Any(file.EndsWith));
        }

        public bool IgnoreUnknownFiles { get; set; }
    }

}
