# UmbracoDynamicDoc
Simple API to create different kinds of documents from an AJAX JSON API Call

1. Drop the DLL into the bin directory of an Umbraco 7.x installation. (Note: This is likely to be distributed as a package and in that case you would want ot import the package instead)
2. Add the list of document type aliases that you want to work with via the API in the web config appSettings:


<appSettings>
	<add key="DynamicDocAllowedTypeAliases" value="Product, LineItem"/>
</appSettings>
