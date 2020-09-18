$(function() {
    'use strict';
    $('#fileupload').fileupload();
    $.getJSON($('#fileupload form').prop('action'), function(files) {
        var fu = $('#fileupload').data('fileupload');
        fu._adjustMaxNumberOfFiles(-files.length);
        fu._renderDownload(files)
            .appendTo($('#fileupload .files'))
            .fadeIn(function() {
                // Fix for IE7 and lower:
                $(this).show();
            });
    });
    $('#fileupload .files a:not([target^=_blank])').live('click', function(e) {
        e.preventDefault();
        $('<iframe style="display:none;"></iframe>')
            .prop('src', this.href)
            .appendTo('body');
    });

});