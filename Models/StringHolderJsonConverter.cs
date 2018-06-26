using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Models
{
	public class StringHolderJsonConverter : JsonConverter
	{
		public override bool CanRead => true;
		public override bool CanWrite => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(StringHolder);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if( reader.TokenType == JsonToken.String )
			{
				return new StringHolder((string)reader.Value);
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var holder = (StringHolder)value;
			writer.WriteValue(holder.Value);
		}
	}

	public class TerrariaItemStringHolderJsonConverter : JsonConverter
	{
		public override bool CanRead => true;
		public override bool CanWrite => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TerrariaItemStringHolder);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if( reader.TokenType == JsonToken.String )
			{
				return new TerrariaItemStringHolder((string)reader.Value);
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var holder = (TerrariaItemStringHolder)value;
			writer.WriteValue(holder.Value);
		}
	}
}
