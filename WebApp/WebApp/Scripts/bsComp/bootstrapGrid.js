//! bootstrapGrid.js
//! version : 1.0.0
//! authors : Didiet Eka Permana (didiet.permana@gmail.com)
//! license : MIT

if (typeof jQuery === 'undefined') {
    throw new Error('Grid\'s JavaScript requires jQuery')
}

+function ($) {
    var version = $.fn.jquery.split(' ')[0].split('.')
    if ((version[0] < 2 && version[1] < 9) || (version[0] == 1 && version[1] == 9 && version[2] < 1)) {
        throw new Error('Grid\'s JavaScript requires jQuery version 1.9.1 or higher')
    }
}(jQuery);

+function ($) {
    'use strict';

    var RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();

    /*
     * Construct class with option
     */
    var Grid = function (element, options) {
        //main
        this.rowLen = 0;
        this.totalRow = 0;
        this.serverAction = false;
        this.gridName = element.id;
        this.pageSizeOption = options.pageSizeOption;
        this.fields = options.fields;
        this.nullImage = options.nullImage;
        this.serverAction = options.serverAction;
        this.transport = options.transport;

        if (!this.transport["type"])
            this.transport["type"] = "API";

        if (!this.transport["create"]["method"])
            this.transport["create"]["method"] = "POST";

        if (!this.transport["read"]["method"])
            this.transport["read"]["method"] = "GET";

        if (!this.transport["update"]["method"])
            this.transport["update"]["method"] = "PUT";

        if (!this.transport["delete"]["method"])
            this.transport["delete"]["method"] = "DELETE";

        //reff
        this.isReff = options.IsReff;
        this.caller = options.caller;
        this.callerInput = options.callerInput;

        this.formValidator;
        this.dataGrid = null;
        this.keyFields;

        this.init(element);
    };

    Grid.VERSION = '1.0.0'

    Grid.prototype = {
        constructor: Grid,

        init: function (element) {
            $(element).append(this.templateTable());

            this.pageGrid = 1;
            this.headerGrid();
            this.pageSizeGrid();

            $('#' + this.gridName + 'Table_PageSize').selectpicker({
                style: 'btn-default',
                width: '60px'
            });

            this.getData();

            if (!this.isReff) {
                var parent = $(element).parent();
                $(parent).append(this.templateAddEdit());

                this.bindAddEditComponent();

                this.formValidator = $('form.form-horizontal').validate();

                $('#' + this.gridName + 'Table_Add').click(this, this.Table_Add_Click);
                $('#Save' + this.gridName + '_Click').click(this, this.Save_Click);
            }

            $('#' + this.gridName + 'Table_Search').keyup(this, this.Table_Search_Change);
            $('#' + this.gridName + 'Table_PageSize').change(this, this.Table_PageSize_Change);

            $('#' + this.gridName + 'gridRefresh').click(this, this.refreshData);

            this.initReff();
        },

        templateTable: function () {
            var strHtml = '<div class="bsGridCompCSS container-fluid row clearfix" tabindex="999" id="' + this.gridName + 'Table">\
                    <div class="navbar navbar-default">\
                        <div class="navbar-header">\
                            <button class="navbar-toggle" data-target=".navbar-inverse-collapse" data-toggle="collapse" type="button">\
                                <span class="icon-bar"></span>\
                                <span class="icon-bar"></span>\
                                <span class="icon-bar"></span>\
                            </button>\
                        </div>\
                        <div class="navbar-collapse navbar-inverse-collapse collapse">\
                            <ul class="nav navbar-nav">\
                                <li><a href="#" id="' + this.gridName + 'Table_Add">Add</a></li>\
                            </ul>\
                            <form class="navbar-form navbar-right">\
                                <input class="form-control col-lg-8" placeholder="Search" type="text" id="' + this.gridName + 'Table_Search" />\
                            </form>\
                        </div>\
                    </div>\
                    <table class="table table-hover">\
                        <thead id="' + this.gridName + 'Table_Header"></thead>\
                        <tbody id="' + this.gridName + 'Table_Content"></tbody>\
                    </table>\
                    <div class="navbar navbar-default">\
                        <div class="navbar-header">\
                            <button class="navbar-toggle" data-target=".navbar-inverse-collapse" data-toggle="collapse" type="button">\
                                <span class="icon-bar"></span>\
                                <span class="icon-bar"></span>\
                                <span class="icon-bar"></span>\
                            </button>\
                        </div>\
                        <div class="navbar-collapse navbar-inverse-collapse collapse">\
                            <ul class="nav navbar-nav navbar-form">\
                                <li>\
                                    <select id="' + this.gridName + 'Table_PageSize"></select>\
                                </li>\
                            </ul>\
                            <ul class="nav navbar-nav navbar-right">\
                                <li>\
                                    <a href="#" id="' + this.gridName + 'gridRefresh" >\
                                        <span class="glyphicon glyphicon-refresh" aria-hidden="true"></span>\
                                    </a>\
                                </li>\
                            </ul>\
                            <ul class="nav navbar-nav navbar-right" id="' + this.gridName + 'gridPageBar"></ul>\
                        </div>\
                    </div>\
                </div>';
            if (this.isReff) {
                return ' \
                    <div class="modal fade" id="' + this.gridName + 'Dialog"> \
                        <div class="modal-dialog"> \
                            <div class="modal-content"> \
                                <div class="modal-header"> \
                                    <button type="button" class="close" data-dismiss="modal" data-toggle="modal" data-target="#' + this.caller + 'AddEditModal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button> \
                                    <h4 class="modal-title">Refference ' + this.gridName + '</h4> \
                                </div> \
                                <div class="modal-body"> ' + strHtml + ' </div> \
                                <div class="modal-footer"> \
                                    <a class="btn btn-default" data-dismiss="modal" data-toggle="modal" data-target="#' + this.caller + 'AddEditModal">Close</a> \
                                </div> \
                            </div> \
                        </div> \
                    </div> \
                ';
            }

            return strHtml;
        },

        templateAddEdit: function () {
            this.keyFields = this.getKey();
            var strHtml = '';
            for (var i = 0; i < this.keyFields.length; i++) {
                strHtml += '<input type="hidden" class="form-control" id="input' + this.keyFields[i]['name'] + '">';
            }


            return ' \
                    <div class="bsGridDialogCompCSS modal fade" id="' + this.gridName + 'AddEditModal"> \
                        <div class="modal-dialog"> \
                            <div class="modal-content"> \
                                <div class="modal-header"> \
                                    <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button> \
                                    <h4 class="modal-title">Modal title</h4> \
                                </div> \
                                <div class="modal-body"> \
                                    <form class="form-horizontal"> \
                                        ' + strHtml + ' \
                                        <fieldset> ' + this.componentField() + ' </fieldset> \
                                    </form> \
                                </div> \
                                <div class="modal-footer"> \
                                    <a class="btn btn-default" data-dismiss="modal">Close</a> \
                                    <button type="button" class="btn btn-primary" id="Save' + this.gridName + '_Click">Save changes</button> \
                                </div> \
                            </div> \
                        </div> \
                    </div> \
                ';
        },

        componentField: function () {
            var strHtml = '';

            var fields = this.fields;
            for (var j = 0; j < fields.length; j++) {
                if (!fields[j]['key']) {
                    if (fields[j]['datatype'] == 'date') {
                        strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"> \
                                        <input type="datetime" class="form-control" id="input' + fields[j]['name'] + '" data-date-format="' + fields[j]['format'] + '" placeholder="' + fields[j]['title'] + '"> \
                                    </div> \
                                </div>';

                        $('#input' + fields[j]['name']).datetimepicker();
                    }
                    else if (fields[j]['datatype'] == 'image') {
                        strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"> \
                                        <img id="input' + fields[j]['name'] + '" src="" width="50" height="50" data-dismiss="modal" data-toggle="modal" data-target=".' + fields[j]['reff'] + 'Picker" /> \
                                    </div> \
                                </div>';
                    }
                    else if (fields[j]['datatype'] == 'enum') {
                        strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"> \
                                    <select id="input' + fields[j]['name'] + '">';
                        var dataEnum = fields[j]['enumdata'];
                        for (var i in dataEnum) {
                            strHtml += '<option value="' + i + '">' + dataEnum[i] + '</option>';
                        }
                        strHtml += '</select></div></div>';
                    }
                    else if (fields[j]['datatype'] == 'int') {
                        if (fields[j]['reff'] != null) {
                            strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"><div class="input-group"> \
                                        <input type="text" class="form-control" id="input' + fields[j]['name'] + '" placeholder="' + fields[j]['title'] + '"> \
                                        <span class="input-group-btn"><button class="btn btn-default" type="button" data-dismiss="modal" data-toggle="modal" data-target="#' + fields[j]['reff'] + 'Dialog" id="' + fields[j]['reff'] + '_button">...</button></span> \
                                    </div></div> \
                                </div>';
                        }
                        else {
                            strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"> \
                                        <input type="text" class="form-control" id="input' + fields[j]['name'] + '" placeholder="' + fields[j]['title'] + '"> \
                                    </div> \
                                </div>';
                        }
                    }
                    else {
                        if (fields[j]['reff'] != null) {
                            strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"><div class="input-group"> \
                                        <input type="text" class="form-control" id="input' + fields[j]['name'] + '" placeholder="' + fields[j]['title'] + '"> \
                                        <span class="input-group-btn"><button class="btn btn-default" type="button" data-dismiss="modal" data-toggle="modal" data-target="#' + fields[j]['reff'] + 'Dialog" id="' + fields[j]['reff'] + '_button">...</button></span> \
                                    </div></div> \
                                </div>';
                        }
                        else {
                            strHtml += '<div class="form-group"> \
                                    <label for="input' + fields[j]['name'] + '" class="col-lg-2 control-label">' + fields[j]['title'] + '</label> \
                                    <div class="col-lg-6"> \
                                        <input type="text" class="form-control" id="input' + fields[j]['name'] + '" placeholder="' + fields[j]['title'] + '"> \
                                    </div> \
                                </div>';
                        }
                    }
                }
            }

            return strHtml;
        },

        bindAddEditComponent: function () {
            var fields = this.fields;
            for (var j = 0; j < fields.length; j++) {
                if (fields[j]['display'] != false) {
                    if (fields[j]['datatype'] == 'date') {
                        $('#input' + fields[j]['name']).datetimepicker();
                    }
                    else if (fields[j]['datatype'] == 'image') {
                    }
                    else if (fields[j]['datatype'] == 'enum') {
                        $('#input' + fields[j]['name']).selectpicker({
                            style: 'btn-default',
                            width: '160px'
                        });
                    }
                    else if (fields[j]['datatype'] == 'int') {
                    }
                    else {
                    }
                }
            }
        },

        refreshData: function(arg){
            var that = arg.data;
            that.getData();
        },

        getData: function () {
            var that = this;
            var postData;

            if (this.serverAction) {
                postData = {
                    "__RequestVerificationToken": RequestVerificationToken,
                    iPage: this.pageGrid,
                    iLength: this.rowLen,
                    strSearch: ""
                };
            }
            else {
                postData = { "__RequestVerificationToken": RequestVerificationToken };
            }

            waitingDialog.show('Please wait', { dialogSize: 'sm', progressType: 'warning' });

            $.ajax({
                method: this.transport.read.method,
                url: this.transport.read.url,
                data: postData
            }).success(function (result) {
                if (that.transport.type == "API") {
                    if (result.total > 0) {
                        that.dataGrid = result.payload;
                        that.totalRow = result.total;
                        that.refreshGrid();
                        waitingDialog.hide();
                    }
                    else if (result.errors != null && result.errors.length > 0) {
                        alert(result.errors);
                    }
                    else if (result.total == 0) {
                        waitingDialog.hide();
                        $("#" + that.gridName + "Table_Content").empty();
                        var htmlContent = '<tr>';
                        htmlContent += '<td colspan="' + fields.length + '">No data exists</td>';
                        htmlContent += '</tr>';
                        $("#" + that.gridName + "Table_Content").append(htmlContent);
                    }
                    else {
                        waitingDialog.hide();
                        alert("Generic error");
                    }
                }
                else {
                    if (result.value.length > 0) {
                        that.dataGrid = result.value;
                        that.totalRow = result.value.length;
                        that.refreshGrid();
                        waitingDialog.hide();
                    }
                    else if (result.value.length == 0) {
                        waitingDialog.hide();
                        $("#" + that.gridName + "Table_Content").empty();
                        var htmlContent = '<tr>';
                        htmlContent += '<td colspan="' + fields.length + '">No data exists</td>';
                        htmlContent += '</tr>';
                        $("#" + that.gridName + "Table_Content").append(htmlContent);
                    }
                    else {
                        waitingDialog.hide();
                        alert("Generic error");
                    }
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                waitingDialog.hide();
                alert("Got some error: " + errorThrown);
            });
        },

        refreshGrid: function () {
            var dataToDisplay;
            var datalen = (this.pageGrid * this.rowLen);
            var i = (this.pageGrid * this.rowLen) - this.rowLen;

            if (this.serverAction)
                var i = 0;

            if (!this.serverAction) {

                var strFilter = $('#' + this.gridName + 'Table_Search').val();

                if (strFilter.length > 0)
                    dataToDisplay = this.filterData(strFilter);
                else
                    dataToDisplay = this.dataGrid;
            }
            else
                dataToDisplay = this.dataGrid;

            if (datalen > dataToDisplay.length)
                datalen = dataToDisplay.length;

            if (i > datalen) {
                i = 0;
                datalen = this.rowLen;

                if (datalen > dataToDisplay.length)
                    datalen = dataToDisplay.length;
            }

            $("#" + this.gridName + "Table_Content").empty();

            var fields = this.fields;
            for (; i < datalen; i++) {
                var htmlContent = '<tr>';

                var j = 0;
                var keyID = '';
                for (; j < fields.length; j++) {
                    if (fields[j]['display'] != false) {
                        if (fields[j]['datatype'] == 'date') {
                            htmlContent += '<td>' + moment(dataToDisplay[i][fields[j]['name']]).format(fields[j]['format']) + '</td>';
                        }
                        else if (fields[j]['datatype'] == 'image') {
                            if (dataToDisplay[i][fields[j]['name']] == null)
                                htmlContent += '<td><img src="' + this.nullImage + '" width="30px" height="30px"></td>';
                            else
                                htmlContent += '<td><img src="' + dataToDisplay[i][fields[j]['name']] + '" width="30px" height="30px"></td>';
                        }
                        else if (fields[j]['datatype'] == 'enum') {
                            htmlContent += '<td>' + fields[j]['enumdata'][dataToDisplay[i][fields[j]['name']]] + '</td>';
                        }
                        else if (fields[j]['datatype'] == 'int') {
                            if (fields[j]['reff'] != null)
                                htmlContent += '<td>' + this.reffName(dataToDisplay[i], fields[j]['reffName']) + '</td>';
                            else
                                htmlContent += '<td>' + dataToDisplay[i][fields[j]['name']] + '</td>';
                        }
                        else {
                            if (fields[j]['reff'] != null)
                                htmlContent += '<td>' + this.reffName(dataToDisplay[i], fields[j]['reffName']) + '</td>';
                            else
                                htmlContent += '<td>' + dataToDisplay[i][fields[j]['name']] + '</td>';
                        }
                    }

                    if (fields[j]['key'] == true) {
                        keyID += dataToDisplay[i][fields[j]['name']];
                    }
                }

                if (this.isReff)
                    htmlContent += '<td><button type="button" class="btn btn-default btn-xs ' + this.gridName + 'Table_Choose_Click" data-id="' + keyID + '" data-name="' + dataToDisplay[i][fields[1]['name']] + '">Choose</button></td>';
                else
                    htmlContent += '<td><button type="button" class="btn btn-default btn-xs ' + this.gridName + 'Table_Edit_Click" data-id="' + keyID + '">Edit</button>&nbsp;<button type="button" class="btn btn-default btn-xs ' + this.gridName + 'Table_Delete_Click" data-id="' + keyID + '">Delete</button></td>';

                htmlContent += '</tr>';
                $('#' + this.gridName + 'Table_Content').append(htmlContent);
            }

            //wired event
            if (this.isReff) {
                $('.' + this.gridName + 'Table_Choose_Click').click(this, this.gridPageBar_Choose);
            }
            else {
                $('.' + this.gridName + 'Table_Edit_Click').click(this, this.gridPageBar_Edit);
                $('.' + this.gridName + 'Table_Delete_Click').click(this, this.gridPageBar_Delete);
            }

            this.gridPageBar_init(dataToDisplay);
        },

        pageSizeGrid: function () {
            $('#' + this.gridName + 'Table_PageSize').empty();
            for (var i = 0; i < this.pageSizeOption.length; i++) {
                if (i == 0)
                    this.rowLen = this.pageSizeOption[i];
                $('#' + this.gridName + 'Table_PageSize').append('<option>' + this.pageSizeOption[i] + '</option>');
            }
        },

        headerGrid: function () {
            $('#' + this.gridName + 'Table_Header').empty();
            var strHtml = '<tr>';
            for (var i = 0; i < this.fields.length; i++) {
                if (this.fields[i]['display'] != false)
                    strHtml += '<th>' + this.fields[i]['title'] + '</th>';
            }
            strHtml += '<th></th>';
            strHtml += '</tr>';
            $('#' + this.gridName + 'Table_Header').append(strHtml);
        },

        Table_Search_Change: function (arg) {
            var that = arg.data;
            var strFilter = $('#' + that.gridName + 'Table_Search').val();
            if (strFilter.length >= 3) {
                that.pageGrid = 1;
                that.refreshGrid();
            }
            else if (strFilter.length == 0) {
                that.pageGrid = 1;
                that.refreshGrid();
            }
        },

        Table_PageSize_Change: function (arg) {
            var that = arg.data;
            if (typeof that.dataGrid != "undefined") {
                that.rowLen = $('#' + that.gridName + 'Table_PageSize').val();
                that.pageGrid = 1;
                if (that.serverAction)
                    that.getData();
                else
                    that.refreshGrid();
            }
        },

        Table_Add_Click: function (arg) {
            var that = arg.data;
            that.formValidator.resetForm();
            $('form.form-horizontal', '#' + that.gridName + 'AddEditModal')[0].reset();
            $('#' + that.gridName + 'AddEditModal .modal-title').html(that.gridName + ' add');

            for (var i = 0; i < that.keyFields.length; i++) {
                if (that.keyFields[i]['datatype'] == 'int')
                    $('#input' + that.keyFields[i]['name']).val(0);
                else
                    $('#input' + that.keyFields[i]['name']).val('');
            }

            $('#' + that.gridName + 'AddEditModal').modal();
        },

        gridPageBar_init: function (data) {
            var totData = data.length;

            if (this.serverAction)
                totData = this.totalRow;

            var pageres = totData % this.rowLen;
            var page = (totData - pageres) / this.rowLen;
            if (pageres > 0)
                page++;

            $("#" + this.gridName + "gridPageBar").empty();
            var htmlContent = '';
            if (this.pageGrid == 1) {
                htmlContent += '<li class="disabled"><a href="#"><span class="glyphicon glyphicon-fast-backward"></span></a></li>';
                htmlContent += '<li class="disabled"><a href="#"><span class="glyphicon glyphicon-backward"></span></a></li>';
            }
            else {
                htmlContent += '<li><a href="#" id="' + this.gridName + 'FirstPage" data-page="1"><span class="glyphicon glyphicon-fast-backward"></span></a></li>';
                htmlContent += '<li><a href="#" id="' + this.gridName + 'PrevPage" data-page="' + (this.pageGrid - 1) + '"><span class="glyphicon glyphicon-backward"></span></a></li>';
            }

            htmlContent += '<li><a href="#">Page ' + this.pageGrid + ' of ' + page + '</a></li>';

            if (this.pageGrid == page) {
                htmlContent += '<li class="disabled"><a href="#"><span class="glyphicon glyphicon-forward"></span></a></li>';
                htmlContent += '<li class="disabled"><a href="#"><span class="glyphicon glyphicon-fast-forward"></span></a></li>';
            }
            else {
                htmlContent += '<li><a href="#" id="' + this.gridName + 'NextPage" data-page="' + (this.pageGrid + 1) + '"><span class="glyphicon glyphicon-forward"></span></a></li>';
                htmlContent += '<li><a href="#" id="' + this.gridName + 'LastPage" data-page="' + page + '"><span class="glyphicon glyphicon-fast-forward"></span></a></li>';
            }

            $("#" + this.gridName + "gridPageBar").append(htmlContent);

            //wired event
            $('#' + this.gridName + 'FirstPage').click(this, this.gridPageBar_Change);
            $('#' + this.gridName + 'PrevPage').click(this, this.gridPageBar_Change);
            $('#' + this.gridName + 'NextPage').click(this, this.gridPageBar_Change);
            $('#' + this.gridName + 'LastPage').click(this, this.gridPageBar_Change);
        },

        gridPageBar_Change: function (arg) {
            var that = arg.data;
            that.pageGrid = $(this).data("page");
            if (that.serverAction)
                that.getData();
            else
                that.refreshGrid();
        },

        filterData: function (arg) {
            var dataToDisplay = [];
            for (var i = 0; i < this.dataGrid.length; i++) {
                var j = 0;
                var strSearch = '';
                var fields = this.fields;
                for (; j < fields.length; j++) {
                    if (fields[j]['display'] != false) {
                        if (fields[j]['datatype'] == 'date') {
                            strSearch += moment(this.dataGrid[i][fields[j]['name']]).format(fields[j]['format']);
                        }
                        else {
                            strSearch += this.dataGrid[i][fields[j]['name']];
                        }
                    }
                }

                if (strSearch.toLowerCase().indexOf(arg.toLowerCase()) >= 0)
                    dataToDisplay.push(this.dataGrid[i]);
            }

            return dataToDisplay;
        },

        gridPageBar_Choose: function (arg) {
            var that = arg.data;
            var Id = $(this).data("id");
            var Name = $(this).data("name");

            $('#' + that.gridName + 'Dialog').modal('hide');

            $('#input' + that.callerInput).val(Name);
            $('#input' + that.callerInput).data("id", Id);

            $('#' + that.caller + 'AddEditModal').modal();
        },

        gridPageBar_Edit: function (arg) {
            var that = arg.data;
            var Id = $(this).data("id");
            that.formValidator.resetForm();
            $('form.form-horizontal', '#' + that.gridName + 'AddEditModal')[0].reset();
            $('#' + that.gridName + 'AddEditModal .modal-title').html(that.gridName + ' edit ' + Id);

            var data;

            for (var i = 0; i < that.keyFields.length; i++) {
                data = that.getObjects(that.dataGrid, that.keyFields[i]['name'], Id);
                $('#Input' + that.keyFields[i]['name']).val(Id);
            }

            if (data.length > 0) {
                that.fillAddEdit(data);
                $('#' + that.gridName + 'AddEditModal').modal();
            }
            else {
                alert("Error retrieving data");
            }
        },

        fillAddEdit: function (data) {
            var fields = this.fields;
            for (var j = 0; j < fields.length; j++) {
                if (fields[j]['datatype'] == 'date') {
                    $('#input' + fields[j]['name']).val(moment(data[0][fields[j]['name']]).format(fields[j]['format']));
                }
                else if (fields[j]['datatype'] == 'image') {
                    if (data[0].SourceIcon == null)
                        $('#input' + fields[j]['name']).attr("src", this.nullImage);
                    else
                        $('#input' + fields[j]['name']).attr("src", data[0][fields[j]['name']]);
                }
                else if (fields[j]['datatype'] == 'enum') {
                    $('#input' + fields[j]['name']).selectpicker('val', data[0][fields[j]['name']]);
                }
                else if (fields[j]['datatype'] == 'int') {
                    if (fields[j]['reff'] != null) {
                        $('#input' + fields[j]['name']).val(this.reffName(data[0], fields[j]['reffName']));
                        $('#input' + fields[j]['name']).data("id", data[0][fields[j]['name']]);
                    }
                    else
                        $('#input' + fields[j]['name']).val(data[0][fields[j]['name']]);
                }
                else {
                    if (fields[j]['reff'] != null) {
                        $('#input' + fields[j]['name']).val(this.reffName(data[0], fields[j]['reffName']));
                        $('#input' + fields[j]['name']).data("id", data[0][fields[j]['name']]);
                    }
                    else
                        $('#input' + fields[j]['name']).val(data[0][fields[j]['name']]);
                }
            }
        },

        gridPageBar_Delete: function (arg) {
            var that = arg.data;
            if (confirm("Are you sure you want to delete this record?")) {
                waitingDialog.show('Please wait', { dialogSize: 'sm', progressType: 'warning' });

                var urlString = that.transport.delete.url;
                if (that.transport.delete.method != "POST") {
                    if (that.transport.type == "API")
                        urlString += "/" + $(this).data("id");
                    else
                        urlString += "(" + $(this).data("id") + ")";
                }

                $.ajax({
                    method: that.transport.delete.method,
                    url: urlString,
                    data: { "__RequestVerificationToken": RequestVerificationToken, id: $(this).data("id") }
                }).success(function (result) {
                    if (that.transport.type == "API") {
                        if (result.total >= 0) {
                            waitingDialog.hide();
                            alert("Record deleted");
                            that.getData();
                        }
                        else if (result.errors.length > 0) {
                            waitingDialog.hide();
                            alert(result.errors);
                        }
                        else {
                            waitingDialog.hide();
                            alert("Generic error");
                        }
                    }
                    else {
                        waitingDialog.hide();
                        alert("Record deleted");
                        that.getData();
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    waitingDialog.hide();
                    alert("Got some error: " + errorThrown);
                });
            }
        },

        initReff: function () {
            var fields = this.fields;
            for (var i = 0; i < fields.length; i++) {
                if (fields[i]['reff'] != undefined) {
                    window[fields[i]['reff']]();
                }
            }
        },

        reffName: function (obj, reffName) {
            var val = obj;
            var strReff = reffName.split(".")
            for (var i = 0; i < strReff.length; i++) {
                val = val[strReff[i]];
            }

            return val;
        },

        Save_Click: function (arg) {
            var that = arg.data;
            var fields = that.fields;

            if (that.formValidator.valid()) {
                var data = {};

                data["__RequestVerificationToken"] = RequestVerificationToken;

                for (var j = 0; j < fields.length; j++) {
                    if (fields[j]['datatype'] == 'date') {
                        data[fields[j]['name']] = $('#input' + fields[j]['name']).val();
                    }
                    else if (fields[j]['datatype'] == 'image') {
                        data[fields[j]['name']] = $('#input' + fields[j]['name']).attr("src");
                    }
                    else if (fields[j]['datatype'] == 'enum') {
                        data[fields[j]['name']] = $('#input' + fields[j]['name']).selectpicker('val');
                    }
                    else if (fields[j]['datatype'] == 'int') {
                        if (fields[j]['reff'] != null)
                            data[fields[j]['name']] = $('#input' + fields[j]['name']).data("id");
                        else
                            data[fields[j]['name']] = $('#input' + fields[j]['name']).val();
                    }
                    else {
                        if (fields[j]['reff'] != null)
                            data[fields[j]['name']] = $('#input' + fields[j]['name']).data("id");
                        else
                            data[fields[j]['name']] = $('#input' + fields[j]['name']).val();
                    }
                }

                waitingDialog.show('Please wait', { dialogSize: 'sm', progressType: 'warning' });

                var key = "";

                for (var i = 0; i < that.keyFields.length; i++) {
                    key = data[that.keyFields[i]['name']];
                }

                data["id"] = key;

                if (key == 0) {
                    $.ajax({
                        method: that.transport.create.method,
                        url: that.transport.create.url,
                        data: data
                    }).success(function (result) {
                        if (that.transport.type == "API") {
                            if (result.total >= 0) {
                                $('#' + that.gridName + 'AddEditModal').modal('hide');
                                that.getData();
                                waitingDialog.hide();
                            }
                            else if (result.errors.length > 0) {
                                waitingDialog.hide();
                                alert(result.errors);
                            }
                            else {
                                waitingDialog.hide();
                                alert("Generic error");
                            }
                        }
                        else {
                            $('#' + that.gridName + 'AddEditModal').modal('hide');
                            that.getData();
                            waitingDialog.hide();
                        }
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        waitingDialog.hide();
                        alert("Got some error: " + errorThrown);
                    });
                }
                else {
                    var urlString = that.transport.update.url;
                    if (that.transport.update.method != "POST") {
                        if(that.transport.type == "API")
                            urlString += "/" + key;
                        else
                            urlString += "(" + key + ")";
                    }

                    $.ajax({
                        method: that.transport.update.method,
                        url: urlString,
                        data: data
                    }).success(function (result) {
                        if (that.transport.type == "API") {
                            if (result.total >= 0) {
                                $('#' + that.gridName + 'AddEditModal').modal('hide');
                                that.getData();
                                waitingDialog.hide();
                            }
                            else if (result.errors.length > 0) {
                                waitingDialog.hide();
                                alert(result.errors);
                            }
                            else {
                                waitingDialog.hide();
                                alert("Generic error");
                            }
                        }
                        else {
                            $('#' + that.gridName + 'AddEditModal').modal('hide');
                            that.getData();
                            waitingDialog.hide();
                        }
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        waitingDialog.hide();
                        alert("Got some error: " + errorThrown);
                    });
                }
            }
        },

        getObjects: function (obj, key, val) {
            var objects = [];
            for (var i in obj) {
                if (!obj.hasOwnProperty(i)) continue;
                if (typeof obj[i] == 'object') {
                    objects = objects.concat(this.getObjects(obj[i], key, val));
                } else if (i == key && obj[key] == val) {
                    objects.push(obj);
                }
            }
            return objects;
        },

        getKey: function () {
            var fields = this.fields;
            var objects = [];
            for (var j = 0; j < fields.length; j++) {
                if (fields[j]['key']) {
                    objects.push(fields[j]);
                }
            }

            return objects;
        }
    }

    // GRID PLUGIN DEFINITION
    // ==========================
    function Plugin(option) {
        return this.each(function () {
            var data = new Grid(this, option);
        })
    }

    var old = $.fn.grid;
    $.fn.grid = Plugin;

    // GRID NO CONFLICT
    // ====================
    $.fn.grid.noConflict = function () {
        $.fn.grid = old
        return this
    }

}(jQuery)