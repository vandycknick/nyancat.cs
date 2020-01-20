# Nyancat

Nyancat running on dotnet core.

[![Build status][ci-badge]][ci-url]
[![NuGet][nuget-package-badge]][nuget-package-url]
[![feedz.io][feedz-package-badge]][feedz-package-url]

![Nyancat terminal](docs/nyancat-console.png)

## Get Started

Download the latest [.NET Core SDK](https://dotnet.microsoft.com/download) for you platform.
Once installed, run the following command to install:

```sh
dotnet tool install --global nyancat
```

Or use the following to upgrade to the latest version:

```sh
dotnet tool update --global nyancat
```

When everything is installed just type `nyancat` in your favourite terminal and have fun! 🎉

## Usage

```sh
Usage: nyancat [options]

Options:
  --version             Show version information
  -i|--intro            Show the introduction / about information at startup
  -n|--no-counter       Do not display the timer
  -s|--no-title         Do not set the title bar text
  -f|--frames <FRAMES>  Display the requested number of frames, then quit
  -?|-h|--help          Show help information
```

## Local installation

Run the following commands to do a release build and install the newly build assembly into your path:

```sh
make
make install
```

[ci-url]: https://github.com/nickvdyck/nyancat.cs
[ci-badge]: https://github.com/nickvdyck/nyancat.cs/workflows/CI/badge.svg

[nuget-package-url]: https://www.nuget.org/packages/nyancat/
[nuget-package-badge]: https://img.shields.io/nuget/v/nyancat.svg?style=flat-square&label=nuget

[feedz-package-url]: https://f.feedz.io/nvd/nyancat-cs/packages/nyancat/latest/download
[feedz-package-badge]: https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fnvd%2Fnyancat-cs%2Fshield%2Fnyancat%2Flatest&label=nyancat
