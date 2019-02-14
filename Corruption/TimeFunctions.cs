using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;

namespace Corruption
{
	/// <summary>
	/// Provides methods for checking Terraria time and events. Also includes methods for delaying execution.  
	/// </summary>
	public static class TimeFunctions
	{
		// Utility function for spawning or replacing after any mechanical boss is defeated.
		public static bool AfterAnyMechBoss()
		{
			return NPC.downedMechBossAny;
		}

		// Utility public static void for spawning or replacing after Golem is defeated.
		public static bool AfterGolem()
		{
			return NPC.downedGolemBoss;
		}

		// Utility public static void for spawning or replacing after the Moon Lord is defeated.
		public static bool AfterMoonLord()
		{
			return NPC.downedMoonlord;
		}

		// Utility public static void for spawning or replacing after Plantera is defeated.
		public static bool AfterPlantera()
		{
			return NPC.downedPlantBoss;
		}

		/// <summary>
		/// Gets a TimeSpan representing the current hour and minute.
		/// </summary>
		/// <returns>TimeSpan</returns>
		public static TimeSpan GetTimeOfDay()
		{
			//ripped from tshocks /time command...
			double num = Main.time / 3600.0;
			num += 4.5;
			if( !Main.dayTime )
			{
				num += 15.0;
			}
			num %= 24.0;

			var hour = (int)Math.Floor(num);
			var min = (int)Math.Round(num % 1.0 * 60.0);
			var ts = new TimeSpan(0, hour, min, 0, 0);

			return ts;
		}

		/// <summary>
		/// Returns whether the current Terraria time is within min and max.
		/// </summary>
		/// <param name="min">Minimum time, in 24 hour format.</param>
		/// <param name="max">Maximum time, in 24 hour format.</param>
		/// <returns>Boolean result.</returns>
		/// <remarks>Min and max are both inclusive.</remarks>
		public static bool DuringTime(string min, string max)
		{
			var timeOfDay = GetTimeOfDay();

			if( !TimeSpan.TryParse(min, out var minTime) )
				return false;

			if( !TimeSpan.TryParse(max, out var maxTime) )
				return false;

			if( minTime <= timeOfDay && maxTime >= timeOfDay )
				return true;

			return false;
		}

		// Utility public static void for spawning or replacing during the day.
		public static bool DuringDay()
		{
			return Main.dayTime;
		}

		// Utility public static void for spawning or replacing during the night.
		public static bool DuringNight()
		{
			return !Main.dayTime;
		}

		// Utility public static void for spawning or replacing during a blood moon.
		public static bool DuringBloodMoon()
		{
			return Main.bloodMoon;
		}
		
		// Utility public static void for spawning or replacing during a frost moon.
		public static bool DuringFrostMoon()
		{
			return Main.snowMoon;
		}

		// Utility public static void for spawning or replacing during an eclipse.
		public static bool DuringEclipse()
		{
			return Main.eclipse;
		}

		public static bool DuringMoonPhase(int phase)
		{
			return Main.moonPhase == phase;
		}

		// Utility public static void for spawning or replacing during hardmode.
		public static bool DuringHardmode()
		{
			return Main.hardMode;
		}
		
		// Utility public static void for spawning or replacing during a pumpkin moon.
		public static bool DuringPumpkinMoon()
		{
			return Main.pumpkinMoon;
		}

		// Utility public static void for spawning or replacing during rain.
		public static bool DuringRain()
		{
			return Main.raining;
		}

		// Utility public static void for spawning or replacing during slime rain.
		public static bool DuringSlimeRain()
		{
			return Main.slimeRain;
		}

		/// <summary>
		/// Pauses execution for the specified duration.
		/// </summary>
		/// <param name="milliseconds"></param>
		public static void Delay(int milliseconds)
		{
			Task.Delay(milliseconds).Wait();
		}

		/// <summary>
		/// Pauses execution for the specified duration.
		/// </summary>
		/// <param name="timeSpan"></param>
		public static void Delay(TimeSpan timeSpan)
		{
			Task.Delay(timeSpan).Wait();
		}
		
		/// <summary>
		/// Pauses execution for the specified duration, using a <see cref="CancellationToken" />.
		/// </summary>
		/// <param name="milliseconds"></param>
		/// <param name="cancellationToken"></param>
		public static void Delay(int milliseconds, CancellationToken cancellationToken )
		{
			Task.Delay(milliseconds, cancellationToken).Wait();
		}

		/// <summary>
		/// Pauses execution for the specified duration, using a <see cref="CancellationToken" />.
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <param name="cancellationToken"></param>
		public static void Delay(TimeSpan timeSpan, CancellationToken cancellationToken)
		{
			Task.Delay(timeSpan, cancellationToken).Wait();
		}
	}
}
