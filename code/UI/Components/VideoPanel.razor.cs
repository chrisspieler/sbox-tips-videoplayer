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
	public bool IsPlaying => !HasReachedEnd && _player is not null && !_player.IsPaused;
	public bool HasReachedEnd => _player != null && _player.PlaybackTime >= _player.Duration;

	private VideoPlayer _player;
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
		Stop();

		if ( IsLoading )
		{
			CancelVideoLoad();
		}

		_videoLoader = new AsyncVideoLoader();
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

		_player = videoPlayer;
		_muted = _player.Muted;

		// Set the background-image property to the VideoPanel's Texture.
		Style.SetBackgroundImage( videoPlayer.Texture );
		StateHasChanged();
	}

	public void Pause() => _player?.Pause();
	public bool IsPaused => _player?.IsPaused == true;
	public void Resume() => _player?.Resume();
	public void TogglePause() => _player?.TogglePause();
	public void Seek( float time ) => _player?.Seek( time );
	public float Duration => _player?.Duration ?? 0f;
	public float PlaybackTime
	{
		get => _player?.PlaybackTime ?? 0f;
		set => Seek( value );
	}

	public bool Muted
	{
		get => _muted;
		set
		{
			_muted = value;
			if ( _player is not null )
			{
				_player.Muted = value;
			}
		}
	}
	private bool _muted;
	
	public void Stop()
	{
		_player?.Stop();
		_player?.Dispose();
		_player = null;
	}

	public void CancelVideoLoad()
	{
		_cancelSource?.Cancel();
		_videoLoader = null;
		_cancelSource = new CancellationTokenSource();
	}

	public override void Tick()
	{
		if ( _player is null )
			return;

		// The VideoPlayer texture will not update unless Present is called.
		_player.Present();

		// Loop when the video concludes.
		if ( ShouldLoop && HasReachedEnd )
		{
			_player.Seek( 0f );
		}
	}

	public void Dispose()
	{
		_cancelSource?.Dispose();
		_player?.Dispose();
		GC.SuppressFinalize( this );
	}

	public override void OnDeleted()
	{
		Dispose();
	}
}
