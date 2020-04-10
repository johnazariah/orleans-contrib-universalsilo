source:=
target:=

suffix:=

source_root:=proto
target_root:=test/templates

copy:
	mkdir -p $(target)
	tar -c --exclude bin --exclude obj --exclude .vs --exclude *-dev.csproj --exclude Properties --exclude *.user $(source) | tar -x --strip-components=2 -C $(target)
	# cp -r $(source) $(target)

load.%.templates :
	- cp -r $(source_root)-$(suffix)/templates/$*/.template.config $(target_root)/$*-$(suffix)

load.%.files :
	- cp $(source_root)-$(suffix)/$*.sln $(target_root)/$*-$(suffix)
	- cp $(source_root)-$(suffix)/$*.Makefile $(target_root)/$*-$(suffix)/Makefile

load.silo-and-client : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)-$(suffix)/grains            target=$(target_root)/$*-$(suffix)/grains copy
	$(MAKE) source=$(source_root)-$(suffix)/grain-tests       target=$(target_root)/$*-$(suffix)/grain-tests copy
	$(MAKE) source=$(source_root)-$(suffix)/standalone-silo   target=$(target_root)/$*-$(suffix)/standalone-silo copy
	$(MAKE) source=$(source_root)-$(suffix)/standalone-client target=$(target_root)/$*-$(suffix)/standalone-client copy
	cp $(source_root)-$(suffix)/standalone-silo.Dockerfile   $(target_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-silo.Makefile     $(target_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Dockerfile $(target_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/standalone-client.Makefile   $(target_root)/$*-$(suffix)
	cp $(source_root)-$(suffix)/$*.docker-compose.yml        $(target_root)/$*-$(suffix)/docker-compose.yml

load.standalone-silo load.standalone-client : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)-$(suffix)/grains      target=$(target_root)/$*-$(suffix)/grains copy
	$(MAKE) source=$(source_root)-$(suffix)/grain-tests target=$(target_root)/$*-$(suffix)/grain-tests copy
	$(MAKE) source=$(source_root)-$(suffix)/$*          target=$(target_root)/$*-$(suffix)/$* copy
	cp $(source_root)-$(suffix)/$*.Dockerfile $(target_root)/$*-$(suffix)

load.webapi-directclient : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)-$(suffix)/grains             target=$(target_root)/$*-$(suffix)/grains copy
	$(MAKE) source=$(source_root)-$(suffix)/grain-controllers  target=$(target_root)/$*-$(suffix)/grain-controllers copy
	$(MAKE) source=$(source_root)-$(suffix)/grain-tests        target=$(target_root)/$*-$(suffix)/grain-tests copy
	$(MAKE) source=$(source_root)-$(suffix)/$*                 target=$(target_root)/$*-$(suffix)/$* copy
	- cp $(source_root)-$(suffix)/$*.Dockerfile $(target_root)/$*-$(suffix)

load-all.% :
	$(MAKE) suffix=$* load.standalone-silo
	$(MAKE) suffix=$* load.standalone-client
	$(MAKE) suffix=$* load.silo-and-client
	$(MAKE) suffix=$* load.webapi-directclient

load: load-all.csharp load-all.fsharp
	@echo running

clean.% :
	- find $(target_root)/$* -name bin -exec rm -rf {} \;
	- find $(target_root)/$* -name obj -exec rm -rf {} \;
	- find $(target_root)/$* -name out -exec rm -rf {} \;
	- find $(target_root)/$* -name .vs -exec rm -rf {} \;
	- find $(target_root)/$* -name Properties -exec rm -rf {} \;
	- find $(target_root)/$* -name *.user -exec rm -rf {} \;
	- find $(target_root)/$* -name Makefile -exec rm -rf {} \;
	- find $(target_root)/$* -name $*.Dockerfile -exec rm -rf {} \;
	- find $(target_root)/$* -name $*.sln -exec rm -rf {} \;

clean: clean.standalone-silo clean.standalone-client clean.silo-and-client clean.webapi-directclient
	@echo Cleaned Template Directories

clean-dev :
	- find . -type d -name bin -exec rm -rf {} \;
	- find . -type d -name obj -exec rm -rf {} \;