using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PM.DTO;
using Xunit;

namespace PM.Integration.Tests;

public class TagsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TagsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTag_ShouldReturnCreatedTag()
    {
        var dto = new ModifyTagDTO { Name = "IntegrationTag" };

        var response = await _client.PostAsJsonAsync("/api/tags", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<TagDTO>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("IntegrationTag");
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTag_ShouldReturnTag_WhenExists()
    {
        // First create
        var createDto = new ModifyTagDTO { Name = "GetTagTest" };
        var created = await _client.PostAsJsonAsync("/api/tags", createDto);
        var tag = await created.Content.ReadFromJsonAsync<TagDTO>();

        // Now get
        var response = await _client.GetAsync($"/api/tags/{tag!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await response.Content.ReadFromJsonAsync<TagDTO>();
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("GetTagTest");
    }

    [Fact]
    public async Task GetTag_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var response = await _client.GetAsync("/api/tags/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListTags_ShouldReturnAllTags()
    {
        await _client.PostAsJsonAsync("/api/tags", new ModifyTagDTO { Name = "Tag1" });
        await _client.PostAsJsonAsync("/api/tags", new ModifyTagDTO { Name = "Tag2" });

        var response = await _client.GetAsync("/api/tags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tags = await response.Content.ReadFromJsonAsync<List<TagDTO>>();
        tags.Should().HaveCountGreaterOrEqualTo(2);
        tags.Select(t => t.Name).Should().Contain(new[] { "Tag1", "Tag2" });
    }

    [Fact]
    public async Task UpdateTag_ShouldReturnNoContent_WhenTagExists()
    {
        var createdResponse = await _client.PostAsJsonAsync("/api/tags", new ModifyTagDTO { Name = "OldName" });
        var tag = await createdResponse.Content.ReadFromJsonAsync<TagDTO>();

        var updateDto = new ModifyTagDTO { Name = "NewName" };
        var response = await _client.PutAsJsonAsync($"/api/tags/{tag!.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/tags/{tag.Id}");
        var updatedTag = await getResponse.Content.ReadFromJsonAsync<TagDTO>();
        updatedTag!.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateTag_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        var updateDto = new ModifyTagDTO { Name = "NonExistent" };
        var response = await _client.PutAsJsonAsync("/api/tags/999", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_ShouldReturnNoContent_WhenTagExists()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tags", new ModifyTagDTO { Name = "ToDelete" });
        var tag = await createResponse.Content.ReadFromJsonAsync<TagDTO>();

        var deleteResponse = await _client.DeleteAsync($"/api/tags/{tag!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/tags/{tag.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/tags/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
