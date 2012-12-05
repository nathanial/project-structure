using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ProjectStructure;

namespace Project_Structure_Tests {
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class TestProject {
        Mock<IProjectIO> _io;
        Mock<INodeFactory> _nfac;

        const string OneNodeProjectName = "foo";
        const string OneNodeProjectText = @"
<Some-Project>
    <Virtual-Folder>C:\Sample</Virtual-Folder>
</Some-Project>
";
        const string EmptyProjectName = "bar";
        const string EmptyProjectText = @"
<Some-Project>
</Some-Project>
";

        [SetUpFixture]
        class SetupDirectories {
            [SetUp]
            public void CreateDirectories() {
                Directory.CreateDirectory(@"C:\Sample");
            }

            [TearDown]
            public void DeleteDirectories() {
                Directory.Delete(@"C:\Sample");
            }
        }

        [SetUp]
        public void Setup() {
            _io = new Mock<IProjectIO>();
            _nfac = new Mock<INodeFactory>();


        }

        [Test]
        public void Init_Loads_Virtual_Folders() {
            _io.Setup(x => x.CachedReadText(OneNodeProjectName))
               .Returns(OneNodeProjectText)
               .Verifiable();

            Scanning_RootFolder_Returns_Nothing();

            var folderMock = new Mock<IFolderNode>();
            folderMock.SetupGet(x => x.Path).Returns("Sample");
            _nfac.Setup(x => x.CreateFolderNode(@"Sample"))
                 .Returns(folderMock.Object)
                 .Verifiable();

            var project = new Project(OneNodeProjectName, _io.Object, _nfac.Object);
            _io.Verify();
            _io.Verify(x => x.AddVirtualFolder(@"C:\Sample"), Times.Once());
            _nfac.Verify();

            Assert.AreEqual(1, project.Children.Count);
            Assert.AreSame(folderMock.Object, project.Children.First());
        }

        [Test]
        public void AddVirtualFolder_Should_SaveToDoc() {
            _io.Setup(x => x.CachedReadText(EmptyProjectName))
               .Returns(EmptyProjectText)
               .Verifiable();

            _io.Setup(m => m.AddVirtualFolder(@"C:\Sample"))
               .Verifiable();

            Scanning_RootFolder_Returns_Nothing();

            var folderMock = new Mock<IFolderNode>();
            folderMock.SetupGet(x => x.Path).Returns("Sample");
            _nfac.Setup(x => x.CreateFolderNode("Sample"))
                 .Returns(folderMock.Object)
                 .Verifiable();

            _io.Setup(x => x.WriteFile(EmptyProjectName, @"
<Sample-Project>
    <Virtual-Folder>C:\Sample</Virtual-Folder>
</Sample-Project>
"));

            var project = new Project(EmptyProjectName, _io.Object, _nfac.Object);
            project.AddVirtualFolder(@"C:\Sample");

            _io.Verify();
            _nfac.Verify();

        }

        void Scanning_RootFolder_Returns_Nothing() {
            _io.Setup(x => x.ListDirectories("."))
               .Returns(new List<string>())
               .Verifiable();

            _io.Setup(x => x.ListFiles("."))
               .Returns(new List<string>())
               .Verifiable();
        }
    }
    // ReSharper restore InconsistentNaming

}
