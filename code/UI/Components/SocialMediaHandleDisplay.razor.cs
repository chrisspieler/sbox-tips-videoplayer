using Sandbox.UI;
using System;

public partial class SocialMediaHandleDisplay : Panel
{
	public SocialMediaIcon Icon { get; set; }
	public string Handle { get; set; } = "example";

	protected override int BuildHash() => HashCode.Combine( Icon, Handle );
}
