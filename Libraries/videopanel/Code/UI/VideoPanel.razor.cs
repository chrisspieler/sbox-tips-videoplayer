using Sandbox.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duccsoft;

/// <summary>
/// A Panel that manages an instance of a VideoPlayer, using its texture as the background image.
/// Supports all the playback controls of VideoPlayer.
/// </summary>
public partial class VideoPanel : Panel, IDisposable, IVideoPanel
{
	/// <summary>
	/// The path/url of a video relative to VideoRoot.
	/// </summary>
	public string VideoPath { get; set; }
	/// <summary>
	/// Specifies where the video may be found, whether it comes from a BaseFileSystem or a website.
	/// Set this to <see cref="VideoRoot.WebStream"/> if VideoPath is a URL.
	/// </summary>
	public VideoRoot VideoRoot { get; set; } = VideoRoot.MountedFileSystem;
	/// <summary>
	/// If true, the video will automatically loop whenever HasReachedEnd is true.
	/// </summary>
	public bool ShouldLoop { get; set; } = true;

	/// <summary>
	/// If true, the video is in the process of being loaded (e.g. from a remote server).
	/// </summary>
	public bool IsLoading => _videoLoader.IsValid() && _videoLoader.IsLoading;
	/// <summary>
	/// If true, the video is playing, is not paused, and has not yet finished playing.
	/// </summary>
	public bool IsPlaying => !HasReachedEnd && _videoPlayer is not null && !_videoPlayer.IsPaused;
	/// <summary>
	/// If true, this video has reached the end of the file. If ShouldLoop is enabled, the video will
	/// automatically loop.
	/// </summary>
	public bool HasReachedEnd => _videoPlayer != null && _videoPlayer.PlaybackTime >= _videoPlayer.Duration;

	#region Controls
	/// <inheritdoc cref="VideoPlayer.Pause"/>
	public void Pause() => _videoPlayer?.Pause();

	/// <inheritdoc cref="VideoPlayer.IsPaused"/>
	public bool IsPaused => _videoPlayer?.IsPaused == true;

	/// <inheritdoc cref="VideoPlayer.Resume"/>
	public void Resume() => _videoPlayer?.Resume();

	/// <inheritdoc cref="VideoPlayer.TogglePause"/>
	public void TogglePause() => _videoPlayer?.TogglePause();

	/// <inheritdoc cref="VideoPlayer.Seek(float)"/>
	public void Seek( float time ) => _videoPlayer?.Seek( time );

	/// <inheritdoc cref="VideoPlayer.Duration"/>
	public float Duration => _videoPlayer?.Duration ?? 0f;

	/// <summary>
	/// Returns the current playback time in seconds of the video, or if setting,
	/// will seek to the specified playback time in seconds.
	/// </summary>
	public float PlaybackTime
	{
		get => _videoPlayer?.PlaybackTime ?? 0f;
		set => Seek( value );
	}

	/// <inheritdoc cref="VideoPlayer.Stop"/>
	public void Stop() => _videoPlayer?.Stop();

	/// <summary>
	/// Provides access to the various audio-related properties of VideoPlayer such
	/// as Volume and Position.
	/// </summary>
	public IAudioAccessor Audio => _audioAccessor;

	/// <summary>
	/// Specifies a GameObject in the world from which the audio of this video shall be emitted.
	/// </summary>
	public GameObject AudioSource 
	{
		get => _audioSource;
		set
		{
			_audioSource = value;
			if ( _audioAccessor is not null )
			{
				_audioAccessor.Target = value;
			}
		}
	}
	private GameObject _audioSource;
	#endregion

	// Private state
	private VideoPlayer _videoPlayer;
	private TrackingAudioAccessor _audioAccessor;
	private AsyncVideoLoader _videoLoader;
	private CancellationTokenSource _cancelSource = new();

	// Refresh detection
	private string _previousVideoPath;
	private VideoRoot _previousVideoRoot;
	private Vector2 _videoTextureSize;

	protected async override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

		// Attempt to play whatever video is specified by the initial VideoPath and VideoRoot.
		await PlayVideo();
	}

	/// <summary>
	/// Attempt to play whatever video is specified by the current VideoPath and VideoRoot.
	/// </summary>
	private async Task PlayVideo()
	{
		// Now we're using the new video path and root.
		_previousVideoPath = VideoPath;
		_previousVideoRoot = VideoRoot;

		// Trying to play nothing? Success means stopping the current video.
		if ( string.IsNullOrWhiteSpace( VideoPath ) )
		{
			Stop();
			return;
		}

		// Cancel whatever other video might be loading.
		if ( IsLoading )
		{
			CancelVideoLoad();
		}

		// Initialize our stuff.
		Initialize();

		// Cache the CancellationToken because Task.IsCanceled isn't whitelisted,
		// and subsequent calls to PlayVideo would create a new CancellationTokenSource.
		var cancelToken = _cancelSource.Token;

		await PlayFromVideoRoot( _videoLoader, cancelToken );

		// If loading the video was cancelled...
		if ( cancelToken.IsCancellationRequested )
		{
			// ...don't touch the video player, and don't update anything, because
			// there may be another video that began loading later and finished before
			// this one, and we don't want to overwrite the effect it had.
			return;
		}

		// Set the background-image property to the VideoPanel's Texture.
		Style.SetBackgroundImage( _videoPlayer.Texture );
		StateHasChanged();
	}

	private void Initialize()
	{
		_videoPlayer ??= new VideoPlayer();
		_audioAccessor ??= new TrackingAudioAccessor();

		_audioAccessor.VideoPlayer = _videoPlayer;
		_audioAccessor.Target = AudioSource;
		_videoLoader = new AsyncVideoLoader( _videoPlayer );
		_cancelSource = new CancellationTokenSource();
	}

	private async Task<VideoPlayer> PlayFromVideoRoot( AsyncVideoLoader loader, CancellationToken cancelToken )
	{
		if ( VideoRoot == VideoRoot.WebStream )
		{
			return await loader.LoadFromUrl( VideoPath, cancelToken );
		}
		else
		{
			return await loader.LoadFromFile( VideoRoot.AsFileSystem(), VideoPath, cancelToken );
		}
	}

	private void CancelVideoLoad()
	{
		_cancelSource?.Cancel();
		_videoLoader = null;
		_cancelSource?.Dispose();
		_cancelSource = null;
	}

	public override void Tick()
	{
		if ( _videoPlayer is null )
			return;

		// If the VideoPath or VideoRoot have changed...
		if ( _previousVideoPath != VideoPath || _previousVideoRoot != VideoRoot )
		{
			// ...play that new video instead.
			_ = PlayVideo();
			return;
		}
		
		// The VideoPlayer texture will not update unless Present is called.
		_videoPlayer.Present();

		// Update the background texture if the video changes size.
		if ( _videoPlayer.Texture.Size != _videoTextureSize )
		{
			_videoTextureSize = _videoPlayer.Texture.Size;
			StateHasChanged();
		}

		// Loop when the video concludes. We use a custom looping mechanism
		// because VideoPlayer.Repeat seems to mess up PlaybackTime.
		if ( ShouldLoop && HasReachedEnd )
		{
			_videoPlayer.Seek( 0f );
		}
	}

	/// <summary>
	/// Stops loading videos, and disposes of our VideoPlayer and TrackingAudioAccessor.
	/// </summary>
	public void Dispose()
	{
		CancelVideoLoad();
		_videoPlayer?.Dispose();
		_audioAccessor?.Dispose();
		GC.SuppressFinalize( this );
	}

	public override void OnDeleted()
	{
		Dispose();
	}
}
