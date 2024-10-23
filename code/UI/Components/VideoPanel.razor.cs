using Sandbox.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class VideoPanel : Panel, IDisposable
{
	[Change]
	public string VideoPath { get; set; }
	public bool ShouldLoop { get; set; } = true;
	public bool IsLoading => _videoLoader.IsValid() && _videoLoader.IsLoading;
	public bool IsPlaying => !HasReachedEnd && _videoPlayer is not null && !_videoPlayer.IsPaused;
	public bool HasReachedEnd => _videoPlayer != null && _videoPlayer.PlaybackTime >= _videoPlayer.Duration;

	private VideoPlayer _videoPlayer;
	private AsyncVideoLoader _videoLoader;
	private CancellationTokenSource _cancelSource = new();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		_ = PlayVideo( VideoPath );
	}

	private async void OnVideoPathChanged( string oldValue, string newValue )
	{
		if ( oldValue == newValue )
			return;

		if ( string.IsNullOrWhiteSpace( newValue ) )
		{
			Stop();
			return;
		}

		await PlayVideo( newValue );
	}

	public async Task PlayVideo( string path )
	{
		if ( IsLoading )
		{
			CancelVideoLoad();
		}

		_videoLoader = new AsyncVideoLoader( _videoPlayer );
		_cancelSource = new CancellationTokenSource();

		// Make copies of the things that might get clobbered while we await.
		var cancelToken = _cancelSource.Token;
		var videoLoader = _videoLoader;

		var videoPlayer = await videoLoader.LoadFromFile( FileSystem.Mounted, path, cancelToken );

		if ( cancelToken.IsCancellationRequested )
		{
			videoPlayer?.Stop();
			videoLoader.Dispose();
			return;
		}

		_videoPlayer = videoPlayer;
		_muted = _videoPlayer.Muted;

		// Set the background-image property to the VideoPanel's Texture.
		Style.SetBackgroundImage( videoPlayer.Texture );
		StateHasChanged();
	}

	public void Pause() => _videoPlayer?.Pause();
	public bool IsPaused => _videoPlayer?.IsPaused == true;
	public void Resume() => _videoPlayer?.Resume();
	public void TogglePause() => _videoPlayer?.TogglePause();
	public void Seek( float time ) => _videoPlayer?.Seek( time );
	public float Duration => _videoPlayer?.Duration ?? 0f;
	public float PlaybackTime
	{
		get => _videoPlayer?.PlaybackTime ?? 0f;
		set => Seek( value );
	}

	public bool Muted
	{
		get => _muted;
		set
		{
			_muted = value;
			if ( _videoPlayer is not null )
			{
				_videoPlayer.Muted = value;
			}
		}
	}
	private bool _muted;
	
	public void Stop()
	{
		_videoPlayer?.Stop();
		_videoPlayer?.Dispose();
		_videoPlayer = null;
	}

	public void CancelVideoLoad()
	{
		_cancelSource?.Cancel();
		_videoLoader = null;
		_cancelSource = new CancellationTokenSource();
	}

	public override void Tick()
	{
		if ( _videoPlayer is null )
			return;

		// The VideoPlayer texture will not update unless Present is called.
		_videoPlayer.Present();

		// Loop when the video concludes.
		if ( ShouldLoop && HasReachedEnd )
		{
			_videoPlayer.Seek( 0f );
		}
	}

	public void Dispose()
	{
		_cancelSource?.Dispose();
		_videoPlayer?.Dispose();
		GC.SuppressFinalize( this );
	}

	public override void OnDeleted()
	{
		Dispose();
	}
}
