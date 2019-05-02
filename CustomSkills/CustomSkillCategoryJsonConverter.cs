using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomSkills
{
	/// <summary>
	/// Provides custom json serialization and deserialization needed for use with the DataDefinitionFormat.
	/// </summary>
	public class CustomSkillCategoryJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => typeof(CustomSkillCategory).IsAssignableFrom(objectType);
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var category = new CustomSkillCategory();
			var jobject = JObject.Load(reader);

			//get category name
			var catProp = jobject.Properties().Where(p => p.Name == "Category").FirstOrDefault();
			if(catProp!=null)
				category.Name = (string)catProp.Value;
	
			//get skills
			var skillsProp = jobject.Properties().Where(p => p.Name == "Skills").FirstOrDefault();
			if(skillsProp!=null)
			{
				//skillsProp.Value
				var skills = JsonConvert.DeserializeObject<List<CustomSkillDefinition>>(skillsProp.Value.ToString());

				foreach(var skill in skills)
					category.Add(skill.Name, skill);
			}
			
			return category;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			var category = value as CustomSkillCategory;

			if(category!=null)
			{
				writer.WritePropertyName("Category");
				writer.WriteValue(category.Name);

				writer.WritePropertyName("Skills");
				var skills = category.Values.AsEnumerable();
				serializer.Serialize(writer,skills);
			}
			
			writer.WriteEndObject();
		}
	}
}
