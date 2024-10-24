﻿using System.Threading.Tasks;
using System.Threading;
using System;

namespace Duccsoft;

public class AsyncVideoLoader : IValid, IDisposable
{
	[ConVar( "video_async_test_delay" )]
	public static int TestDelayMilliseconds { get; set; } = 0;

	public AsyncVideoLoader() 
	{
		VideoPlayer = new VideoPlayer();
		_ownsVideoPlayer = true;
	}

	public AsyncVideoLoader( VideoPlayer player )
	{
		if ( player is null )
		{
			player = new VideoPlayer();
			_ownsVideoPlayer = true;
		}
		VideoPlayer = player;
	}

	public VideoPlayer VideoPlayer { get; private set; }
	public bool IsLoading { get; private set; }
	public bool IsValid => IsLoading || _isFresh;

	private bool _isFresh = true;
	private Action _onLoaded;
	private Action _onAudioReady;
	private bool _ownsVideoPlayer;

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

		if ( !_isFresh )
		{
			throw new InvalidOperationException( $"Attempted to load video twice in same {nameof( AsyncVideoLoader )}" );
		}

		IsLoading = true;
		_isFresh = false;

		bool videoLoaded = false;
		bool audioLoaded = false;

		// Assign private members instead of named methods to the invocation lists of the
		// VideoPlayer delegates to break reference equality between runs.
		_onLoaded = () => videoLoaded = true;
		_onAudioReady = () => audioLoaded = true;

		VideoPlayer.OnLoaded = _onLoaded;
		VideoPlayer.OnAudioReady = _onAudioReady;

		if ( TestDelayMilliseconds > 0 )
		{
			await GameTask.Delay( TestDelayMilliseconds, cancelToken );
		}

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

	public void Dispose()
	{
		if ( !_ownsVideoPlayer )
			return;

		VideoPlayer?.Dispose();
		GC.SuppressFinalize( this );
	}
}
