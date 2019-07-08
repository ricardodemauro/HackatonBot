"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/carHub").build();
var cardHtml = "<div id=\"card_number$CAR_ID\" class=\"col-sm-3 mt-2 mr-3\" style=\"display:none;\">" +
    "<div class=\"card\" style = \"width: 18rem;\" >" +
    "<img class=\"card-img-top\" src=\"$CAR_IMAGE\" alt=\"Card image cap\">" +
    "<div class=\"card-body\">" +
    "<h5 class=\"card-title\">$CAR_NAME</h5>" +
    "<h6 class=\"card-subtitle text-muted\">$CAR_BRAND</h6>" +
    "<p class=\"card-text\">Complete car with all the accessories available for the model.</p>" +
    "<a href=\"#\" class=\"btn btn-primary\">Let's drive!</a>" + 
    "</div>" +
    "</div>" +
    "</div>";

connection.on("AddNewVehicle", function (vehicle) {
    console.log(vehicle);

    let replacedCard = cardHtml.replace("$CAR_ID", vehicle.id);
    replacedCard = replacedCard.replace("$CAR_NAME", vehicle.name);
    replacedCard = replacedCard.replace("$CAR_BRAND", vehicle.brand);

    if (vehicle.Base64Images !== undefined) {
        console.log(vehicle.Base64Images[0]);
        replacedCard = replacedCard.replace("$CAR_IMAGE", vehicle.Base64Images[0].base64);
    } else {
        replacedCard = replacedCard.replace("$CAR_IMAGE", "/car-placeholder-image.jpg");
    }

    console.log(replacedCard);

    let $mainList = $("#mainList");
    $mainList.prepend(replacedCard);
    $(`#card_number${vehicle.id}`).show("drop");
});

connection.on("RemoveVehicle", function (vehicleId) {
    console.log(vehicleId);

    let $mainList = $("#mainList");
    $mainList.find(`#card_number${vehicleId}`).remove("drop", "up");
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

