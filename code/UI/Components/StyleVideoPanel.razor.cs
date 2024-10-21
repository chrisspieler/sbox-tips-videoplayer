using Sandbox.UI;
using System;

public partial class StyleVideoPanel : Panel
{
	public string VideoPath { get; set; }

	protected override int BuildHash() => HashCode.Combine( VideoPath );
}
