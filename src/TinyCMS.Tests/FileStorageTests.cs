using System;
using System.IO;
using System.Text;
using FluentAssertions;
using NSubstitute;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Nodes;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.Storage;
using Xunit;

namespace TinyCMS.Tests
{
    public class FileStorageTests
    {
        public class json_storage
        {
            readonly Container container;
            readonly JsonStorageService jsonStorage;
            readonly MemoryStream fileStream;
            readonly IFile savedFile;

            // before each
            public json_storage()
            {
                var fileStorageService = Substitute.For<IFileStorageService>();
                fileStream = new MemoryStream();
                savedFile = Substitute.For<IFile>();
                savedFile.OpenWrite().Returns(fileStream);
                fileStorageService.RootDirectory.GetFile("").ReturnsForAnyArgs(savedFile);

                jsonStorage = new JsonStorageService(fileStorageService);
                container = new Container();
            }

            [Fact]
            public void loading_a_container_that_does_not_exist_returns_null()
            {
                // Arrange
                savedFile.Exists().Returns(false);

                // Act
                var result = jsonStorage.LoadContainer<Container>("");

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void save_container_writes_json_content_to_filestream()
            {
                // Arrange
                container.RootNode = new Page { Id = "bar" };

                // Act
                jsonStorage.SaveContainer(container, "");

                // Assert
                ReadStream(fileStream).Should().Contain("bar");
            }

            [Fact]
            public void save_container_deletes_old_file_if_it_exists()
            {
                // Arrange
                savedFile.Exists().Returns(true);

                // Act
                jsonStorage.SaveContainer(container, "foo");

                // Assert
                savedFile.Received().Delete();
            }
        }

        public class binary_storage
        {
            readonly Container container;
            readonly BinaryStorageService binaryStorage;
            readonly MemoryStream fileStream;
            readonly IFile savedFile;

            // before each
            public binary_storage()
            {
                var fileStorageService = Substitute.For<IFileStorageService>();
                fileStream = new MemoryStream();
                savedFile = Substitute.For<IFile>();
                savedFile.OpenWrite().Returns(fileStream);
                fileStorageService.RootDirectory.GetFile("").ReturnsForAnyArgs(savedFile);

                binaryStorage = new BinaryStorageService(fileStorageService);
                container = new Container();
            }

            [Fact]
            public void loading_a_container_that_does_not_exist_returns_null()
            {
                // Arrange
                savedFile.Exists().Returns(false);

                // Act
                var result = binaryStorage.LoadContainer<Container>("");

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void save_container_writes_json_content_to_filestream()
            {
                // Arrange
                container.RootNode = new Page { Id = "bar" };

                // Act
                binaryStorage.SaveContainer(container, "");

                // Assert
                ReadStream(fileStream).Should().Contain("bar");
            }

            [Fact(Skip = "this should probably be implemented")]
            public void save_container_deletes_old_file_if_it_exists()
            {
                // Arrange
                savedFile.Exists().Returns(true);

                // Act
                binaryStorage.SaveContainer(container, "foo");

                // Assert
                savedFile.Received().Delete();
            }
        }

        public class node_storage
        {
            readonly IStorageService fileStorageService;

            // before each
            public node_storage()
            {
                fileStorageService = Substitute.For<IStorageService>();
            }

            [Fact]
            public void loads_and_populates_a_container()
            {
                // Arrange
                var container = new Container(new Page());
                fileStorageService
                    .LoadContainer<IContainer>("")
                    .ReturnsForAnyArgs(container);

                var nodeStorage = new NodeFileStorage<IContainer>(fileStorageService);

                // Act
                nodeStorage.Load();

                // Assert
                container.RootNode.Id.Should().Be("root", because: "root id has been populated");
            }

            [Fact]
            public void generates_a_new_container_when_it_loads_null()
            {
                // Arrange
                fileStorageService
                    .LoadContainer<Container>("")
                    .ReturnsForAnyArgs(x => null);

                var nodeStorage = new NodeFileStorage<Container>(fileStorageService);

                // Act
                var container = nodeStorage.Load();

                // Assert
                container.RootNode.Id.Should().Be("root", because: "root id has been created and set");
            }

            [Fact]
            public void generates_a_new_container_when_it_cannot_be_loaded()
            {
                // Arrange
                fileStorageService
                    .LoadContainer<Container>("")
                    .ReturnsForAnyArgs(_ =>
                        throw new Exception());

                var nodeStorage = new NodeFileStorage<Container>(fileStorageService);

                // Act
                var container = nodeStorage.Load();

                // Assert
                container.RootNode.Id.Should().Be("root", because: "root id has been created and set");
            }
        }

        private static string ReadStream(MemoryStream stream)
        {
            return Encoding.ASCII.GetString(stream.ToArray());
        }
    }
}
