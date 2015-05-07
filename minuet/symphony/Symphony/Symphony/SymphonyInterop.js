var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    mb.subscribe("api.symphony.com", function (envelope) {
        console.log("api.symphony.com/incoming envelope: " + envelope.message.content);
        //var content = JSON.parse(envelope.message.content);
        //console.log("api.symphony.com/message content: " + content);
        //window.postMessage(content, "*");
    });

    symphony.interop.outboundChannel = {
        post: function (message) {
          if (message != null && message.callbackTopic != null)
          {
                mb.send(message.callbackTopic, message);
          }
       }
    }
}};
