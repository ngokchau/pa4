$(document).ready(function () {
    loop();

    $("#buildTrie").click(function () {
        BuildTrie();
    });
    $("#startCrawler").click(function () {
        AddCommand("start");
    });
    $("#stopCrawler").click(function () {
        AddCommand("stop");
    });
    $("#clearUrlQueue").click(function () {
        ClearUrlQueue();
    });
    $("#addError").click(function () {
        AddError();
    });
    $("#clearErrors").click(function () {
        ClearErrors();
    });
});

function loop() {
    GetPerformance("Memory", "Available MBytes", "");
    GetPerformance("Processor", "% Processor Time", "_Total");
    GetNoWordsInTrie();
    GetStateOfWorker();
    ToggleBuildTrie();
    GetUrlQueueSize();
    GetErrors();
    setTimeout('loop()', 1000);
}

function GetPerformance(category, counter, instance) {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetPerformance",
        data: JSON.stringify({ categoryName: category, counterName: counter, instanceName: instance }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            (category == "Processor") ? $("#cpu").html(JSON.parse(data.d)) : $("#ram").html(JSON.parse(data.d));
        }
    });
}

function BuildTrie() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/BuildTrie",
        contentType: "application/json; charset=utf-8"
    });
}

function ToggleBuildTrie() {
    ($("#ram").html() < 200) ? $("#buildTrie").addClass("disabled") : $("#buildTrie").removeClass("disabled");
}

function GetNoWordsInTrie() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetNoWordsInTrie",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#noWordsInTrie").html(JSON.parse(data.d));
        }
    });
}

function GetStateOfWorker() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetStateOfWorker",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            (JSON.parse(data.d) == "start") ? $("#crawlerState").html("crawling") : $("#crawlerState").html("idle");
        }
    });
}

function AddCommand(cmd) {
    $.ajax({
        type: "POST",
        url: "admin.asmx/AddCommand",
        data: JSON.stringify({ cmd: cmd }),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    })
}

function GetUrlQueueSize() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetUrlQueueSize",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#urlQueueSize").html(JSON.parse(data.d));
        }
    });
}

function ClearUrlQueue() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/ClearUrlQueue",
        contentType: "application/json; charset=utf-8"
    });
}

function GetErrors() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetErrors",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var result = "";
            for (var i = 0; i < data.d.length; i++) {
                result += data.d[i] + "<br />";
            }
            $("#errors").html(result);
        }
    });
}

function AddError() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/AddError",
        contentType: "application/json; charset=utf-8"
    });
}

function ClearErrors() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/ClearErrors",
        contentType: "application/json; charset=utf-8"
    });
}