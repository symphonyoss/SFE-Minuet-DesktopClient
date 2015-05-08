var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    mb.subscribe("api.symphony.com", function (envelope) {
  		var strEnvelope =  JSON.stringify(envelope);
		console.log('symphonyInterop is working on:', strEnvelope)

		console.log(envelope)

		var msg = JSON.parse(envelope.message.content)
        window.postMessage(msg, "*")
    });
  
    symphony.interop.outboundChannel = {
        post: function (message) {
			console.log("Posting out : ", message);
            if (message != null && message.responseAddress != null)
            {
                mb.send(message.responseAddress, message);
            }
        }
    }
}};
