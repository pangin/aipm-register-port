using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using NativeWifi;

namespace AIPM_Register;

public class frmMain : Form
{
	public delegate void delegateLog(string msg);

	private enum HttpMethod
	{
		POST,
		PUT
	}

	private const string m_0 = "dwapi.dawonai.com";

	private const string m_1 = "dwmqtt.dawonai.com";

	private const string m_2 = "18443";

	private const string m_3 = "8883";

	private const string m_4 = "yes";

	private const string m_5 = "DAWONDNS";

	private const string m_6 = "dwd";

	private IContainer m_0;

	private ListBox m_0;

	private Button m_0;

	private Button m_1;

	private ListView m_0;

	private ColumnHeader m_0;

	private ColumnHeader m_1;

	private ColumnHeader m_2;

	private Button m_2;

	private System.Windows.Forms.Timer m_0;

	private Button m_3;

	private ColumnHeader m_3;

	private TextBox m_0;

	private Label m_0;

	private Label m_1;

	private Label m_2;

	private TextBox m_1;

	private System.Windows.Forms.Timer m_1;

	private System.Windows.Forms.Timer m_2;

	private System.Windows.Forms.Timer m_3;

	private TabPage m_0;

	private TabPage m_1;

	private TabPage m_2;

	private TabPage m_3;

	private Label m_3;

	private Label m_4;

	private Label m_5;

	private Label m_6;

	private Label m_7;

	private TextBox m_2;

	private Label m_8;

	private Button m_4;

	private Label m_9;

	private ListView m_1;

	private ImageList m_0;

	private Button m_5;

	private TabPage m_4;

	private Label m_a;

	private MaskedTextBox m_0;

	private ListView m_2;

	private ColumnHeader m_4;

	private ColumnHeader m_5;

	private ColumnHeader m_6;

	private TextBox m_3;

	private Button m_6;

	private Label m_A;

	private Button m_7;

	private Label m_b;

	private Label m_B;

	private TextBox m_4;

	private Button m_8;

	private TextBox m_5;

	private Label m_c;

	private System.Windows.Forms.Timer m_4;

	private TabPage m_5;

	private Label m_C;

	private System.Windows.Forms.Timer m_5;

	private Label m_d;

	private System.Windows.Forms.Timer m_6;

	private ProgressBar m_0;

	private Button m_9;

	private PictureBox m_0;

	private System.Windows.Forms.Timer m_7;

	private Button m_a;

	private TablessControl m_0;

	private PictureBox m_1;

	private Label m_D;

	private object m_0 = new object();

	private string m_7 = Application.StartupPath + "\\LOG";

	private Queue<string[]> m_0 = new Queue<string[]>();

	private object m_1 = new object();

	private int m_0;

	private bool m_0;

	private WlanClient m_0 = new WlanClient();

	private WlanClient.WlanInterface m_0;

	private DateTime m_0 = DateTime.Now.AddSeconds(-5.0);

	private int m_1;

	private int m_2;

	private int m_3;

	private int m_4;

	private int m_5;

	private int m_6;

	private Hashtable m_0 = new Hashtable();

	private string m_8 = "";

	private string m_9 = "";

	private string m_a = "";

	private string m_A = "";

	private string m_b = "";

	private string m_B = "";

	private string m_c = "";

	private string m_C = "";

	private TcpClient m_0;

	private byte[] m_0 = new byte[1024];

	private StringBuilder m_0 = new StringBuilder();

	private StreamWriter m_0;

	private bool m_1;

	private int m_7;

	private int m_8;

	private string m_d = "";

	private string m_D = "";

	private string m_e = "";

	private string m_E = "";

	private string m_f = "";

	private DateTime m_1 = DateTime.Now.AddHours(-1.0);

	protected override void Dispose(bool disposing)
	{
		if (disposing && this.m_0 != null)
		{
			this.m_0.Dispose();
		}
		base.Dispose(disposing);
	}

	private void _0()
	{
		this.m_0 = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmMain));
		ListViewItem listViewItem = new ListViewItem("Smart Plug 16A (B530, B540)", 0);
		ListViewItem listViewItem2 = new ListViewItem("Smart Plug 16A (B550, B550E)", 1);
		ListViewItem listViewItem3 = new ListViewItem("Smart Plug 16A (B350)", 2);
		ListViewItem listViewItem4 = new ListViewItem("Smart Multi-tap (M130)", 3);
		ListViewItem listViewItem5 = new ListViewItem("Zigbee Hub (G200L)", 4);
		ListViewItem listViewItem6 = new ListViewItem("IR Remote (R110)", 5);
		ListViewItem listViewItem7 = new ListViewItem("IR Remote (R200)", 6);
		ListViewItem listViewItem8 = new ListViewItem("Panelboard 50A (Single-phase)", 7);
		ListViewItem listViewItem9 = new ListViewItem("Panelboard 100A (Three-phase)", 8);
		ListViewItem listViewItem10 = new ListViewItem("Panelboard 400A (Three-phase)", 8);
		ListViewItem listViewItem11 = new ListViewItem("Panelboard 800A (Three-phase)", 8);
		ListViewItem listViewItem12 = new ListViewItem("Solar Power Meter 16A (B550E-SW)", 9);
		ListViewItem listViewItem13 = new ListViewItem("Solar Power Meter 10A (B400-SW)", 10);
		ListViewItem listViewItem14 = new ListViewItem("Smart Plug 10A (B400)", 11);
		ListViewItem listViewItem15 = new ListViewItem("Smart Plug Japan", 12);
		this.m_0 = new ListBox();
		this.m_0 = new System.Windows.Forms.Timer(this.m_0);
		this.m_1 = new System.Windows.Forms.Timer(this.m_0);
		this.m_2 = new System.Windows.Forms.Timer(this.m_0);
		this.m_3 = new System.Windows.Forms.Timer(this.m_0);
		this.m_0 = new ImageList(this.m_0);
		this.m_4 = new System.Windows.Forms.Timer(this.m_0);
		this.m_5 = new System.Windows.Forms.Timer(this.m_0);
		this.m_6 = new System.Windows.Forms.Timer(this.m_0);
		this.m_7 = new System.Windows.Forms.Timer(this.m_0);
		this.m_0 = new TablessControl();
		this.m_0 = new TabPage();
		this.m_0 = new PictureBox();
		this.m_4 = new Label();
		this.m_1 = new Button();
		this.m_1 = new TabPage();
		this.m_3 = new Label();
		this.m_0 = new ListView();
		this.m_0 = new ColumnHeader();
		this.m_1 = new ColumnHeader();
		this.m_2 = new ColumnHeader();
		this.m_3 = new ColumnHeader();
		this.m_1 = new TextBox();
		this.m_2 = new Button();
		this.m_2 = new Label();
		this.m_3 = new Button();
		this.m_1 = new Label();
		this.m_0 = new Label();
		this.m_0 = new TextBox();
		this.m_2 = new TabPage();
		this.m_0 = new MaskedTextBox();
		this.m_4 = new Button();
		this.m_9 = new Label();
		this.m_2 = new TextBox();
		this.m_8 = new Label();
		this.m_7 = new Label();
		this.m_5 = new Label();
		this.m_0 = new Button();
		this.m_1 = new PictureBox();
		this.m_D = new Label();
		this.m_3 = new TabPage();
		this.m_5 = new Button();
		this.m_1 = new ListView();
		this.m_6 = new Label();
		this.m_4 = new TabPage();
		this.m_5 = new TextBox();
		this.m_c = new Label();
		this.m_8 = new Button();
		this.m_3 = new TextBox();
		this.m_6 = new Button();
		this.m_A = new Label();
		this.m_7 = new Button();
		this.m_b = new Label();
		this.m_B = new Label();
		this.m_4 = new TextBox();
		this.m_2 = new ListView();
		this.m_4 = new ColumnHeader();
		this.m_5 = new ColumnHeader();
		this.m_6 = new ColumnHeader();
		this.m_a = new Label();
		this.m_5 = new TabPage();
		this.m_a = new Button();
		this.m_9 = new Button();
		this.m_0 = new ProgressBar();
		this.m_d = new Label();
		this.m_C = new Label();
		this.m_0.SuspendLayout();
		this.m_0.SuspendLayout();
		((ISupportInitialize)this.m_0).BeginInit();
		this.m_1.SuspendLayout();
		this.m_2.SuspendLayout();
		((ISupportInitialize)this.m_1).BeginInit();
		this.m_3.SuspendLayout();
		this.m_4.SuspendLayout();
		this.m_5.SuspendLayout();
		SuspendLayout();
		this.m_0.Font = new Font("굴림체", 9f, FontStyle.Regular, GraphicsUnit.Point, 129);
		this.m_0.FormattingEnabled = true;
		this.m_0.ItemHeight = 12;
		this.m_0.Location = new Point(5, 387);
		this.m_0.Name = "lbLog";
		this.m_0.Size = new Size(529, 220);
		this.m_0.TabIndex = 0;
		this.m_0.Tick += _3;
		this.m_1.Interval = 250;
		this.m_1.Tick += _7;
		this.m_2.Interval = 250;
		this.m_2.Tick += _8;
		this.m_3.Interval = 250;
		this.m_3.Tick += _9;
		this.m_0.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
		this.m_0.TransparentColor = Color.Transparent;
		this.m_0.Images.SetKeyName(0, "B540");
		this.m_0.Images.SetKeyName(1, "B550E");
		this.m_0.Images.SetKeyName(2, "B350");
		this.m_0.Images.SetKeyName(3, "M130");
		this.m_0.Images.SetKeyName(4, "G200L");
		this.m_0.Images.SetKeyName(5, "R110");
		this.m_0.Images.SetKeyName(6, "R200");
		this.m_0.Images.SetKeyName(7, "P110");
		this.m_0.Images.SetKeyName(8, "P230");
		this.m_0.Images.SetKeyName(9, "B550ES");
		this.m_0.Images.SetKeyName(10, "B400S");
		this.m_0.Images.SetKeyName(11, "B400");
		this.m_0.Images.SetKeyName(12, "B343");
		this.m_4.Interval = 2000;
		this.m_4.Tick += a;
		this.m_5.Interval = 1000;
		this.m_5.Tick += E;
		this.m_6.Interval = 5000;
		this.m_6.Tick += f;
		this.m_7.Interval = 50;
		this.m_7.Tick += G;
		this.m_0.Alignment = TabAlignment.Bottom;
		this.m_0.Controls.Add(this.m_0);
		this.m_0.Controls.Add(this.m_1);
		this.m_0.Controls.Add(this.m_2);
		this.m_0.Controls.Add(this.m_3);
		this.m_0.Controls.Add(this.m_4);
		this.m_0.Controls.Add(this.m_5);
		this.m_0.Location = new Point(5, 5);
		this.m_0.Name = "tab";
		this.m_0.SelectedIndex = 0;
		this.m_0.Size = new Size(533, 377);
		this.m_0.TabIndex = 24;
		this.m_0.TabStop = false;
		this.m_0.SelectedIndexChanged += A;
		this.m_0.Controls.Add(this.m_0);
		this.m_0.Controls.Add(this.m_4);
		this.m_0.Controls.Add(this.m_1);
		this.m_0.Location = new Point(4, 4);
		this.m_0.Name = "tabPage1";
		this.m_0.Padding = new Padding(3);
		this.m_0.Size = new Size(525, 351);
		this.m_0.TabIndex = 0;
		this.m_0.Text = "시작";
		this.m_0.UseVisualStyleBackColor = true;
		this.m_0.BackgroundImage = (Image)componentResourceManager.GetObject("picLogo.BackgroundImage");
		this.m_0.BackgroundImageLayout = ImageLayout.None;
		this.m_0.Location = new Point(73, 34);
		this.m_0.Name = "picLogo";
		this.m_0.Size = new Size(382, 57);
		this.m_0.TabIndex = 9;
		this.m_0.TabStop = false;
		this.m_4.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_4.Location = new Point(35, 106);
		this.m_4.Name = "label2";
		this.m_4.Size = new Size(449, 166);
		this.m_4.TabIndex = 8;
		this.m_4.Text = "원활한 연결을 위해\r\n\r\n유선 네트워크 연결을 끊고 시작해주세요.\r\n\r\n등록 과정중에 Wi-Fi가 여러번 재접속 됩니다.\r\n\r\n인터넷을 이용중이라면 작업을 끝내고 시작해주세요.\r\n\r\nAIPM 앱이 설치 및 가입된 모바일 기기를 준비해주세요.";
		this.m_4.TextAlign = ContentAlignment.MiddleCenter;
		this.m_1.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_1.Location = new Point(208, 289);
		this.m_1.Name = "button1";
		this.m_1.Size = new Size(100, 35);
		this.m_1.TabIndex = 7;
		this.m_1.Text = "시작";
		this.m_1.UseVisualStyleBackColor = true;
		this.m_1.Click += _1;
		this.m_1.Controls.Add(this.m_3);
		this.m_1.Controls.Add(this.m_0);
		this.m_1.Controls.Add(this.m_1);
		this.m_1.Controls.Add(this.m_2);
		this.m_1.Controls.Add(this.m_2);
		this.m_1.Controls.Add(this.m_3);
		this.m_1.Controls.Add(this.m_1);
		this.m_1.Controls.Add(this.m_0);
		this.m_1.Controls.Add(this.m_0);
		this.m_1.Location = new Point(4, 4);
		this.m_1.Name = "tabPage2";
		this.m_1.Padding = new Padding(3);
		this.m_1.Size = new Size(525, 351);
		this.m_1.TabIndex = 1;
		this.m_1.Text = "공유기 선택";
		this.m_1.UseVisualStyleBackColor = true;
		this.m_3.AutoSize = true;
		this.m_3.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_3.Location = new Point(14, 13);
		this.m_3.Name = "label1";
		this.m_3.Size = new Size(249, 16);
		this.m_3.TabIndex = 24;
		this.m_3.Text = "사용할 Wi-Fi 공유기 선택 (1/5)";
		this.m_0.Columns.AddRange(new ColumnHeader[4] { this.m_0, this.m_1, this.m_2, this.m_3 });
		this.m_0.Font = new Font("굴림체", 9f, FontStyle.Regular, GraphicsUnit.Point, 129);
		this.m_0.FullRowSelect = true;
		this.m_0.GridLines = true;
		this.m_0.HeaderStyle = ColumnHeaderStyle.Nonclickable;
		this.m_0.Location = new Point(12, 40);
		this.m_0.MultiSelect = false;
		this.m_0.Name = "lvAp";
		this.m_0.Size = new Size(497, 213);
		this.m_0.Sorting = SortOrder.Ascending;
		this.m_0.TabIndex = 15;
		this.m_0.UseCompatibleStateImageBehavior = false;
		this.m_0.View = View.Details;
		this.m_0.SelectedIndexChanged += _4;
		this.m_0.Text = "SSID";
		this.m_0.Width = 220;
		this.m_1.Text = "대역";
		this.m_1.Width = 40;
		this.m_2.Text = "신호 강도";
		this.m_2.Width = 75;
		this.m_3.Text = "보안 유형";
		this.m_3.Width = 140;
		this.m_1.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_1.Location = new Point(92, 290);
		this.m_1.MaxLength = 63;
		this.m_1.Name = "txtSSID";
		this.m_1.ReadOnly = true;
		this.m_1.Size = new Size(321, 26);
		this.m_1.TabIndex = 23;
		this.m_2.Enabled = false;
		this.m_2.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_2.Location = new Point(12, 257);
		this.m_2.Name = "btnApRefresh";
		this.m_2.Size = new Size(90, 30);
		this.m_2.TabIndex = 16;
		this.m_2.Text = "새로 고침";
		this.m_2.UseVisualStyleBackColor = true;
		this.m_2.Click += _2;
		this.m_2.AutoSize = true;
		this.m_2.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_2.Location = new Point(45, 294);
		this.m_2.Name = "lblSSID";
		this.m_2.Size = new Size(45, 16);
		this.m_2.TabIndex = 22;
		this.m_2.Text = "SSID";
		this.m_3.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_3.Location = new Point(419, 288);
		this.m_3.Name = "btnApConnect";
		this.m_3.Size = new Size(90, 57);
		this.m_3.TabIndex = 17;
		this.m_3.Text = "연결";
		this.m_3.UseVisualStyleBackColor = true;
		this.m_3.Visible = false;
		this.m_3.Click += _5;
		this.m_1.BackColor = Color.Khaki;
		this.m_1.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_1.Location = new Point(108, 261);
		this.m_1.Name = "lblInfo";
		this.m_1.Size = new Size(401, 23);
		this.m_1.TabIndex = 21;
		this.m_1.TextAlign = ContentAlignment.MiddleCenter;
		this.m_1.Visible = false;
		this.m_0.AutoSize = true;
		this.m_0.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_0.Location = new Point(14, 323);
		this.m_0.Name = "lblPassword";
		this.m_0.Size = new Size(76, 16);
		this.m_0.TabIndex = 20;
		this.m_0.Text = "비밀번호";
		this.m_0.Visible = false;
		this.m_0.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_0.Location = new Point(92, 319);
		this.m_0.MaxLength = 63;
		this.m_0.Name = "txtPassword";
		this.m_0.Size = new Size(321, 26);
		this.m_0.TabIndex = 19;
		this.m_0.Visible = false;
		this.m_0.TextChanged += _6;
		this.m_2.Controls.Add(this.m_0);
		this.m_2.Controls.Add(this.m_4);
		this.m_2.Controls.Add(this.m_9);
		this.m_2.Controls.Add(this.m_2);
		this.m_2.Controls.Add(this.m_8);
		this.m_2.Controls.Add(this.m_7);
		this.m_2.Controls.Add(this.m_5);
		this.m_2.Controls.Add(this.m_0);
		this.m_2.Controls.Add(this.m_1);
		this.m_2.Controls.Add(this.m_D);
		this.m_2.Location = new Point(4, 4);
		this.m_2.Name = "tabPage3";
		this.m_2.Padding = new Padding(3);
		this.m_2.Size = new Size(525, 351);
		this.m_2.TabIndex = 2;
		this.m_2.Text = "계정 연동";
		this.m_2.UseVisualStyleBackColor = true;
		this.m_0.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_0.Location = new Point(95, 48);
		this.m_0.Mask = "0000 0000";
		this.m_0.Name = "mtxtTempKey";
		this.m_0.ResetOnSpace = false;
		this.m_0.Size = new Size(100, 26);
		this.m_0.TabIndex = 31;
		this.m_0.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
		this.m_0.Click += F;
		this.m_0.KeyPress += _1;
		this.m_4.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_4.Location = new Point(181, 274);
		this.m_4.Name = "btnStep3";
		this.m_4.Size = new Size(160, 37);
		this.m_4.TabIndex = 30;
		this.m_4.Text = "다음 (제품 선택)";
		this.m_4.UseVisualStyleBackColor = true;
		this.m_4.Visible = false;
		this.m_4.Click += b;
		this.m_9.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_9.Location = new Point(23, 141);
		this.m_9.Name = "lblCheckAccount";
		this.m_9.Size = new Size(476, 107);
		this.m_9.TabIndex = 29;
		this.m_9.Text = "연동할 계정이 맞는지 확인하시고\r\n\r\n맞다면 \"다음 (제품 선택)\" 버튼을 누르시고\r\n\r\n맞지 않다면 인증번호를 새로 발급하여 다시 입력해주세요.";
		this.m_9.TextAlign = ContentAlignment.MiddleCenter;
		this.m_9.Visible = false;
		this.m_2.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_2.Location = new Point(95, 83);
		this.m_2.MaxLength = 50;
		this.m_2.Name = "txtAccount";
		this.m_2.ReadOnly = true;
		this.m_2.Size = new Size(402, 26);
		this.m_2.TabIndex = 28;
		this.m_8.AutoSize = true;
		this.m_8.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_8.Location = new Point(16, 88);
		this.m_8.Name = "label6";
		this.m_8.Size = new Size(76, 16);
		this.m_8.TabIndex = 27;
		this.m_8.Text = "연동계정";
		this.m_7.AutoSize = true;
		this.m_7.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_7.Location = new Point(16, 52);
		this.m_7.Name = "label5";
		this.m_7.Size = new Size(76, 16);
		this.m_7.TabIndex = 26;
		this.m_7.Text = "인증번호";
		this.m_5.AutoSize = true;
		this.m_5.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_5.Location = new Point(18, 16);
		this.m_5.Name = "label3";
		this.m_5.Size = new Size(213, 16);
		this.m_5.TabIndex = 25;
		this.m_5.Text = "AIPM 앱 계정과 연동 (2/5)";
		this.m_0.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_0.Location = new Point(201, 47);
		this.m_0.Name = "btnGetKey";
		this.m_0.Size = new Size(110, 28);
		this.m_0.TabIndex = 4;
		this.m_0.Text = "인증";
		this.m_0.UseVisualStyleBackColor = true;
		this.m_0.Click += _0;
		this.m_1.BackgroundImage = (Image)componentResourceManager.GetObject("picPcKey.BackgroundImage");
		this.m_1.BorderStyle = BorderStyle.FixedSingle;
		this.m_1.Location = new Point(67, 121);
		this.m_1.Name = "picPcKey";
		this.m_1.Size = new Size(386, 163);
		this.m_1.TabIndex = 32;
		this.m_1.TabStop = false;
		this.m_D.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_D.Location = new Point(54, 288);
		this.m_D.Name = "lblPcKey";
		this.m_D.Size = new Size(413, 54);
		this.m_D.TabIndex = 33;
		this.m_D.Text = "AIPM 앱의 My page 화면에서 발급 버튼을 눌러\r\n\r\n생성된 인증번호 8자리를 입력해주세요.";
		this.m_D.TextAlign = ContentAlignment.MiddleCenter;
		this.m_3.Controls.Add(this.m_5);
		this.m_3.Controls.Add(this.m_1);
		this.m_3.Controls.Add(this.m_6);
		this.m_3.Location = new Point(4, 4);
		this.m_3.Name = "tabPage4";
		this.m_3.Padding = new Padding(3);
		this.m_3.Size = new Size(525, 351);
		this.m_3.TabIndex = 3;
		this.m_3.Text = "제품 선택";
		this.m_3.UseVisualStyleBackColor = true;
		this.m_5.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_5.Location = new Point(185, 302);
		this.m_5.Name = "btnStep4";
		this.m_5.Size = new Size(156, 37);
		this.m_5.TabIndex = 31;
		this.m_5.Text = "다음 (장치 선택)";
		this.m_5.UseVisualStyleBackColor = true;
		this.m_5.Visible = false;
		this.m_5.Click += c;
		listViewItem.Tag = "S120";
		listViewItem.ToolTipText = "스마트 플러그 (16A)";
		listViewItem2.Tag = "ES120";
		listViewItem2.ToolTipText = "스마트 플러그 (16A)";
		listViewItem3.Tag = "LS130";
		listViewItem3.ToolTipText = "스마트 플러그 (16A)";
		listViewItem4.Tag = "S220";
		listViewItem4.ToolTipText = "스마트 멀티탭 (16A)";
		listViewItem5.Tag = "LS810";
		listViewItem5.ToolTipText = "지그비 허브";
		listViewItem6.Tag = "S510";
		listViewItem6.ToolTipText = "IR 리모컨";
		listViewItem7.Tag = "S501";
		listViewItem7.ToolTipText = "IR 리모컨";
		listViewItem8.Tag = "S310";
		listViewItem8.ToolTipText = "분전반 단상 (50A)";
		listViewItem9.Tag = "S330";
		listViewItem9.ToolTipText = "분전반 3상 (100A)";
		listViewItem10.Tag = "S350";
		listViewItem10.ToolTipText = "분전반 3상 (400A)";
		listViewItem11.Tag = "S370";
		listViewItem11.ToolTipText = "분전반 3상 (800A)";
		listViewItem12.Tag = "ES120S";
		listViewItem12.ToolTipText = "태양광 스마트 플러그 (16A)";
		listViewItem13.Tag = "S600";
		listViewItem13.ToolTipText = "태양광 스마트 플러그 (10A)";
		listViewItem14.Tag = "S110";
		listViewItem14.ToolTipText = "스마트 플러그 (10A)";
		listViewItem15.Tag = "S121";
		listViewItem15.ToolTipText = "일본향 스마트 플러그";
		this.m_1.Items.AddRange(new ListViewItem[15]
		{
			listViewItem, listViewItem2, listViewItem3, listViewItem4, listViewItem5, listViewItem6, listViewItem7, listViewItem8, listViewItem9, listViewItem10,
			listViewItem11, listViewItem12, listViewItem13, listViewItem14, listViewItem15
		});
		this.m_1.LargeImageList = this.m_0;
		this.m_1.Location = new Point(7, 42);
		this.m_1.MultiSelect = false;
		this.m_1.Name = "lvModel";
		this.m_1.ShowItemToolTips = true;
		this.m_1.Size = new Size(510, 246);
		this.m_1.TabIndex = 27;
		this.m_1.UseCompatibleStateImageBehavior = false;
		this.m_1.SelectedIndexChanged += B;
		this.m_1.MouseDoubleClick += _0;
		this.m_6.AutoSize = true;
		this.m_6.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_6.Location = new Point(17, 14);
		this.m_6.Name = "label4";
		this.m_6.Size = new Size(184, 16);
		this.m_6.TabIndex = 26;
		this.m_6.Text = "등록할 제품 선택 (3/5)";
		this.m_4.Controls.Add(this.m_5);
		this.m_4.Controls.Add(this.m_c);
		this.m_4.Controls.Add(this.m_8);
		this.m_4.Controls.Add(this.m_3);
		this.m_4.Controls.Add(this.m_6);
		this.m_4.Controls.Add(this.m_A);
		this.m_4.Controls.Add(this.m_7);
		this.m_4.Controls.Add(this.m_b);
		this.m_4.Controls.Add(this.m_B);
		this.m_4.Controls.Add(this.m_4);
		this.m_4.Controls.Add(this.m_2);
		this.m_4.Controls.Add(this.m_a);
		this.m_4.Location = new Point(4, 4);
		this.m_4.Name = "tabPage5";
		this.m_4.Padding = new Padding(3);
		this.m_4.Size = new Size(525, 351);
		this.m_4.TabIndex = 4;
		this.m_4.Text = "장치 선택";
		this.m_4.UseVisualStyleBackColor = true;
		this.m_5.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_5.Location = new Point(94, 260);
		this.m_5.MaxLength = 63;
		this.m_5.Name = "txtModel";
		this.m_5.ReadOnly = true;
		this.m_5.Size = new Size(321, 26);
		this.m_5.TabIndex = 38;
		this.m_c.AutoSize = true;
		this.m_c.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_c.Location = new Point(10, 263);
		this.m_c.Name = "label9";
		this.m_c.Size = new Size(82, 16);
		this.m_c.TabIndex = 37;
		this.m_c.Text = "선택 제품";
		this.m_8.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_8.Location = new Point(358, 6);
		this.m_8.Name = "btnPreStep3";
		this.m_8.Size = new Size(153, 30);
		this.m_8.TabIndex = 36;
		this.m_8.Text = "이전 (제품 선택)";
		this.m_8.UseVisualStyleBackColor = true;
		this.m_8.Click += d;
		this.m_3.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_3.Location = new Point(94, 289);
		this.m_3.MaxLength = 63;
		this.m_3.Name = "txtDeviceSSID";
		this.m_3.ReadOnly = true;
		this.m_3.Size = new Size(321, 26);
		this.m_3.TabIndex = 35;
		this.m_6.Enabled = false;
		this.m_6.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_6.Location = new Point(14, 226);
		this.m_6.Name = "btnDeviceRefresh";
		this.m_6.Size = new Size(90, 30);
		this.m_6.TabIndex = 29;
		this.m_6.Text = "새로 고침";
		this.m_6.UseVisualStyleBackColor = true;
		this.m_6.Click += C;
		this.m_A.AutoSize = true;
		this.m_A.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_A.Location = new Point(47, 293);
		this.m_A.Name = "label8";
		this.m_A.Size = new Size(45, 16);
		this.m_A.TabIndex = 34;
		this.m_A.Text = "SSID";
		this.m_7.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_7.Location = new Point(421, 260);
		this.m_7.Name = "btnDeviceConnect";
		this.m_7.Size = new Size(90, 84);
		this.m_7.TabIndex = 30;
		this.m_7.Text = "등록";
		this.m_7.UseVisualStyleBackColor = true;
		this.m_7.Visible = false;
		this.m_7.Click += e;
		this.m_b.BackColor = Color.Khaki;
		this.m_b.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_b.Location = new Point(110, 230);
		this.m_b.Name = "lblDeviceInfo";
		this.m_b.Size = new Size(401, 23);
		this.m_b.TabIndex = 33;
		this.m_b.TextAlign = ContentAlignment.MiddleCenter;
		this.m_b.Visible = false;
		this.m_B.AutoSize = true;
		this.m_B.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_B.Location = new Point(47, 321);
		this.m_B.Name = "label10";
		this.m_B.Size = new Size(45, 16);
		this.m_B.TabIndex = 32;
		this.m_B.Text = "MAC";
		this.m_4.Font = new Font("굴림체", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_4.Location = new Point(94, 318);
		this.m_4.MaxLength = 63;
		this.m_4.Name = "txtDeviceMAC";
		this.m_4.ReadOnly = true;
		this.m_4.Size = new Size(321, 26);
		this.m_4.TabIndex = 31;
		this.m_2.Columns.AddRange(new ColumnHeader[3] { this.m_4, this.m_5, this.m_6 });
		this.m_2.Font = new Font("굴림체", 9f, FontStyle.Regular, GraphicsUnit.Point, 129);
		this.m_2.FullRowSelect = true;
		this.m_2.GridLines = true;
		this.m_2.HeaderStyle = ColumnHeaderStyle.Nonclickable;
		this.m_2.Location = new Point(14, 42);
		this.m_2.MultiSelect = false;
		this.m_2.Name = "lvDevice";
		this.m_2.Size = new Size(497, 180);
		this.m_2.Sorting = SortOrder.Ascending;
		this.m_2.TabIndex = 28;
		this.m_2.UseCompatibleStateImageBehavior = false;
		this.m_2.View = View.Details;
		this.m_2.SelectedIndexChanged += D;
		this.m_4.Text = "SSID";
		this.m_4.Width = 120;
		this.m_5.Text = "MAC";
		this.m_5.Width = 100;
		this.m_6.Text = "신호 강도";
		this.m_6.Width = 75;
		this.m_a.AutoSize = true;
		this.m_a.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_a.Location = new Point(17, 14);
		this.m_a.Name = "label7";
		this.m_a.Size = new Size(185, 16);
		this.m_a.TabIndex = 27;
		this.m_a.Text = "등록할 장치 선택 (4/5)";
		this.m_5.Controls.Add(this.m_a);
		this.m_5.Controls.Add(this.m_9);
		this.m_5.Controls.Add(this.m_0);
		this.m_5.Controls.Add(this.m_d);
		this.m_5.Controls.Add(this.m_C);
		this.m_5.Location = new Point(4, 4);
		this.m_5.Name = "tabPage6";
		this.m_5.Padding = new Padding(3);
		this.m_5.Size = new Size(525, 351);
		this.m_5.TabIndex = 5;
		this.m_5.Text = "등록 진행";
		this.m_5.UseVisualStyleBackColor = true;
		this.m_a.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_a.Location = new Point(300, 282);
		this.m_a.Name = "btnPreStep2";
		this.m_a.Size = new Size(160, 37);
		this.m_a.TabIndex = 33;
		this.m_a.Text = "계정 연동 해제";
		this.m_a.UseVisualStyleBackColor = true;
		this.m_a.Visible = false;
		this.m_a.Click += h;
		this.m_9.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_9.Location = new Point(68, 282);
		this.m_9.Name = "btnPreStep3Re";
		this.m_9.Size = new Size(160, 37);
		this.m_9.TabIndex = 31;
		this.m_9.Text = "이전 (제품 선택)";
		this.m_9.UseVisualStyleBackColor = true;
		this.m_9.Visible = false;
		this.m_9.Click += g;
		this.m_0.Location = new Point(18, 80);
		this.m_0.Maximum = 20;
		this.m_0.Name = "progressBar1";
		this.m_0.Size = new Size(490, 23);
		this.m_0.Step = 1;
		this.m_0.TabIndex = 30;
		this.m_d.BackColor = Color.Khaki;
		this.m_d.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_d.Location = new Point(15, 41);
		this.m_d.Name = "lblRegInfo";
		this.m_d.Size = new Size(493, 23);
		this.m_d.TabIndex = 29;
		this.m_d.TextAlign = ContentAlignment.MiddleCenter;
		this.m_C.AutoSize = true;
		this.m_C.Font = new Font("굴림", 12f, FontStyle.Bold, GraphicsUnit.Point, 129);
		this.m_C.Location = new Point(17, 14);
		this.m_C.Name = "label11";
		this.m_C.Size = new Size(127, 16);
		this.m_C.TabIndex = 28;
		this.m_C.Text = "등록 진행 (5/5)";
		base.AutoScaleMode = AutoScaleMode.None;
		base.ClientSize = new Size(541, 612);
		base.Controls.Add(this.m_0);
		base.Controls.Add(this.m_0);
		base.FormBorderStyle = FormBorderStyle.FixedSingle;
		base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "frmMain";
		Text = "AIPM Register";
		base.FormClosing += _0;
		this.m_0.ResumeLayout(performLayout: false);
		this.m_0.ResumeLayout(performLayout: false);
		((ISupportInitialize)this.m_0).EndInit();
		this.m_1.ResumeLayout(performLayout: false);
		this.m_1.PerformLayout();
		this.m_2.ResumeLayout(performLayout: false);
		this.m_2.PerformLayout();
		((ISupportInitialize)this.m_1).EndInit();
		this.m_3.ResumeLayout(performLayout: false);
		this.m_3.PerformLayout();
		this.m_4.ResumeLayout(performLayout: false);
		this.m_4.PerformLayout();
		this.m_5.ResumeLayout(performLayout: false);
		this.m_5.PerformLayout();
		ResumeLayout(performLayout: false);
	}

	public frmMain()
	{
		_0();
		string[] array = Application.ProductVersion.Split('.');
		string text = Text;
		Text = text + " v" + array[0] + "." + array[1];
		this.m_0.Height -= 20;
		base.Controls.Remove(this.m_0);
		base.ClientSize = new Size(base.ClientSize.Width, this.m_0.Size.Height + 10);
	}

	private void _0(string P_0, HttpMethod P_1, string P_2, bool P_3)
	{
		if (P_3)
		{
			this.m_0++;
		}
		if (!this.m_7.Enabled)
		{
			this.m_7.Enabled = true;
		}
		ThreadPool.QueueUserWorkItem(_0, new string[4]
		{
			P_0,
			P_1.ToString(),
			P_2,
			P_3 ? "1" : "0"
		});
	}

	private void _0(object P_0)
	{
		string[] array = (string[])P_0;
		string text = array[0];
		string method = array[1];
		string text2 = array[2];
		string text3 = array[3];
		string text4 = text;
		_0("Request : " + this.m_0 + " : " + text4 + " : " + text2);
		text = "https://dwapi.dawonai.com:18443/api/" + text;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);
		httpWebRequest.Method = method;
		httpWebRequest.ContentType = "application/json";
		httpWebRequest.Accept = "application/json";
		httpWebRequest.Headers.Add("Cache-Control", "no-cache");
		httpWebRequest.Headers.Add("X-HIT-Version", "1.0");
		if (text2 != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text2);
			Stream requestStream = httpWebRequest.GetRequestStream();
			requestStream.Write(bytes, 0, bytes.Length);
			requestStream.Close();
		}
		if (text3.Equals("0"))
		{
			return;
		}
		StreamReader streamReader = null;
		HttpWebResponse httpWebResponse;
		try
		{
			httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
		}
		catch (WebException ex)
		{
			httpWebResponse = (HttpWebResponse)ex.Response;
		}
		string text5 = ((int)httpWebResponse.StatusCode).ToString();
		streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
		string text6 = "";
		if (streamReader != null)
		{
			text6 = streamReader.ReadToEnd();
			streamReader.Close();
		}
		_0("ThreadPool : " + text5 + " : " + text6);
		lock (this.m_1)
		{
			this.m_0.Enqueue(new string[3]
			{
				text5,
				text6.Replace(" ", ""),
				text4
			});
		}
	}

	private void _0(string P_0)
	{
	}

	private string _0(string P_0)
	{
		string text = "";
		int i = 0;
		for (int length = P_0.Length; i < length; i++)
		{
			if (P_0[i] == ':')
			{
				object obj = text;
				text = string.Concat(obj, "\"", P_0[i], (P_0[i + 1] != '{' && P_0[i + 1] != '[') ? "\"" : "");
			}
			else if (P_0[i] == '{')
			{
				text = text + P_0[i] + "\"";
			}
			else if (P_0[i] == ',')
			{
				object obj2 = text;
				text = string.Concat(obj2, (P_0[i - 1] != '}' && P_0[i - 1] != ']') ? "\"" : "", P_0[i], (P_0[i + 1] != '{') ? "\"" : "");
			}
			else
			{
				text = ((P_0[i] != '}' || P_0[i - 1] == '}' || P_0[i - 1] == ']') ? (text + P_0[i]) : (text + "\"" + P_0[i]));
			}
		}
		return text;
	}

	private void _1()
	{
		if (this.m_d.Length != 0 && this.m_D.Length != 0 && this.m_e.Length != 0)
		{
			string text = _0("{account:{pc_key:" + this.m_e + ",user_id:" + this.m_d + "},devices:[{device_id:" + this.m_D + ",msg:{e:[{n:/100/0/31}],o:r}}]}");
			_0("v1/devices/control/check", HttpMethod.POST, text, true);
		}
	}

	private void _0(object P_0, EventArgs P_1)
	{
		if (!this.m_0.MaskCompleted)
		{
			MessageBox.Show("공백 없이 숫자 8자리를 입력해주세요.");
			return;
		}
		this.m_9.Visible = false;
		this.m_4.Visible = false;
		this.m_2.Text = "";
		this.m_e = "";
		this.m_0.Enabled = false;
		string text = _0("{account:{pc_temp_key:" + this.m_0.Text + "}}");
		_0(text);
		_0("v1/accounts/post/getPckey", HttpMethod.POST, text, true);
	}

	private void _2()
	{
		if (this.m_e.Length != 0 && this.m_0 == null && this.m_0 != null && this.m_0.InterfaceState == Wlan.WlanInterfaceState.Connected && _1())
		{
			string text = _0("{account:{pc_key:" + this.m_e + ",user_id:" + this.m_d + "}}");
			_0("v1/accounts/put/deletePckey", HttpMethod.PUT, text, false);
			this.m_e = "";
		}
	}

	private void _0(object P_0, KeyPressEventArgs P_1)
	{
		if (!char.IsDigit(P_1.KeyChar) && P_1.KeyChar != Convert.ToChar(Keys.Back))
		{
			P_1.Handled = true;
		}
	}

	private void _1(object P_0, EventArgs P_1)
	{
		switch (_0())
		{
		case 0:
			MessageBox.Show("사용가능한 무선 네트워크 장치가 없습니다.", "확인");
			return;
		case 1:
			MessageBox.Show("무선 네트워크 장치가 꺼져있습니다.\n장치를 직접 켜주세요.", "확인");
			return;
		case 2:
			if (MessageBox.Show("무선 네트워크 장치가 꺼져있습니다.\n장치를 켜겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (!this.m_0.SetRadioState(this.m_0.InterfaceGuid, Wlan.Dot11RadioState.On))
				{
					MessageBox.Show("장치를 켜는데 실패하였습니다.\n장치를 직접 켜주세요.", "확인");
					return;
				}
				break;
			}
			return;
		case 3:
			if (this.m_0.InterfaceState == Wlan.WlanInterfaceState.Connected && MessageBox.Show("무선 네트워크 연결이 끊어집니다.\n시작하겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.No)
			{
				return;
			}
			break;
		}
		this.m_6 = 1;
		this.m_0.SelectedIndex = this.m_6;
		this.m_0.WlanNotification -= _0;
		this.m_0.WlanNotification += _0;
		_3();
		this.m_2.Enabled = true;
		this.m_2.PerformClick();
	}

	private void _3()
	{
		if (this.m_0 != null)
		{
			try
			{
				if (this.m_0.Connected)
				{
					this.m_0.Close();
					this.m_0 = null;
					_0("Client Closed");
				}
			}
			catch (Exception)
			{
			}
		}
		try
		{
			if (this.m_0 != null)
			{
				if (this.m_0.InterfaceState == Wlan.WlanInterfaceState.Connected)
				{
					this.m_0.Disconnect();
					_0("AP Disconnected");
				}
				if (this.m_b.Length != 0 && _0(this.m_b))
				{
					this.m_0.DeleteProfile(this.m_b);
				}
			}
		}
		catch (Exception ex2)
		{
			_0(ex2.Message);
		}
	}

	private void _4()
	{
		if (this.m_0 != null)
		{
			try
			{
				if (this.m_0.Connected)
				{
					this.m_0.Close();
					this.m_0 = null;
					_0("Client Closed");
				}
			}
			catch (Exception)
			{
			}
		}
		try
		{
			if (this.m_0 != null && this.m_b.Length != 0 && _0(this.m_b))
			{
				this.m_0.DeleteProfile(this.m_b);
			}
		}
		catch (Exception ex2)
		{
			_0(ex2.Message);
		}
	}

	private int _0()
	{
		if (this.m_0.Interfaces.Length == 0)
		{
			return 0;
		}
		if (this.m_0 == null)
		{
			this.m_0 = this.m_0.Interfaces[0];
		}
		if (this.m_0.RadioState.PhyRadioState[0].dot11HardwareRadioState == Wlan.Dot11RadioState.Off)
		{
			return 1;
		}
		if (this.m_0.RadioState.PhyRadioState[0].dot11SoftwareRadioState == Wlan.Dot11RadioState.Off)
		{
			return 2;
		}
		return 3;
	}

	private void _2(object P_0, EventArgs P_1)
	{
		this.m_0.Items.Clear();
		this.m_1.Text = "";
		this.m_0.Visible = false;
		this.m_0.Visible = false;
		this.m_1.Visible = false;
		this.m_3.Visible = false;
		try
		{
			if (this.m_0 != null && this.m_1 == 0)
			{
				this.m_1 = -1;
				this.m_0 = DateTime.Now;
				this.m_0.Enabled = true;
				this.m_2.Enabled = false;
				_0("AP Scan Start");
				this.m_0.Scan();
			}
		}
		catch (Exception ex)
		{
			_0("btnApRefresh_Click Error : " + ex.Message);
		}
	}

	private void _5()
	{
		try
		{
			this.m_0.Clear();
			Wlan.WlanAvailableNetwork[] availableNetworkList = this.m_0.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
			for (int i = 0; i < availableNetworkList.Length; i++)
			{
				Wlan.WlanAvailableNetwork wlanAvailableNetwork = availableNetworkList[i];
				string text = _0(wlanAvailableNetwork.dot11Ssid);
				char[] trimChars = new char[1];
				if (text.Trim(trimChars).Length > 0 && !text.StartsWith("DAWON_IRBD_") && !text.StartsWith("DWD-") && !this.m_0.ContainsKey(text))
				{
					this.m_0.Add(text, _0(wlanAvailableNetwork.dot11DefaultCipherAlgorithm, wlanAvailableNetwork.dot11DefaultAuthAlgorithm));
				}
			}
			Wlan.WlanBssEntry[] networkBssList = this.m_0.GetNetworkBssList();
			for (int j = 0; j < networkBssList.Length; j++)
			{
				Wlan.WlanBssEntry wlanBssEntry = networkBssList[j];
				string text2 = _0(wlanBssEntry.dot11Ssid);
				char[] trimChars2 = new char[1];
				if (text2.Trim(trimChars2).Length > 0 && !text2.StartsWith("DAWON_IRBD_") && !text2.StartsWith("DWD-"))
				{
					uint chCenterFrequency = wlanBssEntry.chCenterFrequency;
					string text3 = chCenterFrequency.ToString().Substring(0, 1) + "G";
					string text4 = (this.m_0.ContainsKey(text2) ? ((string)this.m_0[text2]) : "");
					string text5 = "■";
					if (wlanBssEntry.linkQuality > 20)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 40)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 60)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 80)
					{
						text5 += "■";
					}
					ListViewItem listViewItem = new ListViewItem(text2);
					listViewItem.SubItems.Add(text3);
					listViewItem.SubItems.Add(text5);
					listViewItem.SubItems.Add(text4);
					if (text3.Equals("2G") && (text4.Equals("WPA-개인 TKIP") || text4.Equals("WPA2-개인 AES") || text4.Equals("WEP")))
					{
						listViewItem.BackColor = Color.GreenYellow;
					}
					else
					{
						listViewItem.BackColor = Color.Orange;
					}
					this.m_0.Items.Add(listViewItem);
				}
			}
		}
		catch (Exception ex)
		{
			_0("GetApList Error : " + ex.Message);
		}
	}

	private void _6()
	{
		try
		{
			this.m_0.Clear();
			Wlan.WlanAvailableNetwork[] availableNetworkList = this.m_0.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
			for (int i = 0; i < availableNetworkList.Length; i++)
			{
				Wlan.WlanAvailableNetwork wlanAvailableNetwork = availableNetworkList[i];
				string text = _0(wlanAvailableNetwork.dot11Ssid);
				if ((text.StartsWith(this.m_B) || (this.m_c.Length != 0 && text.StartsWith(this.m_c))) && !this.m_0.ContainsKey(text))
				{
					this.m_0.Add(text, _0(wlanAvailableNetwork.dot11DefaultCipherAlgorithm, wlanAvailableNetwork.dot11DefaultAuthAlgorithm));
				}
			}
			Wlan.WlanBssEntry[] networkBssList = this.m_0.GetNetworkBssList();
			for (int j = 0; j < networkBssList.Length; j++)
			{
				Wlan.WlanBssEntry wlanBssEntry = networkBssList[j];
				string text2 = _0(wlanBssEntry.dot11Ssid);
				if (!text2.StartsWith(this.m_B) && (this.m_c.Length == 0 || !text2.StartsWith(this.m_c)))
				{
					continue;
				}
				uint chCenterFrequency = wlanBssEntry.chCenterFrequency;
				string text3 = chCenterFrequency.ToString().Substring(0, 1) + "G";
				string text4 = (this.m_0.ContainsKey(text2) ? ((string)this.m_0[text2]) : "");
				if (text3.Equals("2G") && text4.Equals("개방형"))
				{
					string text5 = "■";
					if (wlanBssEntry.linkQuality > 20)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 40)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 60)
					{
						text5 += "■";
					}
					if (wlanBssEntry.linkQuality > 80)
					{
						text5 += "■";
					}
					ListViewItem listViewItem = new ListViewItem(text2);
					listViewItem.SubItems.Add(_0(wlanBssEntry.dot11Bssid));
					listViewItem.SubItems.Add(text5);
					listViewItem.BackColor = Color.GreenYellow;
					this.m_2.Items.Add(listViewItem);
				}
			}
			if (this.m_2.Items.Count == 0)
			{
				this.m_b.Text = "해당 제품이 검색되지 않았습니다.";
				this.m_b.Visible = true;
			}
		}
		catch (Exception ex)
		{
			_0("GetDeviceList Error : " + ex.Message);
		}
	}

	private string _0(byte[] P_0)
	{
		int num = P_0.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = P_0[i].ToString("x2");
		}
		return string.Join("", array);
	}

	private string _0(Wlan.Dot11Ssid P_0)
	{
		return Encoding.UTF8.GetString(P_0.SSID, 0, (int)P_0.SSIDLength);
	}

	private string _0(Wlan.Dot11CipherAlgorithm P_0, Wlan.Dot11AuthAlgorithm P_1)
	{
		switch (P_0)
		{
		case Wlan.Dot11CipherAlgorithm.None:
			return "개방형";
		case Wlan.Dot11CipherAlgorithm.TKIP:
			if (P_1 == Wlan.Dot11AuthAlgorithm.RSNA)
			{
				return "WPA-기업 TKIP";
			}
			return "WPA-개인 TKIP";
		case Wlan.Dot11CipherAlgorithm.CCMP:
			if (P_1 == Wlan.Dot11AuthAlgorithm.RSNA)
			{
				return "WPA2-기업 AES";
			}
			return "WPA2-개인 AES";
		case Wlan.Dot11CipherAlgorithm.WEP:
			return "WEP";
		default:
			return P_0.ToString() + "-" + P_1;
		}
	}

	private void _3(object P_0, EventArgs P_1)
	{
		if (this.m_1 == 0)
		{
			return;
		}
		if (this.m_1 == -1)
		{
			if (this.m_0 > DateTime.Now.AddSeconds(-5.0))
			{
				return;
			}
			_0("AP Scan Time Over");
		}
		else if (this.m_1 == 1)
		{
			_0("AP Scan Complete");
			if (this.m_6 == 1)
			{
				_5();
			}
			else
			{
				_6();
			}
		}
		this.m_1 = 0;
		this.m_0.Enabled = false;
		if (this.m_6 == 1)
		{
			this.m_2.Enabled = true;
		}
		else
		{
			this.m_6.Enabled = true;
		}
	}

	private void _0(Wlan.WlanNotificationData P_0)
	{
		if (P_0.notificationSource != Wlan.WlanNotificationSource.ACM)
		{
			return;
		}
		switch ((Wlan.WlanNotificationCodeAcm)P_0.notificationCode)
		{
		case Wlan.WlanNotificationCodeAcm.Disconnected:
			_0("Wifi : Disconnected");
			break;
		case Wlan.WlanNotificationCodeAcm.ConnectionComplete:
			_0("Wifi : ConnectionComplete");
			break;
		case Wlan.WlanNotificationCodeAcm.ScanComplete:
			if (this.m_1 == -1)
			{
				this.m_1 = 1;
			}
			break;
		case Wlan.WlanNotificationCodeAcm.ScanFail:
			if (this.m_1 == -1)
			{
				this.m_1 = 2;
			}
			break;
		}
	}

	private void _4(object P_0, EventArgs P_1)
	{
		if (this.m_0.SelectedIndices.Count == 0)
		{
			return;
		}
		ListViewItem listViewItem = this.m_0.SelectedItems[0];
		string text = listViewItem.Text;
		this.m_1.Text = text;
		this.m_0.Text = "";
		this.m_1.Visible = false;
		this.m_0.Visible = false;
		this.m_0.Visible = false;
		this.m_3.Visible = ((listViewItem.BackColor == Color.GreenYellow) ? true : false);
		if (!listViewItem.SubItems[1].Text.Equals("2G"))
		{
			this.m_1.Text = "2G 대역만 연결 가능합니다.";
			this.m_1.Visible = true;
			return;
		}
		string text2 = listViewItem.SubItems[3].Text;
		if (!text2.Equals("WPA-개인 TKIP") && !text2.Equals("WPA2-개인 AES") && !text2.Equals("WEP"))
		{
			if (text2.Equals("개방형"))
			{
				this.m_1.Text = "개방형은 연결할 수 없습니다.";
				this.m_1.Visible = true;
			}
			else if (text2.IndexOf("기업") > 0)
			{
				this.m_1.Text = "기업용 보안 유형은 연결할 수 없습니다.";
				this.m_1.Visible = true;
			}
			else
			{
				this.m_1.Text = text2 + " 보안 유형은 연결할 수 없습니다.";
				this.m_1.Visible = true;
			}
			return;
		}
		this.m_0.Visible = true;
		this.m_0.Visible = true;
		if (this.m_3.Visible)
		{
			if (_0(text))
			{
				this.m_0.Text = _2(this.m_0.GetProfileXmlUnencrypted(text));
				return;
			}
			this.m_1.Text = "비밀번호를 입력하세요.";
			this.m_1.Visible = true;
		}
	}

	private void _5(object P_0, EventArgs P_1)
	{
		if (this.m_0.SelectedIndices.Count == 0)
		{
			return;
		}
		if (this.m_0.Visible && this.m_0.Text.Length == 0)
		{
			MessageBox.Show("비밀번호를 입력하세요.");
			return;
		}
		string text = this.m_0.SelectedItems[0].SubItems[3].Text;
		if (text.StartsWith("WPA") && this.m_0.Text.Length < 8)
		{
			MessageBox.Show("비밀번호를 8자리 이상 입력하세요.");
			return;
		}
		string text2 = this.m_1.Text;
		string text3 = _0(text2, text, this.m_0.Text);
		if (text3.Length > 0)
		{
			_3();
			this.m_3.Enabled = false;
			this.m_8 = text2;
			this.m_9 = this.m_0.Text;
			this.m_a = text;
			this.m_1.Text = "연결중...";
			this.m_1.Visible = true;
			this.m_0.SetProfile(Wlan.WlanProfileFlags.AllUser, text3, overwrite: true);
			this.m_0.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, text2);
			this.m_2 = 0;
			this.m_1.Enabled = true;
		}
	}

	private bool _0(string P_0)
	{
		Wlan.WlanProfileInfo[] profiles = this.m_0.GetProfiles();
		for (int i = 0; i < profiles.Length; i++)
		{
			Wlan.WlanProfileInfo wlanProfileInfo = profiles[i];
			if (wlanProfileInfo.profileName.Equals(P_0))
			{
				return true;
			}
		}
		return false;
	}

	private string _0(string P_0, string P_1, string P_2)
	{
		string text = "<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>manual</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{2}</authentication><encryption>{3}</encryption><useOneX>false</useOneX></authEncryption>";
		if (P_1.Equals("개방형"))
		{
			string text2 = "open";
			string text3 = "none";
			text = string.Format(text, P_0, _1(P_0), text2, text3);
		}
		else
		{
			string text2;
			string text3;
			if (P_1.Equals("WPA-개인 TKIP"))
			{
				text2 = "WPAPSK";
				text3 = "TKIP";
			}
			else if (P_1.Equals("WPA2-개인 AES"))
			{
				text2 = "WPA2PSK";
				text3 = "AES";
			}
			else
			{
				if (!P_1.Equals("WEP"))
				{
					return "";
				}
				text2 = "open";
				text3 = "WEP";
			}
			text += "<sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>{4}</keyMaterial></sharedKey>";
			text = string.Format(text, P_0, _1(P_0), text2, text3, P_2);
		}
		return text + "</security></MSM></WLANProfile>";
	}

	private string _1(string P_0)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(P_0);
		StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
		byte[] array = bytes;
		foreach (byte b in array)
		{
			if (b == 0)
			{
				break;
			}
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString().ToUpper();
	}

	private string _2(string P_0)
	{
		_0(P_0);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(P_0);
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("protected");
		if (elementsByTagName.Count > 0 && elementsByTagName[0].InnerText.ToLower().Equals("false"))
		{
			elementsByTagName = xmlDocument.GetElementsByTagName("keyMaterial");
			if (elementsByTagName.Count > 0)
			{
				return elementsByTagName[0].InnerText;
			}
		}
		return "";
	}

	private void _6(object P_0, EventArgs P_1)
	{
		if (this.m_0.Visible)
		{
			if (this.m_0.Text.Length == 0)
			{
				this.m_1.Text = "비밀번호를 입력하세요.";
				this.m_1.Visible = true;
			}
			else
			{
				this.m_1.Visible = false;
			}
		}
	}

	private void _7(object P_0, EventArgs P_1)
	{
		if (++this.m_2 > 24)
		{
			this.m_2 = 0;
			this.m_1.Enabled = false;
			_0("AP 연결 실패");
			if (this.m_6 == 1)
			{
				this.m_3.Enabled = true;
				this.m_1.Text = "Wi-Fi 공유기 연결 실패";
				this.m_1.Visible = true;
			}
			else if (this.m_6 == 4)
			{
				this.m_7.Enabled = true;
				this.m_b.Text = "장치 연결 실패";
				this.m_b.Visible = true;
			}
			else if (this.m_6 == 5)
			{
				this.m_d.Text = "Wi-Fi 공유기 연결 실패";
				if (MessageBox.Show("Wi-Fi 공유기 연결에 실패하였습니다\n재시도하겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					this.m_d.Text = "Wi-Fi 공유기 연결중...";
					_3();
					this.m_0.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, this.m_8);
					this.m_2 = 0;
					this.m_1.Enabled = true;
				}
			}
			return;
		}
		try
		{
			if (this.m_0.InterfaceState == Wlan.WlanInterfaceState.Connected)
			{
				this.m_2 = 0;
				this.m_1.Enabled = false;
				_0("AP 연결 성공");
				this.m_3 = 0;
				this.m_2.Enabled = true;
			}
		}
		catch (Exception ex)
		{
			this.m_2 = 0;
			this.m_1.Enabled = false;
			if (this.m_6 == 1)
			{
				this.m_3.Enabled = true;
			}
			else if (this.m_6 == 4)
			{
				this.m_7.Enabled = true;
			}
			_0("AP Error : " + ex.Message);
		}
	}

	private bool _0()
	{
		if (NetworkInterface.GetIsNetworkAvailable())
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 || networkInterface.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				foreach (GatewayIPAddressInformation gatewayAddress in networkInterface.GetIPProperties().GatewayAddresses)
				{
					if (IPAddress.TryParse(gatewayAddress.Address.ToString(), out var address) && !gatewayAddress.Address.IsIPv6LinkLocal && !address.ToString().Equals("0.0.0.0"))
					{
						if (this.m_6 == 4)
						{
							this.m_C = address.ToString();
						}
						_0(address.ToString());
						return true;
					}
				}
			}
		}
		return false;
	}

	private void _8(object P_0, EventArgs P_1)
	{
		if (++this.m_3 > 24)
		{
			this.m_3 = 0;
			this.m_2.Enabled = false;
			_0("Server IP Fail");
			if (this.m_6 == 1)
			{
				this.m_1.Text = "IP 가져오기 실패";
				this.m_1.Visible = true;
				this.m_3.Enabled = true;
			}
			else if (this.m_6 == 4)
			{
				this.m_b.Text = "IP 가져오기 실패";
				this.m_b.Visible = true;
				this.m_7.Enabled = true;
			}
			else if (this.m_6 == 5)
			{
				this.m_d.Text = "IP 가져오기 실패";
				if (MessageBox.Show("IP 가져오기에 실패하였습니다\n재시도하겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					this.m_d.Text = "Wi-Fi 공유기 연결중...";
					_3();
					this.m_0.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, this.m_8);
					this.m_2 = 0;
					this.m_1.Enabled = true;
				}
			}
		}
		else if (_0())
		{
			this.m_3 = 0;
			this.m_2.Enabled = false;
			_0("Server IP Success");
			if (this.m_6 == 1 || this.m_6 == 5)
			{
				this.m_4 = 0;
				this.m_3.Enabled = true;
			}
			else if (this.m_6 == 4)
			{
				this.m_0 = new TcpClient();
				this.m_0.NoDelay = true;
				this.m_0.BeginConnect(this.m_C, 5000, null, null);
				this.m_5 = 0;
				this.m_4.Enabled = true;
			}
		}
	}

	private void _9(object P_0, EventArgs P_1)
	{
		if (++this.m_4 > 24)
		{
			this.m_4 = 0;
			this.m_3.Enabled = false;
			_0("DNS Fail");
			if (this.m_6 == 1)
			{
				this.m_1.Text = "인터넷 연결 실패";
				this.m_1.Visible = true;
				this.m_3.Enabled = true;
			}
			else if (this.m_6 == 5)
			{
				this.m_d.Text = "인터넷 연결 실패";
				if (MessageBox.Show("인터넷 연결에 실패하였습니다\n재시도하겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					this.m_d.Text = "인터넷 연결중...";
					this.m_3.Enabled = true;
				}
			}
		}
		else if (_1())
		{
			this.m_4 = 0;
			this.m_3.Enabled = false;
			_0("DNS Success");
			if (this.m_6 == 1)
			{
				this.m_1.Text = "인터넷 연결 성공";
				this.m_1.Visible = true;
				this.m_3.Enabled = true;
				this.m_6 = 2;
				this.m_0.SelectedIndex = this.m_6;
				this.m_0.Focus();
			}
			else if (this.m_6 == 5)
			{
				this.m_d.Text = "등록 진행중...";
				this.m_0 = false;
				this.m_8 = 0;
				this.m_6.Enabled = true;
			}
		}
	}

	private void a(object P_0, EventArgs P_1)
	{
		if (++this.m_5 > 3 || this.m_0 == null)
		{
			this.m_5 = 0;
			if (this.m_0 != null)
			{
				this.m_0.Close();
				this.m_0 = null;
			}
			this.m_4.Enabled = false;
			_0("Server Connect Fail");
			this.m_b.Text = "장치 연결 실패";
			this.m_b.Visible = true;
			this.m_7.Enabled = true;
			return;
		}
		try
		{
			if (this.m_0 != null && this.m_0.Connected)
			{
				this.m_0.GetStream().BeginRead(this.m_0, 0, 1024, _0, this);
				if (this.m_5 == 1)
				{
					_2("[DUT<-PC] START");
					return;
				}
				string text = _3(this.m_b);
				this.m_D = "DAWONDNS-" + text + "-" + this.m_A;
				string text2 = "{mac:" + this.m_A + ",api_server_addr:dwapi.dawonai.com,api_server_port:18443,server_addr:dwmqtt.dawonai.com,server_port:8883,ssl_support:yes,ssid:" + this.m_8 + ",pass:" + this.m_9 + ",user_id:" + this.m_d + ",company:DAWONDNS,model:" + text + ",lati:" + this.m_E + ",long:" + this.m_f + ",topic:dwd}";
				this.m_1 = false;
				this.m_7 = 3;
				_2(_0(text2));
				this.m_5.Enabled = true;
				this.m_5 = 0;
				this.m_4.Enabled = false;
			}
		}
		catch (IOException ex)
		{
			_0("IOException : " + ex.InnerException.Message);
			this.m_5 = 24;
		}
		catch (Exception ex2)
		{
			_0("tmrTcp_Tick Error : " + ex2.Message);
		}
	}

	private void _0(IAsyncResult P_0)
	{
		if (this.m_0 == null)
		{
			return;
		}
		int num = 0;
		try
		{
			num = this.m_0.GetStream().EndRead(P_0);
			if (num > 0)
			{
				string text = Encoding.Default.GetString(this.m_0, 0, num);
				_0("Recv [" + num.ToString("0000") + "] : " + text);
				if (text.Contains("START_OK"))
				{
					_0("Server Connect Success");
				}
				else if (text.EndsWith("}"))
				{
					this.m_0.Append(text);
					_1(this.m_0.ToString());
					this.m_0.Remove(0, this.m_0.Length);
				}
				else
				{
					this.m_0.Append(text);
				}
				this.m_0.GetStream().BeginRead(this.m_0, 0, 1024, _0, this);
			}
			else
			{
				if (this.m_0.Connected)
				{
					this.m_0.Close();
				}
				this.m_0 = null;
			}
		}
		catch (IOException ex)
		{
			_0("IOException : " + ex.Message);
		}
		catch (Exception ex2)
		{
			_0("Exception : " + ex2.Message);
		}
	}

	private void _1(string P_0)
	{
		if (P_0.Contains("\"respone\":\"OK\""))
		{
			this.m_1 = true;
		}
	}

	private void _2(string P_0)
	{
		if (this.m_0 != null && this.m_0.Connected)
		{
			try
			{
				this.m_0 = new StreamWriter(this.m_0.GetStream());
				this.m_0.WriteLine(P_0);
				this.m_0.Flush();
				_0("Send [" + P_0.Length.ToString("0000") + "] : " + P_0);
				return;
			}
			catch (Exception ex)
			{
				_0("Send Exception : " + ex.Message);
				return;
			}
		}
		_0("Not Connected");
	}

	private bool _1()
	{
		try
		{
			Dns.GetHostEntry("www.google.com");
			return true;
		}
		catch (Exception ex)
		{
			_0(ex.Message);
			return false;
		}
	}

	private void A(object P_0, EventArgs P_1)
	{
		if (this.m_6 != this.m_0.SelectedIndex)
		{
			this.m_0.SelectedIndex = this.m_6;
		}
	}

	private void b(object P_0, EventArgs P_1)
	{
		if (this.m_2.Text.Length > 0)
		{
			this.m_6 = 3;
			this.m_0.SelectedIndex = this.m_6;
			this.m_2.Text = "";
			this.m_9.Visible = false;
			this.m_4.Visible = false;
			this.m_D.Visible = true;
			this.m_1.Visible = true;
		}
	}

	private void B(object P_0, EventArgs P_1)
	{
		if (this.m_1.SelectedIndices.Count == 0)
		{
			this.m_5.Visible = false;
		}
		else if (!this.m_5.Visible)
		{
			this.m_5.Visible = true;
		}
	}

	private void c(object P_0, EventArgs P_1)
	{
		if (this.m_1.SelectedIndices.Count != 0)
		{
			_3();
			this.m_B = "DWD-" + this.m_1.SelectedItems[0].Tag.ToString();
			_0("SSID1 : " + this.m_B);
			if (this.m_1.SelectedItems[0].Tag.ToString().Equals("S110"))
			{
				this.m_c = "DWD-S600";
			}
			else if (this.m_1.SelectedItems[0].Tag.ToString().Equals("S120"))
			{
				this.m_c = "DWD-LS120";
			}
			else if (this.m_1.SelectedItems[0].Tag.ToString().Equals("ES120"))
			{
				this.m_c = "DWD-SS120";
			}
			else if (this.m_1.SelectedItems[0].Tag.ToString().Equals("S501"))
			{
				this.m_c = "DWD-S510";
			}
			else if (this.m_1.SelectedItems[0].Tag.ToString().Equals("S310"))
			{
				this.m_c = "DWD-S311";
			}
			else if (this.m_1.SelectedItems[0].Tag.ToString().Equals("ES120S"))
			{
				this.m_c = "DWD-ES120";
			}
			else
			{
				this.m_c = "";
			}
			_0("SSID2 : " + this.m_c);
			this.m_5.Text = this.m_1.SelectedItems[0].Text;
			this.m_6 = 4;
			this.m_0.SelectedIndex = this.m_6;
			this.m_6.Enabled = true;
			this.m_6.PerformClick();
		}
	}

	private void C(object P_0, EventArgs P_1)
	{
		if (this.m_B.Length == 0)
		{
			return;
		}
		this.m_2.Items.Clear();
		this.m_3.Text = "";
		this.m_4.Text = "";
		this.m_b.Visible = false;
		this.m_7.Visible = false;
		try
		{
			if (this.m_0 != null && this.m_1 == 0)
			{
				this.m_1 = -1;
				this.m_0 = DateTime.Now;
				this.m_0.Enabled = true;
				this.m_6.Enabled = false;
				_0("Device Scan Start");
				this.m_0.Scan();
			}
		}
		catch (Exception ex)
		{
			_0("btnApRefresh_Click Error : " + ex.Message);
		}
	}

	private void d(object P_0, EventArgs P_1)
	{
		this.m_6 = 3;
		this.m_0.SelectedIndex = this.m_6;
	}

	private void _0(object P_0, MouseEventArgs P_1)
	{
		this.m_5.PerformClick();
	}

	private void D(object P_0, EventArgs P_1)
	{
		if (this.m_2.SelectedIndices.Count != 0)
		{
			ListViewItem listViewItem = this.m_2.SelectedItems[0];
			this.m_3.Text = listViewItem.Text;
			this.m_4.Text = listViewItem.SubItems[1].Text;
			this.m_b.Visible = false;
			this.m_7.Visible = true;
		}
	}

	private string _3(string P_0)
	{
		switch (P_0.Split('_')[0].Substring(4))
		{
		case "S120":
			return "B530_W";
		case "LS120":
			return "B540_W";
		case "SS120":
			return "B550_W";
		case "ES120":
			if (!this.m_B.Equals("DWD-ES120S"))
			{
				return "B550E_W";
			}
			return "B550E_SW";
		case "LS130":
			return "B350_W";
		case "S220":
			return "M130_W";
		case "S310":
			return "P110_W";
		case "S311":
			return "P110_WA";
		case "S330":
			return "P230_W";
		case "S350":
			return "P250_W";
		case "S370":
			return "P270_W";
		case "LS810":
			return "G200L_ZB";
		case "S510":
			if (!this.m_B.Equals("DWD-S510"))
			{
				return "R200_W";
			}
			return "R110_W";
		case "S501":
			return "R200_W";
		case "S600":
			if (!this.m_B.Equals("DWD-S600"))
			{
				return "B400_W";
			}
			return "B400_SW";
		case "S110":
			return "B400_WI";
		case "S121":
			return "B343_W";
		default:
			return "";
		}
	}

	private void _0(object P_0, FormClosingEventArgs P_1)
	{
		if (MessageBox.Show("정말로 종료하겠습니까?", "AIPM Register", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			_2();
			_4();
		}
		else
		{
			P_1.Cancel = true;
		}
	}

	private void e(object P_0, EventArgs P_1)
	{
		if (this.m_3.Text.Length == 0)
		{
			return;
		}
		if (!_2())
		{
			MessageBox.Show("연동 유효시간이 지났습니다.\n다시 인증하여주세요.", "확인");
			this.m_6 = 2;
			this.m_0.SelectedIndex = this.m_6;
			return;
		}
		string text = this.m_3.Text;
		string text2 = _0(text, "개방형", "");
		if (text2.Length > 0)
		{
			_3();
			this.m_7.Enabled = false;
			this.m_A = this.m_4.Text;
			this.m_b = text;
			this.m_b.Text = "연결중...";
			this.m_b.Visible = true;
			this.m_0.SetProfile(Wlan.WlanProfileFlags.AllUser, text2, overwrite: true);
			this.m_0.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, text);
			this.m_2 = 0;
			this.m_1.Enabled = true;
		}
	}

	private void E(object P_0, EventArgs P_1)
	{
		if (this.m_1)
		{
			this.m_5.Enabled = false;
			if (this.m_6 == 4)
			{
				this.m_7.Enabled = true;
				this.m_6 = 5;
				this.m_0.SelectedIndex = this.m_6;
				this.m_1 = false;
				this.m_7 = 3;
				_2(_0("{mac:" + this.m_A + ",command:connectap}"));
				this.m_5.Enabled = true;
			}
			else
			{
				if (this.m_6 != 5)
				{
					return;
				}
				_3();
				if (!_0(this.m_8))
				{
					string text = _0(this.m_8, this.m_a, this.m_9);
					if (text.Length <= 0)
					{
						_0("Wi-Fi 공유기 연결 실패");
						this.m_d.Text = "Wi-Fi 공유기 연결 실패";
						this.m_9.Visible = true;
						this.m_a.Visible = true;
						return;
					}
					this.m_0.SetProfile(Wlan.WlanProfileFlags.AllUser, text, overwrite: true);
				}
				this.m_d.Text = "Wi-Fi 공유기 연결중...";
				this.m_0.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, this.m_8);
				this.m_2 = 0;
				this.m_1.Enabled = true;
			}
		}
		else if (--this.m_7 < 1)
		{
			_0("respone fail");
			this.m_7 = 3;
			this.m_1 = true;
		}
	}

	private void f(object P_0, EventArgs P_1)
	{
		if (++this.m_8 > 20)
		{
			this.m_8 = 0;
			this.m_6.Enabled = false;
			this.m_d.Text = "등록 실패 : 응답 시간 초과";
			this.m_9.Visible = true;
			this.m_a.Visible = true;
			return;
		}
		this.m_0.Value = this.m_8;
		if (this.m_8 < 2)
		{
			return;
		}
		if (this.m_0 != null && this.m_0.InterfaceState == Wlan.WlanInterfaceState.Connected && _1())
		{
			if (this.m_0)
			{
				this.m_8 = 0;
				this.m_6.Enabled = false;
				this.m_0 = false;
				this.m_0.Value = this.m_0.Maximum;
				this.m_d.Text = "등록 완료";
				this.m_9.Visible = true;
				this.m_a.Visible = true;
			}
			else
			{
				_1();
			}
		}
		else
		{
			this.m_8 = 0;
			this.m_6.Enabled = false;
			this.m_d.Text = "인터넷에 연결되어있지 않습니다.";
			this.m_9.Visible = true;
			this.m_a.Visible = true;
		}
	}

	private void F(object P_0, EventArgs P_1)
	{
		if (this.m_0.Text.Length == 0)
		{
			this.m_0.Select(0, 0);
		}
	}

	private void _1(object P_0, KeyPressEventArgs P_1)
	{
		if (P_1.KeyChar == '\r' && this.m_0.Text.Length == 8)
		{
			this.m_0.PerformClick();
		}
	}

	private void _7()
	{
		this.m_9.Visible = false;
		this.m_a.Visible = false;
		this.m_0.Value = 0;
		this.m_d.Text = "";
	}

	private void g(object P_0, EventArgs P_1)
	{
		_7();
		this.m_6 = 3;
		this.m_0.SelectedIndex = this.m_6;
		_3();
	}

	private void G(object P_0, EventArgs P_1)
	{
		if (this.m_0.Count > 0)
		{
			string[] array;
			lock (this.m_1)
			{
				array = this.m_0.Dequeue();
			}
			this.m_0--;
			_0("Timer : " + this.m_0 + " : " + array[2] + " : " + string.Join(" : ", array));
			switch (array[2])
			{
			case "v1/devices/control/check":
				if (array[0] == "200")
				{
					string text = array[1];
					int num = text.IndexOf("\"n\":\"/100/0/31\",\"sv\":");
					if (num > 0)
					{
						text = text.Substring(num + 22);
						if (text.StartsWith("true") || text.StartsWith("false"))
						{
							this.m_0 = true;
						}
					}
				}
				else if (this.m_6.Enabled)
				{
					string text = array[1].Replace(" ", string.Empty).ToUpper();
					if (text.Contains("STATUSERROR"))
					{
						this.m_6.Enabled = false;
						this.m_d.Text = "이미 등록된 장치입니다.";
						this.m_0.Value = 0;
						this.m_9.Visible = true;
						this.m_a.Visible = true;
					}
					else if (text.Contains("NOTREGISTERED") && this.m_8 > 10)
					{
						this.m_6.Enabled = false;
						this.m_d.Text = "등록 실패 : 장치를 초기화 후 다시 등록해주세요.";
						this.m_0.Value = 0;
						this.m_9.Visible = true;
						this.m_a.Visible = true;
					}
				}
				break;
			case "v1/accounts/post/getPckey":
				this.m_0.Enabled = true;
				if (array[0] == "200")
				{
					if (array[1].Contains("TIMEFAILED"))
					{
						MessageBox.Show("인증 유효시간이 초과되었습니다.\n앱에서 새 인증번호를 발급해주세요.", "시간 초과");
						break;
					}
					DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(JsonData));
					JsonData jsonData = (JsonData)dataContractJsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(array[1])));
					this.m_d = jsonData.account.user_id;
					this.m_e = jsonData.account.pc_key;
					this.m_E = jsonData.account.pc_lati;
					this.m_f = jsonData.account.pc_long;
					this.m_1 = DateTime.Now;
					this.m_0.Text = "";
					this.m_2.Text = this.m_d;
					this.m_D.Visible = false;
					this.m_1.Visible = false;
					this.m_9.Visible = true;
					this.m_4.Visible = true;
					this.m_4.Focus();
				}
				else
				{
					MessageBox.Show("유효한 인증번호가 아닙니다.", "Error " + array[0]);
				}
				break;
			}
		}
		else if (this.m_0 < 1)
		{
			this.m_7.Enabled = false;
		}
	}

	private bool _2()
	{
		if (this.m_1 < DateTime.Now.AddMinutes(-50.0))
		{
			return false;
		}
		return true;
	}

	private void h(object P_0, EventArgs P_1)
	{
		if (MessageBox.Show("정말로 연동을 해제하겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			_2();
			_7();
			this.m_6 = 2;
			this.m_0.SelectedIndex = this.m_6;
		}
	}
}
