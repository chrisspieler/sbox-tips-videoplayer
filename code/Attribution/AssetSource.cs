/// <summary>
/// Provides attribution and licensing information for an asset.
/// </summary>
[GameResource(RES_NAME, RES_EXTENSION, RES_DESCRIPTION, Icon = RES_ICON, IconBgColor = RES_BG_COLOR)]
public class AssetSource : GameResource
{
	private const string RES_NAME = "Asset Source";
	private const string RES_EXTENSION = "asrc";
	private const string RES_DESCRIPTION = "Provides attribution and license information for an asset.";
	private const string RES_ICON = "source";
	private const string RES_BG_COLOR = "cornflowerblue";

	public string Title { get; set; }
	public string Author { get; set; }
	public string Url { get; set; }
	public AssetLicense License { get; set; }
}
