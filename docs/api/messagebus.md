# paragon.messagebus

Use the `paragon.messagebus` API to add items to Paragon's context menu. You can choose what types of objects your context menu additions apply to, such as images, hyperlinks, and pages.

## Events

The `paragon.messagebus` object emits the following events:

### Event: 'onConnected'

Fired when connected.

```javascript
paragon.messagebus.onConnected.addListener(callback)
```

* `callback` Function

### Event: 'onDisconnected'

Fired when disconnected.

```javascript
paragon.messagebus.onDisconnected.addListener(callback)
```

* `callback` Function

### Event: 'onError'

Fired when an error is encountered.

```javascript
paragon.messagebus.onError.addListener(callback)
```

* `callback` Function

### Event: 'onMessage'

Fired when a message is encountered.

```javascript
paragon.messagebus.onMessage.addListener(callback)
```

* `callback` Function

## Methods

The 'paragon.messagebus' object has the following methods:

### publishMessage(topic, message, callback)

Publish a message to all subscribers of a topic.

* `topic` string - Topic to which the message is to be published.
* `message` object - Content to be published.
* `callback` function - will be called with `callback()`.

###sendMessage(topic, message, responseId, callback)

Send a message to one of the subscribers (to a topic) in a round-robin fashion. Use publish if intent is to deliver to all subscribers.

* `topic` string - Specific topic that the subscriber is subscribed to.
* `message` object - Content to be sent.
* `responseId` string - responseId is a client side application provided string value to coordinate the receipt of a response if the other application listening for the messages on the topic decides to respond. It is optional.
* `callback` function - will be called with `callback()`.

### subscribe(topic, responseId, callback)

Subscribe to the topic referenced.

* `topic` string - Topic corresponding to the message.
* `responseId` string - responseId to coordinate the receipt of a response.
* `callback` function - will be called with `callback()`.

### unsubscribe(topic, responseId, callback)

Unsubscribe from the topic referenced.

* `topic` string - Topic corresponding to the message.
* `responseId` string - responseId to coordinate the receipt of a response.
* `callback` function - will be called with `callback()`.

### getState(topic, callback)

Gets the state of the socket.

* `topic` string - Topic corresponding to the message.
* `callback` function - will be called with `callback()`.
