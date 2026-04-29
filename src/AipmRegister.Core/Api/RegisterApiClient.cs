using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AipmRegister.Core.Models;
using AipmRegister.Core.Models.Json;

namespace AipmRegister.Core.Api;

public sealed class RegisterApiClient : IRegisterApiClient
{
    private readonly HttpClient _http;
    private readonly BackendOptions _backend;

    public RegisterApiClient(HttpClient http, BackendOptions backend)
    {
        _http = http;
        _backend = backend;
        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = _backend.ApiBase;
        }
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        if (!_http.DefaultRequestHeaders.Contains("X-HIT-Version"))
        {
            _http.DefaultRequestHeaders.Add("X-HIT-Version", _backend.HitVersion);
        }
    }

    public async Task<Account?> GetPcKeyAsync(string pcTempKey, CancellationToken ct = default)
    {
        if (pcTempKey.Length != 8 || !pcTempKey.All(char.IsDigit))
        {
            throw new ArgumentException("Auth code must be exactly 8 digits.", nameof(pcTempKey));
        }

        var body = new GetPcKeyRequest(new GetPcKeyAccount(pcTempKey));
        using var content = JsonContent.Create(body, AipmJsonContext.Default.GetPcKeyRequest);
        using var response = await _http.PostAsync("v1/accounts/post/getPckey", content, ct);

        var raw = await response.Content.ReadAsStringAsync(ct);
        if (response.StatusCode != HttpStatusCode.OK || raw.Contains("TIMEFAILED", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var parsed = JsonSerializer.Deserialize(raw, AipmJsonContext.Default.GetPcKeyResponse);
        return parsed?.Account;
    }

    public async Task<(ControlCheckOutcome Outcome, string RawResponse)> ControlCheckAsync(
        Account account,
        string deviceId,
        CancellationToken ct = default)
    {
        var body = new ControlCheckRequest(
            new ControlCheckAccount(account.UserId, account.PcKey),
            new[]
            {
                new ControlCheckDevice(
                    deviceId,
                    new ControlCheckMessage(
                        new[] { new ControlCheckEntry("/100/0/31") },
                        "r")),
            });

        using var content = JsonContent.Create(body, AipmJsonContext.Default.ControlCheckRequest);
        using var response = await _http.PostAsync("v1/devices/control/check", content, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        return (ClassifyOutcome(response.StatusCode, raw), raw);
    }

    public static ControlCheckOutcome ClassifyOutcome(HttpStatusCode status, string raw)
    {
        var compact = raw.Replace(" ", string.Empty).ToUpperInvariant();

        if (compact.Contains("TIMEFAILED"))         return ControlCheckOutcome.AuthCodeExpired;
        if (compact.Contains("STATUSERROR"))        return ControlCheckOutcome.AlreadyRegistered;
        if (compact.Contains("NOTREGISTERED"))      return ControlCheckOutcome.NotRegisteredExceededAttempts;

        if (status != HttpStatusCode.OK) return ControlCheckOutcome.UnknownError;

        var marker = "\"n\":\"/100/0/31\",\"sv\":";
        var idx = raw.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return ControlCheckOutcome.Pending;

        var rest = raw[(idx + marker.Length)..].TrimStart();
        if (rest.StartsWith("\"true\"") || rest.StartsWith("\"false\"") ||
            rest.StartsWith("true") || rest.StartsWith("false"))
        {
            return ControlCheckOutcome.Success;
        }
        return ControlCheckOutcome.Pending;
    }
}
