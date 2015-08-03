CyclopsScaffold
===============

Entity Framework based for creating C.R.U.D for MVC Project. Before using it, please make sure you compile your project that containing EF Model and Context.

Basically, it would create 4 section, BLL, Controller, Models and Views. If the EF model or context in another library from your web app, the BLL directory should be in that library and the rest in your web app.

This tools have been tested in Visual Studio 2013 Community Edition, and it should run on VS 2013 Premium and Ultimate

It can't run express edition of Visual Studio because microsoft not allowed it, here's a [link](https://visualstudiomagazine.com/articles/2014/05/21/no-extensions-for-visual-studio-express.aspx) about it.

This tools was created to help on learning and hopefully help when development.

![User Interface](https://raw.githubusercontent.com/DidietEkaPermana/CyclopsScaffold/master/Image/cs2.png)

##controllers
There are 4 type of controller, whis is:

1. **MVC**, plain MVC controller and REST method call within the controller
2. **WEBAPI**, it will create API folder in controller for REST method and WebApiConfig class
3. **ODataV3**, the same as WEBAPI, but would create odata folder in controller. You should install nuget package for OData V3
4. **ODataV4**, practically the same as ODataV4 but for Ver 4 of Odata

##views
There would be 2 kind of view, plain JQuery and with component
if you choose component, you would need:

1. [moment.js](http://momentjs.com/)
2. [bootstrap-modalloading](https://github.com/ehpc/bootstrap-waitingfor)
3. [bootstrap-select](http://silviomoreto.github.io/bootstrap-select/)
4. [bootstrap-datetimepicker](https://github.com/Eonasdan/bootstrap-datetimepicker)
5. [bootstrapGrid](https://github.com/DidietEkaPermana/bootstrapGrid)

###track

>create scaffold for SPA/angularJS

If you have any question or suggestion please contact me

Company contact: ``didiet.permana@ecomindo.com``

Personal contact: ``didiet_eka_permana@hotmail.com``
