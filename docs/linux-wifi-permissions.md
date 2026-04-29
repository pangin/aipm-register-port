# Linux Wi-Fi permissions

`AipmRegister` 의 Linux 빌드는 `wpa_supplicant` 의 컨트롤 소켓
(`/var/run/wpa_supplicant/<iface>`) 에 직접 연결한다. 일반 사용자 권한으로는
열 수 없으므로 다음 중 하나를 선택해 권한을 부여해야 한다.

## 옵션 1 — 사용자를 `netdev` 그룹에 추가 (권장)

Debian / Ubuntu / Mint 계열:

```bash
sudo usermod -aG netdev $USER
# 로그아웃 후 다시 로그인하면 적용됨
```

Fedora / RHEL 은 그룹명이 `wheel` 인 경우가 있음:

```bash
groups          # 현재 그룹 확인
sudo usermod -aG wheel $USER
```

이 방법이 가장 깔끔하고, 한 번만 적용하면 GUI/CLI 모두 별도 sudo 없이 동작.

## 옵션 2 — 일회성 sudo 실행

```bash
sudo aipm-register-cli \
  --auth-code 12345678 \
  --device-hotspot-ssid DAWON_IRBD_AABBCC \
  --home-ssid MyHomeWifi \
  --home-password 'p@ss'
```

## 옵션 3 — `CAP_NET_ADMIN` 부여 (binary 단위)

`setcap` 으로 binary 자체에 capability 부여:

```bash
sudo setcap cap_net_admin,cap_net_raw+eip $(which aipm-register-cli)
```

systemd unit 으로 실행한다면 `CapabilityBoundingSet=CAP_NET_ADMIN` 추가.

## 인터페이스 자동 검색

기본적으로 `/sys/class/net/<iface>/wireless` 디렉토리가 있는 첫 인터페이스를
사용한다 (`wlan0`, `wlp3s0` 등). 다음 환경 변수로 명시 지정 가능:

```bash
AIPM_WIFI_IFACE=wlan1 aipm-register-cli ...
```

## 트러블슈팅

- **"wpa_supplicant control socket not found"** — `wpa_supplicant` 가 안
  돌고 있거나 (`systemctl status wpa_supplicant`) 컨트롤 소켓 경로가 다름.
  `/var/run/wpa_supplicant/` 디렉토리에 어댑터 이름과 매칭되는 파일이
  있어야 한다. NetworkManager 만 동작하는 시스템(Fedora 일부 빌드 등)은
  `wpa_supplicant -B -i wlan0 -c /etc/wpa_supplicant/wpa_supplicant.conf`
  으로 한 번 띄워야 할 수도 있다.
- **`Permission denied`** — 옵션 1, 2, 3 중 하나 적용.
- **SSID 스캔 결과가 비어있음** — `iw dev wlan0 scan` 으로 직접 시도해
  라디오 자체가 동작하는지 확인.
