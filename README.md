# aipm-register-port

[![build](https://github.com/pangin/aipm-register-port/actions/workflows/build-release.yml/badge.svg)](https://github.com/pangin/aipm-register-port/actions/workflows/build-release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> Reverse engineered a Windows-only **.NET Framework 3.5 WinForms** IoT
> registration tool, then modernized it into a **.NET 10** code base with a
> testable **Core**, a **CLI**, and (in progress) a cross-platform
> **Avalonia GUI** that ships as **single-file binaries for Windows, Linux,
> and macOS**.

The original `AIPM_Register.exe` is a 241 KB tool that registers a DAWON IoT
device (e.g. a smart Wi-Fi infrared remote) to its cloud. It scans for the
device's hotspot, joins it, pushes the user's home Wi-Fi credentials over
plain TCP, and polls the cloud over HTTPS using LwM2M-style messaging. This
project reproduces every observable behavior on top of a modern, OS-agnostic
foundation.

---

## Why this project

| Original | This port |
|---|---|
| .NET Framework 3.5 (released **2007**) | .NET 10 (current LTS) |
| Windows only | Windows / Linux / macOS single-file binaries |
| One 2,432-line `frmMain.cs` (UI + HTTP + TCP + Wi-Fi) | `Core` library + `Cli` + `Gui` (Avalonia) |
| `HttpWebRequest` + hand-written JSON quoter | `HttpClient` + `System.Text.Json` |
| `MessageBox.Show` baked into business logic | `IUserNotifier` abstraction (CLI / GUI implement) |
| No tests | xUnit + WireMock.Net, **CI-verified on every push** |
| `wlanapi.dll` P/Invoke wired directly into the form | `IWifiAdapter` with per-OS implementations |
| Method/parameter names obfuscated to `_0` / `P_0` | Readable identifiers, types, and DI |

---

## How it was done

1. **Capture the binary.** Hashed the sample, recorded its PE/CLR layout in
   [`samples/SHA256.txt`](samples/SHA256.txt).
2. **Decompile.** `ilspycmd` exported the IL to a buildable C# project at
   [`analysis/decompiled-raw/`](analysis/decompiled-raw/).
3. **Map the surface.** Read every method, classified externals, sketched
   the actual workflow in
   [`analysis/notes.md`](analysis/notes.md) and the OS-abstraction matrix in
   [`analysis/api-surface.md`](analysis/api-surface.md).
4. **Rebuild on a modern foundation.** Dropped a fresh .NET 10 solution
   ([`AipmRegister.sln`](AipmRegister.sln)) and ported the workflow piece by
   piece, with each commit corresponding to a recovered concept.
5. **Cover the recovered logic.** xUnit tests pin the parts most likely to
   regress (response-classification, HTTP roundtrip, orchestrator flow).
6. **Ship across OSes.** GitHub Actions builds self-contained, single-file
   binaries on every tagged release.

The git history is intentionally curated as a portfolio artifact — the chain
of commits doubles as a porting journal.

---

## Workflow recovered from the original

```
sequenceDiagram
    participant U as User
    participant P as PC App
    participant C as DAWON Cloud
    participant D as IoT Device (AP)
    participant H as Home Router

    U->>P: 8-digit auth code (issued in mobile app)
    P->>C: POST getPckey
    C-->>P: user_id, pc_key, lat, lng (50min TTL)

    U->>P: home Wi-Fi credentials
    P->>P: scan APs, filter DAWON_IRBD_/DWD-
    P->>D: join device hotspot
    P->>D: TCP :5000 — settings JSON (ssid/pass/lat/lng/cloud URLs)
    Note over D,H: device joins H, registers to MQTT broker

    loop until success or 10x NOTREGISTERED
      P->>C: POST devices/control/check (LwM2M /100/0/31)
      C-->>P: sv=true|false / STATUSERROR / NOTREGISTERED
    end
    P-->>U: result
```

---

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│ AipmRegister.Cli      (System.CommandLine + Hosting)     │
│ ┌────────────────┐    ┌───────────────────────────────┐  │
│ │ ConsoleNotifier│    │ NoopWifiAdapter (placeholder) │  │
│ └────────────────┘    └───────────────────────────────┘  │
└──────────────────────┬───────────────────────────────────┘
                       │ depends on
                       ▼
┌──────────────────────────────────────────────────────────┐
│ AipmRegister.Core                                         │
│  - Models/        domain records (Account, DeviceSettings)│
│  - Api/           IRegisterApiClient + HttpClient impl    │
│  - Devices/       IDeviceTcpSender + TcpClient impl       │
│  - Wifi/          IWifiAdapter (per-OS implementations)   │
│  - Notification/  IUserNotifier (CLI/GUI implement)       │
│  - Orchestration/ RegistrationOrchestrator (the workflow) │
└──────────────────────────────────────────────────────────┘
```

Coming next: `AipmRegister.Gui` (Avalonia) and the Win32 `WlanClient`-backed
`IWifiAdapter` implementation.

---

## Try it

### Pre-built binaries
Download the latest [Release](https://github.com/pangin/aipm-register-port/releases) — three single-file binaries:
- `aipm-register-windows-x64.exe`
- `aipm-register-linux-x64`
- `aipm-register-macos-arm64`

### From source
```bash
git clone https://github.com/pangin/aipm-register-port.git
cd aipm-register-port
dotnet test                                  # run unit tests
dotnet run --project src/AipmRegister.Cli -- --help
```

### CLI usage
```
aipm-register \
  --auth-code 12345678 \
  --device-hotspot-ssid DAWON_IRBD_AABBCC \
  --home-ssid MyHomeWifi \
  --home-password 'p@ss'
```

For now the Wi-Fi steps are skipped (use the placeholder `NoopWifiAdapter`)
so the CLI assumes you have manually joined the device hotspot. Replace the
adapter with `WindowsWifiAdapter` (next commit) or `LinuxWifiAdapter` /
`MacOsWifiAdapter` (follow-ups) for fully automated registration.

---

## Status

| Phase | What | Status |
|---|---|---|
| A | Capture sample, decompile with ILSpy | ✅ |
| B | Class map, register flow, OS-abstraction plan | ✅ |
| C | .NET 10 solution skeleton (Core / CLI / Tests) | ✅ |
| D | Domain models with `System.Text.Json` | ✅ |
| E | API client, TCP sender, Notifier, Orchestrator | ✅ |
| F | xUnit + WireMock test suite (15 cases, CI-verified) | ✅ |
| G | GitHub Actions: multi-OS test + tagged release | ✅ |
| H | Win32 `IWifiAdapter` implementation | ⏳ |
| I | Avalonia GUI (cross-platform) | ⏳ |
| J | First tagged release `v0.1.0` | ⏳ |

---

## Disclaimers

- The original `AIPM_Register.exe` belongs to its respective vendor; this
  repository is a **personal portfolio reverse engineering exercise**. The
  binary is included only as a reproducibility aid; redistribute responsibly.
- All cloud endpoints, message formats, and protocol details are recovered
  from the existing binary's behavior. No information beyond what the binary
  itself reveals at rest.
- Security observations recorded in
  [`analysis/notes.md §7`](analysis/notes.md) are noted for educational
  purposes only.

---

## License

[MIT](LICENSE) © 2026 Kim Seong Uk
