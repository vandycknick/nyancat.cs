# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [v1.1.0] - 2019-02-28
### Added
- Detect color support and fallback gracefully
- Added TrueColor support
- Synchronise frames between renders for better performance

### Fixed
- Gameloop freezes for one frame e436f921828a3af15cd202855ffdc39ade5d6b76
- Use correct reset ansi codes on exiting application on unix systems

## Changed
- Improved linux console driver. No dependencies on external library

## [v1.0.0] - 2018-07-19
### Added
- Ability to switch between scenes
- Added introduction scene

### Changed
- Updated README to show all command line arguments

### Fixed
- Rainbow tail not rendering correct

## [v0.5.0]
Minor improvements and bug fixes
 - Fix NoTitle commandline flag not working
 - Code cleanup and consistency

## [v0.4.0]
New features and improvements
 - Listen to console resize events on windows and unix systems
 - Rewrite implemententation and start using HostBuilder
 - Fix rainbow tail

## [v0.3.0]
Minor improvements and bug fixes
 - Added counter to the bottom of the screen
 - Added frame0 to frames array 🤦‍♂️
 - Trying out tiered compilation

## [v0.2.0]
Minor improvements
 - Improve README and give more guidance around usage
 - Add option to reduce the amount of frames displayed
 - Add option to not change the console title
 - Added version and basic help instructions `--version` and `--help`

## [v0.1.0]
Initial release
 - Initial port of nyancat as a dotnet core command line application
