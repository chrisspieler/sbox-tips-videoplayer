using System;

public class Oscillator : Component
{
	[Property] public Vector3 Direction { get; set; }

	[Property] public float Frequency 
	{
		get => _frequency;
		set
		{
			_frequency = MathF.Max( 0f, value );
		}
	}
	private float _frequency = 1f;

	[Property] public float Amplitude { get; set; } = 25f;

	protected override void OnFixedUpdate()
	{
		var frequency = Frequency * MathF.PI;
		var sin = MathF.Sin( Time.Now * frequency );
		LocalPosition = Direction.Normal * sin * Amplitude;
	}
}
