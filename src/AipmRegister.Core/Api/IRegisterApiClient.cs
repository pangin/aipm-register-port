using AipmRegister.Core.Models;

namespace AipmRegister.Core.Api;

/// Talks to the DAWON cloud REST API. Replaces the original frmMain._0(string,
/// HttpMethod, string, bool) at line 914, which used HttpWebRequest + a hand-
/// written JSON quoter.
public interface IRegisterApiClient
{
    /// Exchanges the user-supplied 8-digit auth code for the Account record
    /// (user_id, pc_key, GPS coordinates). Server returns null/throws on
    /// expiration or invalid code.
    Task<Account?> GetPcKeyAsync(string pcTempKey, CancellationToken ct = default);

    /// Polls device registration status. Returns the parsed outcome plus the
    /// raw response body so callers can log it.
    Task<(ControlCheckOutcome Outcome, string RawResponse)> ControlCheckAsync(
        Account account,
        string deviceId,
        CancellationToken ct = default);
}
