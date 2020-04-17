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

copy:
	mkdir -p $(target)
	tar -c --exclude bin --exclude obj --exclude .vs --exclude *-dev.$(projsuffix) --exclude Properties --exclude *.user $(source) | tar -x --strip-components=2 -C $(target)

# load.%.templates :
# 	- cp -r $(source_root)-$(suffix)/templates/$*/.template.config $(template_root)/$*-$(suffix)

# load.%.files :
# 	- cp $(source_root)-$(suffix)/$*.sln $(template_root)/$*-$(suffix)
# 	- cp $(source_root)-$(suffix)/$*.Makefile $(template_root)/$*-$(suffix)/Makefile

# load.silo-and-client : load.% : load.%.templates load.%.files
# 	$(MAKE) source=$(source_root)-$(suffix)/grains            target=$(template_root)/$*-$(suffix)/grains copy
# 	$(MAKE) source=$(source_root)-$(suffix)/grain-tests       target=$(template_root)/$*-$(suffix)/grain-tests copy
# 	$(MAKE) source=$(source_root)-$(suffix)/standalone-silo   target=$(template_root)/$*-$(suffix)/standalone-silo copy
# 	$(MAKE) source=$(source_root)-$(suffix)/standalone-client target=$(template_root)/$*-$(suffix)/standalone-client copy
# 	cp $(source_root)-$(suffix)/standalone-silo.Dockerfile   $(template_root)/$*-$(suffix)
# 	cp $(source_root)-$(suffix)/standalone-silo.Makefile     $(template_root)/$*-$(suffix)
# 	cp $(source_root)-$(suffix)/standalone-client.Dockerfile $(template_root)/$*-$(suffix)
# 	cp $(source_root)-$(suffix)/standalone-client.Makefile   $(template_root)/$*-$(suffix)
# 	cp $(source_root)-$(suffix)/$*.docker-compose.yml        $(template_root)/$*-$(suffix)/docker-compose.yml

# load.webapi-directclient : load.% : load.%.templates load.%.files
# 	$(MAKE) source=$(source_root)-$(suffix)/grains             target=$(template_root)/$*-$(suffix)/grains copy
# 	$(MAKE) source=$(source_root)-$(suffix)/grain-controllers  target=$(template_root)/$*-$(suffix)/grain-controllers copy
# 	$(MAKE) source=$(source_root)-$(suffix)/grain-tests        target=$(template_root)/$*-$(suffix)/grain-tests copy
# 	$(MAKE) source=$(source_root)-$(suffix)/$*                 target=$(template_root)/$*-$(suffix)/$* copy
# 	- cp $(source_root)-$(suffix)/$*.Dockerfile $(template_root)/$*-$(suffix)

# load-all.% :
# 	$(MAKE) suffix=$* load.standalone-silo
# 	$(MAKE) suffix=$* load.standalone-client
# 	$(MAKE) suffix=$* load.silo-and-client
# 	$(MAKE) suffix=$* load.webapi-directclient

# load: load-all.csharp load-all.fsharp
# 	@echo running

# clean.% :
# 	- find $(template_root)/$* -name bin -exec rm -rf {} \;
# 	- find $(template_root)/$* -name obj -exec rm -rf {} \;
# 	- find $(template_root)/$* -name out -exec rm -rf {} \;
# 	- find $(template_root)/$* -name .vs -exec rm -rf {} \;
# 	- find $(template_root)/$* -name Properties -exec rm -rf {} \;
# 	- find $(template_root)/$* -name *.user -exec rm -rf {} \;
# 	- find $(template_root)/$* -name Makefile -exec rm -rf {} \;
# 	- find $(template_root)/$* -name $*.Dockerfile -exec rm -rf {} \;
# 	- find $(template_root)/$* -name $*.sln -exec rm -rf {} \;

# clean: clean.standalone-silo clean.standalone-client clean.silo-and-client clean.webapi-directclient
# 	@echo Cleaned Template Directories

# clean-dev :
# 	- find . -type d -name bin -exec rm -rf {} \;
# 	- find . -type d -name obj -exec rm -rf {} \;

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

copy-templates.% :
	@echo Copying Templates For $* [$(suffix)]
	cp -r $(source_root)-$(suffix)/templates/$*/.template.config $(template_root)/$*-$(suffix)
	@echo

copy-grains.% :
	@echo Copying Grains Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/grains target=$(template_root)/$*-$(suffix)/grains copy
	@echo

copy-grain-tests.% :
	@echo Copying Grain Tests Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/grain-tests target=$(template_root)/$*-$(suffix)/grain-tests copy
	@echo

copy-project.% :
	@echo Copying Project For $* [$(suffix)]
	$(MAKE) source=$(source_root)-$(suffix)/$* target=$(template_root)/$*-$(suffix)/Template copy
	mv $(template_root)/$*-$(suffix)/Template/$*.$(projsuffix) $(template_root)/$*-$(suffix)/Template/Template.$(projsuffix)
	@echo

copy-makefile.% :
	@echo Copying Makefile For $* [$(suffix)]
	cp $(source_root)-$(suffix)/$*.Makefile   $(template_root)/$*-$(suffix)/Makefile
	@echo

copy-dockerfile.% :
	@echo Copying Dockerfile For $* [$(suffix)]
	- cp $(source_root)-$(suffix)/$*.Dockerfile $(template_root)/$*-$(suffix)/Dockerfile
	@echo

copy-solution.% :
	@echo Copying Solution For $* [$(suffix)]
	cp $(source_root)-$(suffix)/$*.sln $(template_root)/$*-$(suffix)
	mv $(template_root)/$*-$(suffix)/$*.sln $(template_root)/$*-$(suffix)/Template.sln
	@echo

copy-common.% : copy-grains.% copy-grain-tests.% copy-templates.% copy-makefile.% copy-dockerfile.% copy-solution.%
	@echo Copied Common Components For $* [$(suffix)]
	@echo

copy-template.standalone-silo copy-template.standalone-client : copy-template.% : copy-common.% copy-project.%
	@echo Built Template Folder For $* [$(suffix)]
	@echo

copy-template.silo-and-client : copy-template.% : copy-common.%
	$(MAKE) source=$(source_root)-$(suffix)/standalone-silo   target=$(template_root)/$*-$(suffix)/standalone-silo copy
	$(MAKE) source=$(source_root)-$(suffix)/standalone-client target=$(template_root)/$*-$(suffix)/standalone-client copy
	cp $(source_root)-$(suffix)/standalone-silo.Dockerfile   $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-silo.Makefile     $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Dockerfile $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Makefile   $(template_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/$*.docker-compose.yml        $(template_root)/$*-$(suffix)/docker-compose.yml

copy-template-pack.% :
	$(MAKE) suffix=$* copy-template.standalone-silo
	$(MAKE) suffix=$* copy-template.standalone-client
	$(MAKE) suffix=$* copy-template.silo-and-client
	# $(MAKE) suffix=$* copy-template.webapi-directclient

copy-template-pack : copy-template-pack.csharp copy-template-pack.fsharp
	cp Orleans.Contrib.UniversalSilo.Templates.csproj $(target_root)
	@echo Copied Template Pack

pack-template-pack : copy-template-pack
	dotnet build -c Release $(target_root)/Orleans.Contrib.UniversalSilo.Templates.csproj
	dotnet pack --no-build -c Release $(target_root)/Orleans.Contrib.UniversalSilo.Templates.csproj -p:PackageId=Orleans.Contrib.UniversalSilo.Templates -o .
	@echo Built and Packed Template Folders

push-template-pack : pack-template-pack
	dotnet nuget push ./Orleans.Contrib.UniversalSilo.Templates*.nupkg -s https://api.nuget.org/v3/index.json -k $${{secrets.NUGET_API_KEY}}

install-template-pack :
	dotnet new -i Orleans.Contrib.UniversalSilo.Templates*.nupkg

create-proj.% :
	dotnet new orleans-$* -lang $(language) -n SpiffyProject -o scratch/$*/$(suffix)

build-proj.% :
	dotnet build scratch/$*/$(suffix)/SpiffyProject

test-proj.% :
	dotnet test scratch/$*/$(suffix)/SpiffyProject

cleanup-proj.%:
	rm -rf scratch/$*/$(suffix)/SpiffyProject

test-project.% : create-proj.% build-proj.% test-proj.% cleanup-proj.%
	@echo Tested Project $* [$(suffix)]

test-projects.% :
	- $(MAKE) suffix=$* test-project.silo test-project.client test-project.silo-and-client test-project.webapi-directclient

test-projects : test-projects.fsharp test-projects.csharp
	@echo Created Projects

pack-library :
	dotnet build -c Release universal-silo/universal-silo.fsproj
	dotnet pack --no-build -c Release universal-silo/universal-silo.fsproj -p:PackageId=Orleans.Contrib.UniversalSilo -o .
	@echo Built and Packed Library

push-library : pack-library
	dotnet nuget push ./Orleans.Contrib.UniversalSilo*.nupkg -s https://api.nuget.org/v3/index.json -k $${{secrets.NUGET_API_KEY}}

build.% :
	@echo Building $* Solution
	dotnet restore orleans-template-dev-$*.sln
	dotnet build --no-restore orleans-template-dev-$*.sln

build : build.csharp  build.fsharp
	@echo Done Building Projects