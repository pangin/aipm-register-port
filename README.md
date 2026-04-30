# aipm-register-port

[![build](https://github.com/pangin/aipm-register-port/actions/workflows/build-release.yml/badge.svg)](https://github.com/pangin/aipm-register-port/actions/workflows/build-release.yml)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)

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

The git history is intentionally curated so each commit corresponds to a
recovered concept — it doubles as a porting journal.

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

Avalonia GUI is structured as a **5-step wizard** that mirrors the
original Windows app's flow:

```
Welcome → 1/5 Wi-Fi picker → 2/5 auth code → 3/5 product picker
        → 4/5 device picker → 5/5 progress + result
```

ViewModels: Welcome / WifiPicker / AuthCode / ProductPicker / DevicePicker
/ Registering, each backed by a matching `*.axaml`. Korean and English
labels live in `Localization/Strings.cs` and toggle via the header
button. `ProductCatalog` carries all 15 SKUs recovered from the
original (icons rendered as `StreamGeometry` paths, no PNGs).

---

## Try it

### Pre-built binaries
Download the latest [Release](https://github.com/pangin/aipm-register-port/releases) — **18 artifacts** (3 form factors × 6 RIDs):

| Form factor | win-x64 | win-arm64 | linux-x64 | linux-arm64 | osx-arm64 | osx-x64 |
|---|---|---|---|---|---|---|
| **CLI self-contained** | ✅ `.exe` | ✅ `.exe` | ✅ ELF | ✅ ELF | ✅ Mach-O | ✅ Mach-O |
| **CLI Native AOT** ⚡   | ✅ `.exe` | ✅ `.exe` | ✅ ELF | ✅ ELF | ✅ Mach-O | — (cross-build) |
| **GUI** (Avalonia)     | ✅ `.exe` | ✅ `.exe` | ✅ ELF | ✅ ELF | ✅ `.app.zip` | ✅ `.app.zip` |

Naming pattern:

```
aipm-register-{cli,cli-aot,gui}-{windows-x64.exe,
                                  windows-arm64.exe,
                                  linux-x64,
                                  linux-arm64,
                                  macos-arm64[.app.zip],
                                  macos-x64[.app.zip]}
```

Native AOT CLI builds are **~12 MB** with ~20 ms cold start (vs the
~70 MB / ~300 ms self-contained variant). Each RID is built on a
native runner — no cross-compile gymnastics, link toolchain matches
the target.

### From source — step-by-step

#### Common (all OSes)
```bash
git clone https://github.com/pangin/aipm-register-port.git
cd aipm-register-port
dotnet test
dotnet run --project src/AipmRegister.Cli            # interactive CLI verifier
dotnet run --project src/AipmRegister.Cli -- --help
dotnet run --project src/AipmRegister.Gui            # GUI
```

For tagged release-style binaries, see the per-OS publish recipes below.

#### Windows

```powershell
# 1. Prereqs
winget install Microsoft.DotNet.SDK.10
winget install GitHub.cli            # only if you want to mint releases
winget install Git.Git
# Visual Studio Build Tools 2022+ with the "Desktop development with C++"
# workload — required for Native AOT (link.exe + libucrt). Optional unless
# you intend to publish AOT locally.

# 2. Build & test
git clone https://github.com/pangin/aipm-register-port.git
cd aipm-register-port
dotnet build
dotnet test

# 3. Run
dotnet run --project src\AipmRegister.Gui

# 4. Publish self-contained
dotnet publish src\AipmRegister.Cli -c Release -r win-x64 `
  --self-contained -p:PublishSingleFile=true -o out\cli
dotnet publish src\AipmRegister.Gui -c Release -r win-x64 `
  --self-contained -p:PublishSingleFile=true -o out\gui

# 5. Publish Native AOT (needs Build Tools)
dotnet publish src\AipmRegister.Cli -c Release -r win-x64 `
  -p:PublishAot=true -p:InvariantGlobalization=true -o out\cli-aot
```

Wi-Fi automation uses `wlanapi.dll` via the `ManagedNativeWifi` package — no
extra setup. Run as a regular user.

#### Linux (Ubuntu / Debian)

```bash
# 1. Prereqs
sudo apt update
sudo apt install -y dotnet-sdk-10.0 git wpasupplicant
# wpa_supplicant must actually be running for the Wi-Fi adapter to do its
# job. On NetworkManager systems it already is — `systemctl status wpa_supplicant`
# to confirm.

# 2. Permission for the wpa_supplicant control socket (one-time)
sudo usermod -aG netdev $USER
# log out and back in for the group change to take effect
# alternatives: run with sudo, or `setcap cap_net_admin+ep` on the binary
# see docs/linux-wifi-permissions.md for the full menu.

# 3. Build & test
git clone https://github.com/pangin/aipm-register-port.git
cd aipm-register-port
dotnet build
dotnet test

# 4. Run
dotnet run --project src/AipmRegister.Gui          # Avalonia GUI
# or (CLI):
dotnet run --project src/AipmRegister.Cli -- --help

# 5. Publish self-contained
dotnet publish src/AipmRegister.Cli -c Release -r linux-x64 \
  --self-contained -p:PublishSingleFile=true -o out/cli
dotnet publish src/AipmRegister.Gui -c Release -r linux-x64 \
  --self-contained -p:PublishSingleFile=true -o out/gui

# 6. Publish Native AOT (no extra prereqs — clang+ld already on most distros)
dotnet publish src/AipmRegister.Cli -c Release -r linux-x64 \
  -p:PublishAot=true -p:InvariantGlobalization=true -o out/cli-aot
```

If the Wi-Fi adapter throws "control socket not found", `wpa_supplicant`
isn't running for that interface — see [docs/linux-wifi-permissions.md](docs/linux-wifi-permissions.md).

#### macOS (Apple Silicon — works on Intel too with `osx-x64`)

```bash
# 1. Prereqs
brew install --cask dotnet-sdk           # or download the .pkg from microsoft.com
brew install git gh                      # gh only if you want to mint releases
xcode-select --install                   # CLI dev tools (clang, ld) for AOT

# 2. Build & test
git clone https://github.com/pangin/aipm-register-port.git
cd aipm-register-port
dotnet build
dotnet test

# 3. Run
dotnet run --project src/AipmRegister.Gui

# 4. Publish self-contained
dotnet publish src/AipmRegister.Cli -c Release -r osx-arm64 \
  --self-contained -p:PublishSingleFile=true -o out/cli
dotnet publish src/AipmRegister.Gui -c Release -r osx-arm64 \
  --self-contained -p:PublishSingleFile=true -o out/gui

# 5. Bundle the GUI into a real .app (CI does this automatically)
BUNDLE=out/gui/AipmRegister.app
mkdir -p "$BUNDLE/Contents/MacOS" "$BUNDLE/Contents/Resources"
cp out/gui/AipmRegister.Gui "$BUNDLE/Contents/MacOS/AipmRegister"
chmod +x "$BUNDLE/Contents/MacOS/AipmRegister"
# minimal Info.plist — see .github/workflows/build-release.yml for the
# version that ships with NSLocationUsageDescription.

# 6. Publish Native AOT
dotnet publish src/AipmRegister.Cli -c Release -r osx-arm64 \
  -p:PublishAot=true -p:InvariantGlobalization=true -o out/cli-aot
```

The first scan asks for Location permission (macOS 14+ needs it for SSID
visibility). Approve once and the rest of the flow runs without prompts.

### CLI usage
Interactive mode mirrors the GUI wizard and prints the derived
`mac`/`model`/`device_id` plus every `control/check` raw response:

```
aipm-register-cli --interactive --verbose
```

Supplying all required values keeps the non-interactive automation flow:

```
aipm-register-cli \
  --auth-code 12345678 \
  --device-hotspot-ssid DWD-S120_AABBCC \
  --home-ssid MyHomeWifi \
  --home-password 'p@ss'
```

If the host has more than one wireless interface (e.g. an external USB
adapter on top of a built-in radio), the CLI lists them and exits.
Re-run with `--wifi-interface <id>` to pick:

```
aipm-register-cli ... --wifi-interface wlan1
```

`AIPM_WIFI_IFACE` env var works as a fallback (`--wifi-interface`
overrides it). With a single interface present the CLI picks it
silently.

Wi-Fi handling is fully automated on every supported OS:

| OS | Adapter | Backend |
|---|---|---|
| **Windows** | `WindowsWifiAdapter` | `wlanapi.dll` via `ManagedNativeWifi` |
| **Linux**   | `LinuxWifiAdapter`   | `wpa_supplicant` control socket (see [docs/linux-wifi-permissions.md](docs/linux-wifi-permissions.md)) |
| **macOS**   | `MacOsWifiAdapter`   | `networksetup` + `system_profiler SPAirPortDataType -xml` |

---

## Status

| Phase | What | Status |
|---|---|---|
| A | Capture sample, decompile with ILSpy            | ✅ |
| B | Class map, register flow, OS-abstraction plan   | ✅ |
| C | .NET 10 solution skeleton (Core / CLI / Tests)  | ✅ |
| D | Domain models with `System.Text.Json`           | ✅ |
| E | API client, TCP sender, Notifier, Orchestrator  | ✅ |
| F | xUnit + WireMock tests (49 cases, CI-verified)  | ✅ |
| G | GitHub Actions: multi-OS test + tagged release  | ✅ |
| H | Win32 `IWifiAdapter` (ManagedNativeWifi)        | ✅ — `v0.2.0` |
| I | Avalonia GUI + macOS `.app` bundle              | ✅ — `v0.3.0` |
| J | Original 5-step wizard + 15-SKU catalog + Ko/En | ✅ — `v0.4.0` |
| K | Linux + macOS `IWifiAdapter`                    | ✅ — `v0.5.0` |
| L | Native AOT for the CLI (3 OSes)                 | ✅ — `v0.6.0` |
| M | Multi-wireless-interface selection (CLI + GUI)  | ✅ — `v1.3.0` |
| N | Modularization audit (Hosting, parsers, factories) | ✅ — `v1.3.0` |
| O | Apache-2.0 relicense + NOTICE                   | ✅ — `v1.3.0` |
| P | Native AOT for the GUI (Avalonia trim cleanup)  | ✅ — `v1.7.0` |

---

## Disclaimers

- The original `AIPM_Register.exe` and the materials derived from it under
  `analysis/` (decompiled IL, recovered class maps, protocol notes) remain
  the intellectual property of the device vendor. They are included only
  as a reproducibility aid for the porting work under `src/`; redistribute
  responsibly.
- All cloud endpoints, message formats, and protocol details are recovered
  from the existing binary's behavior at rest.
- Security observations recorded in
  [`analysis/notes.md §7`](analysis/notes.md) are noted for educational
  purposes only.

---

## License

[Apache 2.0](LICENSE) © 2026 Kim Seong Uk

Third-party components and the boundaries of this repository's license
are summarized in [`NOTICE`](NOTICE).
