using System;

public partial class StyleVideoScreen : PanelComponent
{
	[Property] public string CurrentVideoPath { get; set; }
	[Property] public List<string> VideoPaths = new();

	private int _currentVideoIndex;

	protected override int BuildHash() => HashCode.Combine( CurrentVideoPath );

	protected override void OnUpdate()
	{
		if ( !VideoPaths.Any() )
			return;

		var nextIndex = _currentVideoIndex;
		if ( Input.Pressed( "right") )
		{
			nextIndex += 1;
		}
		else if ( Input.Pressed( "left" ) )
		{
			nextIndex -= 1;
		}
		nextIndex = nextIndex.UnsignedMod( VideoPaths.Count );
		_currentVideoIndex = nextIndex;
		CurrentVideoPath = VideoPaths[_currentVideoIndex];
	}
}
