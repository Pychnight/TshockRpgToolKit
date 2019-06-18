using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// A script accessible object that tracks CustomSkill activity. 
	/// </summary>
	public class SkillState
	{
		private CustomSkill CustomSkill { get; set; }

		public TSPlayer Player => CustomSkill.Player;

		public float Progress { get; internal set; }
		public TimeSpan ElapsedTime { get; internal set; }

		Dictionary<string,object> variables;

		/// <summary>
		/// Provides keyed access to a SkillState's embedded variables.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				variables.TryGetValue(key, out var result);
				return result;
			}
			set => variables[key] = value;
		}

		internal SkillState(CustomSkill linkedSkill)
		{
			CustomSkill = linkedSkill;
			variables = new Dictionary<string, object>();
		}

		/// <summary>
		///     Determines whether the variable with the specified name exists.
		/// </summary>
		/// <param name="variableName">The name, which must not be <c>null</c>.</param>
		/// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
		public bool HasVariable(string variableName)
		{
			if(string.IsNullOrWhiteSpace(variableName))
				return false;

			return variables.ContainsKey(variableName);
		}
	}	
}
