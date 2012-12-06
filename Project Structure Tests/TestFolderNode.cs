using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ProjectStructure;

namespace Project_Structure_Tests {
    // ReSharper disable InconsistentNaming

    [TestFixture]
    public class TestFolderNode {
        Mock<IProjectIO> _io;
        Mock<INodeFactory> _nfac;

        [SetUp]
        public void Setup() {
            _io = new Mock<IProjectIO>();
            _nfac = new Mock<INodeFactory>();

            _io.Setup(x => x.ListDirectories(It.IsAny<string>())).Returns(new List<string>());
            _io.Setup(x => x.ListFiles(It.IsAny<string>())).Returns(new List<string>());

            _nfac.Setup(x => x.CreateFolderNode(It.IsAny<string>())).Returns<string>(x => CreateFolder(x));
            _nfac.Setup(x => x.CreateFileNode(It.IsAny<string>())).Returns<string>(x => new FileNode(_io.Object, x));
        }

        [Test]
        public void ToString_Returns_ShortName() {
            var folder = CreateFolder("Foo\\Bar");
            Assert.AreEqual("Bar", folder.ToString());
        }

        [Test]
        public void AbsolutePath_Should_DelegateToIO() {
            _io.Setup(x => x.GetAbsolutePath("Foo\\Bar")).Returns("Baz");
            var folder = CreateFolder("Foo\\Bar");
            Assert.AreEqual("Baz", folder.AbsolutePath);
        }

        [Test]
        public void IfRoot_Name_ShouldReturn_RootName() {
            _io.SetupGet(x => x.RootName).Returns("Baz");
            var folder = CreateFolder("Foo", true);
            Assert.AreEqual("Baz", folder.Name);
        }

        [Test]
        public void IfNotRoot_Name_ShouldReturn_ShortDirectoryName() {
            var folder = CreateFolder(@"Fun\Bob");
            Assert.AreEqual("Bob", folder.Name);
        }

        [Test]
        public void CreateSubFolder_Should_CreateNewDirectory() {
            var folder = CreateFolder("Foo");
            folder.CreateSubFolder("Bob");
            _io.Verify(x => x.CreateDirectory("Foo\\Bob"), Times.Once());
        }

        [Test]
        public void CreateSubFolder_Should_AddDirectoryToChildren() {
            var folder = CreateFolder("Foo");
            folder.CreateSubFolder("Bob");
            Assert.AreEqual(1, folder.Children.Count);
            Assert.AreEqual("Bob", folder.Children[0].Name);
        }

        [Test]
        public void CreateSubFolder_Should_IgnoreDuplicates() {
            var folder = CreateFolder("Foo");
            folder.CreateSubFolder("Bob");
            folder.CreateSubFolder("Bob");
            Assert.AreEqual(1, folder.Children.Count);
            Assert.AreEqual("Bob", folder.Children[0].Name);
        }

        [Test]
        public void CreateFile_Should_CreateNewFile() {
            var folder = CreateFolder("Foo");
            folder.CreateFile("Data", "<Hello>");
            _io.Verify(x => x.CreateFile("Foo\\Data", "<Hello>"), Times.Once());
        }

        [Test]
        public void CreateFile_Should_AddFileToChildren() {
            var folder = CreateFolder("Foo");
            folder.CreateFile("Data", "<Hello>");
            Assert.AreEqual(1, folder.Children.Count);
            Assert.AreEqual("Data", folder.Children[0].Name);
        }

        [Test]
        public void CreateFile_Should_IgnoreDuplicates() {
            var folder = CreateFolder("Foo");
            folder.CreateFile("Data", "<Hello>");
            folder.CreateFile("Data", "<Goodbye>");

            Assert.AreEqual(1, folder.Children.Count);
            Assert.AreEqual("Data", folder.Children[0].Name);
        }

        [Test]
        public void Delete_Should_RaisePreviewDelete() {
            var raised = false;
            var folder = CreateFolder("Foo");
            folder.PreviewDeleted += delegate {
                raised = true;
            };
            folder.Delete();
            Assert.IsTrue(raised);
        }

        [Test]
        public void Delete_Should_RaiseDelete() {
            var raised = false;
            var folder = CreateFolder("Foo");
            folder.Deleted += delegate {
                raised = true;
            };
            folder.Delete();
            Assert.IsTrue(raised);
        }

        [Test]
        public void Delete_Should_ThrowPreviewError() {
            var folder = CreateFolder("Foo");
            var ex = new Exception("oops");
            folder.PreviewDeleted += (s, args) => {
                args.Error = ex;
            };
            Assert.Throws<Exception>(folder.Delete);
        }

        [Test]
        public void Delete_Should_SetIsDeletedFlag() {
            var folder = CreateFolder("Foo");
            Assert.IsFalse(folder.IsDeleted);
            folder.Delete();
            Assert.IsTrue(folder.IsDeleted);
        }

        [Test]
        public void Delete_Should_DeleteDirectory() {
            var folder = CreateFolder("Foo\\Bar");
            folder.Delete();
            _io.Verify(x => x.Delete("Foo\\Bar"), Times.Once());
        }

        [Test]
        public void Rename_Should_RaisePreviewRename() {
            var raised = false;
            var folder = CreateFolder("Foo");
            folder.PreviewRenamed += delegate {
                raised = true;
            };
            folder.Rename("Bob");
            Assert.IsTrue(raised);
        }

        [Test]
        public void Rename_Should_RaiseRenamed() {
            var raised = false;
            var folder = CreateFolder("Foo");
            folder.Renamed += delegate {
                raised = true;
            };
            folder.Rename("Bob");
            Assert.IsTrue(raised);
        }

        [Test]
        public void Rename_Should_ThrowPreviewError() {
            var folder = CreateFolder("Foo");
            var ex = new Exception("oops");
            folder.PreviewRenamed += (s, args) => {
                args.Error = ex;
            };
            Assert.Throws<Exception>(() => folder.Rename("Bob"));
        }

        [Test]
        public void Rename_Should_MoveFolder() {
            var folder = CreateFolder("Bob\\Foo");
            folder.Rename("Hat");
            _io.Verify(x => x.Move("Bob\\Foo", "Bob\\Hat"),Times.Once());
        }

        [Test]
        public void Rename_Should_UpdateChildPaths() {
            var folder = CreateFolder("Foo");
            var file = folder.CreateFile("Bar","<Hello>");
            Assert.AreEqual("Foo\\Bar", file.Path);

            folder.Rename("Baz");

            Assert.AreEqual("Baz\\Bar", file.Path);
        }

        [Test]
        public void Move_Should_RaisePreviewMoved() {
            var raised = false;
            var folder = CreateFolder("Foo");

            folder.PreviewMoved += delegate {
                raised = true;
            };

            folder.Move("There");

            Assert.IsTrue(raised);
        }

        [Test]
        public void Move_Should_RaiseMoved() {
            var raised = false;
            var folder = CreateFolder("Foo");
            folder.Moved += delegate {
                raised = true;
            };
            folder.Move("There");
            Assert.IsTrue(raised);
        }

        [Test]
        public void Move_Should_ThrowPreviewError() {
            var folder = CreateFolder("Foo");
            var ex = new Exception("oops");
            folder.PreviewMoved += (s,args)=> {
                args.Error = ex;
            };
            Assert.Throws<Exception>(() => {
                folder.Move("Bar");
            });
        }

        [Test]
        public void Move_Should_MoveFolder() {
            var folder = CreateFolder("Foo\\Bar");
            folder.Move("Baz");
            _io.Verify(x => x.Move("Foo\\Bar", "Baz\\Bar"), Times.Once());
        }

        [Test]
        public void Move_Should_UpdateChildPaths() {
            var folder = CreateFolder("Foo");
            var c1 = folder.CreateFile("C1", "<Hello>");
            var c2 = folder.CreateSubFolder("C2");
            
            Assert.AreEqual(2, folder.Children.Count);
            Assert.AreSame(c1, folder.Children[0]);
            Assert.AreSame(c2, folder.Children[1]);
            Assert.AreEqual("Foo\\C1", c1.Path);
            Assert.AreEqual("Foo\\C2", c2.Path);

            folder.Move("Baz");

            Assert.AreEqual("Baz\\Foo\\C1", c1.Path);
            Assert.AreEqual("Baz\\Foo\\C2", c2.Path);
        }

        [Test]
        public void Refresh_Should_ReloadDirectories() {
            Assert.Fail();
        }

        [Test]
        public void Refresh_Should_ReloadFiles() {
            Assert.Fail();
        }

        [Test]
        public void Path_Should_IOPath() {
            Assert.Fail();
        }

        [Test]
        public void SetParent_Should_Throw_IfNodeIsRoot() {
            Assert.Fail();
        }

        [Test]
        public void OpenInExplorer_Should_DelegateToIO() {
            Assert.Fail();
        }

        [Test]
        public void AddingToChildren_ShouldMoveFilesInOtherFolders() {
            Assert.Fail();
        }

        [Test]
        public void AddingToChildren_ShouldSetParentOfChild() {
            Assert.Fail();
        }

        [Test]
        public void DeletingChild_Should_RemoveFromChildren() {
            Assert.Fail();
        }

        [Test]
        public void RemovingChild_Should_UnsetParent() {
            Assert.Fail();
        }

        [Test]
        public void RemovingChild_Should_UnsetDeletedHandler() {
            Assert.Fail();
        }

        FolderNode CreateFolder(string path, bool isRoot = false) {
            return new FolderNode(_io.Object, _nfac.Object, path, isRoot);
        }

    }


    // ReSharper restore InconsistentNaming

}
