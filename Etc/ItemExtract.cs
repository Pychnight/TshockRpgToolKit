using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// This was only a one-off script to get and format strings from the php items name file. 
/// </summary>
public static class ItemExtract
{
	public static int Main(string[] args)
	{
		//if( args.Length < 1 )
		//{
		//	Console.WriteLine("No inputs.");
		//	return 1;
		//}

		var sb = new StringBuilder(1024 * 8);
		
		var regexString = new Regex("\".*\"",RegexOptions.Compiled);

		//var inFile = args[0];
		var inFile = "items.txt";
		var lines = File.ReadAllLines(inFile);
		//var text = File.ReadAllText(inFile);

		bool useComma = false;

		sb.AppendLine("(");

		foreach(var l in lines)
		{
			var match = regexString.Match(l);
			
			if(match.Success)
			{
				if( useComma )
					sb.AppendLine(",");

				Debug.Print(match.Value);
				sb.Append(match.Value);
				useComma = true;
			}
		}

		sb.AppendLine(");");

		File.WriteAllText("itemnames.txt", sb.ToString());

		return 0;
	}
}