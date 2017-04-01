# Build Instructions for Minuet

[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bhttps%3A%2F%2Fgithub.com%2Fsymphonyoss%2FSFE-Minuet-DesktopClient.svg?size=small)](https://app.fossa.io/projects/git%2Bhttps%3A%2F%2Fgithub.com%2Fsymphonyoss%2FSFE-Minuet-DesktopClient?ref=badge_small)

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
## License

This project is distributed under the Apache License v2. See [LICENSE](./LICENSE) and [NOTICE](./NOTICE) for additional licensing information.
