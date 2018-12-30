using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Data.Nodes;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.SocketServer;
using Xunit;

namespace TinyCMS.Tests
{
    public class DataTests
    {
        [Fact]
        public void a_single_node_has_no_children()
        {
            // Arrange
            var site = new Site() { Id = "root" };

            // Act
            // no op

            // Assert
            site.Children.Should().BeEmpty();
        }

        [Fact]
        public void can_add_a_child_to_a_node()
        {
            // Arrange
            var site = new Site() { Id = "root" };

            // Act
            site.Add(new Page());

            // Assert
            site.Children.Count.Should().Be(1);
        }

        [Fact]
        public void can_add_a_child_to_a_node_even_when_children_is_null()
        {
            // Arrange
            var site = new Site() { Id = "root" };
            site.Children = null;

            // Act
            site.Add(new Page());

            // Assert
            site.Children.Count.Should().Be(1);
        }

        [Fact]
        public void can_add_a_child_to_a_node_by_type()
        {
            // Arrange
            var site = new Site() { Id = "root" };

            // Act
            site.Add<Page>();

            // Assert
            site.Children.Count.Should().Be(1);
        }

        [Fact]
        public void can_get_child_from_container()
        {
            // Arrange
            var site = new Site() { Id = "root" }
                .Add(new Page() { Name = "About", Id = "about" });

            var container = new Container(site);

            // Act
            var aboutPage = container.GetById("about") as Page;

            // Assert
            aboutPage.Name.Should().Be("About");
        }

        [Fact]
        public void get_non_existing_by_id_from_container_returns_null()
        {
            // Arrange
            var site = new Site() { Id = "root" };
            var container = new Container(site);

            // Act
            var aboutPage = container.GetById("about") as Page;

            // Assert
            aboutPage.Should().BeNull();
        }

        [Fact]
        public void can_get_relation_between_parent_and_child()
        {
            // Arrange
            var site = new Site() { Id = "root" }
                .Add(new Page() { Name = "Blog", Id = "blog" });

            var container = new Container(site);
            container.AddRelation(container.GetById("root"), container.GetById("blog"));

            // Act
            var relations = container.GetRelationsById("blog").ToList();

            // Assert
            relations.Should().NotBeEmpty();
        }

        [Fact]
        public void can_watch_for_property_changes()
        {
            // Arrange
            var site = new Site { Id = "root" };
            var container = new Container(site);
            var changedProperty = string.Empty;
            site.PropertyChanged += (sender, e) =>
            {
                changedProperty = e.PropertyName;
            };

            // Act
            site.Id = "new id";

            // Assert
            changedProperty.Should().Be("Id");
        }

        [Fact]
        public void can_watch_for_collection_changes()
        {
            // Arrange
            var site = new Site { Id = "root" };
            var container = new Container(site);
            int eventChildren = 0;

            site.Children.CollectionChanged += (sender, e) =>
            {
                eventChildren = ((ObservableCollection<INode>) sender).Count;
            };

            // Act
            site.Add(new Text());

            // Assert
            eventChildren.Should().Be(1);
        }

        [Fact]
        public void TestFactory()
        {
            // Arrange
            var factory = new NodeTypeFactory();

            // Act
            var newnode = factory.GetNew("text");

            // Assert
            Assert.Equal(newnode.GetType(), typeof(Text));
        }

        public class moving_nodes
        {
            [Fact]
            public void can_move_a_node_to_a_new_parent_node()
            {
                // Arrange
                var site = new Site() { Id = "root" }
                    .Add(new Page() { Name = "Blog", Id = "blog" })
                    .Add(new Page() { Name = "About", Id = "about" });

                var moveData = new MoveData
                {
                    OldParentId = "root",
                    ParentId = "blog",
                    Id = "about"
                };

                var moveRequest = new MoveRequest();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(moveData);
                moveRequest.JsonData = JObject.Parse(json);

                var container = new Container(site);

                // Act
                moveRequest.Move(container);

                // Assert
                var about = container.GetById("about");
                about.ParentId.Should().Be("blog");
                site.Children.Count.Should().Be(1);
            }

            private class MoveRequest : INodeRequest
            {
                public RequestTypeEnum RequestType => RequestTypeEnum.Move;

                public string Data { get; set; }

                public JObject JsonData { get; set; }
                public Dictionary<string, string> QueryString { get; set; }
            }
        }
    }
}
