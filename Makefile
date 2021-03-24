.DEFAULT_GOAL 	:= default

ARTIFACTS 		:= $(shell pwd)/artifacts
BUILD			:= $(shell pwd)/.build
CONFIGURATION	:= Release
CLI_PROJECT		:= Nyancat/Nyancat.csproj
CLI_TOOL		:= nyancat
RUNTIME 		:= linux-x64

.PHONY: purge
purge: clean
	rm -rf .build
	rm -rf artifacts

.PHONY: clean
clean:
	dotnet clean

.PHONY: run
run:
	dotnet run --project $(CLI_PROJECT) --framework net5.0

.PHONY: restore
restore:
	dotnet restore
	dotnet tool restore

.PHONY: default
default: clean restore
	$(MAKE) package

.PHONY: package
package:
	dotnet build $(CLI_PROJECT) -c $(CONFIGURATION)
	dotnet pack $(CLI_PROJECT) --configuration $(CONFIGURATION) \
		--no-build \
		--output $(ARTIFACTS) \
		--include-symbols

.PHONY: package-native
package-native:
	# Prereqs: https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/samples/prerequisites.md#ubuntu-1604
	dotnet publish $(CLI_PROJECT) -c $(CONFIGURATION) \
		--output $(BUILD)/publish/$(RUNTIME) \
		--runtime $(RUNTIME) \
		 --framework net5.0 \
		/p:Mode=CoreRT-ReflectionFree

	@mkdir -p $(ARTIFACTS)
	@strip $(BUILD)/publish/$(RUNTIME)/$(CLI_TOOL)
	@cp $(BUILD)/publish/$(RUNTIME)/$(CLI_TOOL) $(ARTIFACTS)/$(CLI_TOOL).$(RUNTIME)

.PHONY: install
install:
	dotnet tool install --global --add-source $(ARTIFACTS) \
		--version $$(dotnet minver -t v -a minor -v e) \
		$(CLI_TOOL)

.PHONY: uninstall
uninstall:
	dotnet tool uninstall --global $(CLI_TOOL)

.PHONY: checksum
checksum:
	@rm -f $(ARTIFACTS)/SHA256SUMS.txt
	@cd $(ARTIFACTS) && find . -type f -print0 | xargs -0 sha256sum | tee SHA256SUMS.txt

.PHONY: validate
validate:
	@cd $(ARTIFACTS) && cat SHA256SUMS.txt | sha256sum -c -
