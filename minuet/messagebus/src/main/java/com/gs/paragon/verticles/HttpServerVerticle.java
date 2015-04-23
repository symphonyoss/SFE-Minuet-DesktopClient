/*
 * Copyright 2014 Goldman Sachs.
 * Created by William Stamatakis
 */
package com.gs.paragon.verticles;

import org.apache.commons.io.IOUtil;
import org.vertx.java.core.AsyncResult;
import org.vertx.java.core.Handler;
import org.vertx.java.core.buffer.Buffer;
import org.vertx.java.core.eventbus.EventBus;
import org.vertx.java.core.eventbus.Message;
import org.vertx.java.core.http.HttpServerRequest;
import org.vertx.java.core.http.ServerWebSocket;
import org.vertx.java.core.json.JsonObject;
import org.vertx.java.platform.Verticle;
import org.apache.log4j.Logger;
import java.io.InputStream;
import java.util.Properties;
import java.util.concurrent.ConcurrentHashMap;

public class HttpServerVerticle extends Verticle {
    private static final Logger logger = Logger.getLogger(HttpServerVerticle.class);
    private static int DEFAULT_PORT = 65534;
    public void start() {
        String portProp = System.getProperty("paragon.messagebroker.port");

        int port = DEFAULT_PORT;
        if(portProp != null) {
            port = Integer.parseInt(portProp);
        }

        final Integer portNumber = port;

        if(logger.isInfoEnabled())
            logger.info("Starting Paragon Message Broker (HTTP) verticle on port - " + port);

        vertx.createHttpServer().requestHandler(new Handler<HttpServerRequest>() {
            public void handle(HttpServerRequest req) {
                if (req.path().equals("/ws.html") || req.path().equals("/meow.js")) {
                    byte[] bytes = null;
                    InputStream stream = null;

                    try {
                        stream = getClass().getResourceAsStream(req.path());
                        if (req.path().equals("/ws.html")) {
                            String html = IOUtil.toString(stream);
                            html = html.replace("{BROKER_PORT_NUMBER}", portNumber.toString());
                            bytes = IOUtil.toByteArray(html);
                        } else {
                            bytes = IOUtil.toByteArray(stream);
                        }
                        req.response().setStatusCode(200);
                        req.response().headers().add("Content-Type", "text/html; charset=UTF-8");
                        req.response().headers().add("Cache-Control", "no-cache");
                        req.response().end(new Buffer(bytes));
                    } catch (Exception ex) {
                        logger.error("Error processing request : " + ex.getMessage());
                        req.response().setStatusCode(500);
                        req.response().end(ex.getMessage());
                    }

                    try {
                        if (stream != null)
                            stream.close();
                    } catch (final Exception ex) {
                        logger.error("Error closing file stream : " + ex.getMessage());
                    }
                } else {
                    req.response().setStatusCode(200);
                    req.response().headers().add("Cache-Control", "no-cache");
                    req.response().end("Paragon Message Bus Rocks!");
                }
            }
        }).websocketHandler(new Handler<ServerWebSocket>() {
            public void handle(final ServerWebSocket socket) {
                socket.dataHandler(new Handler<Buffer>() {
                    @Override
                    public void handle(Buffer buffer) {
                        String message = buffer.toString();
                        processIncomingSocketMessage(message, socket);
                    }
                });

                socket.closeHandler(new Handler<Void>() {
                    @Override
                    public void handle(Void aVoid) {
                        // Remove topic-to-handler map from socket.
                        ConcurrentHashMap<String, Handler<Message<JsonObject>>> handlerMap = _socketHandlerMap.remove(socket);
                        if (handlerMap != null) {
                            // Unregister all associated handlers.
                            EventBus eb = vertx.eventBus();
                            for (String address : handlerMap.keySet()) {
                                eb.unregisterHandler(address, handlerMap.get(address));
                            }
                        }
                    }
                });
            }

            private String getPartValue(String query, String partName) {
                String finalPart = null;
                if (query != null) {
                    String[] parts = query.split("\\?" + partName + "=", 2);
                    if (parts.length != 2) {
                        parts = query.split("&" + partName + "=", 2);
                    }
                    if (parts.length == 2) {
                        System.out.println(parts[1]);

                        String[] interimPart = parts[1].split("&", 2);
                        finalPart = interimPart[0];

                        //TODO: remove
                        System.out.println(finalPart);
                    }
                }
                return finalPart;
            }

            private void processIncomingSocketMessage(String message, final ServerWebSocket socket) {
                JsonObject objMessage = new JsonObject(message);
                String type = objMessage.getString("type");
                String address = objMessage.getString("address");

                if (logger.isTraceEnabled())
                    logger.trace("Processing incoming socket message of type : " + type);

                if (type.equals("register")) {
                    registerTopic(address, objMessage.getString("rid"), socket);
                } else if (type.equals("unregister")) {
                    unregisterTopic(address, objMessage.getString("rid"), socket);
                } else if (type.equals("send")) {
                    sendMessage(address, objMessage.getString("rid"), objMessage.getObject("message"), socket);
                } else if (type.equals("publish")) {
                    publishMessage(address, objMessage.getObject("message"), socket);
                }
            }

            private void registerTopic(final String address, final String rid, final ServerWebSocket socket) {
                // {"type":"register", "address":"whatever"[,"rid":"12345"]
                if (logger.isTraceEnabled()) {
                    logger.trace("registerTopic :: address = " + (address != null ? address : "") + ", rid = " + (rid != null ? rid : ""));
                }
                if (address != null) {
                    final Handler<Message<JsonObject>> theHandler;

                    // Get the registered handler for this socket on the given address.
                    final ConcurrentHashMap<String, Handler<Message<JsonObject>>> addressMap = _socketHandlerMap.get(socket);
                    Handler<Message<JsonObject>> aHandler = null;
                    if (addressMap != null) {
                        aHandler = addressMap.get(address);
                    }

                    // If a register handler is not found, then create one and register it.
                    if (aHandler == null) {
                        EventBus eb = vertx.eventBus();

                        theHandler = new Handler<Message<JsonObject>>() {
                            @Override
                            public void handle(Message<JsonObject> message) {
                                // Wrap incoming message in Paragon protocol envelope, and forward to socket end point.

                                if (logger.isTraceEnabled()) {
                                    logger.trace("Incoming message (form event bus) on address '" + message.address() + ";. Reply address is " + message.replyAddress());
                                }

                                // {"type":"message", "address":"whatever"[, "replyaddress":"12456"], "message":{...}}
                                JsonObject payload = new JsonObject().putString("type", "message").putString("address", message.address());
                                if (!(message.replyAddress() == null || message.replyAddress().isEmpty())) {
                                    // add replyaddress to payload
                                    payload.putString("replyaddress", message.replyAddress());
                                }
                                // add content to payload
                                payload.putObject("message", message.body());

                                // send payload to socket endpoint
                                socket.writeTextFrame(payload.toString());
                            }
                        };
                        if (logger.isTraceEnabled()) {
                            logger.trace("registerTopic :: Registering a new handler");
                        }
                        eb.registerHandler(address, theHandler, new Handler<AsyncResult<Void>>() {
                            @Override
                            public void handle(AsyncResult<Void> voidAsyncResult) {
                                boolean registered = voidAsyncResult.succeeded();
                                if (registered) {
                                    ConcurrentHashMap<String, Handler<Message<JsonObject>>> theAddressMap = addressMap;
                                    if (theAddressMap == null) {
                                        theAddressMap = new ConcurrentHashMap<String, Handler<Message<JsonObject>>>();
                                        _socketHandlerMap.put(socket, theAddressMap);
                                    }
                                    theAddressMap.put(address, theHandler);
                                }
                                // Send response
                                if (!(rid == null || rid.isEmpty())) {
                                    // {type:"response", address:"whatever", rid:"1234", registered:true|false}
                                    JsonObject message = new JsonObject().putString("type", "response")
                                            .putString("address", address)
                                            .putString("rid", rid)
                                            .putBoolean("registered", registered);

                                    socket.writeTextFrame(message.toString());
                                }
                            }
                        });
                    } else {
                        if (!(rid == null || rid.isEmpty())) {
                            // Send response
                            // {type:"response", address:"whatever", rid:"1234", registered:true|false}
                            JsonObject response = new JsonObject().putString("type", "response")
                                    .putString("address", address)
                                    .putString("rid", rid)
                                    .putBoolean("registered", true);

                            socket.writeTextFrame(response.toString());
                        }
                    }
                }
            }

            private void unregisterTopic(final String address, final String rid, final ServerWebSocket socket) {
                // {"type":"unregister", "address":"whatever"[, rid:"1234"]}
                if (logger.isTraceEnabled()) {
                    logger.trace("unregisterTopic - address = " + (address != null ? address : "") + ", rid = " + (rid != null ? rid : ""));
                }
                if (address != null) {
                    final ConcurrentHashMap<String, Handler<Message<JsonObject>>> addressMap = _socketHandlerMap.get(socket);

                    Handler<Message<JsonObject>> handler = null;
                    if (addressMap != null) {
                        handler = addressMap.get(address);
                    }

                    if (handler != null) {
                        EventBus eb = vertx.eventBus();
                        eb.unregisterHandler(address, handler, new Handler<AsyncResult<Void>>() {
                            @Override
                            public void handle(AsyncResult<Void> voidAsyncResult) {
                                boolean unregistered = voidAsyncResult.succeeded();
                                if (unregistered) {
                                    addressMap.remove(address);
                                }
                                if (!(rid == null || rid.isEmpty())) {
                                    // {type:"response", address:"whatever", rid:"1234", unregistered:true|false}
                                    JsonObject response = new JsonObject()
                                            .putString("type", "response")
                                            .putString("address", address)
                                            .putString("rid", rid)
                                            .putBoolean("unregistered", unregistered);
                                    socket.writeTextFrame(response.toString());
                                }
                            }
                        });
                    } else {
                        if (!(rid == null || rid.isEmpty())) {
                            // {type:"response", address:"whatever", rid:"1234", unregistered:true|false}
                            JsonObject response = new JsonObject()
                                    .putString("type", "response")
                                    .putString("address", address)
                                    .putString("rid", rid)
                                    .putBoolean("unregistered", true);
                            socket.writeTextFrame(response.toString());
                        }
                    }
                }
            }

            private void sendMessage(final String address, final String rid, JsonObject msg, final ServerWebSocket socket) {
                // {"type":"send", "address":"whatever"[, "rid":"1234"], "message":{...}}
                if (logger.isTraceEnabled()) {
                    logger.trace("sendMessage :: address = " + (address != null ? address : "") + ", rid = " + (rid != null ? rid : ""));
                }
                if (!(address == null || address.isEmpty() || msg == null) && (rid == null || rid.isEmpty())) {
                    EventBus eb = vertx.eventBus();
                    eb.send(address, msg);
                } else if (!(address == null || address.isEmpty() || rid == null || rid.isEmpty() || msg == null)) {
                    EventBus eb = vertx.eventBus();
                    eb.send(address, msg, new Handler<Message<JsonObject>>() {
                        @Override
                        public void handle(Message<JsonObject> message) {
                            if (logger.isTraceEnabled()) {
                                logger.trace("Received Response : " + message.body().toString() + ". Reply reply address is " + message.replyAddress());
                            }

                            // {"type":"response", "address":"whatever", "rid":"1234",
                            //  "message":{...} [,"replyaddress":"4321"]}
                            JsonObject msg = message.body();
                            JsonObject response = new JsonObject().putString("type", "response")
                                    .putString("address", address)
                                    .putString("rid", rid)
                                    .putObject("message", msg);
                            if (!(message.replyAddress() == null || message.replyAddress().isEmpty())) {
                                response.putString("replyaddress", message.replyAddress());
                            }
                            socket.writeTextFrame(response.toString());
                        }
                    });
                }
            }

            private void publishMessage(String address, JsonObject msg, final ServerWebSocket socket) {
                // {"type":"publish", "address":"whatever", "message":{...}}
                if (logger.isTraceEnabled()) {
                    logger.trace("publishMessage :: address = " + (address != null ? address : ""));
                }
                if (address != null && !address.isEmpty() && msg != null) {
                    EventBus eb = vertx.eventBus();
                    eb.publish(address, msg);
                }
            }

            private ConcurrentHashMap<ServerWebSocket, ConcurrentHashMap<String, Handler<Message<JsonObject>>>> _socketHandlerMap = new ConcurrentHashMap<ServerWebSocket, ConcurrentHashMap<String, Handler<Message<JsonObject>>>>();
        }).listen(portNumber, "localhost");
    }
}
