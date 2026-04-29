using AipmRegister.Core.Process;
using AipmRegister.Core.Wifi;
using AipmRegister.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Hosting.Tests;

public sealed class AddAipmWifiPlatformTests
{
    [Fact]
    public void Registers_IProcessRunner_AsSingleton_OnEveryPlatform()
    {
        var sp = BuildProvider();

        var a = sp.GetRequiredService<IProcessRunner>();
        var b = sp.GetRequiredService<IProcessRunner>();

        Assert.IsType<DefaultProcessRunner>(a);
        Assert.Same(a, b);
    }

    [Fact]
    public void Registers_IWifiInterfaceEnumerator_AsSingleton()
    {
        var sp = BuildProvider();

        var a = sp.GetRequiredService<IWifiInterfaceEnumerator>();
        var b = sp.GetRequiredService<IWifiInterfaceEnumerator>();

        Assert.NotNull(a);
        Assert.Same(a, b);
    }

    [Fact]
    public void Registers_IWifiAdapterFactory_AsSingleton()
    {
        var sp = BuildProvider();

        var a = sp.GetRequiredService<IWifiAdapterFactory>();
        var b = sp.GetRequiredService<IWifiAdapterFactory>();

        Assert.NotNull(a);
        Assert.Same(a, b);
    }

    [Fact]
    public void Factory_Create_CachesAdapterPerInterfaceId()
    {
        var sp = BuildProvider();
        var factory = sp.GetRequiredService<IWifiAdapterFactory>();

        var iface = new WifiInterface(Id: SampleId(0), DisplayName: "Test 0");
        var a = factory.Create(iface);
        var b = factory.Create(iface);

        // Same id → same adapter (Phase 2.3 of v1.3 contract).
        Assert.Same(a, b);
    }

    [Fact]
    public void Factory_Create_DifferentIds_ReturnDifferentAdapters()
    {
        var sp = BuildProvider();
        var factory = sp.GetRequiredService<IWifiAdapterFactory>();

        var a = factory.Create(new WifiInterface(Id: SampleId(0), DisplayName: "Test 0"));
        var b = factory.Create(new WifiInterface(Id: SampleId(1), DisplayName: "Test 1"));

        Assert.NotSame(a, b);
    }

    /// Windows' factory parses the interface id as a Guid (NativeWifi
    /// GUID), so the synthetic test ids have to be parseable on every
    /// platform — Linux/macOS treat them as opaque strings.
    private static string SampleId(int n) =>
        $"00000000-0000-0000-0000-{n:D12}";

    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAipmWifiPlatform();
        return services.BuildServiceProvider();
    }
}
