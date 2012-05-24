using System;
using System.IO;
using ProjectStructure.API;
using ProjectStructure.Impl;

namespace Sample {
    class Program {
        static void Main(string[] args) {
            var builder = new ProjectBuilder();
            builder.Register(new CSVFileProvider());

            if(Directory.Exists("Foo")) {
                Directory.Delete("Foo",true);
            }
            if(File.Exists("Fundus.csv")) {
                File.Delete("Fundus.csv");
            }

            File.WriteAllText(".\\Project.xml", "<Project></Project>");
            var project = builder.Build(".\\Project.xml");
            var foo = project.CreateSubFolder("Foo");
            var fundus = project.CreateFile("Fundus.csv", "hats");
            var ffun = foo.CreateFile("InnerFoo.csv", "Hat");

            foreach (var folder in project.Subfolders()) {
                Console.WriteLine(folder);
            }

            foreach(var file in project.Files()) {
                Console.WriteLine(file);
            }

            foreach (var file in foo.Files()) {
                Console.WriteLine(file);
            }
         }
    }

    class CSVFileProvider : IFileProvider {
        public string[] Extensions {
            get { return new[]{".csv"}; }
        }

        public IFileNode Create(string path, IProjectIO io) {
            return new FileNode(io,path);
        }
    }

}
