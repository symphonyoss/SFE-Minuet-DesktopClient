var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    var sysstatus = "Ready";

    mb.subscribe("api.symphony.com", function (envelope) {
        console.log("api.symphony.com/incoming envelope: ", envelope);

        window.postMessage(envelope.message, "*");

    });

    mb.subscribe("com.symphony.system", function (envelope) {
        console.log("system.symphony.com/message: ", envelope);
        console.log("system.symphony.com/envelope.replyAddress/ ", envelope.replyAddress);
        console.log("system.symphony.com/envelope.message.action/ ", envelope.message.action);

        if (envelope.replyAddress != null) {
            switch(envelope.message.action) {
                case 'Query_System_Status':
                    mb.send(envelope.replyAddress, { type: 'System_Response', action: 'Query_System_Status', status: sysstatus });
                    break;
                case 'Bring_Symphony_To_Front':
                    paragon.app.window.getCurrent(function (win) {
                        win.restore(function () {
                            win.setAlwaysOnTop(true, function () {
                                win.setAlwaysOnTop(false);
                                mb.send(envelope.replyAddress, { type: 'System_Response', action: 'Bring_Symphony_To_Front', result: true });
                            });
                        });
                    });
                    break;
            }
        }

    });
   
    mb.publish("com.symphony.system.events", { type: "System_Status", status: sysstatus });

    symphony.interop.outboundChannel = {
        post: function (message) {
            if (message != null && message.responseAddress != null)
            {
                mb.send(message.responseAddress, message);
            }
        }
    }
}};
