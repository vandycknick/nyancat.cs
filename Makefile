.PHONY: purge clean default install uninstall setup
.DEFAULT_GOAL := default

ARTIFACTS 		:= $(shell pwd)/artifacts
CONFIGURATION	:= Release
CLI_PROJECT		:= Nyancat/Nyancat.csproj
CLI_TOOL		:= nyancat

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

package:
	dotnet build $(CLI_PROJECT) -c $(CONFIGURATION)
	dotnet pack $(CLI_PROJECT) --configuration $(CONFIGURATION) \
		--no-build \
		--output $(ARTIFACTS) \
		--include-symbols

package-public:
	dotnet build $(CLI_PROJECT) -c $(CONFIGURATION) /p:PublicRelease=true
	dotnet pack $(CLI_PROJECT) --configuration $(CONFIGURATION) \
		--no-build \
		--output $(ARTIFACTS) \
		--include-symbols \
		/p:PublicRelease=true

install:
	dotnet tool install --global --add-source $(ARTIFACTS) \
		--version $$(nbgv get-version -v NuGetPackageVersion) \
		$(CLI_TOOL)

uninstall:
	dotnet tool uninstall --global $(CLI_TOOL)
