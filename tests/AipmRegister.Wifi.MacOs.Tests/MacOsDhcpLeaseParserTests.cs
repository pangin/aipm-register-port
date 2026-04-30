using AipmRegister.Wifi.MacOs;

namespace AipmRegister.Wifi.MacOs.Tests;

public sealed class MacOsDhcpLeaseParserTests
{
    [Fact]
    public void ParseRouter_Returns_First_Router_From_Ipconfig_Getpacket_Output()
    {
        const string output = """
op = BOOTREPLY
yiaddr = 192.168.8.24
options:
router (ip_mult): {192.168.8.1, 192.168.8.254}
domain_name_server (ip_mult): {192.168.8.1}
end (none):
""";

        Assert.Equal("192.168.8.1", MacOsDhcpLeaseParser.ParseRouter(output));
    }

    [Fact]
    public void ParseRouter_ReturnsNull_When_Router_Option_Is_Missing()
    {
        Assert.Null(MacOsDhcpLeaseParser.ParseRouter("yiaddr = 192.168.8.24"));
    }
}
