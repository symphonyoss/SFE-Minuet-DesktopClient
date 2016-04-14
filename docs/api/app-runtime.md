# paragon.app.runtime

Use the `paragon.app.runtime` API to manage the app lifecycle. The app runtime manages app installation, controls the event page, and can shut down the app at anytime.

## Events

The `paragon.app.runtime` object emits the following events:

### Event: 'onLaunched'

Fired when an app is launched.

```javascript
paragon.app.runtime.onLaunched.addListener(callback)
```

* `callback` Function

### Event: 'onExiting'

Fired when an app is exiting.

```javascript
paragon.app.runtime.onExiting.addListener(callback)
```

* `callback` Function

### Event: 'onProtocolInvoke'

Fired when an app receives a command.

```javascript
paragon.app.runtime.onProtocolInvoke.addListener(callback) 
```

* `callback` Function

## Methods

The 'paragon.app.runtime' object has the following methods:

### getArgs(callback)

Gets the arguments of the application.

* `callback` Function

`callback` will be called with `callback(Dictionary<string, object>)` containing the application argments.

### getCurrentApp(callback)

Gets the current application.

`callback` will be called with `callback(info)` containing the application object.

`info` Object:
* `appId` string
* `appInstanceId` string
* `browserPid` string
* `workspaceId` string

### getAppVersion(callback)

Gets the current application version.

`callback` will be called with `callback(version)` containing the application version.

`version` string


### applogfile(callback)

Gets the current application log path.

`callback` will be called with `callback(file)` containing the log file path.

`file` string

### applogfiles(callback)

Gets a list of the archived log files for the current application.

`callback` will be called with `callback(files)` containing an array of log file paths.

`files` Array - Array of file strings