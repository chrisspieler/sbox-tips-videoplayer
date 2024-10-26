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
	public virtual string VideoPath { get; set; }
	/// <summary>
	/// Specifies where the video may be found, whether it comes from a BaseFileSystem or a website.
	/// Set this to <see cref="VideoRoot.WebStream"/> if VideoPath is a URL.
	/// </summary>
	public virtual VideoRoot VideoRoot { get; set; } = VideoRoot.MountedFileSystem;
	/// <summary>
	/// If true, the video will automatically loop whenever HasReachedEnd is true.
	/// </summary>
	public virtual bool ShouldLoop { get; set; } = true;

	/// <summary>
	/// If true, the video is in the process of being loaded (e.g. from a remote server).
	/// </summary>
	public virtual bool IsLoading => _videoLoader.IsValid() && _videoLoader.IsLoading;
	/// <summary>
	/// If true, the video is playing, is not paused, and has not yet finished playing.
	/// </summary>
	public virtual bool IsPlaying => !HasReachedEnd && VideoPlayer is not null && !VideoPlayer.IsPaused;
	/// <summary>
	/// If true, this video has reached the end of the file. If ShouldLoop is enabled, the video will
	/// automatically loop.
	/// </summary>
	public virtual bool HasReachedEnd => VideoPlayer != null && VideoPlayer.PlaybackTime >= VideoPlayer.Duration;

	#region Controls
	/// <inheritdoc cref="VideoPlayer.Pause"/>
	public virtual void Pause() => VideoPlayer?.Pause();

	/// <inheritdoc cref="VideoPlayer.IsPaused"/>
	public virtual bool IsPaused => VideoPlayer?.IsPaused == true;

	/// <inheritdoc cref="VideoPlayer.Resume"/>
	public virtual void Resume() => VideoPlayer?.Resume();

	/// <inheritdoc cref="VideoPlayer.TogglePause"/>
	public virtual void TogglePause() => VideoPlayer?.TogglePause();

	/// <inheritdoc cref="VideoPlayer.Seek(float)"/>
	public virtual void Seek( float time ) => VideoPlayer?.Seek( time );

	/// <inheritdoc cref="VideoPlayer.Duration"/>
	public virtual float Duration => VideoPlayer?.Duration ?? 0f;

	/// <summary>
	/// Returns the current playback time in seconds of the video, or if setting,
	/// will seek to the specified playback time in seconds.
	/// </summary>
	public virtual float PlaybackTime
	{
		get => VideoPlayer?.PlaybackTime ?? 0f;
		set => Seek( value );
	}

	/// <inheritdoc cref="VideoPlayer.Stop"/>
	public virtual void Stop() => VideoPlayer?.Stop();

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

	// Internal state
	protected VideoPlayer VideoPlayer { get; set; }
	private AsyncVideoLoader _videoLoader;
	private TrackingAudioAccessor _audioAccessor;
	private CancellationTokenSource _cancelSource = new();



	protected async override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

		VideoPlayer = new VideoPlayer();
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

		OnPreVideoLoad();

		_cancelSource = new CancellationTokenSource();

		// Cache the CancellationToken because Task.IsCanceled isn't whitelisted,
		// and subsequent calls to PlayVideo would create a new CancellationTokenSource.
		var cancelToken = _cancelSource.Token;
		VideoPlayer = await LoadVideo( cancelToken );

		// If loading the video was cancelled...
		if ( cancelToken.IsCancellationRequested )
		{
			// ...don't touch the video player, and don't update anything, because
			// there may be another video that began loading later and finished before
			// this one, and we don't want to overwrite the effect it had.
			return;
		}

		ConfigureAudio();
		UpdateBackgroundImage();
		OnPostVideoLoad();
	}

	/// <summary>
	/// Called during PlayVideo before LoadVideo.
	/// </summary>
	protected virtual void OnPreVideoLoad() { }

	/// <summary>
	/// By default, will ensure that a TrackingAudioAccessor is created and
	/// configured to use the VideoPlayer and AudioSource of this VideoPanel.
	/// <br/><br/>
	/// Called in PlayVideo after a video is loaded, but before OnPostVideoLoad.
	/// </summary>
	protected virtual void ConfigureAudio()
	{
		_audioAccessor ??= new TrackingAudioAccessor();
		_audioAccessor.VideoPlayer = VideoPlayer;
		_audioAccessor.Target = AudioSource;
	}

	/// <summary>
	/// By default, sets the background image of this panel to VideoPlayer.Texture and rebuilds the UI.
	/// <br/><br/>
	/// Called in PlayVideo after a video is loaded, but before OnPostVideoLoad.
	/// </summary>
	protected virtual void UpdateBackgroundImage()
	{
		if ( VideoPlayer is null )
			return;

		// Set the background-image property to the VideoPanel's Texture.
		Style.SetBackgroundImage( VideoPlayer.Texture );
		StateHasChanged();
	}

	/// <summary>
	/// Called at the very end of PlayVideo, only if the video is loaded successfully.
	/// </summary>
	protected virtual void OnPostVideoLoad() { }
	

	/// <summary>
	/// Load whatever video is specified by the current VideoPath and VideoRoot, returning
	/// an instance of a VideoPlayer. Called during PlayVideo.
	/// </summary>
	protected virtual async Task<VideoPlayer> LoadVideo( CancellationToken cancelToken )
	{
		_videoLoader = new AsyncVideoLoader( VideoPlayer );

		if ( VideoRoot == VideoRoot.WebStream )
		{
			return await _videoLoader.LoadFromUrl( VideoPath, cancelToken );
		}
		else
		{
			return await _videoLoader.LoadFromFile( VideoRoot.AsFileSystem(), VideoPath, cancelToken );
		}
	}

	private void CancelVideoLoad()
	{
		_cancelSource?.Cancel();
		_videoLoader = null;
		_cancelSource?.Dispose();
		_cancelSource = null;
	}

	// Refresh detection
	private string _previousVideoPath;
	private VideoRoot _previousVideoRoot;
	private Vector2 _videoTextureSize;

	private bool VideoHasChanged => _previousVideoPath != VideoPath || _previousVideoRoot != VideoRoot;
	private bool VideoSizeChanged => VideoPlayer.Texture.Size != _videoTextureSize;

	public override void Tick()
	{
		if ( VideoPlayer is null )
			return;

		// If the VideoPath or VideoRoot have changed...
		if ( VideoHasChanged )
		{
			// ...play that new video instead.
			_ = PlayVideo();
			return;
		}

		if ( VideoSizeChanged )
		{
			_videoTextureSize = VideoPlayer.Texture.Size;
			HandleResize();
		}
		
		// The VideoPlayer texture will not update unless Present is called.
		VideoPlayer.Present();

		DetectAndHandleLoop();
	}

	/// <summary>
	/// By default, rebuilds the UI. Called whenever the VideoPlayer size is of a different
	/// size than was previously recorded, which is expected to happen when loading a video.
	/// </summary>
	protected virtual void HandleResize()
	{
		StateHasChanged();
	}

	/// <summary>
	/// By default, detects whether the video has reached its conclusion. 
	/// If so, and if ShouldLoop is true, the playback time will be 
	/// set to the start of the video.
	/// </summary>
	protected virtual void DetectAndHandleLoop()
	{
		// We use a custom looping mechanism because VideoPlayer.Repeat seems to mess up PlaybackTime.
		if ( ShouldLoop && HasReachedEnd )
		{
			VideoPlayer.Seek( 0f );
		}
	}

	/// <summary>
	/// Stops loading videos, and disposes of our VideoPlayer and TrackingAudioAccessor.
	/// </summary>
	public virtual void Dispose()
	{
		CancelVideoLoad();
		VideoPlayer?.Dispose();
		_audioAccessor?.Dispose();
		GC.SuppressFinalize( this );
	}

	public override void OnDeleted()
	{
		Dispose();
	}
}
