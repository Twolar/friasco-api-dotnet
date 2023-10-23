
namespace friasco_api_integration_tests.Controllers;

[TestFixture]
public class UsersControllerIntegrationTests : IntegrationTestBase
{
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _client = CreateHttpClient();
    }

    [TearDown]
    public void TearDown()
    {
        CleanUpHttpClient(_client);
    }

    [Test]
    public async Task GetAll_ReturnsOkResult()
    {
        // var response = await _client.GetAsync("/Users");
        // response.EnsureSuccessStatusCode();

        // var expectedResponseContent = "";

        // Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        // Assert.That(response.Content, Is.EqualTo(expectedResponseContent));
    }

    [Test]
    public async Task GetById_ReturnsOkResult()
    {
        // var id = 1;
        // var response = await _client.GetAsync($"/Users/{id}");
        // response.EnsureSuccessStatusCode();

        // var expectedResponseContent = "";

        // Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        //Assert.That(response.Content, Is.EqualTo(expectedResponseContent));
    }

    [Test]
    public async Task Create_ReturnsOkResult()
    {
        //var response = await _client.PostAsync("/Users", userJson);
        // response.EnsureSuccessStatusCode();

        // var expectedResponseContent = "";

        // Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        //Assert.That(response.Content, Is.EqualTo(expectedResponseContent));
    }

    [Test]
    public async Task Update_ReturnsOkResult()
    {
        // var response = await _client.PutAsync($"/Users/{id}", userJson);
        // response.EnsureSuccessStatusCode();

        // var expectedResponseContent = "";

        // Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        //Assert.That(response.Content, Is.EqualTo(expectedResponseContent));
    }

    [Test]
    public async Task Delete_ReturnsOkResult()
    {
        // int id = 1;
        // var response = await _client.DeleteAsync($"/Users/{id}");
        // response.EnsureSuccessStatusCode();

        // var expectedResponseContent = "";

        // Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        //Assert.That(response.Content, Is.EqualTo(expectedResponseContent));
    }
}

