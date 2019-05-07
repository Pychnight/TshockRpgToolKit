using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	/// <summary>
	/// Tracks player state per learned skill.
	/// </summary>
	public class PlayerSkillInfo
	{
		public int CurrentLevel { get; set; }
		public int CurrentUses { get; set; }
		public DateTime LastFireTime { get; set; }

		//public void OnReload()
		//{
		//	LastFireTime 
		//}
	}
}
