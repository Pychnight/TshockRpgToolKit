using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Triggers
{
	public enum TriggerStatus
	{
		Running,
		Success,
		Fail,
	}

	public static class TriggerStatusExtensions
	{
		public static TriggerStatus ToTriggerStatus(this bool value)
		{
			return value ? TriggerStatus.Success : TriggerStatus.Running;
		}
	}
}
