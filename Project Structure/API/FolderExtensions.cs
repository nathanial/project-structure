using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectStructure.API {
    public static class ProjectNodeExtensions {
        public static IFolderNode GetProjectParent(this IProjectNode node) {
            var folder = node as IFolderNode;
            if (folder != null && folder.IsRootNode) {
                return folder;
            }
            return node.Parent.GetProjectParent();
        }
    }

    public static class FolderExtensions {
        public static string NewFolderName(this IFolderNode node) {
            var subdirs = node.Children.OfType<IFolderNode>().ToList();
            if (subdirs.Any(x => x.Name.StartsWith("New Folder"))) {
                var lastIndex = FindLastNewFolder(subdirs);
                return string.Format("New Folder ({0})", lastIndex + 1);
            }
            return "New Folder";
        }

        public static IList<IFolderNode> Subfolders(this IFolderNode node) {
            return node.Children.OfType<IFolderNode>().ToList();
        }

        public static IList<IFileNode> Files(this IFolderNode node) {
            return node.Children.OfType<IFileNode>().ToList();
        } 

        static int FindLastNewFolder(IEnumerable<IFolderNode> subdirs) {
            var regex = new Regex(@".* \((\d+)\)");
            var matches = subdirs.Where(x => regex.IsMatch(x.Name));
            var indexes = new List<int>();
            foreach (IFolderNode x in matches) {
                string value = regex.Match(x.Name).Groups[1].Value;
                indexes.Add(int.Parse(value));
            }
            return indexes.Count > 0 ? indexes.Max() : 0;
        }
    }
}
