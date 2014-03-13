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
    $("#loadUrlQueue").click(function () {
        AddCommand("load");
    })
    $("#clearUrlQueue").click(function () {
        ClearQueue("url");
    });
    $("#addError").click(function () {
        AddError();
    });
    $("#clearErrors").click(function () {
        ClearQueue("error");
    });
    $("#clearUrlsCrawled").click(function () {
        ClearQueue("urlsCrawled")
    });
    $("#resetNumberofUrlCrawled").click(function () {
        ResetNumberUrlCrawled();
    });
    $("#resetIndex").click(function () {
        ResetIndex();
    });
});

function loop() {
    GetPerformance("Memory", "Available MBytes", "");
    GetPerformance("Processor", "% Processor Time", "_Total");
    GetStateOfWorker();
    GetTrieStat("number of lines");
    GetTrieStat("last line");
    GetNumbUrlCrawled();
    ToggleBuildTrie();
    GetUrlQueueSize();
    GetLastTen("errors");
    GetLastTen("urls");
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
            if (category == "Processor")
            {
                $("#cpu").html("CPU: " + JSON.parse(data.d));
            }
            else
            {
                $("#ram").html("RAM: " + JSON.parse(data.d));
            }
        }
    });
}

function BuildTrie() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/BuildTrie",
        contentType: "application/json; charset=utf-8",
        success: function () {
            GetTrieStat("number of lines");
            GetTrieStat("last line");
        }
    });
}

function ToggleBuildTrie() {
    ($("#ram").html() < 200) ? $("#buildTrie").addClass("disabled") : $("#buildTrie").removeClass("disabled");
}

function GetTrieStat(stat) {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetTrieStat",
        data: JSON.stringify({ type: stat }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            (stat == "last line") ? $("#lastLineInsertToTrie").html("Last Entry: " + JSON.parse(data.d)) : $("#numberOfLinesInsertToTrie").html("# Entries: " +JSON.parse(data.d));
        }
    });
}

function GetNumbUrlCrawled() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetNumbUrlCrawled",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#numberOfUrlCrawled").html("# Url Crawled: " + JSON.parse(data.d));
        }
    });
}

function GetStateOfWorker() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetStateOfWorker",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (JSON.parse(data.d) == "start") {
                $("#crawlerState").html("crawling")
            } else if (JSON.parse(data.d) == "stop") {
                $("#crawlerState").html("idle");
            } else {
                $("#crawlerState").html("loading");
            }
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

function ResetNumberUrlCrawled() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/ResetNumbUrlCrawled",
        contentType: "application/json; charset=utf-8",
    });
}

function ResetIndex() {
    $.ajax({
        type: "POST",
        url: "admin.asmx/ResetIndex",
        contentType: "application/json; charset=utf-8",
    });
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

function GetLastTen(type) {
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetLastTen",
        data: JSON.stringify({ type: type }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var result = "";
            for (var i = 0; i < data.d.length; i++) {
                result += data.d[i] + "<br />";
            }
            (type == "errors") ? $("#errors").html(result) : $("#urlsCrawled").html(result) ;
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

function ClearQueue(queue) {
    $.ajax({
        type: "POST",
        url: "admin.asmx/ClearQueue",
        data: JSON.stringify({ queue: queue }),
        contentType: "application/json; charset=utf-8"
    });
}