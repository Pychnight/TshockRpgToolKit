using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	///// <summary>
	///// Converts json objects into either Categories(with includes), or the given model type. 
	///// </summary>
	///// <typeparam name="TModel"></typeparam>
	//public class IModelConverter<TModel> : CustomCreationConverter<IModel> where TModel : IModel, new()
	//{
	//	public override IModel Create(Type objectType)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public IModel Create(Type objectType, JObject jObject)
	//	{
	//		IModel result = null;
	//		var isCategory = jObject.Properties().FirstOrDefault(jp => jp.Name == "Category") != null;

	//		if( isCategory )
	//			result = new CategoryModel();
	//		else
	//			result = new TModel();

	//		return result;

	//		//throw new ApplicationException($"Type '{objectType}' is not valid!");
	//	}

	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		// Load JObject from stream 
	//		JObject jObject = JObject.Load(reader);

	//		// Create target object based on JObject 
	//		var target = Create(objectType, jObject);

	//		// Populate the object properties 
	//		serializer.Populate(jObject.CreateReader(), target);
			
	//		return target;
	//	}
	//}

	public class IModelConverter<TModel> : CustomCreationConverter<IModel> where TModel : IModel, new()
	{
		public override IModel Create(Type objectType)
		{
			throw new NotImplementedException();
		}

		public IModel Create(Type objectType, JObject jObject)
		{
			IModel result = null;
			var isCategory = jObject.Properties().FirstOrDefault(jp => jp.Name == "Category") != null;

			if( isCategory )
				result = new CategoryModel();
			else
				result = new TModel();

			return result;

			//throw new ApplicationException($"Type '{objectType}' is not valid!");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// Load JObject from stream 
			JObject jObject = JObject.Load(reader);

			// Create target object based on JObject 
			var target = Create(objectType, jObject);

			// Populate the object properties 
			serializer.Populate(jObject.CreateReader(), target);

			return target;
		}
	}
}
