using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace AIPM_Register.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[CompilerGenerated]
[DebuggerNonUserCode]
internal class Resources
{
	private static ResourceManager _0;

	private static CultureInfo _0;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(Resources._0, null))
			{
				ResourceManager resourceManager = new ResourceManager("AIPM_Register.Properties.Resources", typeof(Resources).Assembly);
				Resources._0 = resourceManager;
			}
			return Resources._0;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return _0;
		}
		set
		{
			_0 = cultureInfo;
		}
	}

	internal Resources()
	{
	}
}
