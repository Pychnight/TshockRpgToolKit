using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace RpgToolsEditor.Controls
{
	public class CategoryLoader<TModel> where TModel : IModel, new()
	{
		public List<IModel> ParseCategorysAndModels(string json)
		{
			var items = JArray.Parse(json);

			var results = impl(items);

			return results;
		}

		public List<IModel> impl(JArray jArray)
		{
			var results = new IModel[jArray.Count];

			for (var i = 0; i < results.Length; i++)
			{
				if (jArray[i].HasValues && jArray[i].Type == JTokenType.Object)
				{
					var jObject = JObject.Parse(jArray[i].ToString());

					if (jObject.ContainsKey("Category"))
					{
						var itemJson = jArray[i].ToString();

						var cat = new CategoryModel();
						JsonConvert.PopulateObject(itemJson, cat);
						results[i] = cat;
					}
					else
					{
						//its other root level object
						var itemJson = jArray[i].ToString();

						var model = new TModel();
						JsonConvert.PopulateObject(itemJson, model);
						results[i] = model;
					}
				}
			}

			return results.ToList();
		}
	}
}
