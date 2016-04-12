# paragon.contextMenu

Use the `paragon.contextMenu` API to add items to Paragon's context menu. You can choose what types of objects your context menu additions apply to, such as images, hyperlinks, and pages.

## Events

The `paragon.contextMenu` object emits the following events:

### Event: 'onclick'

Fired when a context menu is clicked.

```javascript
paragon.contextMenu.onclick.addListener(callback)
```

* `callback` Function

## Methods

The 'paragon.contextMenu' object has the following methods:

### create(createProperties, callback)

Create a context menu

* `createProperties` object
* `callback` function - will be called with `callback(integer id)`.

`CreateProperties` object:

* `checked` boolean.
* `enabled` boolean.
* `groupId` string.
* `id` integer.
* `parentId` integer.
* `title` string.
* `title` enum of "normal", "checkbox", "radio", or "separator". Defaults to 'normal' if not specified.

### update(id, updateProperties, callback)

Update a context menu

* `id` integer - The ID of the context menu item to update.
* `createProperties` object - The properties to update. Accepts the same values as the create function.
* `callback` function - will be called with `callback()`.

### remove(id, callback)

Removes a context menu item.

* `id` integer - The ID of the context menu item to remove.
* `callback` function - will be called with `callback()`.

### removeAll(callback)

Removes all items from the context menu.

* `callback` function - will be called with `callback()`.
