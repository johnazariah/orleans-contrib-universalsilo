LibraryVersion:=0.5.2

source:=
target:=

suffix:=

source_root:=proto
target_root:=test
template_root:=$(target_root)/templates

ifeq ($(suffix),csharp)
	projsuffix=csproj
	language=C\#
else
	projsuffix=fsproj
	language=F\#
endif

docker-stop :
	@echo Stopping all  containers
	- docker stop $(shell docker ps -aq)

docker-kill : docker-stop
	@echo Killing and removing all containers
	- docker rm -f $(shell docker ps -aq)
	- docker kill  $(shell docker ps -aq)

docker-clean : docker-kill
	@echo Pruning all images
	- docker image prune -af

all : clean build templates-all

clean : clean-packages clean-templates
	@echo Done cleaning

clean-templates :
	- rm -rf test scratch

clean-packages :
	- rm *.nupkg

build : build.csharp build.fsharp
	@echo Done Building Projects

build.% :
	@echo Building $* Solution
	dotnet restore            orleans-template-dev-$*.sln
	dotnet build --no-restore orleans-template-dev-$*.sln
	dotnet test  --no-build   orleans-template-dev-$*.sln

templates-all : clean-templates pack-template-pack install-template-pack test-projects

pack-template-pack : copy-template-pack
	$(MAKE) project_path=$(target_root)/Orleans.Contrib.UniversalSilo.Templates.csproj package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) pack

copy-template-pack : copy-template-pack.csharp #copy-template-pack.fsharp
	cp Orleans.Contrib.UniversalSilo.Templates.csproj $(target_root)
	@echo Copied Template Pack

copy-template-pack.% :
	$(MAKE) suffix=$* copy-template.standalone-silo copy-template.standalone-client copy-template.silo-and-client copy-template.webapi-directclient

copy-template.standalone-silo copy-template.standalone-client : copy-template.% : copy-common.% copy-project.%
	@echo Built Template Folder For $* [$(suffix)]
	@echo

copy-template.webapi-directclient : copy-template.% : copy-common.% copy-grain-controllers.% copy-project.%
	@echo Built Template Folder For $* [$(suffix)]
	@echo

copy-template.silo-and-client : copy-template.% : copy-common.%
	$(MAKE) source=$(source_root)-$(suffix)/standalone-silo   target=$(template_root)/$*-$(suffix)/standalone-silo copy
	$(MAKE) source=$(source_root)-$(suffix)/standalone-client target=$(template_root)/$*-$(suffix)/standalone-client copy
	sed -e "s/<ProjectReference.*universal-silo.fsproj/<PackageReference Version=\"$(LibraryVersion)\" Include=\"Orleans.Contrib.UniversalSilo/g" $(source_root)-$(suffix)/standalone-silo/standalone-silo-dev.$(projsuffix) > $(template_root)/$*-$(suffix)/standalone-silo/standalone-silo.$(projsuffix)
	sed -e "s/<ProjectReference.*universal-silo.fsproj/<PackageReference Version=\"$(LibraryVersion)\" Include=\"Orleans.Contrib.UniversalSilo/g" $(source_root)-$(suffix)/standalone-client/standalone-client-dev.$(projsuffix) > $(template_root)/$*-$(suffix)/standalone-client/standalone-client.$(projsuffix)
	cp $(source_root)-$(suffix)/standalone-silo.Dockerfile   $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-silo.Makefile     $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Dockerfile $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Makefile   $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/$*.docker-compose.yml        $(template_root)/$*-$(suffix)/docker-compose.yml

copy-common.% : copy-grains.% copy-grain-tests.% copy-templates.% copy-makefile.% copy-dockerfile.% copy-tyefile.% copy-solution.%
	@echo Copied Common Components For $* [$(suffix)]
	@echo

copy-grains.% :
	@echo Copying Grains Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/grains target=$(template_root)/$*-$(suffix)/grains copy
	@echo

copy-grain-tests.% :
	@echo Copying Grain Tests Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/grain-tests target=$(template_root)/$*-$(suffix)/grain-tests copy
	sed -e "s/<ProjectReference.*universal-silo.fsproj/<PackageReference Version=\"$(LibraryVersion)\" Include=\"Orleans.Contrib.UniversalSilo/g" $(source_root)-$(suffix)/grain-tests/grain-tests-dev.$(projsuffix) > $(template_root)/$*-$(suffix)/grain-tests/grain-tests.$(projsuffix)
	@echo

copy-templates.% :
	@echo Copying Templates For $* [$(suffix)]
	cp -r $(source_root)-$(suffix)/_templates/$*/.template.config $(template_root)/$*-$(suffix)
	@echo

copy-grain-controllers.% :
	@echo Copying Grain Controllers Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/grain-controllers target=$(template_root)/$*-$(suffix)/grain-controllers copy
	@echo

copy-project.% :
	@echo Copying Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/$* target=$(template_root)/$*-$(suffix)/Template copy
	sed -e "s/<ProjectReference.*universal-silo.fsproj/<PackageReference Version=\"$(LibraryVersion)\" Include=\"Orleans.Contrib.UniversalSilo/g" $(source_root)-$(suffix)/$*/$*-dev.$(projsuffix) > $(template_root)/$*-$(suffix)/Template/Template.$(projsuffix)
	@echo

copy-makefile.% :
	@echo Copying Makefile For $* [$(suffix)]
	- sed -e "s/$*/Template/g" $(source_root)-$(suffix)/$*.Makefile > $(template_root)/$*-$(suffix)/Makefile
	@echo

copy-tyefile.% :
	@echo Copying Tye File For $* [$(suffix)]
	- sed -e "s/$*/Template/g" $(source_root)-$(suffix)/$*.tye > $(template_root)/$*-$(suffix)/tye.yaml
	@echo

copy-dockerfile.% :
	@echo Copying Dockerfile For $* [$(suffix)]
	- sed -e "s/$*/Template/g" $(source_root)-$(suffix)/$*.Dockerfile > $(template_root)/$*-$(suffix)/Template.Dockerfile
	@echo

copy-solution.% :
	@echo Copying Solution For $* [$(suffix)]
	cp $(source_root)-$(suffix)/$*.sln $(template_root)/$*-$(suffix)
	mv $(template_root)/$*-$(suffix)/$*.sln $(template_root)/$*-$(suffix)/Template.sln
	@echo

copy:
	mkdir -p $(target)
	tar -c --exclude bin --exclude obj --exclude .vs --exclude *-dev.$(projsuffix) --exclude Properties --exclude *.user $(source) | tar -x --strip-components=2 -C $(target)

install-template-pack :
	dotnet new -i Orleans.Contrib.UniversalSilo.Templates.$(LibraryVersion).nupkg

test-projects : test-projects.csharp #test-projects.fsharp
	@echo Created Projects

test-projects.% :
	- $(MAKE) suffix=$* test-project.silo test-project.client test-project.silo-and-client test-project.webapi

test-project.% : create-proj.% build-proj.% test-proj.%
	@echo Tested Project $* [$(suffix)]

create-proj.% :
	- dotnet new orleans-$* -lang $(language) -n SpiffyProject -o scratch/$(suffix)/$*

build-proj.% :
	- dotnet build scratch/$(suffix)/$*/SpiffyProject.sln

test-proj.% :
	- dotnet test --no-build scratch/$(suffix)/$*/SpiffyProject.sln

cleanup-proj.%:
	- rm -rf scratch/$(suffix)/$*

pack-library :
	$(MAKE) project_path=universal-silo/universal-silo.fsproj package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) pack

push-template-pack : pack-template-pack
	$(MAKE)  package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) push

push-library : pack-library
	$(MAKE) package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) push

pack :
	dotnet build -c Release $(project_path)
	dotnet pack --no-build -c Release $(project_path) -p:PackageId=$(package_name) -p:PackageVersion=$(package_version) -o .
	@echo Built and Packed Library

push :
	dotnet nuget push ./$(package_name).$(package_version).nupkg -s https://api.nuget.org/v3/index.json -k $${{secrets.NUGET_API_KEY}}
	@echo Pushed Library to Nuget
