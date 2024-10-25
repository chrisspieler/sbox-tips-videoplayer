using System;

namespace Duccsoft;

/// <summary>
/// Represents a presentation-related object that must be updated every frame.
/// For example, a video that must update a texture, or a sound that must update 
/// its position.
/// </summary>
public interface IMediaUpdateListener : IDisposable
{
	void MediaUpdate();

	public static void Register( IMediaUpdateListener listener )
	{
		if ( listener is null )
			return;

		var system = Game.ActiveScene?.GetSystem<MediaUpdateSystem>();
		if ( system is null )
			return;

		system.Register( listener );
	}

	public static void Unregister( IMediaUpdateListener listener )
	{
		if ( listener is null )
			return;

		var system = Game.ActiveScene?.GetSystem<MediaUpdateSystem>();
		if ( system is null )
			return;

		system.Unregister( listener );
	}
}
