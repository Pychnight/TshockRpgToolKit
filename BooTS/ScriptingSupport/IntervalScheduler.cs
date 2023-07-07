using Corruption.PluginSupport;
using System;
using System.Diagnostics;

namespace BooTS
{
	/// <summary>
	/// Runs a script at a repeating interval.
	/// </summary>
	internal class IntervalScheduler : Scheduler
	{
		internal TimeSpan Interval;
		internal TimeSpan lastRunTime;

		internal IntervalScheduler(string interval)
		{
			if (TimeSpan.TryParse(interval, out var result))
				Interval = result;
			else
				BooScriptingPlugin.Instance.LogPrint($"IntervalScheduler: Unable to parse time '{interval}'. ", TraceLevel.Error);

			lastRunTime = new TimeSpan(0, 0, 0);
		}

		internal override bool OnUpdate(TimeSpan currentTime)
		{
			var delta = currentTime - lastRunTime;

			if (delta.Ticks < 0)
			{
				//in case of 24hr wrap around..
				delta = lastRunTime - currentTime;// - lastRunTime;
			}

			if (delta >= Interval)
			{
				if (Condition?.Invoke() != false)
				{
					//BooScriptingPlugin.Instance.RunScript(Script);
					lastRunTime = currentTime;
					return true;
				}
			}

			return false;
		}
	}
}
