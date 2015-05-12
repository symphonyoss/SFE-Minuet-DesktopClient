var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    var sysstatus = "Ready";

    mb.subscribe("api.symphony.com", function (envelope) {
        window.postMessage(envelope.message, "*");
    });

    mb.subscribe("system.symphony.com", function (envelope) {
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
