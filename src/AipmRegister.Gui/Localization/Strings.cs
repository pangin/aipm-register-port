namespace AipmRegister.Gui.Localization;

/// All UI strings keyed by stable IDs. Korean values follow the original
/// frmMain.cs hardcoded text (see analysis/notes.md §10.5). English values
/// are translations geared for portfolio readers.
internal static class Strings
{
    public static readonly IReadOnlyDictionary<string, string> Ko = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // App
        ["App.Title"]                      = "AIPM Register",
        ["App.SubTitle"]                   = "DAWON IoT 디바이스 등록 도우미 (.NET 10 + Avalonia)",
        ["Lang.Toggle"]                    = "EN",

        // Step 0 — Welcome
        ["Welcome.Body"]                   = "원활한 연결을 위해\n유선 네트워크 연결을 끊고 시작해주세요.\n등록 과정중에 Wi-Fi가 여러번 재접속 됩니다.\n인터넷을 이용중이라면 작업을 끝내고 시작해주세요.\nAIPM 앱이 설치 및 가입된 모바일 기기를 준비해주세요.",
        ["Welcome.StartButton"]            = "시작",
        ["Welcome.ConfirmTitle"]           = "확인",
        ["Welcome.ConfirmBody"]            = "무선 네트워크 연결이 끊어집니다. 시작하겠습니까?",

        // Step 1/5 — Wi-Fi picker
        ["Step1.Title"]                    = "사용할 Wi-Fi 공유기 선택 (1/5)",
        ["Step1.HeaderSsid"]               = "SSID",
        ["Step1.HeaderBand"]               = "대역",
        ["Step1.HeaderSignal"]             = "신호 강도",
        ["Step1.HeaderSecurity"]           = "보안 유형",
        ["Step1.SsidLabel"]                = "SSID",
        ["Step1.PasswordLabel"]            = "비밀번호",
        ["Step1.RefreshButton"]            = "새로 고침",
        ["Step1.ConnectButton"]            = "연결",
        ["Step1.SecurityOpen"]             = "개방형",
        ["Step1.SecurityWep"]              = "WEP",
        ["Step1.SecurityWpa"]              = "WPA-개인 TKIP",
        ["Step1.SecurityWpa2"]             = "WPA2-개인 AES",
        ["Step1.SecurityWpa3"]             = "WPA3-개인",

        // Step 2/5 — Auth code
        ["Step2.Title"]                    = "AIPM 앱 계정과 연동 (2/5)",
        ["Step2.AuthCodeLabel"]            = "인증번호",
        ["Step2.AuthButton"]               = "인증",
        ["Step2.LinkedAccountLabel"]       = "연동계정",
        ["Step2.Hint"]                     = "AIPM 앱의 My page 화면에서 발급 버튼을 눌러\n생성된 인증번호 8자리를 입력해주세요.",
        ["Step2.ConfirmHint"]              = "연동할 계정이 맞는지 확인하시고\n맞다면 \"다음 (제품 선택)\" 버튼을 누르시고\n맞지 않다면 인증번호를 새로 발급하여 다시 입력해주세요.",
        ["Step2.NextButton"]               = "다음 (제품 선택)",

        // Step 3/5 — Product picker
        ["Step3.Title"]                    = "등록할 제품 선택 (3/5)",
        ["Step3.NextButton"]               = "다음 (장치 선택)",

        // Step 4/5 — Device picker
        ["Step4.Title"]                    = "등록할 장치 선택 (4/5)",
        ["Step4.HeaderSsid"]               = "SSID",
        ["Step4.HeaderMac"]                = "MAC",
        ["Step4.HeaderSignal"]             = "신호 강도",
        ["Step4.SelectedProductLabel"]     = "선택 제품",
        ["Step4.MacLabel"]                 = "MAC",
        ["Step4.RefreshButton"]            = "새로 고침",
        ["Step4.PrevButton"]               = "이전 (제품 선택)",
        ["Step4.RegisterButton"]           = "등록",

        // Step 5/5 — Registering
        ["Step5.Title"]                    = "등록 진행 (5/5)",
        ["Step5.Done"]                     = "등록 완료",
        ["Step5.InProgress"]               = "등록 중...",
        ["Step5.PrevButton"]               = "이전 (제품 선택)",
        ["Step5.UnbindButton"]             = "계정 연동 해제",

        // Errors / confirmations
        ["Error.AlreadyRegistered"]        = "이미 등록된 장치입니다.",
        ["Error.NotRegistered"]            = "등록 실패 : 장치를 초기화 후 다시 등록해주세요.",
        ["Error.AuthExpired"]              = "인증 유효시간이 초과되었습니다.\n앱에서 새 인증번호를 발급해주세요.",
        ["Error.AuthInvalid"]              = "유효한 인증번호가 아닙니다.",
        ["Error.AuthFormat"]               = "공백 없이 숫자 8자리를 입력해주세요.",
        ["Error.WifiConnectFailed"]        = "Wi-Fi 연결에 실패했습니다.",
        ["Error.DeviceTcpFailed"]          = "장치와의 통신에 실패했습니다.",
        ["Confirm.Unbind"]                 = "정말로 연동을 해제하겠습니까?",
        ["Confirm.Exit"]                   = "정말로 종료하겠습니까?",
        ["Confirm.Yes"]                    = "예",
        ["Confirm.No"]                     = "아니오",

        // Product names — match original ToolTipText word for word
        ["Product.SmartPlug16A.S120.Name"]  = "스마트 플러그 (16A)",
        ["Product.SmartPlug16A.ES120.Name"] = "스마트 플러그 (16A)",
        ["Product.SmartPlug16A.LS130.Name"] = "스마트 플러그 (16A)",
        ["Product.SmartMultitap16A.Name"]   = "스마트 멀티탭 (16A)",
        ["Product.ZigbeeHub.Name"]          = "지그비 허브",
        ["Product.IrRemote.S510.Name"]      = "IR 리모컨",
        ["Product.IrRemote.S501.Name"]      = "IR 리모컨",
        ["Product.PanelboardSingle50A.Name"]= "분전반 단상 (50A)",
        ["Product.PanelboardThree100A.Name"]= "분전반 3상 (100A)",
        ["Product.PanelboardThree400A.Name"]= "분전반 3상 (400A)",
        ["Product.PanelboardThree800A.Name"]= "분전반 3상 (800A)",
        ["Product.SolarSmartPlug16A.Name"]  = "태양광 스마트 플러그 (16A)",
        ["Product.SolarSmartPlug10A.Name"]  = "태양광 스마트 플러그 (10A)",
        ["Product.SmartPlug10A.Name"]       = "스마트 플러그 (10A)",
        ["Product.SmartPlugJP.Name"]        = "일본향 스마트 플러그",
        ["Product.Unknown.Name"]            = "알 수 없는 제품",
    };

    public static readonly IReadOnlyDictionary<string, string> En = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // App
        ["App.Title"]                      = "AIPM Register",
        ["App.SubTitle"]                   = "DAWON IoT device pairing helper (.NET 10 + Avalonia)",
        ["Lang.Toggle"]                    = "한국어",

        // Step 0 — Welcome
        ["Welcome.Body"]                   = "For a smooth registration\nDisconnect any wired network before starting.\nWi-Fi will reconnect several times during the flow.\nClose internet-using apps before continuing.\nKeep your phone with the AIPM app signed in nearby.",
        ["Welcome.StartButton"]            = "Start",
        ["Welcome.ConfirmTitle"]           = "Confirm",
        ["Welcome.ConfirmBody"]            = "Your Wi-Fi connection will drop. Start anyway?",

        // Step 1/5
        ["Step1.Title"]                    = "Pick a Wi-Fi router (1/5)",
        ["Step1.HeaderSsid"]               = "SSID",
        ["Step1.HeaderBand"]               = "Band",
        ["Step1.HeaderSignal"]             = "Signal",
        ["Step1.HeaderSecurity"]           = "Security",
        ["Step1.SsidLabel"]                = "SSID",
        ["Step1.PasswordLabel"]            = "Password",
        ["Step1.RefreshButton"]            = "Refresh",
        ["Step1.ConnectButton"]            = "Connect",
        ["Step1.SecurityOpen"]             = "Open",
        ["Step1.SecurityWep"]              = "WEP",
        ["Step1.SecurityWpa"]              = "WPA Personal",
        ["Step1.SecurityWpa2"]             = "WPA2 Personal",
        ["Step1.SecurityWpa3"]             = "WPA3 Personal",

        // Step 2/5
        ["Step2.Title"]                    = "Link AIPM app account (2/5)",
        ["Step2.AuthCodeLabel"]            = "Auth code",
        ["Step2.AuthButton"]               = "Verify",
        ["Step2.LinkedAccountLabel"]       = "Linked account",
        ["Step2.Hint"]                     = "Open the AIPM app, go to My Page,\ntap Issue, and enter the 8-digit code shown there.",
        ["Step2.ConfirmHint"]              = "Confirm the linked account is yours.\nIf so, click \"Next (Product)\".\nIf not, re-issue the auth code and try again.",
        ["Step2.NextButton"]               = "Next (Product)",

        // Step 3/5
        ["Step3.Title"]                    = "Pick a product (3/5)",
        ["Step3.NextButton"]               = "Next (Device)",

        // Step 4/5
        ["Step4.Title"]                    = "Pick a device (4/5)",
        ["Step4.HeaderSsid"]               = "SSID",
        ["Step4.HeaderMac"]                = "MAC",
        ["Step4.HeaderSignal"]             = "Signal",
        ["Step4.SelectedProductLabel"]     = "Selected product",
        ["Step4.MacLabel"]                 = "MAC",
        ["Step4.RefreshButton"]            = "Refresh",
        ["Step4.PrevButton"]               = "Back (Product)",
        ["Step4.RegisterButton"]           = "Register",

        // Step 5/5
        ["Step5.Title"]                    = "Registering (5/5)",
        ["Step5.Done"]                     = "Registration complete",
        ["Step5.InProgress"]               = "Registering...",
        ["Step5.PrevButton"]               = "Back (Product)",
        ["Step5.UnbindButton"]             = "Unbind account",

        // Errors / confirmations
        ["Error.AlreadyRegistered"]        = "Device is already registered to another account.",
        ["Error.NotRegistered"]            = "Registration failed. Reset the device and try again.",
        ["Error.AuthExpired"]              = "Auth code has expired.\nIssue a new one from the AIPM app.",
        ["Error.AuthInvalid"]              = "Auth code is not valid.",
        ["Error.AuthFormat"]               = "Enter exactly 8 digits, no spaces.",
        ["Error.WifiConnectFailed"]        = "Failed to connect to the Wi-Fi network.",
        ["Error.DeviceTcpFailed"]          = "Failed to communicate with the device.",
        ["Confirm.Unbind"]                 = "Really unbind the account?",
        ["Confirm.Exit"]                   = "Really quit?",
        ["Confirm.Yes"]                    = "Yes",
        ["Confirm.No"]                     = "No",

        // Product names
        ["Product.SmartPlug16A.S120.Name"]  = "Smart Plug (16A) — S120",
        ["Product.SmartPlug16A.ES120.Name"] = "Smart Plug (16A) — ES120",
        ["Product.SmartPlug16A.LS130.Name"] = "Smart Plug (16A) — LS130",
        ["Product.SmartMultitap16A.Name"]   = "Smart Multi-tap (16A)",
        ["Product.ZigbeeHub.Name"]          = "Zigbee Hub",
        ["Product.IrRemote.S510.Name"]      = "IR Remote — S510",
        ["Product.IrRemote.S501.Name"]      = "IR Remote — S501",
        ["Product.PanelboardSingle50A.Name"]= "Panelboard Single-Phase (50A)",
        ["Product.PanelboardThree100A.Name"]= "Panelboard Three-Phase (100A)",
        ["Product.PanelboardThree400A.Name"]= "Panelboard Three-Phase (400A)",
        ["Product.PanelboardThree800A.Name"]= "Panelboard Three-Phase (800A)",
        ["Product.SolarSmartPlug16A.Name"]  = "Solar Smart Plug (16A)",
        ["Product.SolarSmartPlug10A.Name"]  = "Solar Smart Plug (10A)",
        ["Product.SmartPlug10A.Name"]       = "Smart Plug (10A)",
        ["Product.SmartPlugJP.Name"]        = "Smart Plug (JP)",
        ["Product.Unknown.Name"]            = "Unknown product",
    };
}
