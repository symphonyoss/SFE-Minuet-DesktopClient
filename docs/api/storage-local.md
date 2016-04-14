# paragon.storage.local

Use the `paragon.storage.local` API to store, retrieve, and track changes to user data.

## Methods

The 'paragon.storage.local' object has the following methods:

### get(keys, callback)

Returns an object that has the properties corresponding to the keys referenced from the local storage store.

* `keys` Object - String or an array of strings pointing to names in the local storage store.
* `callback` function - will be called with `callback(object)`.

### set(value, callback)

Adds/Replaces and saves values to the local store.

* `value` Object - Value referenced in the store.
* `callback` function - will be called with `callback()`.

### remove(keys, callback)

Removes the item(s) referenced from the storage.

* `keys` Object - Item(s) in the storage.
* `callback` function - will be called with `callback()`.

### clear(callback)

Deletes local storage file.

* `callback` function - will be called with `callback()`.
