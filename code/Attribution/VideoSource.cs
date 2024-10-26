using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides attribution and licensing information for a video asset such as an MP4 file.
/// </summary>
[GameResource( RES_NAME, RES_EXTENSION, RES_DESCRIPTION, Icon = RES_ICON, IconBgColor = RES_BG_COLOR )]
public class VideoSource : AssetSource
{
	private const string RES_NAME = "Video Source";
	private const string RES_EXTENSION = "vsrc";
	private const string RES_DESCRIPTION = "Attribution and licensing information for a video file.";
	private const string RES_ICON = "video_file";
	private const string RES_BG_COLOR = "cornflowerblue";


	/// <summary>
	/// The location to search for the VideoPath.
	/// </summary>
	public VideoRoot VideoRoot { get; set; } = VideoRoot.MountedFileSystem;
	/// <summary>
	/// Depending on VideoRoot, this may be a URL or relative file path.
	/// </summary>
	public string VideoPath { get; set; }
	public int Year { get; set; }

	public async Task<VideoPlayer> LoadVideo( CancellationToken cancelToken = default )
	{
		var videoLoader = new AsyncVideoLoader();
		return await videoLoader.LoadFromFile( FileSystem.Mounted, VideoPath, cancelToken );
	}
}
