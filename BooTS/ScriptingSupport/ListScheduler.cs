using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BooTS
{
	/// <summary>
	/// Runs a script at specified times.
	/// </summary>
	internal class ListScheduler : Scheduler
	{
		List<TimeSpan> times;
		TimeSpan? lastCheckTime; //keeps from rerunning the script multiple times, as the current time may extend through multiple game updates.

		internal ListScheduler(IEnumerable<string> times)
		{
			this.times = new List<TimeSpan>();

			foreach (var t in times)
			{
				if (TimeSpan.TryParse(t, out var result))
					this.times.Add(result);
				else
					BooScriptingPlugin.Instance.LogPrint($"ListScheduler: Unable to parse time '{t}'. ", TraceLevel.Error);
			}
		}

		internal override bool OnUpdate(TimeSpan currentTime)
		{
			if (currentTime == lastCheckTime)
				return false;

			lastCheckTime = currentTime;

			if (times.Contains(currentTime))
			{
				if (Condition?.Invoke() != false)
				{
					//BooScriptingPlugin.Instance.RunScript(Script);
					return true;
				}
			}

			return false;
		}
	}
}
