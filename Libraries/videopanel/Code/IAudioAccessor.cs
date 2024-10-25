using Sandbox.Audio;

namespace Duccsoft;

public interface IAudioAccessor
{
	public GameObject Target { get; set; }
	public Vector3 Position { get; set; }
	public float Volume { get; set; }
	public bool Muted { get; set; }
	public bool ListenLocal { get; set; }
	public Mixer TargetMixer { get; set; }
}
