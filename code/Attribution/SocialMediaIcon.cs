/// <summary>
/// Defines a set of prefixes that can be used to associate an online handle with the 
/// path of an icon image that represents a social media platform.
/// </summary>
[GameResource( RES_NAME, RES_EXTENSION, RES_DESCRIPTION, Icon = RES_ICON, IconBgColor = RES_BG_COLOR )]
public class SocialMediaIcon : GameResource
{
	private const string RES_NAME = "Social Media Icon";
	private const string RES_EXTENSION = "sico";
	private const string RES_DESCRIPTION = "Associates a social media domain name with an icon representing that platform.";
	private const string RES_ICON = "share";
	private const string RES_BG_COLOR = "cornflowerblue";

	/// <summary>
	/// A list of prefixes that uniquely identify the platform in question -
	/// for example, a domain name or URI scheme.
	/// </summary>
	public List<string> Prefixes { get; set; }
	/// <summary>
	/// An icon to be displayed in small sizes beside the user handle.
	/// </summary>
	[ImageAssetPath]
	public string IconPath { get; set; }
	/// <summary>
	/// Returns true if either the prefix or the user handle are case sensitive.
	/// </summary>
	public bool CaseSensitive { get; set; } = false;

	/// <summary>
	/// Given a string, returns whether a valid handle for this social media platform 
	/// could be extracted from <paramref name="input"/> given the prefixes defined
	/// for this platform.
	/// </summary>
	/// <param name="input">A string containing a social media handle, possibly with a
	/// prefix at the start.</param>
	/// <param name="userHandle">The extracted user handle if this method returns true; otherwise, a null.</param>
	public virtual bool TryExtractUserHandle( string input, out string userHandle )
	{
		userHandle = null;
		foreach( var prefix in Prefixes )
		{
			var lowerPrefix = prefix;
			if ( !CaseSensitive )
			{
				lowerPrefix = prefix.ToLower();
				input = input.ToLower();
			}
			var prefixStartIndex = input.IndexOf( lowerPrefix );

			if ( prefixStartIndex < 0 )
				continue;

			var handleStartIndex = prefixStartIndex + prefix.Length;
			userHandle = input[handleStartIndex..];
			return true;
		}
		return false;
	}
}
