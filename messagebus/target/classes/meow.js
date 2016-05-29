/*
 * Copyright 2016 Symphony Communication Services, LLC
 * Created by William Stamatakis.
 * meow.js
 * Description: [M]essaging [E]xtended [O]ver [W]ebsockets
 */

var Paragon = Paragon || {};
Paragon.MessageBus = function(url) {
    // url format: ws://localhost:<PORT_NUMNER>/paragon/messagebus?appid=com.gs.tech.desktop.app1

    var that = this;
    var handlerMap = {};
    var socket = null;
    if (window.WebSocket) {
        socket = new WebSocket(url);
        socket.onmessage = function(event) {
            console.log("Received data from websocket: " + event.data);
            var key = null;
            var message = JSON.parse(event.data);

            if (message.type == "response"){
                key = message.rid;
            }
            else if (message.type == "message"){
                key = message.address;
            }
            if (key){
                var handler = handlerMap[key];
                if (handler){
                    handler(message);
                    if (message.type == "response"){
                        // **Note: response handlers are for one time use.
                        // remove handler
                        delete handlerMap[key];
                    }

                }
            }

        }
        socket.onopen = function(event) {
            alert("Web Socket opened!");
        };
        socket.onclose = function(event) {
            // Remove handlers from memory.
            delete handlerMap;
            alert("Web Socket closed.");
        };
    } else {
        alert("Your browser does not support Websockets. (Use Chrome)");
    }

    that.send = function(address, message, responseHandler){
        checkParam('address', address, 'string');
        checkParam('responseHandler', responseHandler, 'function', true);

        // {type:"send", address:"whatever", message:{content:...}[, rid:"newGuid"]}
        var payload = {type:"send", address:address, message:{content:message}};
        if (responseHandler){
            var rid = newGUID();
            payload.rid = rid;
            handlerMap[rid] = responseHandler;
        }
        var str = JSON.stringify(payload);
        socket.send(str);
    }

    that.publish = function(address, message){
        checkParam('address', address, 'string');
        //checkParam('message', message, 'object');

        // {type:"publish", address:"whatever", message:{content:...}}
        var payload = {type:"publish", address:address, message:{content:message}};
        var str = JSON.stringify(payload);
        socket.send(str);
    }

    that.subscribe = function(address, subscriptionHandler, responseHandler){
        checkParam('address', address, 'string');
        checkParam('subscriptionHandler', subscriptionHandler, 'function');
        checkParam('responseHandler', responseHandler, 'function', true);

        // **Note: Only register a single subscriptionHandler per unique address.
        var hSubscription = handlerMap[address];
        if (!hSubscription){
            // Add to subscription handler to handlerMap
            handlerMap[address] = subscriptionHandler;

            // {type:"register", address:"whatever" [, rid:"newGuid"]}
            var payload = {type:"register", address:address};
            if (responseHandler){
                var rid = newGUID();
                payload.rid = rid;
                handlerMap[rid] = responseHandler;
            }
            var str = JSON.stringify(payload);
            socket.send(str);
        }
    }

    that.unsubscribe = function(address, responseHandler){
        checkParam('address', address, 'string');
        checkParam('responseHandler', responseHandler, 'function', true);

        // **Note: Only unsubscribe if a subscriptionHandler exists for the address.
        var hSubscription = handlerMap[address];
        if (hSubscription){
            // Remove subscription handler from handlerMap
            delete handlerMap[address];

            // {type:"unregister", address:"whatever" [, rid:"newGuid"]}
            var payload = {type:"unregister", address:address};
            if (responseHandler){
                var rid = newGUID();
                payload.rid = rid;
                handlerMap[rid] = responseHandler;
            }
            var str = JSON.stringify(payload);
            socket.send(str);
        }
    }

    function checkParam(paramName, param, paramType, optional)
    {
        if(!optional && !paramName)
        {
            throw new Error("Parameter name must be provided.");
        }

        if(!optional && !paramType)
        {
            throw new Error("Parameter type must be provided.");
        }

        if(!optional && !param)
        {
            throw new Error("Parameter " + paramName + " can't be null.");
        }

        if(!optional && typeof param != paramType)
        {
            throw new Error("Parameter " + paramName + " type mismatch.  The expected type is " + paramType);
        }

    }

    function newGUID() {
        return "xxxxxxxx-xxxx-4xxx-yxxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (a, b) {
            return b = Math.random() * 16, (a == "y" ? b & 3 | 8 : b | 0).toString(16);
        });
    }

    return that;
}
