using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ProjectStructure;

namespace Project_Structure_Tests {
    [TestFixture]
    public class TestNodeFactory {
        Mock<IProjectIO> _io;

        [SetUp]
        public void Setup() {
            _io = new Mock<IProjectIO>();
            _io.Setup(x => x.ListDirectories(It.IsAny<string>())).Returns(new List<string>());
            _io.Setup(x => x.ListFiles(It.IsAny<string>())).Returns(new List<string>());

        }

        [Test]
        public void CreateFolderNode_CreatesNode() {
            var nfac = new NodeFactory(_io.Object);

            var node = nfac.CreateFolderNode("Foo");
            Assert.IsNotNull(node);
        }

        [Test]
        public void CreateFileNode_LooksUpProviders_AndCreatesNode() {
            var nfac = new NodeFactory(_io.Object);
            var provider = new Mock<IFileProvider>();
            var newNode = new Mock<IFileNode>();
            provider.Setup(x => x.Extensions).Returns(new[] {".txt"});
            provider.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<IProjectIO>())).Returns(newNode.Object);
            nfac.Register(provider.Object);

            Assert.AreSame(newNode.Object, nfac.CreateFileNode("foo.txt"));
        }

    }
}
