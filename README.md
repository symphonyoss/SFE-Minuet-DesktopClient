# Build Instructions for Minuet

## Prerequisites

* Download Express 2013 for Windows Desktop - [download VS2013 Community Edition for free](http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx)

## Get the code

```powershell
$ git clone git@github.com:SymphonyOSF/SFE-Minuet-DesktopClient.git
```

## Build Minuet

```powershell
$ powershell ./scripts/build.ps1
```

After the build is complete, you can find `paragon.exe` under `bin\paragon`.

## Build the sample

Once Minuet has been built you can build the sample from `sample-app`.

```powershell
$ powershell ./scripts/build-sample.ps1
```

## Run the sample

Once Minuet and the sample have been built you can run the app.

Note that the `/start-app` switch can not take a relative path.

```powershell
$ call "bin/paragon/paragon.exe" /start-app:"C:/full-path-to-sample-pgx/bin/apps/sample.pgx"
```

## Contributing

1. Fork it (<https://github.com/symphonyoss/SFE-Minuet-DesktopClient/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Read our [contribution guidelines](.github/CONTRIBUTING.md) and [Community Code of Conduct](https://www.finos.org/code-of-conduct)
4. Commit your changes (`git commit -am 'Add some fooBar'`)
5. Push to the branch (`git push origin feature/fooBar`)
6. Create a new Pull Request

## License

The code in this repository is distributed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0).

Copyright 2016-2019 Symphony LLC
