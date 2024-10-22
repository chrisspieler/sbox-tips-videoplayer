using Sandbox.UI;
using System;

public partial class AttributionPanel : Panel
{
	public VideoSource Source 
	{
		get => _source;
		set
		{
			_source = value;
			Icon = GetSocialMediaIcon();
		}
	}
	private VideoSource _source;
	private string Title => Source?.Title;
	private string Url => Source?.Url;
	private string Year => Source is null ? string.Empty : Source.Year.ToString();
	private string Author => Source?.Author;
	private string Handle { get; set; }
	private SocialMediaIcon Icon { get; set; }

	private List<SocialMediaIcon> Icons { get; set; } = new();

	protected override int BuildHash() => HashCode.Combine( Source );

	public AttributionPanel()
	{
		Icons = ResourceLibrary.GetAll<SocialMediaIcon>().ToList();
	}

	private SocialMediaIcon GetSocialMediaIcon()
	{
		if ( Source is null || Author is null )
			return null;

		foreach( var icon in Icons )
		{
			if ( icon is null || !icon.TryExtractUserHandle( Author, out string userHandle ) )
				continue;

			Handle = userHandle;
			return icon;
		}
		return null;
	}
}
