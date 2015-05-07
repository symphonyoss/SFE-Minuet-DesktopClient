var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    mb.subscribe("api.symphony.com", function (envelope) {
  		var strEnvelope =  JSON.stringify(envelope);
		console.log('symphonyInterop is working on:', strEnvelope)

		var msg = envelope.message
        window.postMessage(msg, "*")
    });
  
    symphony.interop.outboundChannel = {
        post: function (message) {
			console.log("Posting out : " + message);
            if (message != null && message.callbackTop != null)
            {
                mb.send(message.callbackTopic, message);
            }
        }
    }
}};
