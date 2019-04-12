.DEFAULT_GOAL := default

ARTIFACTS 		:= $(shell pwd)/artifacts
CONFIGURATION	:= Release
CLI_PROJECT		:= Nyancat/Nyancat.csproj
CLI_TOOL		:= nyancat

purge: clean
	rm -rf .build

clean:
	rm -rf artifacts
	dotnet clean

default: clean setup
	dotnet build -c $(CONFIGURATION)
	dotnet pack $(CLI_TOOL) -c $(CONFIGURATION) --no-build -o $(ARTIFACTS)

install:
	dotnet tool install -g --add-source $(ARTIFACTS) $(CLI_TOOL)

uninstall:
	dotnet tool uninstall -g $(CLI_TOOL)

setup:
	dotnet restore
