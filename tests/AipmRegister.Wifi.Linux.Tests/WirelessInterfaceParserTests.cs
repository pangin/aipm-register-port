using AipmRegister.Wifi.Linux;

namespace AipmRegister.Wifi.Linux.Tests;

public sealed class WirelessInterfaceParserTests
{
    [Fact]
    public void ParseIwDev_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(WirelessInterfaceParser.ParseIwDev(string.Empty));
        Assert.Empty(WirelessInterfaceParser.ParseIwDev("   "));
    }

    [Fact]
    public void ParseIwDev_SingleInterface()
    {
        const string output = """
phy#0
    Interface wlan0
        ifindex 3
        addr 00:11:22:33:44:55
        type managed
""";
        var parsed = WirelessInterfaceParser.ParseIwDev(output);
        Assert.Equal(new[] { "wlan0" }, parsed);
    }

    [Fact]
    public void ParseIwDev_MultiRadio_ReturnsBothInOrder()
    {
        const string output = """
phy#0
    Interface wlan0
        ifindex 3
phy#1
    Interface wlx00aabbccddee
        ifindex 5
""";
        var parsed = WirelessInterfaceParser.ParseIwDev(output);
        Assert.Equal(new[] { "wlan0", "wlx00aabbccddee" }, parsed);
    }

    [Fact]
    public void ParseIwDev_DeduplicatesRepeatedInterfaceLines()
    {
        // Defensive: some `iw` invocations re-emit interface lines under
        // a "type AP" subsection. We dedupe to keep enumeration stable.
        const string output = """
Interface wlan0
    type managed
Interface wlan0
    type AP
""";
        Assert.Equal(new[] { "wlan0" }, WirelessInterfaceParser.ParseIwDev(output));
    }

    [Fact]
    public void ParseIwDev_IgnoresLinesWithoutInterfacePrefix()
    {
        const string output = """
phy#0
    addr 00:11:22:33:44:55
    Manage interface? no
""";
        Assert.Empty(WirelessInterfaceParser.ParseIwDev(output));
    }
}
