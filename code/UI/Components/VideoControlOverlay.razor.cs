using Sandbox.UI;
using System;

public partial class VideoControlOverlay : Panel
{
	public VideoPanel VideoPanel { get; set; }

	protected override int BuildHash()
	{
		return HashCode.Combine( VideoPanel?.IsPlaying, VideoPanel?.IsPaused, VideoPanel?.Muted, TimecodeAreaClass, (int)ProgressSeconds );
	}

	private string PlayButtonIcon
	{
		get 
		{
			if ( !VideoPanel.IsValid() )
				return "play_arrow";

			return VideoPanel.IsPaused ? "play_arrow" : "pause";
		}
	}

	private string TimecodeAreaClass => ProgressSeconds > 3600 || DurationSeconds > 3600
		? "big"
		: string.Empty;
	private string ProgressText => FormatTime( ProgressSeconds );
	private string DurationText => FormatTime( DurationSeconds );

	private string FormatTime( float seconds )
	{
		var time = TimeSpan.FromSeconds( seconds );
		return time.Hours > 0
			? time.ToString( "hh':'mm':'ss" )
			: time.ToString( "mm':'ss" );
	}

	private float ProgressSeconds => VideoPanel?.PlaybackTime ?? 0f;
	private float DurationSeconds => VideoPanel?.Duration ?? 0f;
	private float ProgressPercent
	{
		get
		{
			if ( ProgressSeconds == 0f || DurationSeconds == 0f )
				return 0f;

			return ProgressSeconds / DurationSeconds;
		}
	}
	private float PlaybackTime
	{
		get => ProgressSeconds;
		set => VideoPanel?.Seek( value );
	}
	private string VolumeButtonIcon => VideoPanel?.Muted == false ? "volume_up" : "volume_off";

	private void ProgressBarChanged( float value )
	{
		PlaybackTime = value;
	}

	private void TogglePause() => VideoPanel?.TogglePause();
	private void ToggleMute()
	{
		if ( !VideoPanel.IsValid() )
			return;

		VideoPanel.Muted = !VideoPanel.Muted;
	}
}
