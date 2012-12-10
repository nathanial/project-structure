using System;
using Moq;
using NUnit.Framework;
using ProjectStructure;

namespace Project_Structure_Tests {
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class TestFileNode {
        Mock<IProjectIO> _io;

        [SetUp]
        public void Setup() {
            _io = new Mock<IProjectIO>();
        }

        [Test]
        public void Test_Failure() {
            Assert.Fail();
        }

        [Test]
        public void ToString_Returns_FilePath() {
            var file = CreateFile("Foo");
            Assert.AreEqual("Foo",file.ToString());
        }

        [Test]
        public void AbsolutePath_Delegates_To_IO() {
            var file = CreateFile("Foo");
            var path = file.AbsolutePath;
            _io.Verify(x => x.GetAbsolutePath("Foo"),Times.Once());
        }

        [Test]
        public void Name_Returns_ShortNameOfPath() {
            var file = CreateFile("Foo\\Bar\\Baz");
            Assert.AreEqual("Baz", file.Name);
        }


        [Test]
        public void GetData_Should_GetFromFile() {
            var file = CreateFile("Foo\\Bar\\Baz");
            var d = file.Data;
            _io.Verify(x => x.CachedReadRaw("Foo\\Bar\\Baz"), Times.Once());
        }

        [Test]
        public void SetData_Should_RaisePreviewModified() {
            var preview = false;
            var file = CreateFile();
            file.PreviewModified += delegate {
                preview = true;
            };
            file.Data = new byte[]{0,1};
            
            Assert.IsTrue(preview);
        }

        [Test]
        public void SetData_Should_ThrowIfErrorOnPreview() {
            var file = CreateFile();
            var ex = new Exception("Throw me");
            file.PreviewModified += (s, args) => {
                args.Error = ex;
            };

            Assert.Throws<Exception>(() => file.Data = new byte[] {0, 1});
        }

        [Test]
        public void SetData_Should_RaiseModified() {
            var modified = false;
            var file = CreateFile();
            file.Modified += delegate {
                modified = true;
            };

            file.Data = new byte[] {0, 1};

            Assert.IsTrue(modified);
        }

        [Test]
        public void SetData_Should_WriteFile() {
            var file = CreateFile("Fun");
            file.Data = new byte[]{0,1};
            _io.Verify(x => x.WriteFile("Fun",new byte[]{0,1}), Times.Once());
        }

        [Test]
        public void Rename_Should_ThrowIfErrorOnPreview() {
            var file = CreateFile();
            var ex = new Exception("Throw me");
            file.PreviewRenamed += (s, args) => {
                args.Error = ex;
            };
            Assert.Throws<Exception>(() => file.Rename("Bob"));
        }

        [Test]
        public void Rename_Should_RaisePreviewRenamed() {
            var file = CreateFile();
            var flag = false;
            file.PreviewRenamed += delegate {
                flag = true;
            };

            file.Rename("Bob");

            Assert.IsTrue(flag);
        }

        [Test]
        public void Rename_Should_RaiseRenamed() {
            var file = CreateFile();
            var flag = false;
            file.Renamed += delegate {
                flag = true;
            };
            file.Rename("Bob");
            Assert.IsTrue(flag);
        }

        [Test]
        public void Rename_Should_ChangeName() {
            var file = CreateFile();
            file.Rename("Bob");
            Assert.AreEqual("Bob",file.Name);
        }

        [Test]
        public void Rename_Should_MoveFile() {
            var file = CreateFile("Foo\\Bar");
            file.Rename("Bob");
            _io.Verify(x=> x.Move("Foo\\Bar", "Foo\\Bob"), Times.Once());
        }

        [Test]
        public void Rename_Should_RenameWithoutMovingFolders() {
            var file = CreateFile("Foo");
            file.Rename("Bob");
            _io.Verify(x => x.Move("Foo", "Bob"), Times.Once());
        }

        [Test]
        public void Move_Should_RaisePreviewMoved() {
            var raised = false;
            var file = CreateFile();
            file.PreviewMoved += delegate {
                raised = true;
            };
            file.Move("Bob");
            Assert.IsTrue(raised);
        }

        [Test]
        public void Move_Should_RaiseMoved() {
            var raised = false;
            var file = CreateFile();
            file.Moved += delegate {
                raised = true;
            };
            file.Move("Bob");
            Assert.IsTrue(raised);
        }

        [Test]
        public void Move_Should_ThrowIfErrorOnPreview() {
            var file = CreateFile();
            var ex = new Exception("oops");
            file.PreviewMoved += (s,args)=> {
                args.Error = ex;
            };

            Assert.Throws<Exception>(() => file.Move("Bob"));
        }

        [Test]
        public void Move_Should_MoveFile() {
            var file = CreateFile("Foo\\Bar");
            file.Move("Bob");
            _io.Verify(x => x.Move("Foo\\Bar","Bob\\Bar"),Times.Once());
        }

        [Test]
        public void Delete_Should_RaisePreviewDeleted() {
            var flag = false;
            var file = CreateFile();
            file.PreviewDeleted += delegate {
                flag = true;
            };
            file.Delete();
            Assert.IsTrue(flag);
        }

        [Test]
        public void Delete_Should_RaiseDeleted() {
            var flag = false;
            var file = CreateFile();
            file.Deleted += delegate {
                flag = true;
            };
            file.Delete();
            Assert.IsTrue(flag);
        }

        [Test]
        public void Delete_Should_ThrowIfPreviewHasError() {
            var file = CreateFile();
            var ex = new Exception("oops");
            file.PreviewDeleted += (s, args) => {
                args.Error = ex;
            };
            Assert.Throws<Exception>(file.Delete);
        }

        [Test]
        public void Delete_Should_Set_Deleted_Flag() {
            var file = CreateFile();
            Assert.IsFalse(file.IsDeleted);
            file.Delete();
            Assert.IsTrue(file.IsDeleted);
        }

        [Test]
        public void Delete_Should_DeleteFile() {
            var file = CreateFile("Not me");
            file.Delete();
            _io.Verify(x =>x.Delete("Not me"));
        }

        [Test]
        public void Children_ShouldBe_Null() {
            var file = CreateFile();
            Assert.IsNull(file.Children);
        }

        [Test]
        public void OpenInExplorer_ShouldDelegateTo_IO() {
            var file = CreateFile();
            file.OpenInExplorer();
            _io.Verify(x=> x.OpenInExplorer(file), Times.Once());
        }

        [Test]
        public void Parent_IsGettable_And_IsSettable() {
            var file = CreateFile();
            var o = new Mock<IProjectNode>().Object;
            file.Parent = o;
            Assert.AreSame(o, file.Parent);
        }

        FileNode CreateFile() {
            return CreateFile("Foo");
        }

        FileNode CreateFile(string name) {
            return new FileNode(_io.Object,name);
        }
    }
    // ReSharper restore InconsistentNaming

}
