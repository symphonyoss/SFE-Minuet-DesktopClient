
// Singleton appbridge instance.
var appbridge = new function () {
    // Capture 'this' and define local vars.
    var self = this,
        mainWindow,
        notificationHandlersRegistered = false,
        onFileUploadCallback,
        onRegisterFocusCallback,
        notificationIds = {};

    /* *************************************************
     * Public functions.
     * *************************************************/

    self.Log = function (message) {
        console.log("[JS]", message);
    };

    self.RefreshAuthCookie = function (onRefresh) {
    };

    self.Shutdown = function () {
    };

    self.GetClientInfoRequest = function (callbackName) {
        symphony.systeminfo.getSystemInfo(function (clientInfo) {
            execCallback(callbackName, JSON.stringify(clientInfo));
        });
    };

    self.OpenUrl = function (url) {
        symphony.external.openUrl(url);
    };

    self.CallByKerberos = function (kerberos) {
        symphony.external.callByKerberos(kerberos);
    };

    self.RegisterFileUploadCallback = function (fileUploadCallback) {
        onFileUploadCallback = fileUploadCallback;
    };

    self.OpenScreenSnippetTool = function () {
        console.log("paragon.snippets.capture");

        paragon.snippets.capture(function (snippet) {
            var toUpload = {
                filename: "screenSnippet." + snippet.imageType,
                content: snippet.bytes
            };

            console.log(onFileUploadCallback);
            execCallback(onFileUploadCallback, JSON.stringify(toUpload));
        });
    };

    self.ClearAlerts = function () {
        console.log("paragon.notifications.clearAll");
        paragon.notifications.clearAll();
    };

    self.PlayChime = function () {
        console.log("playChime");
    };

    self.PostAlert = function (alertOptions) {
        console.log("paragon.notifications.create");

        // Verify that onClicked onClosed event handlers have been attached.
        if (!notificationHandlersRegistered) {
            registerNotificationHandlers();
        }

        // Translate the alert options we received into a notifications
        // options object to send to paragon.
        var opts = createNotificationOptions(alertOptions);

        // Create the notification.
        paragon.notifications.create(opts, function (notificationId) {
            // If a callback was specified for the notification (they are
            // optional), store the callback info for later use.
            if (opts.callback) {
                notificationIds[notificationId] = {
                    method: opts.callback,
                    args: opts.callbackArg
                };
            }

            // Get the main app window.
            getMainWindow(function (win) {
                // Cause the taskbar icon for the window to flash.
                // -1 --> flash forever
                // timeout 1500 ms - time between flashes
                win.drawAttention(true, -1, 1500);
            });
        });
    };

    self.ShowAlertSettings = function () {
        console.log("paragon.notifications.openSettings");
        paragon.notifications.openSettings();
    };

    self.RemoveAlertGrouping = function (group) {
        console.log("paragon.notifications.clearGroup: " + group);
        paragon.notifications.clearGroup(group);
    };

    self.GetTempValue = function (key, callbackName) {
        symphony.cache.getValue(key, function (val) {
            execCallback(callbackName, JSON.stringify(val));
        });
    };

    self.SetTempValue = function (key, value) {
        symphony.cache.setValue(key, value);
    };

    self.Activate = function (windowName, activate) {
        console.log("activate window: " + windowName + "(" + activate + ")");

        if (!activate) {
            symphony.app.window.showWindowWithNoActivate(windowName);
        } else {
            symphony.app.window.showWindow(windowName);
        }
    };

    self.Close = function (windowName) {
        console.log("close window: " + windowName);
        paragon.app.window.getById(windowName, function (window) {
            if (window != null) {
                console.log("closing window: " + windowName);
                window.close();
            }
        });
    };

    self.GetActiveWindow = function (callbackName) {
        console.log("get active window");

        /* TODO:  
         *  GetActiveWindow is called every time a notification is received. The
         *  nested callbacks below could be simplified to reduce the amount of
         *  cross-process messaging that needs to take place. 
         *
         *  Adding a paragon.app.window.getActive() function to get the ID of 
         *  the active app window (or null if none are active) would simplify 
         *  the call structure to:
         *
         *      paragon.app.window.getActiveWindowId(function (appWindowId) {
         *          if (appWindowId) {
         *              execCallback(callbackName, JSON.stringify({ windowName: appWindowId }));
         *          }
         *      });
         * 
         *  Note that we may want to also add a second function to get the active
         *  window itself (paragon.app.window.getActive), but to streamline the
         *  cross-process callbacks as much as possible, getActiveWindowId would
         *  be ideal.
         */

        // Get all app windows.
        paragon.app.window.getAll(function (appWindows) {

            // Iterate all app windows.
            for (var i = 0, j = appWindows.length; i < j; ++i) {
                // Fetch the ID for the current window. This is async call, so even
                // if the first window in the list is the active one, the cross-process
                // calls to getId (and isWindowActive) will be made for all windows
                // every time GetActiveWindow is called.
                appWindows[i].getId(function (id) {
                    // Get the active status of the window.
                    symphony.app.window.isWindowActive(id, function (isActive) {
                        if (isActive) {
                            // We got the active window, exec the callback.
                            console.log("active window: " + id);
                            execCallback(callbackName, JSON.stringify({ windowName: id }));
                        }
                    });
                });
            }
        });
    };

    self.RegisterFocusCallback = function (callback) {
        onRegisterFocusCallback = callback;
    };

    self.SetMinWidth = function (windowName, minWidth) {
    };

    /* *************************************************
     * Private functions.
     * *************************************************/

    // Execute named callbacks defined in the global (window) scope.
    function execCallback(callbackName, arg) {
        // Callback functions are defined in the global scope. Check for
        // the existence of the named callback function on the window object.
        if (!callbackName in window) {
            // The specified callback does not exist.
            return;
        }

        // Callback found, execute it.
        window[callbackName](arg);
    }

    // Translate options from JSON to an options object for use by Paragon.
    function createNotificationOptions(options) {
        var notificationOptions = {
            backgroundColor: "505050",
            blinkColor: "ffa500",
            canBlink: false,
            canPlaySound: false,
            callback: null,
            callbackArg: null,
            group: "default",
            iconUrl: null,
            isClickable: true,
            isPersistent: false,
            message: null,
            soundFile: null,
            title: null,
        };

        var alertOption = JSON.parse(options);

        if (alertOption) {
            if (alertOption.color) {
                notificationOptions.backgroundColor = alertOption.color;
            }
            if (alertOption.blinkColor) {
                notificationOptions.blinkColor = alertOption.blinkColor;
            }
            if (alertOption.blink) {
                notificationOptions.canBlink = alertOption.blink;
            }
            if (alertOption.playSound) {
                notificationOptions.canPlaySound = alertOption.playSound;
            }
            if (alertOption.callback) {
                notificationOptions.callback = alertOption.callback;
            }
            if (alertOption.callbackArg) {
                notificationOptions.callbackArg = alertOption.callbackArg;
            }
            if (alertOption.grouping) {
                notificationOptions.group = alertOption.grouping;
            }
            if (alertOption.imageUri) {
                notificationOptions.iconUrl = alertOption.imageUri;
            }
            if (alertOption.persistent) {
                notificationOptions.isPersistent = alertOption.persistent;
            }
            if (alertOption.message) {
                notificationOptions.message = alertOption.message;
            }
            if (alertOption.soundFile) {
                notificationOptions.soundFile = alertOption.soundFile;
            }
            if (alertOption.title) {
                notificationOptions.title = alertOption.title;
            }
        }

        return notificationOptions;
    }

    // Get the main Symphony window.
    function getMainWindow(callback) {
        if (!mainWindow) {
            paragon.app.window.getById('main', function (win) {
                mainWindow = win;
                callback(win);
            });
        } else {
            callback(mainWindow);
        }
    };

    // Register onClicked and onClosed event listeners for handling
    // notification related lifecycle events.
    function registerNotificationHandlers() {
        notificationHandlersRegistered = true;

        // Attach an onClicked event handler.
        paragon.notifications.onClicked.addListener(function (notificationId) {
            // Bring the main Symphony window to the foreground.
            symphony.app.window.showWindow("main");

            // Extract the callback for the current notification.
            var cb = notificationIds[notificationId];
            if (cb) {
                // We found stored callback info, execute it.
                execCallback(cb.method, cb.args);
            }
        });

        // Attach an onClosed event handler.
        paragon.notifications.onClosed.addListener(function (notificationId) {
            // Remove the callback info for the notification that has closed.
            delete notificationIds[notificationId];
        });
    }
};

