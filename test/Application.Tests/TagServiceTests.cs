using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Services;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Utils.Tests;
using PM.DTO;
using Xunit;

namespace M.Application.Services.Tests
{
    public class TagServiceTests
    {
        private readonly Mock<ITagRepository> _repoMock;
        private readonly TagService _service;

        public TagServiceTests()
        {
            _repoMock = new Mock<ITagRepository>();
            _service = new TagService(_repoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Call_Repository_And_Return_DTO()
        {
            // Arrange
            var tagName = "RRSP";
            var tag = new Tag(tagName);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(tag);

            // Act
            var result = await _service.CreateAsync(tagName);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(tagName);
            _repoMock.Verify(r => r.CreateAsync(It.Is<Tag>(t => t.Name == tagName), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_TagDTO_When_Found()
        {
            // Arrange
            var tag = TestEntityFactory.CreateTag("TFSA");
            int id = tag.Id;
            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("TFSA");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);

            var result = await _service.GetByIdAsync(99);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ListAsync_Should_Return_All_TagDTOs()
        {
            // Arrange
            var tag1 = TestEntityFactory.CreateTag("RRSP");
            var tag2 = TestEntityFactory.CreateTag("TFSA");
            var tags = new List<Tag> { tag1, tag2 };
            _repoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tags);

            // Act
            var result = await _service.ListAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(t => t.Name).Should().Contain(new[] { "RRSP", "TFSA" });
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Tag_Exists()
        {
            // Arrange
            var tag = TestEntityFactory.CreateTag("OldName"); // original tag
            Tag? capturedTag = null; // will hold the argument passed to UpdateAsync

            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(tag);

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
                     .Callback<Tag, CancellationToken>((t, _) => capturedTag = t)
                     .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(1, "NewName");

            // Assert
            result.Should().BeTrue();

            capturedTag.Should().NotBeNull();
            capturedTag!.Id.Should().Be(1);
            capturedTag.Name.Should().Be("NewName");

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_Tag_NotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);

            var result = await _service.UpdateAsync(99, "NewName");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Call_Repo_Delete()
        {
            _repoMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _service.DeleteAsync(1);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
