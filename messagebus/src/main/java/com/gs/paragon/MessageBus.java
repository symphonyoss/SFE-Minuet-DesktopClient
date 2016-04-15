package com.gs.paragon;


import java.io.IOException;
import java.net.URL;

import org.apache.log4j.Logger;
import org.vertx.java.core.AsyncResult;
import org.vertx.java.core.Handler;
import org.vertx.java.platform.PlatformLocator;
import org.vertx.java.platform.PlatformManager;

import com.gs.paragon.verticles.HttpServerVerticle;

public class MessageBus {
    private static final Logger logger = Logger.getLogger(MessageBus.class);

    public static void main(String[] args) throws IOException {
        if( logger.isInfoEnabled()) {
            logger.info("Paragon message broker starting ...");
        }
        
        //default number of verticle instances to deploy is 2
        int instances = 2;

        // Parse command line
        if (args != null && args.length > 0){
            int i = 0;
            int exceededUpperBound = args.length;
            while (i < exceededUpperBound){
                // look for -instances argument followed by valid integer value
                if (args[i].equals("-instances") && (i+1) < exceededUpperBound) {
                    String value = args[i + 1];
                    if (!(value.startsWith("-"))) {
                        try {
                            // parse command command line arg as int
                            int count = Integer.parseInt(value);

                            // max verticle instances is 10
                            count = (count > 10) ? 10 : count;

                            // min verticle instances is 1
                            instances = (count == 0) ? 1 : count;

                            // increment over value arg
                            i++;
                        } catch (NumberFormatException e) {
                            System.err.println("Argument " + value + " must be an integer.");
                            System.exit(1);
                        }
                    }
                }
                // todo check for other args in the future here similar to above
                //
                // increment to next arg
                i++;
            }
        }

        // print to console -instances {instances}
        if( logger.isInfoEnabled()) {
            logger.info("Number of instances : " + instances);
            logger.info("Deploying verticles ...");
        }

        // Deploy verticles
        PlatformManager pm = PlatformLocator.factory.createPlatformManager();
        
        
        pm.deployVerticle(HttpServerVerticle.class.getName(), null, new URL[]{}, instances, null, new Handler<AsyncResult<String>>() {
            @Override
            public void handle(AsyncResult<String> asyncResult) {
                if(asyncResult.succeeded()){
                    if( logger.isInfoEnabled()) {
                        logger.info("Message Broker Bridge verticle deployed : Deployment ID: " + asyncResult.result());
                    }
                }
                else {
                    logger.error("Error deploying Message Broker Bridge verticle : " + asyncResult.cause().getStackTrace().toString());
                }
            }
        });

        System.in.read();
    }
}
