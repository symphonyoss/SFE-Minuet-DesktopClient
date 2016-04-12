# paragon.log

Use the `paragon.log` API to maintain log - Debug, Error, Warn etc

## Methods

The 'paragon.log' object has the following methods:

### debug(args, callback)

Writes to debugging log.

* `args` Object - System object to be logged.
* `callback` function - will be called with `callback()`.

### error(args, callback)

Writes to the error log.

* `args` Object - System object to be logged.
* `callback` function - will be called with `callback()`.

### info(args, callback)

Writes to information log.

* `args` Object - System object to be logged.
* `callback` function - will be called with `callback()`.

### warn(args, callback)

Writes to warn log.

* `args` Object - System object to be logged.
* `callback` function - will be called with `callback()`.