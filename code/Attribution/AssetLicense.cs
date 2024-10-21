/// <summary>
/// Describes the state of licensing for an asset containing copyrighted material. 
/// This may refer to a specific license, to the fact that a work has no license 
/// and therefore is copyrighted by default, or to a state of being used without 
/// permission as legally protected under a particular exemption to copyright law.
/// <br/><br/>
/// I am not a lawyer, and XML comments are not legal advice.
/// </summary>
[GameResource( RES_NAME, RES_EXTENSION, RES_DESCRIPTION, Icon = RES_ICON, IconBgColor = RES_BG_COLOR )]
public class AssetLicense : GameResource
{
	private const string RES_NAME = "License Status";
	private const string RES_EXTENSION = "lcns";
	private const string RES_DESCRIPTION = "Specifies either a copyright license, or a state of being unlicensed.";
	private const string RES_ICON = "description";
	private const string RES_BG_COLOR = "cornflowerblue";

	public string Name { get; set; }
	public string Description { get; set; }
	public string Url { get; set; }
	public string Text { get; set; }
}
