# UmbracoDynamicDoc
Simple API to create different kinds of documents from an AJAX JSON API Call

1. Drop the DLL into the bin directory of an Umbraco 7.x installation. (Note: This is likely to be distributed as a package and in that case you would want ot import the package instead)
2. Add the list of document type aliases that you want to work with via the API in the web config appSettings:

```
<appSettings>
	<add key="DynamicDocAllowedTypeAliases" value="Product, LineItem"/>
</appSettings>
```

2. The URL should be something like `http://YourUmbracoSite.com/umbraco/api/DynamicDoc/{Action}/{Id}`

Example: 

Get document by id (get whatever document is ID 51):

`GET http://costigno.com/umbraco/api/DynamicDoc/GetDocument/51`

Get all documents by name (get all documents named 'ProductPage'):

`GET http://costigno.com/umbraco/api/DynamicDoc/GetAllDocumentsByName/ProductPage`

Get documents by type alias (get all documents of type 'Product'):

`GET http://costigno.com/umbraco/api/DynamicDoc/GetAllDocumentsByType/Product`

Create or Edit Document (Create or update a Product):

`POST http://costigno.com/umbraco/api/DynamicDoc/PostDocument/Product?name=NewDoc&parentId=88&userId=0`

POSTED JSON:
```JavaScript
{
	"Id": "12", 	// Note: This is required if you wish to modify the document ratehr than create a new one.
	"name": "NewDoc", // Note: Pass either here or in the URL
	"parendId": "88", // Note: Pass either here or in the URL
	"userId": "0", // Note: Pass either here or in the URL. Optional
	"address1": "123 Fake Street",
	"city": "Springfield"
}
```

Create or Edit a Document (Create or update a Product Using the JSON structure from the GetDocument calls):

`PUT http://costigno.com/umbraco/api/DynamicDoc/PutDocument/Product?name=NewDoc&parentId=88&userId=0`

Delete a document by id (Delete document with ID of 55):

`DELETE http://costigno.com/umbraco/api/DynamicDoc/DeleteDocument/55`

That should do it!






