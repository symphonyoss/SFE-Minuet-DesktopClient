# paragon.system

Use the `paragon.system` API to add items to Paragon's context menu. You can choose what types of objects your context menu additions apply to, such as images, hyperlinks, and pages.

## Events

The `paragon.system` object emits the following events:

### Event: 'onDisplaySettingsChanged'

Fired when the system display settings are changed.

```javascript
paragon.system.onDisplaySettingsChanged.addListener(callback)
```

* `callback` Function

## Methods

The 'paragon.system' object has the following methods:

### getCpuInfo(callback)

Gets the CPU Information.

* `callback` function - will be called with `callback(object cpuInfo)`.

`cpuInfo` object:

* `architecture` string - Processor architecture.
* `cpuName` string - Processor Identifier.
* `cpuUsage` double.
* `platform` string - Platform of the processor build.
* `processorCount` integer.

### getMemoryInfo(callback)

Gets the memory information.

* `callback` function - will be called with `callback(object memoryInfo)`.

`memoryInfo` object:

* `commitTotalPages` long.
* `commitLimitPages` long.
* `commitPeakPages` long.
* `physicalTotalBytes` long.
* `physicalAvailableBytes` long.
* `physicalUsedBytes` long.
* `systemCacheBytes` long.
* `kernelTotalBytes` long.
* `kernelPagedBytes` long.
* `kernelNonPagedBytes` long.
* `pageSizeBytes` long.
* `memoryUsage` double.
* `handlesCount` integer.
* `processCount` integer.
* `threadCount` integer.

### getScreenInfo(callback)

Gets the screen information.

* `callback` function - will be called with `callback(array of object infos)`.

`callback` will be called with `callback(infos)` containing an array of ScreenInfo.

`infos` Array - Array of ScreenInfo

`ScrenInfo` object:

* `name` string.
* `isPrimary` boolean.
* `bounds` Bounds object.
* `workArea` Bounds object.

`Bounds` object:

* `left` integer.
* `height` integer.
* `top` integer.
* `width` integer.