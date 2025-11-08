using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PM.API.Controllers;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Integration.Tests;

namespace PM.Integration.Controllers.Tests;

public class TagsControllerTests
{
    private readonly Mock<ITagService> _tagServiceMock;
    private readonly TagsController _controller;

    public TagsControllerTests()
    {
        _tagServiceMock = new Mock<ITagService>();
        _controller = new TagsController(_tagServiceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenTagExists()
    {
        var tagDto = TestTagFactory.CreateTagDto();
        _tagServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tagDto);

        var result = await _controller.Get(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(tagDto);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenTagDoesNotExist()
    {
        _tagServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((TagDTO?)null);

        var result = await _controller.Get(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task List_ReturnsAllTags()
    {
        var tags = new List<TagDTO>
        {
            TestTagFactory.CreateTagDto(1),
            TestTagFactory.CreateTagDto(2, "AnotherTag")
        };
        _tagServiceMock.Setup(s => s.ListAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tags);

        var result = await _controller.List();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task Create_ReturnsCreatedTag()
    {
        var dto = TestTagFactory.CreateModifyDto();
        var createdTag = TestTagFactory.CreateTagDto();

        _tagServiceMock.Setup(s => s.CreateAsync(dto.Name, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(createdTag);

        var result = await _controller.Create(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(createdTag);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdateSucceeds()
    {
        var dto = TestTagFactory.CreateModifyDto();

        _tagServiceMock.Setup(s => s.UpdateAsync(1, dto.Name, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTagDoesNotExist()
    {
        var dto = TestTagFactory.CreateModifyDto();

        _tagServiceMock.Setup(s => s.UpdateAsync(1, dto.Name, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        _tagServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTagDoesNotExist()
    {
        _tagServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NotFoundResult>();
    }
}
