using System;

public partial class VideoPlayerScreen : PanelComponent
{
	[Property] public string CurrentVideoPath { get; set; }

	protected override int BuildHash() => HashCode.Combine( CurrentVideoPath );
}
