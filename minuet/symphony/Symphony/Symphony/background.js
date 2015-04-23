paragon.app.runtime.onLaunched.addListener(function() {

    var appVersion = 'Unknown version';
    var appUrl = 'https://livecurrent-ny.wpt.services.gs.com/webcontroller/routev2';

    var getArgs = new Promise(function(resolve) {
        paragon.app.runtime.getArgs(function (args) {
            if (args) {
                switch (args['env']) {
                    case 'local':
                        appUrl = "https://localhost:3000/";
                    default:
                }
            }
            resolve();
        });
    });

    var getVersion = new Promise(function(resolve) {
        paragon.app.runtime.getAppVersion(function(version) {
            appVersion = version;
            resolve();
        });
    });

    getVersion
    .then(function() {
        return getArgs;
    })
    .then(function () {
        return getAppSettings();
    }).then(function (settings) {
        var systemMenu = {
            refresh: 1011,
            minimizeOnClose: 1021,
            editShortcuts: 1031,
            exit: 1041,
            about: 1051
        };

        var createParams = {
            id: "main",
            autoSaveLocation: true,
            outerBounds: settings.outerBounds,
            minimizeOnClose: settings.isMinimizeOnCloseChecked,
            hotKeysEnabled: settings.hotkeys[0].isEnabled,
            frame: {
                systemMenu: {
                    items: [{
                        header: 'Refresh',
                        id: systemMenu.refresh,
                        enabled: true
                    }, {
                        header: 'Minimize on Close',
                        id: systemMenu.minimizeOnClose,
                        enabled: true,
                        checkable: true,
                        isChecked: settings.isMinimizeOnCloseChecked
                    }, {
                        header: 'Edit Shortcuts',
                        id: systemMenu.editShortcuts,
                        enabled: true
                    }, {
                        header: 'Exit',
                        id: systemMenu.exit,
                        enabled: true
                    }, {
                        header: 'Version ' + appVersion,
                        id: systemMenu.about,
                        enabled: false
                    }]
                },
            }
        };
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange=function() {
            if (xmlhttp.readyState==4 && xmlhttp.status==200) {
                var config = JSON.parse( xmlhttp.responseText );
                paragon.app.window.create(config.url, createParams, function(createdWindow) {
                    createdWindow.onPageLoaded.addListener(function (url) {
                        
                        if (url == "https://dev6.symphony.com/client/index.html") {
                            createdWindow.executeJavaScript('Symphony.Interop.init();');
                        }

                    });

                    paragon.notifications.setSettings(settings.notifications);

                    paragon.notifications.onSettingsChanged.addListener(function (notificationSettings) {
                        settings.notifications = notificationSettings;
                        paragon.storage.local.set({ notifications: notificationSettings });
                    });

                    createdWindow.onSystemMenuItemClicked.addListener(function (id, checked) {
                        switch (id) {
                            case systemMenu.refresh:
                                createdWindow.refresh();
                                break;
                            case systemMenu.minimizeOnClose:
                                createdWindow.setMinimizeOnClose(checked);
                                settings.isMinimizeOnCloseChecked = checked;
                                paragon.storage.local.set({ isMinimizeOnCloseChecked: checked });
                                break;
                            case systemMenu.editShortcuts:
                                symphony.settings.showEditHotKeysDialog(settings.hotkeys[0]);
                                break;
                            case systemMenu.exit:
                                createdWindow.close();
                                break;
                            case systemMenu.about:
                                break;
                            default:
                        }
                    });

                    //createdWindow.onBoundsChanged.addListener(function (bounds) {
                    //    var outerBounds = settings.outerBounds;
                    //    outerBounds.left = bounds.left;
                    //    outerBounds.top = bounds.top;
                    //    outerBounds.width = bounds.width;
                    //    outerBounds.height = bounds.height;

                    //    paragon.storage.local.set({ outerBounds: outerBounds });
                    //});

                    var hk = settings.hotkeys[0];
                    createdWindow.setHotKeys(hk.name, hk.modifier, hk.key);

                    symphony.settings.onHotKeysEdited.addListener(function (enabled, modifier, key) {
                        createdWindow.setHotKeysEnabled(enabled);
                        createdWindow.setHotKeys(hk.name, modifier, key);
                        hk.isEnabled = enabled;
                        hk.modifier = modifier;
                        hk.key = key;
                        paragon.storage.local.set({ hotkeys: settings.hotkeys });
                    });

                    createdWindow.onHotKeyPressed.addListener(function (name) {
                        console.log("hotkeyPressed:",name);
                        if (name === hk.name) {
                            symphony.app.window.showWindow("main");
                        }
                    });

                });
            }
        }
        xmlhttp.open("GET", "/config.json");
        xmlhttp.send();
    });
});

function getAppSettings() {
    return getLocalAppSettings().then(function (settings) {
        console.log("settings.migrated:", settings.migrated);
        
        if (!settings.migrated) {
            return migrateLegacySettings(settings);
        } else {
            return settings;
        }
    });
}

function getLocalAppSettings() {
    var settings = {        
        hotkeys: [{
            name: 'bringToFocus',
            isEnabled: false,
            modifier: 'Control, Shift',
            key: 'S'
        }],
        migrated: false,
        outerBounds: {
            height: 640,
            width: 768,
            minHeight: 300,
            minWidth: 560,
        },
        isMinimizeOnCloseChecked: false,
        windowState: null,
        notifications: {
            selectedMonitor: 0,
            position : 'TopRight'
        }
    };

    return getValues([
        'migrated',
        'outerBounds',
        'isMinimizeOnCloseChecked',
        'windowState',
        'hotkeys',
        'notifications'
    ]).then(function (result) {
        for (var k in result) {
            if (result.hasOwnProperty(k)) {
                settings[k] = result[k];
            }
        }
        return settings;
    });
}

function getValues(key) {
    return new Promise(function (resolve, reject) {
        console.log("Get from LocalStorage: ", key);
        paragon.storage.local.get(key, function (value, err, errMsg) {
            if (err) {
                console.error('Error loading settings from local storage.', errMsg);
                resolve({});
            } else {
                resolve(value);
            }
        });
    });
}

function migrateLegacySettings(settings) {
    return new Promise(function (resolve, reject) {
        symphony.settings.getLegacy(function (legacySettings) {
            if (legacySettings) {
                var windowPlacement = legacySettings.windowPlacement;
                settings.outerBounds = toOuterBounds(windowPlacement);
                settings.isMinimizeOnCloseChecked = legacySettings.isMinimiseOnCloseChecked;
                
                if (legacySettings.hotKeys) {
                    var hk = settings.hotkeys[0];
                    hk.modifier = legacySettings.hotKeys.modifierKeys;
                    hk.key = legacySettings.hotKeys.keys;
                    hk.isEnabled = legacySettings.hotKeys.isEnabled;
                }
            }
            settings.migrated = true;
            paragon.storage.local.set(settings, function (result, errCode, errMsg) {
                if (errCode) {
                    console.error('Failed to migrate legacy settings.', errMsg);
                }
                resolve(settings);
            });
        });
    });
}

function toOuterBounds(windowPlacement) {
    var top;
    var left;
    var height = 640;
    var width = 768;

    if (windowPlacement && windowPlacement.normalPosition) {
        var position = windowPlacement.normalPosition;

        top = position.top;
        left = position.left;
        height = position.bottom - position.top;
        width = position.right - position.left;
    }

    var bounds = {
        height: height,
        width: width,
        minHeight: 300,
        minWidth: 560,
    };
    
    if (top) {
        bounds.top = top;
    }
    
    if (left) {
        bounds.left = left;
    }

    return bounds;
}