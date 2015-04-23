var Symphony = Symphony || {};
Symphony.Interop = new Object();
Symphony.Interop.init = function () {
    
    paragon.messagebus.onMessage.addListener(function (topic, message) {
        
        window.postMessage({
            "version": 1,
            "event": "open:im",
            "payload": {
                "userName": "matt",
                "sendMessage": message.message.content
            }
        }, "*");
    });
   
    /*
    window.onMessage.addListener(function (message) {
        paragon.messagebus.publish("out.symphony.com", message);
    });
    */
    paragon.messagebus.subscribe("client.symphony.com");
}
