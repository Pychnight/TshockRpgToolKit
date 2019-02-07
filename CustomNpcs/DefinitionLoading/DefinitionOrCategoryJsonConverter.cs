using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	internal class DefinitionOrCategoryJsonConverter : JsonConverter
	{
		Type definitionType;

		public override bool CanWrite => false;

		public DefinitionOrCategoryJsonConverter(Type definitionType)
		{
			this.definitionType = definitionType;
		}

		public override bool CanConvert(Type objectType)
		{
			var result = typeof(DefinitionBase).IsAssignableFrom(objectType);
			return result;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject item = JObject.Load(reader);

			var category = item.Property("Category") ?? item.Property("category");//try to catch either casing.

			if( category != null )
			{
				//this is a category object, with includes
				var result = item.ToObject<CategoryDefinition>();

				//result.JsonPath = reader.Path;

				return result;
			}
			else
			{
				// is definition
				var result = item.ToObject(definitionType);
				return result;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
