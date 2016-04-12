# paragon.runtime

Use the `paragon.runtime` API to manage the runtime. Runtime manages app launch, services and running applications, and app close.

## Events

The `paragon.runtime` object emits the following events:

### Event: 'onAppLaunched'

Fired when an app is launched.

```javascript
paragon.runtime.onAppLaunched.addListener(callback)
```

* `callback` Function

### Event: 'onAppExited'

Fired when an app has exit.

```javascript
paragon.runtime.onAppExited.addListener(callback)
```

* `callback` Function