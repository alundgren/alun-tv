function searchForShow(partialName) {
    //todo: escape
    $.ajax({
        url: "/Show/SearchAsync?partialName=" + partialName,
        success: function (data) {
            renderSearchHits({ hits: data });
            $.mobile.changePage($("#searchHitsPage"));
        }
    });
}
function renderSearchHits(data) {
    $("#searchHits li.hit").remove();
    $("#hitsTemplate").tmpl(data).insertAfter($("#hitsDivider"));
    try {
        $("#searchHits").listview("refresh");
    } catch (e) {
        // ignored
    }
    $("#searchHits li.hit a").click(function () {
        var sourceId = $(this).find("input[type=hidden]").val();
        $.ajax({
            url: "/Show/AddAsync?sourceId=" + sourceId,
            success: function () {
                $.mobile.changePage($("#showsPage"));
                fetchAndRenderShows();
            }
        });
    });
}
function fetchAndRenderShows() {
    $.ajax({
        url: "/WatchList/IndexAsync",
        success: function (data) {
            renderShows({ shows: data.Available }, { shows: data.Future });
            $.mobile.changePage($("#showsPage"));
        }
    });
}
function renderShows(availableShows, futureShows) {
    $("#shows li.show").remove();
    //Available
    $("#showsTemplate").tmpl(availableShows).insertAfter($("#availableDivider"));
    //Future
    $("#showsTemplate").tmpl(futureShows).insertAfter($("#futureDivider"));
    try {
        $("#shows").listview("refresh");
    } catch (e) {
        // ignored
    }
    $("#shows li a").click(function () {
        $("#currentSourceId").attr("value", $(this).find("input[type=hidden]").val());
    });
}
function initSignalR() {
    var connection = $.connection('event');
    connection.received(function (data) { //TODO: Debounce these
        if(data == "watchlist") {
            //Watchlist data changed on the server. Refresh if needed
            if($.mobile.activePage[0].id == "showsPage") {
                fetchAndRenderShows();
            }
        }
        fetchAndRenderShows();
    });
    connection.start();
    return connection;
}
$(document).ready(function () {
    var connection = initSignalR();
    $("#watched-ref").click(function () {
        var sourceId = $("#currentSourceId").attr("value");
        //TODO; encode sourceId
        $.ajax({
            url: "/WatchList/WatchedAsync?sourceId=" + sourceId,
            success: function () {
                $.mobile.changePage($("#showsPage"));
                fetchAndRenderShows();
            }
        });
    });
    $("#watched-season-ref").click(function () {
        var sourceId = $("#currentSourceId").attr("value");
        //TODO; encode sourceId
        $.ajax({
            url: "/WatchList/WatchedSeasonAsync?sourceId=" + sourceId,
            success: function () {
                $.mobile.changePage($("#showsPage"));
                fetchAndRenderShows();
            }
        });
    });
    $("#search").submit(function () {
        searchForShow($("#partialName").val());
        return false;
    });



    fetchAndRenderShows();
});