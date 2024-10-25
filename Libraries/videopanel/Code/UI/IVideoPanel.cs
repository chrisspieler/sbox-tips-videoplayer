namespace Duccsoft;

public interface IVideoPanel : IValid
{
	float Duration { get; }
	bool IsPlaying { get; }
	float PlaybackTime { get; }
	bool IsPaused { get; }
	bool Muted { get; set; }
	void Seek( float time );
	void TogglePause();
}
