using System.Net;
using System.Net.Sockets;
using System.Text;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;

namespace AipmRegister.Core.Tests;

public sealed class DeviceTcpSenderTests
{
    [Fact]
    public async Task SendSettingsAsync_Sends_Legacy_Start_Line_Before_Settings_Json()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var server = AcceptOneAsync(listener);

        var sut = new DeviceTcpSender();
        var reply = await sut.SendSettingsAsync(
            IPAddress.Loopback.ToString(),
            endpoint.Port,
            SampleSettings());

        var (startLine, settingsJson) = await server.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("[DUT<-PC] START", startLine);
        Assert.Contains("\"model\":\"B530_W\"", settingsJson);
        Assert.Contains("\"respone\":\"OK\"", reply);
    }

    private static async Task<(string? StartLine, string? SettingsJson)> AcceptOneAsync(TcpListener listener)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var startLine = await reader.ReadLineAsync();
        var startResponse = Encoding.UTF8.GetBytes("START_OK\r\n");
        await stream.WriteAsync(startResponse);
        await stream.FlushAsync();

        var settingsJson = await reader.ReadLineAsync();

        var response = Encoding.UTF8.GetBytes("{\"respone\":\"OK\"}\r\n");
        await stream.WriteAsync(response);
        await stream.FlushAsync();

        return (startLine, settingsJson);
    }

    private static DeviceSettings SampleSettings() => new(
        Mac:               "AABBCC",
        ApiServerAddress:  "dwapi.dawonai.com",
        ApiServerPort:     "18443",
        MqttServerAddress: "dwmqtt.dawonai.com",
        MqttServerPort:    "8883",
        SslSupport:        "yes",
        HomeSsid:          "HOME_AP",
        HomePassword:      "homepass",
        UserId:            "u1",
        Company:           "DAWONDNS",
        Model:             "B530_W",
        Latitude:          "37",
        Longitude:         "127",
        Topic:             "dwd");
}
