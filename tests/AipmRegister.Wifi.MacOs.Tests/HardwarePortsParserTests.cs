using AipmRegister.Wifi.MacOs;

namespace AipmRegister.Wifi.MacOs.Tests;

public sealed class HardwarePortsParserTests
{
    [Fact]
    public void Parse_EmptyInput_ReturnsEmpty()
        => Assert.Empty(HardwarePortsParser.Parse(string.Empty));

    [Fact]
    public void Parse_SingleWifiBlock_YieldsOneInterface()
    {
        const string output = """
Hardware Port: Ethernet
Device: en5
Ethernet Address: aa:bb:cc:dd:ee:ff

Hardware Port: Wi-Fi
Device: en0
Ethernet Address: 11:22:33:44:55:66
""";
        var parsed = HardwarePortsParser.Parse(output);
        Assert.Single(parsed);
        Assert.Equal("en0", parsed[0].Id);
        Assert.Equal("Wi-Fi (en0)", parsed[0].DisplayName);
    }

    [Fact]
    public void Parse_MultipleWifiBlocks_YieldsBothInDocumentOrder()
    {
        const string output = """
Hardware Port: Wi-Fi
Device: en0
Ethernet Address: 11:22:33:44:55:66

Hardware Port: USB-WiFi-AX
Device: en7
Ethernet Address: 22:33:44:55:66:77

Hardware Port: Wi-Fi
Device: en9
Ethernet Address: 33:44:55:66:77:88
""";
        var parsed = HardwarePortsParser.Parse(output);
        Assert.Equal(2, parsed.Count);
        Assert.Equal("en0", parsed[0].Id);
        Assert.Equal("en9", parsed[1].Id);
    }

    [Fact]
    public void Parse_SkipsWifiBlocksWithoutDeviceLine()
    {
        const string output = """
Hardware Port: Wi-Fi
Ethernet Address: 11:22:33:44:55:66
(missing Device line)
""";
        Assert.Empty(HardwarePortsParser.Parse(output));
    }

    [Fact]
    public void Parse_OnlyMatchesPortLabelContainingWiFi_CaseInsensitive()
    {
        const string output = """
Hardware Port: WI-FI
Device: en0

Hardware Port: Ethernet
Device: en1
""";
        var parsed = HardwarePortsParser.Parse(output);
        Assert.Single(parsed);
        Assert.Equal("en0", parsed[0].Id);
    }
}
