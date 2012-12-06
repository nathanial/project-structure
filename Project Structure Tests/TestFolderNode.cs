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
        }

        [Test]
        public void ToString_Returns_ShortName() {
            Assert.Fail();
        }

        [Test]
        public void AbsolutePath_Should_DelegateToIO() {
            Assert.Fail();
        }

        [Test]
        public void IfRoot_Name_ShouldReturn_RootName() {
            Assert.Fail();
        }

        [Test]
        public void IfNotRoot_Name_ShouldReturn_ShortDirectoryName() {
            Assert.Fail();
        }

        [Test]
        public void CreateSubFolder_Should_CreateNewDirectory() {
            Assert.Fail();
        }


        [Test]
        public void CreateSubFolder_Should_AddDirectoryToChildren() {
            Assert.Fail();
        }

        [Test]
        public void CreateSubFolder_Should_IgnoreDuplicates() {
            Assert.Fail();
        }

        [Test]
        public void CreateFile_Should_CreateNewFile() {
            Assert.Fail();
        }

        [Test]
        public void CreateFile_Should_AddFileToChildren() {
            Assert.Fail();
        }

        [Test]
        public void CreateFile_Should_IgnoreDuplicates() {
            Assert.Fail();
        }

        [Test]
        public void Delete_Should_RaisePreviewDelete() {
            Assert.Fail();
        }

        [Test]
        public void Delete_Should_RaiseDelete() {
            Assert.Fail();
        }

        [Test]
        public void Delete_Should_ThrowPreviewError() {
            Assert.Fail();
        }

        [Test]
        public void Delete_Should_SetIsDeletedFlag() {
            Assert.Fail();
        }

        [Test]
        public void Delete_Should_DeleteDirectory() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_RaisePreviewRename() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_RaiseRenamed() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_ThrowPreviewError() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_MoveFile() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_RemoveOldChild() {
            Assert.Fail();
        }

        [Test]
        public void Rename_Should_AddNewChild() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_RaisePreviewMoved() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_RaiseMoved() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_ThrowPreviewError() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_MoveFile() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_RemoveOldFile() {
            Assert.Fail();
        }

        [Test]
        public void Move_Should_AddNewFile() {
            Assert.Fail();
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
    

    }


    // ReSharper restore InconsistentNaming

}
