"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/carHub").build();
var cardHtml = "<div class=\"col-sm - 3\">" +
    "<div class=\"card\" style = \"width: 18rem;\" >" + 
    "<img class=\"card-img-top\" src=\"https://t1-cms-1.images.toyota-europe.com/toyotaone/ieen/corolla_1980x1020_tcm-3044-1555806.jpg\" alt=\"Card image cap\">" +
    "<div class=\"card-body\">" +
    "<h5 class=\"card-title\">Corolla</h5>" + 
    "<h6 class=\"card-subtitle text-muted\">Toyota</h6>" + 
    "<p class=\"card-text\">Complete car with all the accessories available for the model.</p>" +
    "</div>" +
    "</div>" + 
    "</div >";

connection.on("AddNewVehicle", function (vehicle) {
    console.log(vehicle);


    //var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    //var encodedMsg = user + " says " + msg;
    //var li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

