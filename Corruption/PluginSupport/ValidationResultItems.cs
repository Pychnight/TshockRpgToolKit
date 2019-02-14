using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// Base class for errors, warnings, or other info within a <see cref="ValidationResult"/>.
	/// </summary>
	public abstract class ValidationResultItem
	{
		public ValidationResult Parent { get; internal set; }
		public string Message { get; set; }
		public object Source { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			var src = Source;

			//if we dont have a specific source set, try to grab a source from Parent.
			if(src==null)
				src = Parent?.Source;
			
			if(src!=null)
			{
				sb.Append(src.ToString());
				sb.Append(": ");
			}
			
			if ( !string.IsNullOrWhiteSpace(Message) )
				sb.Append(Message);

			return sb.ToString();
		}
	}

	//public class ValidationInfo : ValidationResultItem
	//{
	//}

	public class ValidationWarning : ValidationResultItem
	{
		public ValidationWarning() { }
		public ValidationWarning(string message, string source = null)
		{
			Message = message;
			Source = source;
		}
	}

	public class ValidationError : ValidationResultItem
	{
		public ValidationError() { }
		public ValidationError(string message, string source = null)
		{
			Message = message;
			Source = source;
		}
	}
}
