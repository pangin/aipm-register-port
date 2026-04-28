using System;
using System.Windows.Forms;

namespace AIPM_Register;

public class TablessControl : TabControl
{
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 4904 && !base.DesignMode)
		{
			m.Result = (IntPtr)1;
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
