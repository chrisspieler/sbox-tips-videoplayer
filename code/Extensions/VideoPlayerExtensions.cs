using System.Threading;
using System.Threading.Tasks;

public static class VideoPlayerExtensions
{
	public static async Task PlayAsync( this VideoPlayer videoPlayer, string url, CancellationToken cancelToken = default )
	{
		if ( videoPlayer is null )
			return;

		var videoLoader = new AsyncVideoLoader( videoPlayer );
		await videoLoader.LoadFromUrl( url, cancelToken );
	}

	public static async Task PlayAsync( this VideoPlayer videoPlayer, BaseFileSystem fileSystem, string path, CancellationToken cancelToken = default )
	{
		if ( videoPlayer is null )
			return;

		var videoLoader = new AsyncVideoLoader( videoPlayer );
		await videoLoader.LoadFromFile( fileSystem, path, cancelToken );
	}
}
