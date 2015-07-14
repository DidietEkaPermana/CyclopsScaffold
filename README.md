CyclopsScaffold
===============

Entity Framework based for creating C.R.U.D for MVC Project. Before using it, please make sure you compile your project that containing EF Model and Context.

Basically, it would create 4 section, BLL, Controller, Models and Views. If the EF model or context in another library from your web app, the BLL directory should be in that library and the rest in your web app.

This tools have been tested in Visual Studio 2013 Community Edition.


![User Interface](https://raw.githubusercontent.com/DidietEkaPermana/CyclopsScaffold/master/Image/cs2.png)

for views, there would be 2 kind of view, plain JQuery and with component
if you choose component, you would need:

1. [moment.js](http://momentjs.com/)
2. [bootstrap-modalloading](https://github.com/ehpc/bootstrap-waitingfor)
3. [bootstrap-select](http://silviomoreto.github.io/bootstrap-select/)
4. [bootstrap-datetimepicker](https://github.com/Eonasdan/bootstrap-datetimepicker)
5. [bootstrapGrid](https://github.com/DidietEkaPermana/bootstrapGrid)


###track

>create scaffold for OData v4

>create scaffold for angularJS