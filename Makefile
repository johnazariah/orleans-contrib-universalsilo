source:=
target:=

source_root = proto
target_root = template-pack/templates

copy:
	mkdir -p $(target)
	tar -c --exclude bin --exclude obj --exclude .vs --exclude *-dev.csproj --exclude Properties --exclude *.user $(source) | tar -x --strip-components=2 -C $(target)
	# cp -r $(source) $(target)

load.%.templates :
	- cp -r $(source_root)/templates/$*/.template.config $(target_root)/$*

load.%.files :
	- cp $(source_root)/$*.sln $(target_root)/$*
	- cp $(source_root)/$*.Makefile $(target_root)/$*/Makefile

load.silo-and-client : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)/grains            target=$(target_root)/$*/grains copy
	$(MAKE) source=$(source_root)/grain-tests       target=$(target_root)/$*/grain-tests copy
	$(MAKE) source=$(source_root)/standalone-silo   target=$(target_root)/$*/standalone-silo copy
	$(MAKE) source=$(source_root)/standalone-client target=$(target_root)/$*/standalone-client copy
	cp $(source_root)/standalone-silo.Dockerfile   $(target_root)/$*
	cp $(source_root)/standalone-silo.Makefile     $(target_root)/$*
	cp $(source_root)/standalone-client.Dockerfile $(target_root)/$*
	cp $(source_root)/standalone-client.Makefile   $(target_root)/$*
	cp $(source_root)/$*.docker-compose.yml        $(target_root)/$*/docker-compose.yml

load.standalone-silo load.standalone-client : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)/grains      target=$(target_root)/$*/grains copy
	$(MAKE) source=$(source_root)/grain-tests target=$(target_root)/$*/grain-tests copy
	$(MAKE) source=$(source_root)/$*          target=$(target_root)/$*/$* copy
	cp $(source_root)/$*.Dockerfile $(target_root)/$*

load.webapi-directclient : load.% : load.%.templates load.%.files
	$(MAKE) source=$(source_root)/grains             target=$(target_root)/$*/grains copy
	$(MAKE) source=$(source_root)/grain-controllers  target=$(target_root)/$*/grain-controllers copy
	$(MAKE) source=$(source_root)/grain-tests        target=$(target_root)/$*/grain-tests copy
	$(MAKE) source=$(source_root)/$*                 target=$(target_root)/$*/$* copy
	- cp $(source_root)/$*.Dockerfile $(target_root)/$*

load: load.standalone-silo load.standalone-client load.silo-and-client load.webapi-directclient

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
