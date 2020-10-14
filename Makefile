LibraryVersion:=0.7.2
NugetApiKey:=

source:=
target:=

lang:=

proto_root:=proto
templates_root:=templates

scratch:=scratch
copy_target_root:=$(scratch)/build/templates

test_install_root:=$(scratch)/test
scratch_proj:=Cornflake

ifeq ($(lang),csharp)
projsuffix=csproj
language=C\#
language_name=CSharp
else
projsuffix=fsproj
language=F\#
language_name=FSharp
endif

template-types:=webapi-directclient standalone-silo standalone-client silo-and-client
project-types:=webapi silo client silo-and-client
languages:=csharp fsharp

# Docker Targets
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

# Utility Targets
copy:
	mkdir -p $(target)
# Support BSDTAR which is now native on Windows 10, and which is preferred on Windows even though it is unable to do piping! :-/
# http://gnuwin32.sourceforge.net/packages/gtar.htm
	tar -c --exclude bin --exclude obj --exclude .vs --exclude Properties --exclude *.user -f $(target).tar $(source)
	tar -x --strip-components=2 -C $(target) -f $(target).tar
	- rm -f $(target).tar

replace-pattern :
ifneq ("$(wildcard $(copy_target_root)/$(template)-$(lang)/$(replace_in_file))", "")
	sed -e "s/$(replace_pattern)/$(replacement_pattern)/g" $(copy_target_root)/$(template)-$(lang)/$(replace_in_file) > $(copy_target_root)/$(template)-$(lang)/$(replace_in_file).tmp
	mv $(copy_target_root)/$(template)-$(lang)/$(replace_in_file).tmp $(copy_target_root)/$(template)-$(lang)/$(replace_in_file)
endif

pack :
	dotnet build -c Release $(project_path)
	dotnet pack  --no-build -c Release $(project_path) -p:PackageId=$(package_name) -p:PackageVersion=$(package_version) -o .
	@echo Built and Packed Library
	@echo

push :
	dotnet nuget push ./$(package_name).$(package_version).nupkg -s https://api.nuget.org/v3/index.json -k $(NugetApiKey)
	@echo Pushed Library to Nuget
	@echo

# Main Targets
setup-git-user :
	git config user.name  server
	git config user.email server@server.com
	@echo Done Setting Up Git User
	@echo

all : clean build setup-templates test-templates

clean : clean-packages clean-template-pack
	@echo Done cleaning
	@echo

build : dn-clean-project dn-restore-project dn-build-project dn-test-project
	@echo Done Building Projects
	@echo

dn-clean-project :
	- dotnet clean orleans-template-dev.sln

dn-restore-project :
	dotnet restore --force orleans-template-dev.sln

dn-build-project :
	dotnet build --no-restore orleans-template-dev.sln

dn-test-project :
	dotnet test --no-build orleans-template-dev.sln

setup-templates : clean-template-pack copy-template-pack pack-template-pack install-template-pack
	@echo Done Setting Up Templates
	@echo

# Library Targets
push-library : pack-library
	$(MAKE) package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) push

pack-library :
	$(MAKE) project_path=universal-silo/universal-silo.fsproj package_name=Orleans.Contrib.UniversalSilo package_version=$(LibraryVersion) pack

# Clean Targets
clean-packages :
	- rm *.nupkg

clean-template-pack :
	- rm -rf $(scratch)

# Template Targets
push-template-pack : clean-template-pack copy-template-pack pack-template-pack
	$(MAKE) package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) push

# Template Copy Targets
copy-template-pack : $(foreach l,$(languages),$(foreach t,$(template-types),copy-template.$(t).$(l)))
	cp $(templates_root)/Orleans.Contrib.UniversalSilo.Templates.csproj $(scratch)/build
	@echo Copied Template Pack
	@echo

copy-template.% : lang=$(subst .,,$(suffix $*))
copy-template.% : template=$(basename $*)
copy-template.% :
	$(MAKE) lang=$(lang) copy-single-template.$(template)

copy-single-template.standalone-silo copy-single-template.standalone-client : copy-single-template.% : copy-common.% copy-project.%
	@echo Built Template Folder For $* [$(lang)]
	- rm $(copy_target_root)/$*-$(lang)/Template/$*.$(projsuffix)
	@echo

copy-single-template.webapi-directclient : copy-single-template.% : copy-common.% copy-grain-controllers.% copy-project.%
	@echo Built Template Folder For $* [$(lang)]
	- rm $(copy_target_root)/$*-$(lang)/Template/$*.$(projsuffix)
	@echo

copy-single-template.silo-and-client : copy-single-template.% : copy-common.%
	$(MAKE) source=$(proto_root)-$(lang)/standalone-silo   target=$(copy_target_root)/$*-$(lang)/Template.Silo copy
	$(MAKE) source=$(proto_root)-$(lang)/standalone-client target=$(copy_target_root)/$*-$(lang)/Template.Client copy
	$(MAKE) src_project_file=standalone-silo/standalone-silo.$(projsuffix)     dest_project_file=$*-$(lang)/Template.Silo/Template.Silo.$(projsuffix)     replace-project-reference-with-nuget-reference
	$(MAKE) src_project_file=standalone-client/standalone-client.$(projsuffix) dest_project_file=$*-$(lang)/Template.Client/Template.Client.$(projsuffix) replace-project-reference-with-nuget-reference
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Silo.Makefile      template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Client.Makefile    template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Silo.Dockerfile    template=$* replace-pattern
	$(MAKE) replace_pattern=_PROJ_SUFFIX_ replacement_pattern=$(projsuffix) replace_in_file=Template.Client.Dockerfile  template=$* replace-pattern

copy-common.% : copy-grains.% copy-grain-tests.% copy-templates.% copy-ignores.%
	@echo Copied Common Components For $* [$(lang)]
	@echo

copy-grains.% :
	@echo Copying Grains Project For $* [$(lang)]
	$(MAKE) source=$(proto_root)-$(lang)/grains target=$(copy_target_root)/$*-$(lang)/grains copy
	@echo

copy-grain-tests.% :
	@echo Copying Grain Tests Project For $* [$(lang)]
	$(MAKE) source=$(proto_root)-$(lang)/grain-tests target=$(copy_target_root)/$*-$(lang)/grain-tests copy
	$(MAKE) src_project_file=grain-tests/grain-tests.$(projsuffix) dest_project_file=$*-$(lang)/grain-tests/grain-tests.$(projsuffix) replace-project-reference-with-nuget-reference
	@echo

copy-templates.% :
	@echo Copying Templates For $* [$(lang)]
	cp -rvT $(templates_root)/$*/ $(copy_target_root)/$*-$(lang)
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
	@echo Copying Grain Controllers Project For $* [$(lang)]
	$(MAKE) source=$(proto_root)-$(lang)/grain-controllers target=$(copy_target_root)/$*-$(lang)/grain-controllers copy
	@echo

copy-project.% :
	@echo Copying Project For $* [$(lang)]
	$(MAKE) source=$(proto_root)-$(lang)/$* target=$(copy_target_root)/$*-$(lang)/Template copy
	$(MAKE) src_project_file=$*/$*.$(projsuffix) dest_project_file=$*-$(lang)/Template/Template.$(projsuffix) replace-project-reference-with-nuget-reference
	@echo

copy-ignores.% :
	@echo Copying .gitignore and .dockerignore For $* [$(lang)]
	- cp .gitignore    $(copy_target_root)/$*-$(lang)/.gitignore
	- cp .dockerignore $(copy_target_root)/$*-$(lang)/.dockerignore
	@echo

replace-project-reference-with-nuget-reference :
	- sed -e "s/<ProjectReference.*universal-silo.fsproj\"/<PackageReference Include=\"Orleans.Contrib.UniversalSilo\" Version=\"$(LibraryVersion)\"/g" $(proto_root)-$(lang)/$(src_project_file) > $(copy_target_root)/$(dest_project_file)

pack-template-pack :
	$(MAKE) project_path=$(scratch)/build/Orleans.Contrib.UniversalSilo.Templates.csproj package_name=Orleans.Contrib.UniversalSilo.Templates package_version=$(LibraryVersion) pack

uninstall-template-pack :
	dotnet new -u Orleans.Contrib.UniversalSilo.Templates

install-template-pack : uninstall-template-pack
	dotnet new -i Orleans.Contrib.UniversalSilo.Templates.$(LibraryVersion).nupkg

# Template Test Targets
test-templates : $(foreach l,$(languages),$(foreach t,$(project-types),test-template.$(t).$(l)))
	@echo Completed Testing Templates
	@echo

test-template.% : lang=$(subst .,,$(suffix $*))
test-template.% : template=$(basename $*)
test-template.% :
	$(MAKE) lang=$(lang) template=$(template) test-scratch-project

test-scratch-project : create-scratch-project test-dotnet-flow test-docker-flow

test-docker-flow :
	$(MAKE) -C $(test_install_root)/$(lang)/$(template)/ docker-build

test-dotnet-flow :
	$(MAKE) -C $(test_install_root)/$(lang)/$(template)/ dotnet-secrets-init dotnet-build dotnet-test

create-scratch-project :
	mkdir -p $(test_install_root)
	dotnet new orleans-$(template) -lang $(language) -n $(scratch_proj) -o $(test_install_root)/$(lang)/$(template)
