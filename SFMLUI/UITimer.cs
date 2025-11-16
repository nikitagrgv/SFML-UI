using System.Diagnostics;

namespace SFMLUI;

public class UITimer
{
	private readonly static List<UITimer> Timers = new();
	private static long _time = 0;

	private bool _enqueued;
	private bool _active;
	private float _curTime;

	public event Action? Triggered;

	public float Interval { get; set; }
	public bool SingleShot { get; set; }
	public bool IsActive => _active;

	public void Restart()
	{
		if (!_enqueued)
		{
			Debug.Assert(!Timers.Contains(this));
			Timers.Add(this);
			_enqueued = true;
		}

		_active = true;
		_curTime = 0;
	}

	public void Stop()
	{
		_active = false;
		_curTime = 0;
	}

	internal static void Update()
	{
		if (_time == 0)
		{
			_time = Stopwatch.GetTimestamp();
			return;
		}

		long newTime = Stopwatch.GetTimestamp();
		float elapsed = (float)Stopwatch.GetElapsedTime(_time, newTime).TotalSeconds;
		_time = newTime;

		int curTimersCount = Timers.Count;
		for (int i = 0; i < curTimersCount; i++)
		{
			UITimer timer = Timers[i];
			Debug.Assert(timer._enqueued);

			if (!timer.IsActive)
			{
				// TODO: Optimize
				Timers.RemoveAt(i);
				--i;
				--curTimersCount;
				timer._enqueued = false;
				continue;
			}

			timer._curTime += elapsed;
			if (!(timer._curTime >= timer.Interval))
			{
				continue;
			}

			timer.Triggered?.Invoke();
			timer._curTime = 0;

			if (!timer.SingleShot)
			{
				continue;
			}

			timer._active = false;
			Timers.RemoveAt(i);
			--i;
			--curTimersCount;
			timer._enqueued = false;
		}
	}
}