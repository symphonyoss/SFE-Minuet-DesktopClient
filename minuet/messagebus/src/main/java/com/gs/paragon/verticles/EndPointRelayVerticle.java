package com.gs.paragon.verticles;

/**
 * Created by Stamatakis on 5/12/2015.
 */

import org.vertx.java.core.AsyncResult;
import org.vertx.java.core.Handler;
import org.vertx.java.core.eventbus.EventBus;
import org.vertx.java.core.eventbus.Message;
import org.vertx.java.core.json.JsonObject;
import org.vertx.java.platform.Verticle;
import org.apache.log4j.Logger;

public class EndPointRelayVerticle extends Verticle {
    private static final Logger logger = Logger.getLogger(EndPointRelayVerticle.class);
    private static final long TIMEOUT = 3000;
    private static final String ADDRESS = "com.paragon.endpointrelay";
    private static final String ADDRESS_FOR_EVENTS = "com.paragon.endpointrelay.events";
    private static final String ENDPOINT_TOPIC = "com.symphony.system";
    private static final String ENDPOINT_TOPIC_FOR_EVENTS = "com.symphony.system.events";
    private static final String QUERY_STATUS = "Query_System_Status";
    private static final String BRING_TO_FRONT = "Bring_Symphony_To_Front";

    public void start() {
        EventBus eb = vertx.eventBus();

        Handler<Message<JsonObject>> theHandler = new Handler<Message<JsonObject>>() {
            @Override
            public void handle(Message<JsonObject> message) {
                if (!(message.replyAddress() == null || message.replyAddress().isEmpty())) {
                    final String repAddress = message.replyAddress();
                    final JsonObject msg = message.body();
                    System.out.println(msg);
                    String action = msg.getString("action");
                    if (!(action == null || action.isEmpty())) {
                        switch(action){
                            case QUERY_STATUS:
                            case BRING_TO_FRONT:
                                Handler<AsyncResult<Message<JsonObject>>> endpointHandler = new Handler<AsyncResult<Message<JsonObject>>>() {
                                    @Override
                                    public void handle(AsyncResult<Message<JsonObject>> result) {
                                        JsonObject response = null;
                                        if (result.succeeded()){
                                            response = result.result().body();
                                            System.out.println("success");
                                        }
                                        else{
                                            System.out.println("failed");

                                            String action = msg.getString("action");
                                            if (action.equals(QUERY_STATUS)) {
                                                //{ type: 'System_Response', action: 'Query_System_Status', status:sysstatus}
                                                response = new JsonObject().putString("type", "System_Response")
                                                        .putString("action", action)
                                                        .putString("status", "offline");
                                            }
                                            else if (action.equals(BRING_TO_FRONT)){
                                                response = new JsonObject().putString("type", "System_Response")
                                                        .putString("action", action)
                                                        .putBoolean("result", false);
                                            }
                                        }
                                        System.out.println(response);

                                        EventBus eb = vertx.eventBus();
                                        eb.send(repAddress, response);
                                    }
                                };
                                EventBus eb = vertx.eventBus();
                                eb.sendWithTimeout(ENDPOINT_TOPIC, msg, TIMEOUT, endpointHandler);

                                break;
                            }

                        }
                    }
                }
            };

        if (logger.isTraceEnabled()) {
            logger.trace("registerTopic :: Registering a new handler");
        }
        eb.registerHandler(ADDRESS, theHandler, new Handler<AsyncResult<Void>>() {
            @Override
            public void handle(AsyncResult<Void> voidAsyncResult) {
                boolean registered = voidAsyncResult.succeeded();
                if (registered) {
                    if (logger.isTraceEnabled()) {
                        logger.trace("registerTopic :: Registered handler = " + registered);
                    }
                }

            }
        });

        // Subscribe to endpoint topic for oneway event emission
        eb.registerHandler(ENDPOINT_TOPIC_FOR_EVENTS, new Handler<Message<JsonObject>>() {
            @Override
            public void handle(Message<JsonObject> message) {
                // Republish on endpoint relay for events topic
                EventBus eb = vertx.eventBus();
                eb.publish(ADDRESS_FOR_EVENTS, message.body());
            }
        });
    }
}
