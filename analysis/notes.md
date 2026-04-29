# AIPM_Register v1.5 — Reverse Engineering Notes

원본 분석 베이스라인. 모든 인용 라인 번호는 [`analysis/decompiled-raw/`](decompiled-raw/) 기준.

## 1. 한 문장 요약

**DAWON AI/IoT 서버에 사용자의 IoT 디바이스(예: WiFi 적외선 리모트, 스마트 플러그)를 등록해주는 Windows 도우미 앱**. 사용자의 공유기 자격증명을 디바이스에 직접 TCP 로 흘려넣고, 클라우드 등록 상태를 폴링한다.

## 2. 메타데이터

| 항목 | 값 |
|---|---|
| Original target | `.NET Framework 3.5` (`net35`), x86 32-bit |
| Type | WinExe (WinForms), `WindowsDesktop` SDK |
| Obfuscator | 일부 적용 (메서드/매개변수 이름 → `_0`, `P_0`; `Evaluation` 사용자정의 Attribute로 표시) |
| References | `System.Xml`, `System.ServiceModel.Web`, `System.Runtime.Serialization` |
| Native deps | NativeWifi P/Invoke (`Wlan.cs`, `WlanClient.cs` — ManagedWifi 라이브러리 fork) |

## 3. 백엔드 인프라 (하드코드 상수, line 29~41)

| Const | 값 | 용도 |
|---|---|---|
| `m_0` | `dwapi.dawonai.com` | REST API host |
| `m_2` | `18443` | REST API port (HTTPS, 비표준) |
| `m_1` | `dwmqtt.dawonai.com` | MQTT host |
| `m_3` | `8883` | MQTT port (TLS) |
| `m_4` | `yes` | `ssl_support` 플래그 (디바이스에 전달) |
| `m_5` | `DAWONDNS` | `company` 식별자 |
| `m_6` | `dwd` | MQTT topic prefix |

**도메인 추정**: 다원디엔에스/다원AI (한국 IoT 회사). DAWON_IRBD_, DWD- prefix 의 SSID 는 등록 대상 디바이스의 hotspot 이름으로 보임 (스캔 결과에서 1229, 1240 라인에 필터링 됨).

## 4. 클래스 맵

| 파일 | 역할 |
|---|---|
| `0/0.cs` | 난독화된 Program.cs. 단일 인스턴스 Mutex `"AIPM_Register"`, `Application.Run(new frmMain())` |
| `Evaluation.cs` | `[Evaluation("warning")]` 사용자정의 Attribute (의미 불명, 다른 코드에서 직접 사용 흔적은 적음) |
| `AIPM_Register/account.cs` | POCO: `user_id`, `pc_key`, `pc_lati`, `pc_long` |
| `AIPM_Register/JsonData.cs` | wrapper: `account` 한 개 |
| `AIPM_Register/TablessControl.cs` | 탭 헤더 숨기는 WinForms 트릭 (`WM_UPDATEUISTATE`(0x128) intercept) |
| `AIPM_Register/frmMain.cs` | 2432줄, 모든 UI + 비즈니스 로직 + HTTP/TCP/Wi-Fi 처리 |
| `NativeWifi/*.cs` | ManagedWifi 라이브러리 (Win32 wlanapi.dll P/Invoke). 거의 그대로 |

## 5. 화면 전환 모델 (`TablessControl` + `m_6` 인덱스)

탭 인덱스가 곧 워크플로우 단계.

| Index | 추정 화면 | 진입/이탈 트리거 |
|---|---|---|
| 0 | 시작/welcome | 앱 실행 → m_6 = 0 |
| 1 | 인터넷 연결 체크 | DNS resolve 성공 시 → m_6 = 2 (line 1816~1822) |
| 2 | **인증번호 입력 (8자리)** | `getPckey` API 성공 시 → m_4 활성화 |
| 3 | 연동 해제 / 등록 정보 | `_2()` (line 2424) 메뉴에서 |
| 4 | **장치 발견 (TCP 연결 후 MAC 표시)** | 장치 hotspot 연결 + TCP 5000 포트 OK |
| 5 | **등록 진행 + 폴링** | DNS OK → control/check 폴링 |

## 6. Register 진입점 흐름 (가장 중요)

### 6.1 인증번호 → 사용자 정보 받기
1. 사용자가 모바일 앱에서 **8자리 임시 인증번호** 발급 후 PC 앱에 입력
2. `_0(object, EventArgs)` (line 1031) — 등록 버튼 클릭:
   - MaskedTextBox 검증 (8자리 숫자)
   - `{account:{pc_temp_key:<8자리>}}` 직접 문자열 조립
   - `_0(string)` (line 994) 가 quote 보강 → valid JSON
   - **`POST v1/accounts/post/getPckey`** 호출 (line 914)
3. 응답 `200 OK` 시 `JsonData` 역직렬화 (DataContractJsonSerializer, line 2385):
   - `user_id`, `pc_key`, `pc_lati`, `pc_long` 저장
   - **응답 만료 시간 50분** (`_2()` line 2413: `DateTime.Now - 50min`)
4. `TIMEFAILED` 응답 시: "인증 유효시간이 초과되었습니다"

### 6.2 디바이스 자동 발견
1. NativeWifi 로 SSID 스캔 (line 1218 `_5()`):
   - `DAWON_IRBD_` / `DWD-` prefix 만 별도 처리 (등록 대상 디바이스 hotspot)
2. 디바이스 hotspot 의 WLAN 프로필 XML 동적 생성 (line 1560 `_0(string,string,string)`)
3. `WlanClient.Connect(...)` (line 1761) 으로 PC 가 디바이스 hotspot 에 합류
4. DNS resolve 검증 → 성공 시 TCP 연결 시도
5. `TcpClient` `5000` 포트 (line 1779) — 디바이스가 AP 모드일 때 5000 에서 PC 의 명령 대기

### 6.3 디바이스 설정 주입 (TCP 메시지)
디바이스에 보내는 JSON (line 1864), valid JSON 으로 변환 후 TCP write:

```json
{
  "mac": "<device MAC>",
  "api_server_addr": "dwapi.dawonai.com",
  "api_server_port": "18443",
  "server_addr": "dwmqtt.dawonai.com",
  "server_port": "8883",
  "ssl_support": "yes",
  "ssid": "<유저 공유기 SSID>",
  "pass": "<유저 공유기 비밀번호 평문>",
  "user_id": "<유저 ID>",
  "company": "DAWONDNS",
  "model": "<디바이스 model>",
  "lati": "<위도>",
  "long": "<경도>",
  "topic": "dwd"
}
```

`device_id` 는 클라우드에서 `DAWONDNS-<model>-<MAC>` 형태로 만들어짐 (line 1863).

### 6.4 등록 결과 폴링
1. `_1()` (line 1022) 가 주기적으로 호출:
   ```json
   {"account":{"pc_key":"...","user_id":"..."},
    "devices":[{"device_id":"...","msg":{"e":[{"n":"/100/0/31"}],"o":"r"}}]}
   ```
   **`POST v1/devices/control/check`**
2. 응답 `sv` 값이 `true`/`false` 면 등록 진행 OK (line 2349)
3. `STATUSERROR` → "이미 등록된 장치입니다"
4. `NOTREGISTERED` 가 10번 이상 → "장치를 초기화 후 재등록하세요"

### 6.5 LWM2M 흔적
- `/100/0/31` = `<Object ID>/<Instance>/<Resource>` (LwM2M 표준 객체 경로 표현)
- `e: [{n: ...}], o: r` = LwM2M JSON Senml 변형 (n=name, sv=stringValue, o=operation, r=read)
- DAWON 이 IoT 디바이스 LwM2M 호환으로 만들어 둔 듯

## 7. 의심·보안 관찰

| 항목 | 라인 | 설명 |
|---|---|---|
| 손코딩 JSON 직렬화 | 994~1019, 1026, 1864 | `{a:b,c:d}` 같은 라이트 형식을 `{"a":"b","c":"d"}` 로 바꾸는 char-by-char 루프. **이스케이프 처리 없음** — `,`/`:`/`{`/`}` 가 값 안에 들어가면 깨짐. SSID/패스워드에 콜론 등이 있으면 위험 |
| 사용자 Wi-Fi 비번 평문 TCP 전송 | 1864 | LAN 안에서만이지만 디바이스 측 sniff 가능 |
| TLS 미적용 (디바이스 TCP) | 1779 | `TcpClient` 평문, port 5000. 디바이스가 hotspot AP 일 때만 통신해서 외부 노출은 적음 |
| TLS 검증 우회 코드 | 없음 | `ServicePointManager.ServerCertificateValidationCallback` 미사용 ✓ |
| 8자리 숫자 인증번호 | 1035, 2304 | 1억 가지. 50분 유효라 brute-force 까진 어렵지만 짧음 |
| 단일 인스턴스 Mutex 이름 | `0/0.cs` | `"AIPM_Register"` 평문 (다른 프로세스가 같은 이름 mutex 잡으면 차단) |

## 8. 외부 의존성 (포팅 매핑 표는 [api-surface.md](api-surface.md) 참고)

분석 시간을 줄이기 위해, 포팅 시 OS 추상화가 필요한 항목만 한 줄로:
- **WiFi 스캔/연결/프로필 등록**: Win32 wlanapi.dll → Linux NetworkManager DBus / wpa_supplicant, macOS CoreWLAN
- **HTTP**: HttpWebRequest → HttpClient (DI)
- **TCP 5000 client**: 그대로 (BSD socket, 모든 OS)
- **JSON**: DataContractJsonSerializer + 손코딩 → System.Text.Json
- **GPS**: 디바이스에 전달만 (PC 가 직접 측정 안 함, 서버가 발급)

## 9. 포팅 시 정리할 우선순위

1. **JSON 손코딩 → System.Text.Json 으로 전부 교체** (보안 + 가독성 + 한 번에 끝)
2. **frmMain 2432줄 → Core 서비스 4~5개로 분리**:
   - `IRegisterApiClient` (HTTP REST)
   - `IDeviceTcpSender` (TCP 5000 메시지 송신)
   - `IWifiScanner` (OS-abstracted)
   - `IWifiProfileInstaller` (OS-abstracted)
   - `IRegistrationOrchestrator` (위 4개를 엮은 워크플로우)
3. **상태머신**: `m_6` 탭 인덱스 + `m_8`(폴링 카운터) + 타이머 4개 → 명시적 enum + Channel 기반 비동기 파이프라인
4. **다국어 메시지**: 한국어 한정 메시지 → resource string 분리 (포트폴리오 영어 README 와 매칭)

---

## 10. 5-step Wizard Breakdown (v0.4 GUI 재구성용)

원본은 `TablessControl` (frmMain.cs `TablessControl.cs`) + `m_6` 인덱스 (0~5) 로 5단계 위저드를 단일 창 안에서 전환. 각 단계 = `TabPage`, `m_6 = N; m_0.SelectedIndex = m_6` 으로 화면 이동.

### 10.1 단계별 인덱스 + 전환 트리거
| Index | 화면 | 전환 트리거 |
|---|---|---|
| 0 | 시작 (Welcome) | 앱 실행 |
| 1 | (1/5) Wi-Fi 공유기 선택 | "시작" 클릭 (frmMain.cs:1095) |
| 2 | (2/5) AIPM 앱 계정 연동 | "연결" 후 DNS 응답 OK (frmMain.cs:1822) |
| 3 | (3/5) 등록할 제품 선택 | "다음(제품선택)" 클릭 (frmMain.cs:1988) |
| 4 | (4/5) 등록할 장치 선택 | "다음(장치선택)" 클릭 (frmMain.cs:2047) |
| 5 | (5/5) 등록 진행 | "등록" 후 Wi-Fi 연결 OK (frmMain.cs:2212) |
| (back) 4 → 3 / 5 → 3 | "이전(제품선택)" 클릭 (frmMain.cs:2085) |
| (back) 5 → 2 | "계정 연동 해제" 클릭 + 확인 (frmMain.cs:2422~2430) |

### 10.2 Wi-Fi 색상 규칙 (frmMain.cs:1267~1275)
```csharp
if (band == "2G" && security in ["WPA-개인 TKIP", "WPA2-개인 AES", "WEP"])
    listViewItem.BackColor = Color.GreenYellow;   // 디바이스가 연결 가능
else
    listViewItem.BackColor = Color.Orange;        // 5GHz 또는 미지원 보안 — 디바이스가 못 붙음
```
디바이스(IoT MCU)가 2.4GHz 만 지원해서, 5GHz AP 와 WPA3 같은 신형 보안은 모두 Orange 로 경고.

### 10.3 제품 카탈로그 (frmMain.cs:652~681 + `c()` line 2010 + `_3()` line 2106)
ListView `m_1` 에 고정 등록되는 15개 항목. PDF 가이드의 "8개 그리드" 는 마케팅용 간소화이고 실제 코드는 15 SKU.

| Tag | ToolTip (제품명, 한국어) | 보조 prefix (m_c) | 디바이스 응답 모델 코드 |
|---|---|---|---|
| `S120`   | 스마트 플러그 (16A)        | `DWD-LS120` | `B530_W` |
| `ES120`  | 스마트 플러그 (16A)        | `DWD-SS120` | `B550E_W` (또는 `B550E_SW` if `m_B==DWD-ES120S`) |
| `LS130`  | 스마트 플러그 (16A)        | (없음)      | `B350_W` |
| `S220`   | 스마트 멀티탭 (16A)        | (없음)      | `M130_W` |
| `LS810`  | 지그비 허브                | (없음)      | `G200L_ZB` |
| `S510`   | IR 리모컨                  | (없음)      | `R200_W` (또는 `R110_W` if `m_B==DWD-S510`) |
| `S501`   | IR 리모컨                  | `DWD-S510`  | `R200_W` |
| `S310`   | 분전반 단상 (50A)          | `DWD-S311`  | `P110_W` |
| `S330`   | 분전반 3상 (100A)          | (없음)      | `P230_W` |
| `S350`   | 분전반 3상 (400A)          | (없음)      | `P250_W` |
| `S370`   | 분전반 3상 (800A)          | (없음)      | `P270_W` |
| `ES120S` | 태양광 스마트 플러그 (16A) | `DWD-ES120` | `B550E_SW` |
| `S600`   | 태양광 스마트 플러그 (10A) | (없음)      | `B400_W` (또는 `B400_SW` if `m_B==DWD-S600`) |
| `S110`   | 스마트 플러그 (10A)        | `DWD-S600`  | `B400_WI` |
| `S121`   | 일본향 스마트 플러그       | (없음)      | `B343_W` |

**Hotspot SSID 필터**: 4/5 화면은 선택된 제품의 `m_B = "DWD-" + Tag` (주 prefix) + `m_c` (보조 prefix) 둘 중 하나로 시작하는 SSID 만 표시.

**모델 코드 추출**: 디바이스가 TCP 5000 응답으로 보낸 페이로드의 첫 토큰을 `Split('_')[0].Substring(4)` 로 잘라 (예: `DWD-S120` → `S120`) `_3()` switch 로 매핑. 결과가 `device_id = "DAWONDNS-{model}-{mac}"` 의 model 부분.

### 10.4 진행 표시 (5/5, frmMain.cs `f()` line 2253)
- ProgressBar `m_0` 의 `Maximum` 은 폴링 한도 = 20 (frmMain.cs:1737 `m_8 > 24` 와 별도, 5/5 의 `++m_8 > 20` 흐름)
- 각 폴링 tick 마다 `Value++`
- 성공 응답 (`sv: true|false`) 시 `Value = Maximum` + Label `m_d.Text = "등록 완료"`
- 실패 시 Label 빨간색 + 메시지박스 ("이미 등록된 장치입니다", "장치를 초기화 후...")

### 10.5 핵심 한국어 문자열 (resx 시드)
| 위치 | 한국어 |
|---|---|
| Welcome body | `원활한 연결을 위해\n유선 네트워크 연결을 끊고 시작해주세요.\n등록 과정중에 Wi-Fi가 여러번 재접속 됩니다.\n인터넷을 이용중이라면 작업을 끝내고 시작해주세요.\nAIPM 앱이 설치 및 가입된 모바일 기기를 준비해주세요.` (frmMain.cs:428) |
| Welcome confirm | `무선 네트워크 연결이 끊어집니다. 시작하겠습니까?` |
| Step1 title | `사용할 Wi-Fi 공유기 선택 (1/5)` |
| Step2 title | `AIPM 앱 계정과 연동 (2/5)` |
| Step2 hint | `AIPM 앱의 My page 화면에서 발급 버튼을 눌러 생성된 인증번호 8자리를 입력해주세요.` (frmMain.cs:631) |
| Step2 next | `다음 (제품 선택)` |
| Step3 title | `등록할 제품 선택 (3/5)` |
| Step3 next | `다음 (장치 선택)` |
| Step4 title | `등록할 장치 선택 (4/5)` |
| Step4 prev | `이전 (제품 선택)` |
| Step4 register | `등록` |
| Step5 title | `등록 진행 (5/5)` |
| Step5 done | `등록 완료` |
| Step5 unbind | `계정 연동 해제` |
| Common refresh | `새로 고침` |
| Error: AlreadyRegistered | `이미 등록된 장치입니다.` (frmMain.cs:2361) |
| Error: NotRegistered | `등록 실패 : 장치를 초기화 후 다시 등록해주세요.` (frmMain.cs:2369) |
| Error: AuthExpired | `인증 유효시간이 초과되었습니다.\n앱에서 새 인증번호를 발급해주세요.` (frmMain.cs:2382) |
| Error: AuthInvalid | `유효한 인증번호가 아닙니다.` (frmMain.cs:2402) |
| Error: AuthFormat | `공백 없이 숫자 8자리를 입력해주세요.` (frmMain.cs:1035) |
| Confirm: Unbind | `정말로 연동을 해제하겠습니까?` (frmMain.cs:2424) |
| Confirm: Exit | `정말로 종료하겠습니까?` (frmMain.cs:2163) |

이 표가 곧 `Strings.ko.resx` 의 시드.
