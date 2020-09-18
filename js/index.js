$(document).ready(function () {
    var myMap;
    var mySFile;
    var myobjectManager;
    var S = 'width: 100%; height:' + $(window).height() + 'px';
    $('#map').attr("style", S);
    UpdateElements();
    ymaps.ready(init_map);
}); 
//ФУНКЦИИ:
function init_map() {
    myMap = new ymaps.Map("map", {
        center: [64.19, 90.47],
        zoom: 4
    });
    myMap.container.fitToViewport();
}
function Menu_open() {
    $('#Menu').html('<iframe name="Frame1" width="99%" height="98%" scrolling="none" src="FileUI.html"></iframe>');
    $(function () {
        $("#Menu").dialog({
            title: "Файлы данных для отчетов",
            resizable: false,
            height: 600,
            width: 800,
            modal: true,
            buttons: {
                Ok: function () {
                    UpdateElements();
                    myMap.geoObjects.removeAll();
                    $(this).dialog("close");
                }
            }
        });
    });
}
function GetName(fnam) {
    return fnam.substr(0, fnam.lastIndexOf('.xlsx'));
}
function UpdateElements() {
    $.getJSON('FileTransferHandler.ashx', function (data, textStatus, jqXHR) {
        $(".Elements").html('');
        for (var i = 0; i < data.length; i++) {
            $('.Elements').append(
                '<label for="radio-' + i.toString() + '" >' + GetName(data[i].name) + '</label ><input type="radio" name="radio-0" id="radio-' + i.toString() + '" onchange="GeoMap(\'' + data[i].name + '\')" />'
            );
        };
        $(".Elements").controlgroup({ "direction": "vertical" });
    });
}
function GeoMap(FName) {
    mySFile = FName;
    myMap.geoObjects.removeAll();
    myobjectManager = new ymaps.ObjectManager({ clusterize: true, geoObjectOpenBalloonOnClick: false, clusterOpenBalloonOnClick: false });
    myMap.geoObjects.add(myobjectManager);
    myobjectManager.objects.events.add(['click'], onPointClick);
    myobjectManager.clusters.events.add(['click'], onClusterEvent);
    $.getJSON('PointTransferHandler.ashx?FN=' + FName + '&Current=0', function (data, textStatus, jqXHR) {
        myobjectManager.add(data);
        var AllCount = Number(jqXHR.getResponseHeader('AllCount'));
        var Current = Number(jqXHR.getResponseHeader('Current'));
        if (Current < AllCount) {
            var timeout_id = setTimeout(
                function action() {
                    $.getJSON('PointTransferHandler.ashx?FN=' + FName + '&Current=' + Current, function (data1, textStatus1, jqXHR1) {
                        myobjectManager.add(data1);
                        AllCount = Number(jqXHR1.getResponseHeader('AllCount'));
                        Current = Number(jqXHR1.getResponseHeader('Current'));
                        if (Current < AllCount) {
                            timeout_id = setTimeout(action, 1000);
                        }
                    });
                }, 1000);
        }

    });
}
function onPointClick(e) {
    var objectId = e.get('objectId');
    var objectCoords = myobjectManager.objects.getById(objectId).geometry.coordinates;
    myMap.balloon.open(objectCoords, 'Загрузка описания...', {});
    $.ajax({
        url: "DetailInfoHandler.ashx?FN=" + mySFile + '&id=;' + objectId.toString() + ';',
        success: function (data) {
            myMap.balloon.open(objectCoords, data, {});
        }
    });
}
function onClusterEvent(e) {
    if (myMap.getZoom() == 19) {
        var ClusterId = e.get('objectId');
        var Elements = myobjectManager.clusters.getById(ClusterId).properties.geoObjects;
        var id = ';'; $.each(Elements, function (index, Element) { id = id + Element.id + ';'; });
        var objectCoords = myobjectManager.clusters.getById(ClusterId).geometry.coordinates;
        myMap.balloon.open(objectCoords, 'Загрузка описания...', {});
        $.ajax({
            url: "DetailInfoHandler.ashx?FN=" + mySFile + '&id=' + id,
            success: function (data) {
                myMap.balloon.open(objectCoords, data, {});
            }
        });
    }
}