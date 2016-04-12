# Quick Start

This tutorial walks you through creating your first Minuet `Packaged App`.

### A Minuet Packaged App contains these components:

* The `manifest` tells Minuet about your app, what it is, and how to launch it.
* `App icons` must be included in the package as well.
* The `background script` is used to create the event page responsible for managing the app life cycle.
* All code must be included in a packaged app.  This includes HTML, JS, CSS and Plugins.

## 1. Create the manifest

First create your `manifest.json` file.

```javascript
{
    name: "Awesome",
    description: "My first App is awesome.",
    id : "awesome",
    version: "0.1",
    manifest_version: 2,
    app: {
        background: {
            scripts: ["background.js"]
        }
    }
}
```

## 2. Create the background script

Next create a new file called `background.js` with the following content.

```javascript
paragon.app.runtime.onLaunched.addListener(function() {
    paragon.app.window.create('window.html', {
        'bounds': {
            'width': 400,
            'height': 500
        }
    });
});
```

## 3. Create a window page
Create your `window.html` file

```html
<html>
    <head>
    <body>
        <div>Hello, everything is awesome!</div>
    </body>
</html>
```

## 5. Run your app