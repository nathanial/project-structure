using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectStructure {
    public class ProjectBuilder {
        readonly IList<IFileProvider> _fileProviders = new List<IFileProvider>();
        readonly IList<IProjectProvider> _projectProviders = new List<IProjectProvider>();

        public IProject Build(string path, bool newProject) {
            var io = new ProjectIO(Path.GetDirectoryName(path));
            var nfac = new NodeFactory(io);
            foreach (var p in _fileProviders) nfac.Register(p);

            var pprovider = FindProvider(path);
            if (newProject) {
                pprovider.CreateDefaultContents(Path.GetFileName(path), io, nfac);
            }
            return pprovider.Create(Path.GetFileName(path), io, nfac);
        }

        public void RegisterFileType(IFileProvider provider) {
            _fileProviders.Add(provider);
        }

        public void RegisterProjectType(IProjectProvider provider) {
            _projectProviders.Add(provider);
        }

        IProjectProvider FindProvider(string file) {
            return _projectProviders.FirstOrDefault(x => file.EndsWith(x.Extension));
        }
    }
}