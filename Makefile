LibraryVersion:=0.7.0
NugetApiKey:=

source:=
target:=

suffix:=

proto_root:=proto
templates_root:=templates

target_root:=scratch/build
output_root:=$(target_root)/templates

scratch_root:=scratch/test
scratch_proj:=Cornflake

ifeq ($(suffix),csharp)

projsuffix=csproj
language=C\#
language_name=CSharp

else

projsuffix=fsproj
language=F\#
language_name=FSharp

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
	- rm -rf $(target_root) $(scratch_root)

clean-packages :
	- rm *.nupkg

build :
	dotnet restore            orleans-template-dev.sln
	dotnet build --no-restore orleans-template-dev.sln
	dotnet test  --no-build   orleans-template-dev.sln
	@echo Done Building Projects

templates-all : clean-templates pack-template-pack install-template-pack test-projects

pack-template-pack : copy-template-pack
	$(MAKE) project_path=$(target_root)/Orleans.Contrib.UniversalSilo.Templates.csproj package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) pack

uninstall-template-pack :
	dotnet new -u Orleans.Contrib.UniversalSilo.Templates

install-template-pack : uninstall-template-pack
	dotnet new -i Orleans.Contrib.UniversalSilo.Templates.$(LibraryVersion).nupkg

copy-template-pack : copy-template-pack.csharp copy-template-pack.fsharp
	cp $(templates_root)/Orleans.Contrib.UniversalSilo.Templates.csproj $(target_root)
	@echo Copied Template Pack
	@echo

copy-template-pack.% :
	@echo
	$(MAKE) suffix=$* copy-template.webapi-directclient copy-template.standalone-silo copy-template.standalone-client copy-template.silo-and-client
	@echo

copy-template.standalone-silo copy-template.standalone-client : copy-template.% : copy-common.% copy-project.%
	@echo Built Template Folder For $* [$(suffix)]
	- rm $(output_root)/$*-$(suffix)/Template/$*.$(projsuffix)
	@echo

copy-template.webapi-directclient : copy-template.% : copy-common.% copy-grain-controllers.% copy-project.%
	@echo Built Template Folder For $* [$(suffix)]
	- rm $(output_root)/$*-$(suffix)/Template/$*.$(projsuffix)
	@echo

copy-template.silo-and-client : copy-template.% : copy-common.%
	$(MAKE) source=$(proto_root)-$(suffix)/standalone-silo   target=$(output_root)/$*-$(suffix)/Template.Silo copy
	$(MAKE) source=$(proto_root)-$(suffix)/standalone-client target=$(output_root)/$*-$(suffix)/Template.Client copy
	$(MAKE) src_project_file=standalone-silo/standalone-silo.$(projsuffix)     dest_project_file=$*-$(suffix)/Template.Silo/Template.Silo.$(projsuffix)     replace-project-reference-with-nuget-reference
	$(MAKE) src_project_file=standalone-client/standalone-client.$(projsuffix) dest_project_file=$*-$(suffix)/Template.Client/Template.Client.$(projsuffix) replace-project-reference-with-nuget-reference
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Silo.Makefile    template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Client.Makefile  template=$* replace-pattern

copy-common.% : copy-grains.% copy-grain-tests.% copy-templates.% copy-ignores.%
	@echo Copied Common Components For $* [$(suffix)]
	@echo

copy-grains.% :
	@echo Copying Grains Project For $* [$(suffix)]
	$(MAKE) source=$(proto_root)-$(suffix)/grains target=$(output_root)/$*-$(suffix)/grains copy
	@echo

copy-grain-tests.% :
	@echo Copying Grain Tests Project For $* [$(suffix)]
	$(MAKE) source=$(proto_root)-$(suffix)/grain-tests target=$(output_root)/$*-$(suffix)/grain-tests copy
	$(MAKE) src_project_file=grain-tests/grain-tests.$(projsuffix) dest_project_file=$*-$(suffix)/grain-tests/grain-tests.$(projsuffix) replace-project-reference-with-nuget-reference
	@echo

copy-templates.% :
	@echo Copying Templates For $* [$(suffix)]
	cp -rvT $(templates_root)/$*/ $(output_root)/$*-$(suffix)
	@echo
	@echo Fixing up Language Specific Suffixes
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Makefile     template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.sln template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Dockerfile   template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=tye.yaml     template=$* replace-pattern

	$(MAKE) replace_pattern=_LANGNAME_ replacement_pattern=$(language_name) replace_in_file=.template.config/template.json template=$* replace-pattern
	$(MAKE) replace_pattern=_LANG_     replacement_pattern=$(language)      replace_in_file=.template.config/template.json template=$* replace-pattern
	@echo

copy-grain-controllers.% :
	@echo Copying Grain Controllers Project For $* [$(suffix)]
	$(MAKE) source=$(proto_root)-$(suffix)/grain-controllers target=$(output_root)/$*-$(suffix)/grain-controllers copy
	@echo

copy-project.% :
	@echo Copying Project For $* [$(suffix)]
	$(MAKE) source=$(proto_root)-$(suffix)/$* target=$(output_root)/$*-$(suffix)/Template copy
	$(MAKE) src_project_file=$*/$*.$(projsuffix) dest_project_file=$*-$(suffix)/Template/Template.$(projsuffix) replace-project-reference-with-nuget-reference
	@echo

copy-ignores.% :
	@echo Copying .gitignore and .dockerignore For $* [$(suffix)]
	- cp .gitignore    $(output_root)/$*-$(suffix)/.gitignore
	- cp .dockerignore $(output_root)/$*-$(suffix)/.dockerignore
	@echo

copy:
	mkdir -p $(target)
	# Support BSDTAR which is now native on Windows 10, and which is preferred on Windows even though it is unable to do piping! :-/
	# http://gnuwin32.sourceforge.net/packages/gtar.htm
	tar -c --exclude bin --exclude obj --exclude .vs --exclude Properties --exclude *.user -f $(target).tar $(source)
	tar -x --strip-components=2 -C $(target) -f $(target).tar
	- rm -f $(target).tar

replace-pattern :
ifneq ("$(wildcard $(output_root)/$(template)-$(suffix)/$(replace_in_file))", "")
	sed -e "s/$(replace_pattern)/$(replacement_pattern)/g" $(output_root)/$(template)-$(suffix)/$(replace_in_file) > $(output_root)/$(template)-$(suffix)/$(replace_in_file).tmp
	mv $(output_root)/$(template)-$(suffix)/$(replace_in_file).tmp $(output_root)/$(template)-$(suffix)/$(replace_in_file)
endif

replace-project-reference-with-nuget-reference :
	- sed -e "s/<ProjectReference.*universal-silo.fsproj\"/<PackageReference Include=\"Orleans.Contrib.UniversalSilo\" Version=\"$(LibraryVersion)\"/g" $(proto_root)-$(suffix)/$(src_project_file) > $(output_root)/$(dest_project_file)

test-projects : test-projects.csharp test-projects.fsharp
	@echo Created Projects

test-projects.% :
	- $(MAKE) suffix=$* test-project.webapi test-project.silo test-project.client test-project.silo-and-client

test-project.% : create-scratch-project.%
	$(MAKE) -C $(scratch_root)/$(suffix)/$*/ init dotnet-build dotnet-test
	@echo Tested Project $* [$(suffix)]

create-scratch-project.% :
	mkdir -p $(scratch_root)
	dotnet new orleans-$* -lang $(language) -n $(scratch_proj) -o $(scratch_root)/$(suffix)/$*

cleanup-proj.%:
	- rm -rf $(scratch_root)/$(suffix)/$*

pack-library :
	$(MAKE) project_path=universal-silo/universal-silo.fsproj package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) pack

push-template-pack : pack-template-pack
	$(MAKE)  package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) push

push-library : pack-library
	$(MAKE) package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) push

pack :
	dotnet build -c Release $(project_path)
	dotnet pack  --no-build -c Release $(project_path) -p:PackageId=$(package_name) -p:PackageVersion=$(package_version) -o .
	@echo Built and Packed Library

push :
	dotnet nuget push ./$(package_name).$(package_version).nupkg -s https://api.nuget.org/v3/index.json -k $(NugetApiKey)
	@echo Pushed Library to Nuget
