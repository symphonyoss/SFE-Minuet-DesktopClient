# paragon.idle

Use the `paragon.idle` API to detect when the machine's idle state changes.

## Events

The `paragon.idle` object emits the following events:

### Event: 'stateChanged'

Fired when the system changes to an active, idle or locked state. The event fires with "locked" if the screen is locked or the screensaver activates, "idle" if the system is unlocked and the user has not generated any input for a specified number of seconds, and "active" when the user generates input on an idle system.

```javascript
paragon.idle.stateChanged.addListener(callback)
```

* `callback` Function

## Methods

The 'paragon.idle' object has the following methods:

### queryState(detectionIntervalInSeconds, callback)

Returns "locked" if the system is locked, "idle" if the user has not generated any input for a specified number of seconds, or "active" otherwise.

* `detectionIntervalInSeconds` integer - The system is considered idle if detectionIntervalInSeconds seconds have elapsed since the last user input detected.
* `callback` function - will be called with `callback(enum of "active", "idle", or "locked" newState)`.

### setDetectionInterval(detectionIntervalInSeconds, callback)

Sets the interval, in seconds, used to determine when the system is in an idle state for stateChanged events. The default interval is 10 seconds.

* `intervalInSeconds` integer - Threshold, in seconds, used to determine when the system is in an idle state.
* `callback` function - will be called with `callback()`.