using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using System.Linq;
using System;
using Umbraco.Core.Services;

namespace UmbracoDynamicDoc
{
	public class DynamicDocController : UmbracoApiController
	{
		public ContentDocument GetDocument(int Id)
		{

			var content = Services.ContentService.GetById(Id);
			return new ContentDocument(Services.DataTypeService, content);
		}

		public IEnumerable<ContentDocument> GetAllDocumentsByName(string Id)
		{
			int total;
			var docs = Services.ContentService.GetPagedDescendants(-1, 0, int.MaxValue, out total).Where(n => n.Name == Id);
			return docs.Select(n => new ContentDocument(Services.DataTypeService, n));
		}

		public IEnumerable<ContentDocument> GetAllDocumentsByType(string Id)
		{
			int total;
			var docs = Services.ContentService.GetPagedDescendants(-1, 0, int.MaxValue, out total).Where(n => n.ContentType.Alias == Id);
			return docs.Select(n => new ContentDocument(Services.DataTypeService, n));
		}

		public ContentDocument PostDocument([FromBody] Dictionary<string, string> values, [FromUri] string Id /*contentTypeAlias*/ = null, [FromUri] string name = null, [FromUri] int parentId = 0, [FromUri] int userId = 0)
		{
			if (string.IsNullOrWhiteSpace(name))
				if (!values.TryGetValue("name", out name))
					throw new ArgumentNullException("name");

			if (parentId == 0)
			{
				string parentIdStr;
				if (!values.TryGetValue("parentId", out parentIdStr))
					throw new ArgumentNullException("parentId");

				if (!int.TryParse(parentIdStr, out parentId))
					throw new InvalidCastException(string.Format("Unable to cast value '{0}' to integer", parentIdStr));
			}

			string contentTypeAlias = Id;
			if (string.IsNullOrWhiteSpace(contentTypeAlias))
				if (!values.TryGetValue("contentTypeAlias", out contentTypeAlias))
					throw new ArgumentException("contentTypeAlias");

			if (userId == 0)
			{
				string userStr;
				if (values.TryGetValue("userId", out userStr))
					if (!int.TryParse(userStr, out userId))
						throw new InvalidCastException(string.Format("Unable to cast value '{0}' as integer", userStr));
			}
			
			
			var doc = Services.ContentService.CreateContent(name, parentId, contentTypeAlias, userId);

			MapPropertyData(doc, values);

			Services.ContentService.SaveAndPublishWithStatus(doc, userId);

			return new ContentDocument(Services.DataTypeService, doc);
		}

		private void MapPropertyData(IContent doc, Dictionary<string, string> values)
		{
			foreach (string key in values.Keys)
			{
				var prop = doc.Properties.SingleOrDefault(n => n.Alias == key);
				if (prop != null)
					prop.Value = values[key];
			}
		}

		public void UpdateDocument() { }

		public void DeleteDocument(int id) { }
	}


	public class ContentDocument
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public string ContentType { get; set; }
		public int ContentTypeId { get; set; }
		public int ParentId { get; set; }
		public int Level { get; set; }
		public string Path { get; set; }

		public ICollection<Property> Properties { get; set; }

		public ContentDocument()
		{
			Properties = new List<Property>();
		}

		public ContentDocument(IDataTypeService service, IContent content) : this()
		{
			Id = content.Id;
			ContentTypeId = content.ContentTypeId;
			ContentType = content.ContentType.Name;
			Name = content.Name;
			ParentId = content.ParentId;
			Level = content.Level;
			Path = content.Path;

			foreach(var propInfo in content.PropertyTypes.OrderBy(n => n.SortOrder))
			{
				
				var p = new Property
				{
					Name = propInfo.Name,
					Alias = propInfo.Alias,
					Description = propInfo.Description,
					Required = propInfo.Mandatory,
					Validation = propInfo.ValidationRegExp,
					DataType = service.GetDataTypeDefinitionById(propInfo.DataTypeDefinitionId).Name,
					Value = (content.Properties.SingleOrDefault(n => n.Alias == propInfo.Alias).Value ?? string.Empty).ToString()
				};
				Properties.Add(p);
			}
		}

	}

	public class Property
	{
		public string Name { get; set; }
		public string Alias { get; set; }
		public string Description { get; set; }
		public bool Required { get; set; }
		public string Validation { get; set; }
		public string DataType { get; set; }
		public string Value { get; set; }
	}
}
