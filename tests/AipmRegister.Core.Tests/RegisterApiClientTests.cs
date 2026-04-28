using AipmRegister.Core.Api;
using AipmRegister.Core.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace AipmRegister.Core.Tests;

public sealed class RegisterApiClientTests : IDisposable
{
    private readonly WireMockServer _mock;
    private readonly HttpClient _http;
    private readonly RegisterApiClient _client;

    public RegisterApiClientTests()
    {
        _mock = WireMockServer.Start();
        var backend = new BackendOptions(); // hostname/port only used for non-test code paths
        _http = new HttpClient { BaseAddress = new Uri(_mock.Url! + "/api/") };
        _client = new RegisterApiClient(_http, backend);
    }

    public void Dispose()
    {
        _mock.Stop();
        _http.Dispose();
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("abcdefgh")]
    public async Task GetPcKey_Throws_On_NonEightDigits(string code)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetPcKeyAsync(code));
    }

    [Fact]
    public async Task GetPcKey_Returns_Account_On_200()
    {
        _mock
            .Given(Request.Create().WithPath("/api/v1/accounts/post/getPckey").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(
                "{\"account\":{\"user_id\":\"u1\",\"pc_key\":\"k1\",\"pc_lati\":\"37.5\",\"pc_long\":\"127.0\"}}"));

        var account = await _client.GetPcKeyAsync("12345678");

        Assert.NotNull(account);
        Assert.Equal("u1",  account!.UserId);
        Assert.Equal("k1",  account.PcKey);
        Assert.Equal("37.5", account.Latitude);
        Assert.Equal("127.0", account.Longitude);
    }

    [Fact]
    public async Task GetPcKey_Returns_Null_On_TimeFailed()
    {
        _mock
            .Given(Request.Create().WithPath("/api/v1/accounts/post/getPckey").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"status\":\"TIMEFAILED\"}"));

        var account = await _client.GetPcKeyAsync("12345678");
        Assert.Null(account);
    }

    [Fact]
    public async Task ControlCheck_Posts_LwM2M_Path_And_Recognises_Success()
    {
        _mock
            .Given(Request.Create().WithPath("/api/v1/devices/control/check").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(
                "{\"e\":[{\"n\":\"/100/0/31\",\"sv\":\"true\"}]}"));

        var account = new Account("u1", "k1", "37.5", "127.0");
        var (outcome, raw) = await _client.ControlCheckAsync(account, "DAWONDNS-MODEL-AABBCC");

        Assert.Equal(ControlCheckOutcome.Success, outcome);
        Assert.Contains("/100/0/31", raw);
    }
}
