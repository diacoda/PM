using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PM.API.Controllers;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Integration.Tests;

namespace PM.Integration.Controllers.Tests;

public class CashFlowControllerTests
{
    private readonly Mock<ICashFlowService> _cashFlowServiceMock;
    private readonly CashFlowController _controller;

    public CashFlowControllerTests()
    {
        _cashFlowServiceMock = new Mock<ICashFlowService>();
        _controller = new CashFlowController(_cashFlowServiceMock.Object);
    }
    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        _cashFlowServiceMock.Setup(s => s.DeleteCashFlowAsync(1, It.IsAny<CancellationToken>()));

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCashFlowDoesNotExist()
    {
        // Arrange
        _cashFlowServiceMock
            .Setup(s => s.DeleteCashFlowAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cash flow not found."));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Cash flow not found.");
        problemDetails.Detail.Should().Be("Cash flow not found.");
    }

}