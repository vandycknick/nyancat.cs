.PHONY: purge clean default install uninstall setup
.DEFAULT_GOAL := default

ARTIFACTS 		:= $(shell pwd)/artifacts
CONFIGURATION	:= Release
CLI_PROJECT		:= Nyancat/Nyancat.csproj
CLI_TOOL		:= nyancat
RUNTIME 		:= linux-x64

purge: clean
	rm -rf .build
	rm -rf artifacts

clean:
	dotnet clean

run:
	dotnet run --project $(CLI_PROJECT)

setup:
	dotnet restore

default: clean setup
	$(MAKE) package

restore:
	dotnet restore
	dotnet tool restore

package:
	dotnet build $(CLI_PROJECT) -c $(CONFIGURATION)
	dotnet pack $(CLI_PROJECT) --configuration $(CONFIGURATION) \
		--no-build \
		--output $(ARTIFACTS) \
		--include-symbols

package-native:
	dotnet publish $(CLI_PROJECT) -c $(CONFIGURATION) \
		--output $(ARTIFACTS)/$(RUNTIME) \
		--runtime $(RUNTIME) \
		/p:Mode=CoreRT-High

install:
	dotnet tool install --global --add-source $(ARTIFACTS) \
		--version $$(dotnet minver -t v -a minor -v e) \
		$(CLI_TOOL)

uninstall:
	dotnet tool uninstall --global $(CLI_TOOL)
