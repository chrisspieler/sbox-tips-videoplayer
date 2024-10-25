using Sandbox.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duccsoft;

public partial class VideoPanel : Panel, IDisposable, IVideoPanel
{
	[Change]
	public string VideoPath { get; set; }
	public bool ShouldLoop { get; set; } = true;
	public bool IsLoading => _videoLoader.IsValid() && _videoLoader.IsLoading;
	public bool IsPlaying => !HasReachedEnd && _videoPlayer is not null && !_videoPlayer.IsPaused;
	public bool HasReachedEnd => _videoPlayer != null && _videoPlayer.PlaybackTime >= _videoPlayer.Duration;

	public IAudioAccessor Audio => _audioAccessor;
	public GameObject AudioSource 
	{
		get => _audioSource;
		set
		{
			_audioSource = value;
			if ( _audioAccessor is not null )
			{
				_audioAccessor.Target = value;
				_audioAccessor.Update();
			}
		}
	}
	private GameObject _audioSource;

	private VideoPlayer _videoPlayer;
	private TrackingAudioAccessor _audioAccessor;
	private AsyncVideoLoader _videoLoader;
	private CancellationTokenSource _cancelSource = new();
	private Vector2 _videoTextureSize;

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

		if ( _videoPlayer is null )
		{
			_videoPlayer = new VideoPlayer();
			_audioAccessor = new TrackingAudioAccessor();
		}

		_audioAccessor.VideoPlayer = _videoPlayer;
		_audioAccessor.Target = AudioSource;
		_videoLoader = new AsyncVideoLoader( _videoPlayer );
		_cancelSource = new CancellationTokenSource();

		// Make copies of the things that might get clobbered while we await.
		var cancelToken = _cancelSource.Token;
		var videoLoader = _videoLoader;

		var videoPlayer = await videoLoader.LoadFromFile( FileSystem.Mounted, path, cancelToken );

		if ( cancelToken.IsCancellationRequested )
		{
			videoPlayer?.Stop();
			return;
		}

		_audioAccessor.Update();
		_videoPlayer = videoPlayer;

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
	
	public void Stop()
	{
		_videoPlayer?.Stop();
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
		if ( _videoPlayer.Texture.Size != _videoTextureSize )
		{
			_videoTextureSize = _videoPlayer.Texture.Size;
			StateHasChanged();
		}

		_audioAccessor.Update();

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
