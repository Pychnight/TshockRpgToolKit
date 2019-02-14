using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	/// <summary>
	/// Base class for objects that may be used to schedule running of Scripts.
	/// </summary>
	public abstract class Scheduler
	{
		internal Func<bool> Condition { get; set; }
		//internal Script Script { get; set; }

		/// <summary>
		/// Checks time and other conditions, to see if the script should be run.
		/// </summary>
		/// <param name="currentTime">TimeSpan representing time of day in Terraria time.</param>
		internal abstract bool OnUpdate(TimeSpan currentTime);
		
		//---- Public DSL ---- 

		/// <summary>
		/// Creates a Scheduler that runs at the specified Terraria times.
		/// </summary>
		/// <param name="times"></param>
		/// <returns></returns>
		public static Scheduler RunAt(params string[] times)
		{
			return new ListScheduler(times);
		}

		/// <summary>
		/// Creates a Scheduler that runs at an interval, in Terraria time.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		public static Scheduler RunEvery(string interval)
		{
			return new IntervalScheduler(interval);
		}

		/// <summary>
		/// Attaches additional conditions that determine if a Script should run at a scheduled time. 
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public Scheduler When(Func<bool> condition)
		{
			Condition = condition;
			return this;
		}
	}
}
