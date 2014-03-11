$(document).ready(function () {
    $("#input").on("keyup", function () {
        Search();
        //QuerySuggestion();
        QueryAws();
    });

    $("#search").click(function () {
        Search();
    })
});

function QueryAws() {
    var input = $("#input").val().trim();
    $.ajax({
        crossDomain: true,
        contentType: "application/json; charset=utf-8",
        url: "http://ec2-54-186-27-42.us-west-2.compute.amazonaws.com/pa1/show-json.php",
        data: { playerName: input },
        dataType: "jsonp",
        success: function (data) {
            var result = "";
            for (var key in data) {
                result += data[key].name + "<br />";
            }
            $("#structureDataResult").html(result);
        }
    });
}

function QuerySuggestion() {
    var input = $("#input").val().trim();
    $.ajax({
        type: "POST",
        url: "admin.asmx/Suggest",
        data: JSON.stringify({ input: input }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var result = "";
            if (input != "" && input != " ") {
                for (var i = 0; i < data.d.length; i++) {
                    result += data.d[i] + "<br />";
                }
                $("#suggestion").html(result);
            }
            else {
                $("#suggestion").html("");
            }
        }
    });
}

function Search() {
    var input = $("#input").val().trim();
    $.ajax({
        type: "POST",
        url: "admin.asmx/Search",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ input: input }),
        dataType: "json",
        success: function (data) {
            var result = "";
            if (input != "" && input != " ") {
                for (var key in data) {
                    result += data[key] + "<br />";
                }
                $("#searchResult").html(result);
            }
            else {
                $("#searchResult").html("");
            }
        }
    });
}