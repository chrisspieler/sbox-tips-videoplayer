using Sandbox.Audio;

namespace Duccsoft;

/// <summary>
/// Stores and applies the audio settings of a VideoPlayer.
/// </summary>
public class TrackingAudioAccessor : IAudioAccessor
{
	public TrackingAudioAccessor() { }
	public TrackingAudioAccessor( VideoPlayer videoPlayer )
	{
		VideoPlayer = videoPlayer;
	}

	public VideoPlayer VideoPlayer 
	{
		get => _videoPlayer;
		set
		{
			_videoPlayer = value;
			Update();
		}
	}
	private VideoPlayer _videoPlayer;

	public GameObject Target 
	{
		get => _target;
		set
		{
			_target = value;
			_listenLocal = !_target.IsValid();
			if ( !_listenLocal )
			{
				_position = _target.WorldPosition;
			}
			Update();
		}
	}
	private GameObject _target;

	public Vector3 Position 
	{
		get => _position;
		set
		{
			_position = value;
			_target = null;
			Update();
		}
	}
	private Vector3 _position;

	public float Volume 
	{ 
		get => _volume;
		set
		{
			_volume = value;
			Update();
		}
	}
	private float _volume = 1f;

	public bool ListenLocal 
	{
		get => _listenLocal;
		set
		{
			_listenLocal = value;
			if ( _listenLocal )
			{
				_target = null;
			}
			Update();
		}
	}
	private bool _listenLocal;
	public Mixer TargetMixer
	{
		get => _targetMixer;
		set
		{
			_targetMixer = value;
			Update();
		}
	}
	private Mixer _targetMixer;

	public bool Muted
	{
		get => _muted;
		set 
		{
			_muted = value;
			Update();
		}
	}
	private bool _muted;

	/// <summary>
	/// Updates properties of the VideoPlayer to match this configuration.
	/// Should be called every frame.
	/// </summary>
	public void Update()
	{
		if ( VideoPlayer is null )
			return;

		if ( Target.IsValid() )
		{
			_position = Target.WorldPosition;
		}

		VideoPlayer.Muted = Muted;
		VideoPlayer.Audio.Position = Position;
		VideoPlayer.Audio.Volume = Volume;
		VideoPlayer.Audio.ListenLocal = ListenLocal;
		VideoPlayer.Audio.TargetMixer = TargetMixer;
	}
}
