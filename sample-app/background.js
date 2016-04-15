paragon.app.runtime.onLaunched.addListener(function () {
    paragon.app.window.create('window.html', {
        'bounds': {
            'width': 400,
            'height': 500
        }
    });
});