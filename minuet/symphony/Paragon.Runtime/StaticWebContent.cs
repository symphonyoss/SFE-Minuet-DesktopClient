using System.Diagnostics;

namespace Paragon.Runtime
{
    internal static class StaticWebContent
    {
        public static string GetResourceNotAccessibleErrorPage(string url)
        {
            const string html =
@"<html>
<head>
    <title>Resource Load Error</title>
    <style>
        body { background: #272b30; font-family: Arial; color: white; display: -webkit-flex; position: absolute; top: 0; bottom: 0; right: 0; left: 0; }
        .centered { margin: auto; }
    </style>
</head>
<body>
    <div class=""centered"">
        <h3>Application load error</h3>
        <div>The requested resource is not accessible</div>
        <div>
            <span style=""font-size: smaller;"">{TARGET_URI}</span>
        </div>
    </div>
</body>
</html>";

            return html.Replace("{TARGET_URI}", url);
        }

        public static string GetInvalidCertErrorPage(string url)
        {
            const string html =
@"<html>
<head>
    <title>Invalid Certificate</title>
    <style>
        body { background: #272b30; font-family: Arial; color: white; display: -webkit-flex; position: absolute; top: 0; bottom: 0; right: 0; left: 0; }
        .centered { margin: auto; }
    </style>
</head>
<body>
    <div class=""centered"">
        <h3>Application load error</h3>
        <div>The remote server has an invalid security certificate</div>
        <div>
            <span style=""font-size: smaller;"">{TARGET_URI}</span>
        </div>
    </div>
</body>
</html>";

            return html.Replace("{TARGET_URI}", url);
        }

        public static string GetLoadErrorPage(string url, string errorMessage, string logPath)
        {
            const string html =
                @"
<!DOCTYPE html>
<html>
<head>
    <title>Application not available</title>
    <style>
        body { background: #272b30; font-family: Arial; color: white; display: -webkit-flex; position: absolute; top: 0; bottom: 0; right: 0; left: 0; }
        .centered { margin: auto; }
        button { margin-top: 20px; }
        #errorMessage { max-width: 500px; }
    </style>
    <script>
        function reload() {
            window.location = '{TARGET_URI}';
        }
    </script>
</head>
<body>
    <div class=""centered"">
        <h3>Application load error</h3>
        <div id=""errorMessage"">{ERR_MESSAGE}</div>
        <div>
            <span>Additional details may be available in the log file:</span>
            <br/>
            <span style=""font-size: smaller;"">{LOG_PATH}</span>
        </div>
        <button class=""btn btn-default center-block"" onclick=""reload()"">Reload</button>
    </div>
</body>
</html>";

            return html
                .Replace("{TARGET_URI}", url)
                .Replace("{ERR_MESSAGE}", errorMessage)
                .Replace("{LOG_PATH}", logPath);
        }

        public static string GetOnLoadEndScript()
        {
            /*  
             * Provide window drag functionality by attaching event listeners to elements that
             * have a .drag class associated with them.
             * 
             * The following events are used to achieve this:
             *
             *      mousedown: add a mousemove event listener to the clicked .drag element
             *
             *      mouseup:   remove the mousemove event listener added in the mousedown listener
             *
             *      mousemove: remove the mousemove event listener added in the mousedown listener
             *                 and initiate a drag move operation.
             * 
             */

            const string script =
                @"
// Assign listeners to draggable elements.
(function () {
    var mouseMoveListener = function onMouseMove(e) {
        e.srcElement.removeEventListener('mousemove', mouseMoveListener);
        paragon.app.window.getCurrent(function (win) { win.startDrag(); });
    };

    function assignDraggableListeners() {
        var draggables = document.getElementsByClassName('draggable');
        for (var i = 0, j = draggables.length; i < j; ++i) {
            draggables[i].addEventListener('mousedown', function (e) {
                var el = e.srcElement;
                while (el) {
                    if (el.className && el.className.split(' ').indexOf('drag') > -1) {
                        e.srcElement.addEventListener('mousemove', mouseMoveListener);
                        break;
                    }

                    el = el.parentNode;
                }
            });

            draggables[i].addEventListener('mouseup', function (e) {
                e.srcElement.removeEventListener('mousemove', mouseMoveListener);
            });
        }
    }

    assignDraggableListeners();
})();";

            return script;
        }

        public static string GetOnLoadStartScript()
        {
            const string script = @"
(function() {

    var Levels = {ALL: -1, OFF: 0, CRITICAL: 1, ERROR: 3, WARNING: 7, INFORMATION: 15, VERBOSE: 31};
    paragon.log.Levels = Object.freeze(Levels);

    var log = console.log,
        debug = console.debug,
        info = console.info,
        warn = console.warn,
        error = console.error;

    console.log = function() {
        log.apply(this, arguments);
        paragon.log.info(convertArgs(arguments));
    };

    console.debug = function() {
        debug.apply(this, arguments);
        paragon.log.debug(convertArgs(arguments));
    };

    console.info = function() {
        info.apply(this, arguments);
        paragon.log.info(convertArgs(arguments));
    };

    console.warn = function() {
        warn.apply(this, arguments);
        paragon.log.warn(convertArgs(arguments));
    };

    console.error = function() {
        error.apply(this, arguments);
        paragon.log.error(convertArgs(arguments));
    };

    function convertArgs(args) {
        // Serialize Object types here. If we send them through as-is,
        // there is a significant performance impact around dealing with
        // circular references. The built-in serializer handles this better.

        for (var i = 0, j = args.length; i < j; ++i) {
            if (typeof args[i] != 'object' || args[i] == null) {
                continue;
            }

            var typeName = null;

            try {
                typeName = args[i].constructor.name;
                var arg = typeName + ': ' + JSON.stringify(args[i]);
                if (arg.length > 255) {
                    arg = arg.substring(0, 252) + '...';
                }

                args[i] = arg;
            } catch (exc) {
                if (typeName != null) {
                    args[i] = typeName +  ' (unable to serialize)';
                } else {
                    args[i] = 'Unable to serialize log argument';
                }
            }
        }

        return args;
    }
})();";
            return script;
        }
    }
}