namespace Duccsoft;

public class MediaUpdateSystem : GameObjectSystem
{
	public MediaUpdateSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, Update, "Media Update" );
	}

	private HashSet<IMediaUpdateListener> _listeners = new();

	private void Update()
	{
		foreach( var listener in _listeners )
		{
			listener?.MediaUpdate();
		}
	}

	public void Register( IMediaUpdateListener listener )
	{
		_listeners.Add( listener );
	}

	public void Unregister( IMediaUpdateListener listener )
	{
		_listeners.Remove( listener );
	}
}
