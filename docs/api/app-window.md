# paragon.app.window

Use the `paragon.app.window` API to create related browser windows and co-ordinate them being in workspaces. Should be instantiated once per application.

## Methods

The 'paragon.app.runtime' object has the following methods:

### create(startUrl, options, callback)

Creates a window

* `startUrl` string - location of the window.
* `options` CreateWindowOptions - properties to Create Window.
* `callback` function - will be called with `callback(ApplicationWindow window)`.

`CreateWindowOptions`:

* `id` string -  Id to identify the window. This will be used to remember the size and position of the window. (OPTIONAL).
* `outerBounds` BoundsSpecification -  Used to specify the initial position, initial size and constraints of the window (including window decorations such as the title bar and frame). If an id is also specified and a window with a matching id has been shown before, the remembered bounds will be used instead. Note that the padding between the inner and outer bounds is determined by the OS. Therefore setting the same bounds property for both the innerBounds and outerBounds will result in an error.
* `frame` FrameOptions -  Window frame options.
* `initialState` string -  Initial state of the window, allowing it to be created already fullscreen, maximized or minimized.
* `hidden` boolean -  If hidden, show() will make the window appear.
* `resizable` boolean -  If true, the window will be resizable by the user. Defaults to true.
* `alwaysOnTop` boolean -  If true, the window will stay above most other windows. If there are multiple windows of this kind, the currently focused window will be in the foreground. Requires "alwaysOnTopWindows" permission. Defaults to false.
* `focused` boolean -  If true, the window will be focused when created. Defaults to true.
* `minimizeOnClose` boolean -  If true, clicking the close button will minimize the window. Defaults to false.
* `hotKeysEnabled` boolean -  If true, enables the support of hot keys for this window. Defaults to false.

`callback` will be called with `callback(Dictionary<string, object>)` containing the application argments.


### getCurrentApp(callback)

Returns an `ApplicationWindow` object for the current script context (ie JavaScript 'window' object). This can also be called on a handle to a script context for another page.

`callback` function - will be called with `callback(ApplicationWindow window)`.

### getAll(callback)

Returns an `ApplicationWindow` object for the current script context (ie JavaScript 'window' object). This can also be called on a handle to a script context for another page.

`callback` will be called with `callback(windows)` containing an array of ApplicationWindow.

`windows` Array - Array of ApplicationWindow

### getById(id, callback)

Returns an `ApplicationWindow` with the given id. If no window with the given id exists, null is returned.

* `id` string - id associated with the window
* `callback` function - will be called with `callback(ApplicationWindow window)`.

## Types

`FrameOptions`:

* `icon` boolean
* `minimizeButton` boolean
* `maximizeButton` boolean
* `type` string - enum of "notSpecified", "paragon", "windowsDefault" or "none"
* `systemMenu` SystemMenuItem 

`SystemMenuItem`:

* `header` string
* `id` integer
* `checkable` boolean
* `isChecked` string
* `enable` boolean 

`BoundsSpecification`:

* `left` double
* `top` double
* `width` double
* `height` double
* `minWidth` double
* `minHeight` double
* `maxWidth` double
* `maxHeight` double

`ApplicationWindow`:
