var symphony = symphony || {};
symphony.interop = {createChannel: function(){
    var mb = meow.messagebroker();
    mb.subscribe("api.symphony.com", function (envelope) {
  		var strEnvelope =  JSON.stringify(envelope);
		console.log('symphonyInterop is working on:', strEnvelope);
		console.log("envelope", envelope);

		var msg;
		if (envelope.message.hasOwnProperty('content')){
			msg = JSON.parse(envelope.message.content);
		} else {
			msg = envelope.message;
		}

		console.log("msg", msg);
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
