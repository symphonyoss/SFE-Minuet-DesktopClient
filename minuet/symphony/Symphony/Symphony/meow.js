var meow = meow || {};

meow.messagebroker = function () {
    var that = this;
    var handlerMap = {};
    var errorListenerMap = {};
    var connectedListenerMap = {};
    var disconnectedListenerMap = {};

    that.onError = {
        addListener: function (listener) {
            checkParam("listener", listener, "function", false);
            errorListenerMap[listener] = listener;
        },
        removeListener: function (listener) {
            checkParam("listener", listener, "function", false);
            delete errorListenerMap[listener];
        }
    };
        
    that.onConnected = {
        addListener: function (listener) {
            checkParam("listener", listener, "function", false);
            connectedListenerMap[listener] = listener;
        },
        removeListener: function (listener) {
            checkParam("listener", listener, "function", false);
            delete connectedListenerMap[listener];
        }
    };

    that.onDisconnected = {
        addListener: function (listener) {
            checkParam("listener", listener, "function", false);
            disconnectedListenerMap[listener] = listener;
        },
        removeListener: function (listener) {
            checkParam("listener", listener, "function", false);
            delete disconnectedListenerMap[listener];
        }
    };

    that.send = function (address, message, responseHandler) {
        checkParam('address', address, 'string');
        checkParam('message', message, 'object');
        checkParam('responseHandler', responseHandler, 'function', true);

        // {type:"send", address:"whatever", message:{...}[, rid:"newGuid"]}
        if (responseHandler) {
            var rid = newGUID();
            handlerMap[rid] = responseHandler;
            paragon.messagebus.sendMessage(address, message, rid);
        }
        else {
            paragon.messagebus.sendMessage(address, message)
        }

    }

    that.publish = function (address, message) {
        checkParam('address', address, 'string');
           
        // {type:"publish", address:"whatever", message:{...}}
        paragon.messagebus.publishMessage(address, message);
    }

    that.subscribe = function (address, subscriptionHandler, responseHandler) {
        console.log("subscribing to topic " + address);

        checkParam('address', address, 'string');
        checkParam('subscriptionHandler', subscriptionHandler, 'function');
        checkParam('responseHandler', responseHandler, 'function', true);

        // **Note: Only register a single subscriptionHandler per unique address.
        var hSubscription = handlerMap[address];
        if (!hSubscription) {
            // Add to subscription handler to handlerMap
            handlerMap[address] = subscriptionHandler;

            // {type:"register", address:"whatever" [, rid:"newGuid"]}
            if (responseHandler) {
                var rid = newGUID();
                handlerMap[rid] = responseHandler;

                paragon.messagebus.subscribe(address, rid);
            }
            else {
                paragon.messagebus.subscribe(address);
            }
        }
    }

    that.unsubscribe = function (address, responseHandler) {
        checkParam('address', address, 'string');
        checkParam('responseHandler', responseHandler, 'function', true);

        // **Note: Only unsubscribe if a subscriptionHandler exists for the address.
        var hSubscription = handlerMap[address];
        if (hSubscription) {
            // Remove subscription handler from handlerMap
            delete handlerMap[address];

            // {type:"unregister", address:"whatever" [, rid:"newGuid"]}
            if (responseHandler) {
                var rid = newGUID();
                handlerMap[rid] = responseHandler;

                paragon.messagebus.unsubscribe(address, rid);
            }
            else {
                paragon.messagebus.unsubscribe(address);
            }
        }
    }

    // help functions
    function checkParam(paramName, param, paramType, optional) {
        if (!optional && !paramName) {
            throw new Error("Parameter name must be provided.");
        }

        if (!optional && !paramType) {
            throw new Error("Parameter type must be provided.");
        }

        if (!optional && !param) {
            throw new Error("Parameter " + paramName + " can't be null.");
        }

        if (!optional && typeof param != paramType) {
            throw new Error("Parameter " + paramName + " type mismatch.  The expected type is " + paramType);
        }

    }

    function newGUID() {
        return "xxxxxxxx-xxxx-4xxx-yxxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (a, b) {
            return b = Math.random() * 16, (a == "y" ? b & 3 | 8 : b | 0).toString(16);
        });
    }

    paragon.messagebus.onMessage.addListener(function (topic, message) {

        var key = null;

        if (message.type == "response") {
            key = message.rid;
        }
        else if (message.type == "message") {
            key = message.address;
        }
        if (key) {
            var handler = handlerMap[key];
            if (handler) {
                handler(message);
                if (message.type == "response") {
                    // **Note: response handlers are for one time use.
                    // remove handler
                    delete handlerMap[key];
                }

            }
        }

    });

    paragon.messagebus.onError.addListener(function (reason) {
        var key;
        for (key in errorListenerMap) {
            errorListenerMap[key](reason);
        }
    });

    paragon.messagebus.onConnected.addListener(function (status) {
        var key;
        for (key in connectedListenerMap) {
            connectedListenerMap[key](reason);
        }
    });

    paragon.messagebus.onDisconnected.addListener(function () {
        delete handlerMap;
        delete connectedListenerMap;

        var key;
        for (key in disconnectedListenerMap) {
            disconnectedListenerMap[key](reason);
        }
        delete disconnectedListenerMap;
    });
    return that;
}
