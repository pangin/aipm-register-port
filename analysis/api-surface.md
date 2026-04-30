# API & OS Surface — Porting Mapping Table

원본이 의존하는 외부 API 와 멀티-OS 포팅 시 대체 방법 매핑.

## 1. 사외 HTTP API (DAWON cloud)

| Method | Path | Body | Response | 사용 위치 |
|---|---|---|---|---|
| `POST` | `/api/v1/accounts/post/getPckey` | `{"account":{"pc_temp_key":"<8자리>"}}` | `{"account":{"user_id":..., "pc_key":..., "pc_lati":..., "pc_long":...}}` 또는 `TIMEFAILED` | frmMain:1043 |
| `POST` | `/api/v1/devices/control/check` | `{"account":{"pc_key":..., "user_id":...}, "devices":[{"device_id":..., "msg":{"e":[{"n":"/100/0/31"}], "o":"r"}}]}` | `200` + `{"e":[..., {"n":"/100/0/31","sv":"true"|"false"}]}` 또는 `STATUSERROR`/`NOTREGISTERED` | frmMain:1027 |

**공통 헤더**: `Content-Type: application/json`, `Accept: application/json`, `Cache-Control: no-cache`, `X-HIT-Version: 1.0`.

**Base URL**: `https://dwapi.dawonai.com:18443/`. (HTTPS, 기본 인증서 검증)

## 2. LAN TCP (디바이스 ↔ PC)

| Direction | Endpoint | Payload | 비고 |
|---|---|---|---|
| PC → device | `<device-AP-gateway>:5000` | `{"mac":..., "api_server_addr":"dwapi.dawonai.com", ..., "ssid":..., "pass":..., "lati":..., "long":...}` (line 1864) | 평문 TCP, 디바이스가 hotspot AP 일 때만 |

**device_id 합성**: `DAWONDNS-<model>-<MAC>`. `<model>` 은 선택된 장치 hotspot SSID를 `_3(string)`에 넣어 추출한다 (line 1862, `_3(string)` line 2106). TCP 응답 문자열은 모델 코드로 쓰지 않는다.

## 3. .NET BCL (그대로 .NET 8 가능)

| 원본 | 포팅 형태 |
|---|---|
| `System.Net.HttpWebRequest` | `System.Net.Http.HttpClient` (DI 친화) |
| `System.Net.Sockets.TcpClient` | 그대로 또는 `Socket` (cancellation token 지원) |
| `System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()` | 그대로 (cross-OS 동작) |
| `System.Runtime.Serialization.Json.DataContractJsonSerializer` | `System.Text.Json` (.NET 8 표준, 빠름) |
| `System.Threading.ThreadPool.QueueUserWorkItem` | `Task.Run` / `IHostedService` |
| `System.Threading.Mutex("AIPM_Register")` | `Mutex` 그대로 — Linux/macOS 도 named mutex 동작 |
| `System.Windows.Forms.*` | **GUI 는 Avalonia 로 교체** (Phase G) |

## 4. WiFi (가장 큰 OS 의존)

원본은 `NativeWifi/*.cs` (P/Invoke `wlanapi.dll`). 다음 작업이 OS 별로 분기:

| 동작 | Win32 (원본) | Linux 대체 | macOS 대체 |
|---|---|---|---|
| AP 스캔 | `WlanClient.GetAvailableNetworkList`, `GetNetworkBssList` | `nmcli dev wifi list` 또는 wpa_supplicant DBus | `corewlan` framework / `airport -s` |
| 시그널 강도 | `wlanBssEntry.linkQuality` | nmcli 출력 SIGNAL 컬럼 | corewlan `rssiValue` |
| 인증/암호화 | `Dot11AuthAlgorithm`, `Dot11CipherAlgorithm` | nmcli SECURITY | corewlan `securityModes` |
| 프로필 등록 | `WlanProfile XML` + `SetProfile` | nmcli `connection add` 또는 `wpa_supplicant.conf` 추가 | corewlan `setWEPKey:` / `associateToNetwork:` |
| 특정 SSID 연결 | `WlanClient.Connect(Profile, BssType, SSID)` | `nmcli connection up <name>` | `associateToNetwork:` |
| AP 끊기 + 프로필 삭제 | `WlanClient.DeleteProfile` | `nmcli connection delete` | corewlan `disassociate` |

**포팅 전략**:
- `IWifiAdapter` 인터페이스를 Core 에 정의
- `Win32WifiAdapter` (P/Invoke 그대로 보존, .NET 8 에서도 동작)
- `LinuxWifiAdapter` (CliRunner 로 nmcli, 또는 D-Bus client `Tmds.DBus`)
- `MacOSWifiAdapter` (P/Invoke `Foundation.framework` 또는 `airport -s` shell)
- DI 로 RuntimeInformation.IsOSPlatform 으로 한 개 등록

권장: 1단계는 Win32 만, 2단계 (다른 OS) 는 stretch goal — 실제 디바이스 등록 테스트는 Windows 에서만 가능하기 때문.

## 5. 단일 인스턴스 보장

원본: 전역 named mutex `"AIPM_Register"`.
.NET 8 에서도 그대로 가능. 다만 Linux 에서는 named mutex 가 `/tmp` 의 파일 mutex 로 매핑됨 — 동작은 OK. macOS 도 마찬가지.

## 6. UI 메시지 (한국어 → 다국어)

원본은 모든 메시지가 한국어 하드코드. 포팅 시 `.resx` 또는 JSON resource:
- `Resources.ko.resx`, `Resources.en.resx`
- 키 예: `Error.AlreadyRegistered`, `Status.SearchingDevice`, `Auth.CodeExpired`

(포트폴리오 README 영어 narrative 와 자연스럽게 결합)

## 7. 자료 흐름 다이어그램 (Mermaid)

```
sequenceDiagram
    participant U as User
    participant P as PC App
    participant C as DAWON Cloud
    participant D as IoT Device (AP)
    participant H as Home Router

    U->>P: 8-digit auth code
    P->>C: POST getPckey (auth code)
    C-->>P: user_id, pc_key, lat, lng (50min TTL)

    U->>P: select home Wi-Fi (SSID/pass)
    P->>P: scan APs, filter DAWON_IRBD_/DWD- prefix
    Note over P: install WLAN profile for device hotspot
    P->>D: connect to device hotspot
    P->>D: TCP :5000 — settings JSON (ssid/pass/lat/lng/cloud URLs)
    Note over D: device joins H, registers to C via MQTT

    loop until success or 10x NOTREGISTERED
      P->>C: POST devices/control/check (LwM2M /100/0/31)
      C-->>P: sv=true|false / STATUSERROR / NOTREGISTERED
    end
    P-->>U: success / failure
```
