.PHONY: purge clean default install uninstall setup
.DEFAULT_GOAL := default

ARTIFACTS 		:= $(shell pwd)/artifacts
CONFIGURATION	:= Release
CLI_PROJECT		:= Nyancat/Nyancat.csproj
CLI_TOOL		:= nyancat

VERSION			= $(shell xmllint --xpath "/Project/PropertyGroup/VersionPrefix/text()" Version.props)
VERSION_SUFFIX	:= -build.

purge: clean
	rm -rf .build

clean:
	rm -rf artifacts
	dotnet clean

default: clean setup
	$(MAKE) package

package:
	dotnet build $(CLI_PROJECT) -c $(CONFIGURATION) \
		-p:BuildNumber=$(shell git rev-list --count HEAD) \
		-p:SourceRevisionId=$(shell git rev-parse HEAD)
	dotnet pack $(CLI_TOOL) --configuration $(CONFIGURATION) \
		--no-build \
		--output $(ARTIFACTS) \
		-p:PackAsTool=true \
		-p:BuildNumber=$(shell git rev-list --count HEAD) \
		-p:SourceRevisionId=$(shell git rev-parse HEAD)

install:
	dotnet tool install --global --add-source $(ARTIFACTS) \
		--version $(VERSION)$(VERSION_SUFFIX)* \
		$(CLI_TOOL)

uninstall:
	dotnet tool uninstall --global $(CLI_TOOL)

setup:
	dotnet restore
