using System;
using System.Threading;
using System.Windows.Forms;
using AIPM_Register;

namespace _0;

internal static class _0
{
	private static bool m_0;

	private static Mutex m_0;

	[STAThread]
	private static void _0()
	{
		if (global::_0._0.m_0)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new frmMain());
		}
		else
		{
			MessageBox.Show("프로그램이 이미 실행중입니다.", Application.ProductName);
		}
	}

	static _0()
	{
		global::_0._0.m_0 = false;
		global::_0._0.m_0 = new Mutex(initiallyOwned: true, "AIPM_Register", out global::_0._0.m_0);
	}
}
