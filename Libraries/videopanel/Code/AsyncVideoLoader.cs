using System.Threading.Tasks;
using System.Threading;
using System;

namespace Duccsoft;

/// <summary>
/// Provides a handy asynchronous wrapper for loading a VideoPlayer and waiting
/// until its video and audio are both loaded.
/// </summary>
public class AsyncVideoLoader
{
	public AsyncVideoLoader() 
	{
		VideoPlayer = new VideoPlayer();
	}

	public AsyncVideoLoader( VideoPlayer player )
	{
		VideoPlayer = player ?? new VideoPlayer();
	}

	public VideoPlayer VideoPlayer { get; private set; }
	public bool IsLoading { get; private set; }

	private Action _onLoaded;
	private Action _onAudioReady;

	public async Task<VideoPlayer> LoadFromUrl( string url, CancellationToken cancelToken = default )
	{
		void Play( VideoPlayer player ) => player.Play( url );

		await Load( Play, cancelToken );
		return VideoPlayer;
	}

	public async Task<VideoPlayer> LoadFromFile( BaseFileSystem fileSystem, string path, CancellationToken cancelToken )
	{
		void Play( VideoPlayer player ) => player.Play( fileSystem, path );

		await Load( Play, cancelToken );
		return VideoPlayer;
	}

	private async Task Load( Action<VideoPlayer> playAction, CancellationToken cancelToken = default )
	{
		// Attempting to play a video from a thread would throw an exception.
		await GameTask.MainThread( cancelToken );

		if ( IsLoading )
		{
			throw new InvalidOperationException( "Another video was already being loaded. Check IsLoading or create a new instance of AsyncVideoLoader." );
		}

		IsLoading = true;

		bool videoLoaded = false;
		bool audioLoaded = false;

		// Assign private members instead of named methods to the invocation lists of the
		// VideoPlayer delegates to break reference equality between runs.
		_onLoaded = () => videoLoaded = true;
		_onAudioReady = () => audioLoaded = true;

		VideoPlayer.OnLoaded = _onLoaded;
		VideoPlayer.OnAudioReady = _onAudioReady;

		playAction?.Invoke( VideoPlayer );

		// Non-blocking spin until video and audio are loaded.
		while ( !videoLoaded || !audioLoaded )
		{
			// If OnLoaded or OnAudioReady are changed externally before we're finished
			// loading, the video will likely never load. Abort to avoid spinning forever.
			var callbacksChanged = _onLoaded != VideoPlayer.OnLoaded || _onAudioReady != VideoPlayer.OnAudioReady;
			if ( callbacksChanged || cancelToken.IsCancellationRequested )
			{
				IsLoading = false;
				return;
			}

			await GameTask.Yield();
		}

		IsLoading = false;
	}
}
