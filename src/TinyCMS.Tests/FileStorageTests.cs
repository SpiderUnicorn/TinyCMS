using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Data.Nodes;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.SocketServer;
using TinyCMS.Storage;
using Xunit;

namespace TinyCMS.Tests
{
    public class FileStorageTests
    {
        public class json_storage
        {
            private readonly Container container;
            private readonly JsonStorageService jsonStorage;
            private readonly MemoryStream fileStream;
            private readonly IFile savedFile;

            // before each
            public json_storage()
            {
                savedFile = Substitute.For<IFile>();
                var fileStorageService = Substitute.For<IFileStorageService>();
                savedFile.Exists().Returns(true);
                fileStream = new MemoryStream();
                savedFile.OpenWrite().Returns(fileStream);

                fileStorageService.RootDirectory.GetFile("").ReturnsForAnyArgs(savedFile);

                jsonStorage = new JsonStorageService(fileStorageService);

                container = new Container();
            }

            [Fact]
            public void writes_json_content_to_filestream()
            {
                // Arrange
                container.RootNode = new Page { Id = "bar" };

                // Act
                jsonStorage.SaveContainer(container, "");

                // Assert
                ReadStream(fileStream).Should().Contain("bar");
            }

            [Fact]
            public void deletes_files_if_it_exists()
            {
                // Arrange
                savedFile.Exists().Returns(true);

                // Act
                jsonStorage.SaveContainer(container, "foo");

                // Assert
                savedFile.Received().Delete();
            }
        }

        public class node_storage
        {
            private readonly IStorageService fileStorageService;

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
