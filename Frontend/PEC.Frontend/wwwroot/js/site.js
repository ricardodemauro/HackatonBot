"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/carHub").build();
var cardHtml = "<div class=\"col-sm-3 mt-2 mr-3\">" +
    "<div class=\"card\" style = \"width: 18rem;\" >" + 
    "<img class=\"card-img-top\" src=\"$CAR_IMAGE\" alt=\"Card image cap\">" +
    "<div class=\"card-body\">" +
    "<h5 class=\"card-title\">$CAR_NAME</h5>" + 
    "<h6 class=\"card-subtitle text-muted\">$CAR_BRAND</h6>" + 
    "<p class=\"card-text\">Complete car with all the accessories available for the model.</p>" +
    "</div>" +
    "</div>" + 
    "</div>";

connection.on("AddNewVehicle", function (vehicle) {
    console.log(vehicle);

    let replacedCard = cardHtml.replace("$CAR_IMAGE", vehicle.Base64Images[0]);
    replacedCard = replacedCard.replace("$CAR_NAME", vehicle.Name);
    replacedCard = replacedCard.replace("$CAR_BRAND", vehicle.Brand);

    let $mainList = $("#mainList");
    $mainList.prepend(replacedCard);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

