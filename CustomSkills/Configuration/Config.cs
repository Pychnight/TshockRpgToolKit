using Corruption.PluginSupport;
using Newtonsoft.Json;

namespace CustomSkills
{
	/// <summary>
	/// Custom Skills Configuration.
	/// </summary>
	[JsonObject]
	internal sealed class Config : JsonConfig
	{
		/// <summary>
		/// Gets the Config instance.
		/// </summary>
		public static Config Instance { get; internal set; } = new Config();

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();
			
			//do validation here!

			return result;
		}
	}
}
