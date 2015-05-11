var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    var sysstatus = "Ready";

    mb.subscribe("api.symphony.com", function (envelope) {
        window.postMessage(envelope.message, "*");
    });

    mb.subscribe("system.symphony.com", function (envelope) {
        if (envelope.replyAddress != null && envelope.message.action == "Query_System_Status") {
            mb.send(envelope.replyAddress, {type: "System_Status", status: sysstatus});
        }
    });
   
    mb.onDisconnected.addListener(function () {
        sysstatus = "offline";
        mb.publish("system.events.symphony.com", { type: "System_Status", status: sysstatus });
    });

    mb.publish("system.events.symphony.com", { type: "System_Status", status: sysstatus});

    symphony.interop.outboundChannel = {
        post: function (message) {
            if (message != null && message.responseAddress != null)
            {
                mb.send(message.responseAddress, message);
            }
        }
    }
}};
