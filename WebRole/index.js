$(document).ready(function () {
    $("#structureData").hide();
    $("#input").on("keyup", function () {
        Search();
        QuerySuggestion();
        QueryAws();
    });

    $("#search").click(function () {
        InsertToTrie();
    });
});

function QueryAws() {
    var input = $("#input").val().toLowerCase().trim();
    $.ajax({
        crossDomain: true,
        contentType: "application/json; charset=utf-8",
        url: "http://ec2-54-186-27-42.us-west-2.compute.amazonaws.com/pa1/show-json.php",
        data: { playerName: input },
        dataType: "jsonp",
        success: function (data) {
            if (input != "" && input != " ")
            {
                var name = "";
                var stats = "";
                for (var key in data) {
                    name = data[key].name;
                    stats += "GP: " + data[key].gp + "<br />" +
                             "FGP: " + data[key].fgp + "<br />" +
                             "TPP: " + data[key].tpp + "<br />" +
                             "FTP: " + data[key].ftp + "<br />" +
                             "PPG: " + data[key].ppg + "<br />";
                }
                if (name != "") {
                    $("#structureDataPlayerName").html(name);
                    $("#structureDataPlayerStats").html(stats);
                    $("#structureData").show();
                }
            }
            else
            {
                $("#structureData").hide();
                $("#structureDataPlayerName").html("");
            }
        }
    });
}

function QuerySuggestion() {
    var input = $("#input").val().toLowerCase().trim();
    $.ajax({
        type: "POST",
        url: "admin.asmx/Suggest",
        data: JSON.stringify({ input: input }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var result = "<select multiple class=\"form-control\" style=\"z-index: 9999; position: absolute; width: 92.7%; height: 190px\">";
            if (input != "" && input != " " && input.charAt(0) == data.d[0].charAt(0)) {
                for (var i = 0; i < data.d.length; i++) {
                    result += "<option onclick=\"$('#input').val(this.value);$('#suggestion').html('');Search();QueryAws();\">" + data.d[i] + "</option>";
                }
                $("#suggestion").html(result + "</select>");
            }
            else {
                $("#suggestion").html("");
            }
        }
    });
}

function InsertToTrie() {
    var input = $("#input").val().toLowerCase().trim();
    $.ajax({
        type: "POST",
        url: "admin.asmx/InsertToTrie",
        data: JSON.stringify({ word: input }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
    });
}

function Search() {
    var input = $("#input").val().toLowerCase().trim().split(" ");
    if (input != "" && input != " ") {
        var uniqueResults = [];
        for (var j = 0; j < input.length; j++) {
            $.ajax({
                type: "POST",
                url: "admin.asmx/Search",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ input: input[j] }),
                dataType: "json",
                success: function (data) {
                    var result = "";
                    for (var i = 0; i < data.d.length; i++) {
                        if (!(data.d[i] + "<br />" in uniqueResults)) {
                            uniqueResults.push(data.d[i] + "<br />");
                            result += data.d[i] + "<br />";
                        }
                    }
                    $("#searchResult").html(result);
                }
            });
        }
    }
    else {
        $("#searchResult").html("");
    }
}