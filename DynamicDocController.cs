using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using System.Linq;
using System;
using Umbraco.Core.Services;
using UmbracoDynamicDoc.Exceptions;
using System.Configuration;

namespace UmbracoDynamicDoc
{
	public sealed class DynamicDocController : UmbracoApiController
	{
		private static readonly Dictionary<int, string> ContentTypeLookup = new Dictionary<int, string>();

		/// <summary>Returns a document based in it's ID</summary>
		public ContentDocument GetDocument(int Id)
		{
			// Security Check
			AssertContentTypeAllowed(Id);

			var content = Services.ContentService.GetById(Id);
			return new ContentDocument(Services.DataTypeService, content);
		}

		/// <summary>
		/// Gets a document by it's Name anywhere in the tree structure
		/// </summary>
		/// <param name="Id">The name of the document</param>
		/// <returns>A list of documents with that name</returns>
		public IEnumerable<ContentDocument> GetAllDocumentsByName(string Id)
		{
			int total;
			var docs = Services.ContentService.GetPagedDescendants(-1, 0, int.MaxValue, out total).Where(n => n.Name == Id);

			// Security Check
			docs.ToList().ForEach(n => AssertContentTypeAllowed(n.ContentTypeId));

			return docs.Select(n => new ContentDocument(Services.DataTypeService, n));
		}

		/// <summary>
		/// Gets all documents of a specified document type alias. 
		/// </summary>
		/// <param name="Id">Content type alias</param>
		public IEnumerable<ContentDocument> GetAllDocumentsByType(string Id)
		{
			// Security Check
			AssertContentTypeAllowed(Id);

			int total;
			var docs = Services.ContentService.GetPagedDescendants(-1, 0, int.MaxValue, out total).Where(n => n.ContentType.Alias == Id);
			return docs.Select(n => new ContentDocument(Services.DataTypeService, n));
		}

		/// <summary>
		/// Creates or Modifies a Document
		/// </summary>
		/// <param name="values">A Dictionary of key / values of document or property data.</param>
		/// <param name="Id">Content Type Alias (required in the URL or the dictionary)</param>
		/// <param name="name">Name of the Document (required in the URL or the dictionary)</param>
		/// <param name="parentId">The parent node id (required in the URL or the dictionary)</param>
		/// <param name="userId">User created the document (optional)</param>
		/// <returns>Created or Updated Document</returns>
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

			// Security Check
			AssertContentTypeAllowed(contentTypeAlias);

			IContent doc;
			int docId;
			if (values.ContainsKey("Id") && !string.IsNullOrWhiteSpace(values["Id"]) && int.TryParse(values["Id"], out docId))
				doc = Services.ContentService.GetById(docId);
			else
				doc = Services.ContentService.CreateContent(name, parentId, contentTypeAlias, userId);

			MapPropertyData(doc, values);

			var result = Services.ContentService.SaveAndPublishWithStatus(doc, userId);
			if (!result.Success)
				throw new InvalidOperationException("Unable to save and publish document. Error: " + result.Exception.ToString());

			return new ContentDocument(Services.DataTypeService, result.Result.ContentItem);
		}

		/// <summary>
		/// Creates or Replaces a Document from a ContentDocument prototpye
		/// </summary>
		/// <param name="value">ContentDocument object to create or update</param>
		/// <param name="Id">Contnent Type Alias</param>
		/// <param name="userId">User / Author (optional)</param>
		/// <returns>The ID of the created or updated document</returns>
		public int PutDocument([FromBody] ContentDocument value, [FromUri] string Id /*contentTypeAlias*/ = null, [FromUri] int userId = 0)
		{
			if (!string.IsNullOrWhiteSpace(Id) && string.IsNullOrWhiteSpace(value.ContentType))
				value.ContentType = Id;

			if (value.ContentTypeId == default(int) && string.IsNullOrWhiteSpace(value.ContentType))
				throw new ArgumentNullException("ContentType / ContentTypeId");
			else
				value.ContentType = Services.ContentTypeService.GetContentType(value.ContentTypeId).Alias;

			if (value.ParentId == null)
				throw new ArgumentNullException("ParentId");

			if (string.IsNullOrWhiteSpace(value.Name))
				throw new ArgumentNullException("Name");

			// Security Check
			AssertContentTypeAllowed(value.ContentType);

			IContent doc;
			if (value.Id != default(int))
				doc = Services.ContentService.GetById(value.Id);
			else
				doc = Services.ContentService.CreateContent(value.Name, value.ParentId, value.ContentType, userId);

			value.ToDocument(doc);
			
			var result = Services.ContentService.SaveAndPublishWithStatus(doc, userId);
			if(!result.Success)
				throw new InvalidOperationException("Unable to save and publish document. Error: " + result.Exception.ToString());

			return result.Result.ContentItem.Id;
		}

		/// <summary>
		/// Delete a document by Id
		/// </summary>
		/// <returns>The Id of the document that was deleted.</returns>
		public int DeleteDocument(string Id)
		{
			int docId;
			if (!int.TryParse(Id, out docId))
				throw new InvalidCastException(string.Format("Unable to convert value '{0}' to an integer.", Id));

			var doc = Services.ContentService.GetById(docId);

			// Security Check
			AssertContentTypeAllowed(doc.ContentTypeId);

			Services.ContentService.Delete(doc);
			return doc.Id;
		}

		private void AssertContentTypeAllowed(int contentAliasId)
		{
			Initialize();

			if (!ContentTypeLookup.ContainsKey(contentAliasId))
				throw new AccessException(string.Format("Content type Id of '{0}' is not listed in the allowed document types.", contentAliasId));
		}

		private void AssertContentTypeAllowed(string contentTypeAlias)
		{
			Initialize();

			if (!ContentTypeLookup.ContainsValue(contentTypeAlias))
				throw new AccessException(string.Format("Content type alias of '{0}' is not listed in the allowed document type aliases.", contentTypeAlias));
		}

		private void Initialize()
		{
			if (ContentTypeLookup.Count == 0)
			{
				lock (ContentTypeLookup)
				{
					// Double check to see if a lock just release may have done this already.
					if (ContentTypeLookup.Count == 0)
					{
						string[] allowedTypes = ConfigurationManager.AppSettings["DynamicDocAllowedTypeAliases"].Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);

						if (allowedTypes.Length == 0)
							throw new ApplicationConfigurationException("Unable to find AppSetting named \"DynamicDocAllowedTypeAliases\" with allowed document / content type aliases.");

						foreach (var type in Services.ContentTypeService.GetAllContentTypes())
							if (allowedTypes.Contains(type.Alias))
								ContentTypeLookup.Add(type.Id, type.Alias);
					}
				}
			}
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
	}
}
