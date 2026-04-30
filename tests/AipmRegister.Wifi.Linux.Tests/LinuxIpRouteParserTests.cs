using AipmRegister.Wifi.Linux;

namespace AipmRegister.Wifi.Linux.Tests;

public sealed class LinuxIpRouteParserTests
{
    [Theory]
    [InlineData("default via 192.168.4.1 dev wlan0 proto dhcp metric 600", "192.168.4.1")]
    [InlineData("default via 10.0.0.1 dev wlp3s0",                          "10.0.0.1")]
    [InlineData("default via 172.16.0.1 dev wlan0 onlink",                  "172.16.0.1")]
    public void ParseDefaultGateway_PicksTheViaIpAfterDefaultRoute(string raw, string expected)
        => Assert.Equal(expected, LinuxIpRouteParser.ParseDefaultGateway(raw));

    [Fact]
    public void ParseDefaultGateway_ReturnsNull_OnEmptyOrWhitespace()
    {
        Assert.Null(LinuxIpRouteParser.ParseDefaultGateway(string.Empty));
        Assert.Null(LinuxIpRouteParser.ParseDefaultGateway("   "));
    }

    [Fact]
    public void ParseDefaultGateway_ReturnsNull_WhenNoDefaultLine()
        => Assert.Null(LinuxIpRouteParser.ParseDefaultGateway(
            "192.168.4.0/24 dev wlan0 proto kernel scope link src 192.168.4.42"));

    [Fact]
    public void ParseDefaultGateway_ReturnsNull_WhenViaIsMalformed()
        => Assert.Null(LinuxIpRouteParser.ParseDefaultGateway("default via not.an.ip dev wlan0"));

    [Fact]
    public void ParseDefaultGateway_HandlesMultiLineWithLeadingNoise()
    {
        const string raw = """
some-other-line
default via 192.168.8.1 dev wlan0 proto dhcp metric 600
""";
        Assert.Equal("192.168.8.1", LinuxIpRouteParser.ParseDefaultGateway(raw));
    }
}
