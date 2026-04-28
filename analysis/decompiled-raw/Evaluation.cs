using System;

internal class Evaluation : Attribute
{
	internal readonly string Warning;

	internal Evaluation(string P_0)
	{
		Warning = P_0;
	}
}
