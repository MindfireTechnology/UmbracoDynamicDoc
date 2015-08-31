using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace UmbracoDynamicDoc
{
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

			foreach (var propInfo in content.PropertyTypes.OrderBy(n => n.SortOrder))
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

		public void ToDocument(IContent document)
		{
			document.Id = Id;
			document.Name = Name;
			document.ParentId = ParentId;

			foreach (var property in Properties)
			{
				if (document.HasProperty(property.Alias))
				{
					document.Properties[property.Alias].Value = property.Value;
				}
			}
		}
	}

}
