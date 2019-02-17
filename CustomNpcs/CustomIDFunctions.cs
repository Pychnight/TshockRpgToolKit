using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	/// <summary>
	/// This class exists solely to support CustomIDContains() in scripts, without using instance. 
	/// </summary>
	public static class CustomIDFunctions
	{
		internal static string CurrentID { get; set; }

		public static bool CustomIDContains(string pattern)
		{
			if( string.IsNullOrWhiteSpace(CurrentID) )
				return false;

			return CurrentID.Contains(pattern);
		}
	}
}
