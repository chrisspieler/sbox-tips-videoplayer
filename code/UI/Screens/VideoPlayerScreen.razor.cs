using System;

public partial class VideoPlayerScreen : PanelComponent
{
	[Property] public VideoSource CurrentVideo { get; set; }
	public string CurrentVideoPath => CurrentVideo?.VideoPath;

	[Property] public List<VideoSource> Videos = new();
	[Property] public bool AutoPopulateVideosOnStart { get; set; }
	[Property, InputAction] public string ForwardAction { get; set; } = "right";
	[Property, InputAction] public string BackAction { get; set; } = "left";

	private VideoPanel VideoPanel { get; set; }
	private bool UseVideoPlayer { get; set; }

	private int _currentVideoIndex;

	protected override int BuildHash() => HashCode.Combine( CurrentVideo, Videos, UseVideoPlayer );

	protected override void OnStart()
	{
		base.OnStart();

		if ( AutoPopulateVideosOnStart )
		{
			PopulateVideos();
		}
	}

	protected override void OnUpdate()
	{
		if ( !Videos.Any() )
			return;

		var nextIndex = _currentVideoIndex;
		if ( Input.Pressed( ForwardAction ) )
		{
			nextIndex += 1;
		}
		else if ( Input.Pressed( BackAction ) )
		{
			nextIndex -= 1;
		}
		nextIndex = nextIndex.UnsignedMod( Videos.Count );
		_currentVideoIndex = nextIndex;
		CurrentVideo = Videos[_currentVideoIndex];
	}

	public void PopulateVideos()
	{
		Videos = ResourceLibrary.GetAll<VideoSource>().ToList();
	}
}
