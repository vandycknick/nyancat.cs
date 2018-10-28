# Nyancat

Nyancat running on dotnet core.

[![Build status][appveyor-ci-badge]][appveyor-ci-url]
[![NuGet][nuget-package-badge]][nuget-package-url]

![Nyancat terminal](docs/nyancat-console.png)

## Get Started

Download the [2.1.300](https://www.microsoft.com/net/download/windows) .NET Core SDK or newer.
Once installed, running the following  to install the application:

```sh
dotnet tool install --global nyancat
```

Or use the following when upgrading from a previous version:

```sh
dotnet tool update --global nyancat
```

When everything is installed just type `nyancat` in your favorite terminal. Have fun! 🎉

## Usage

```sh
Usage: nyancat [options]

Options:
  --version             Show version information
  -i|--intro            Show the introduction / about information at startup
  -n|--no-counter       Do not display the timer
  -s|--no-title         Do not set the titlebar text
  -f|--frames <FRAMES>  Display the requested number of frames, then quit
  -?|-h|--help          Show help information
```

[appveyor-ci-url]: https://ci.appveyor.com/project/nickvandyck/nyancat-cs/branch/master
[appveyor-ci-badge]: https://img.shields.io/appveyor/ci/nickvandyck/nyancat-cs/master.svg?label=appveyor&style=flat-square

[nuget-package-url]: https://www.nuget.org/packages/nyancat/
[nuget-package-badge]: https://img.shields.io/nuget/v/nyancat.svg?style=flat-square&label=nuget
