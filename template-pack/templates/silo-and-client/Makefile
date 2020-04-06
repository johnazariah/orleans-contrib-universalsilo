# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

SHELL = CMD

# project name
silo=standalone-silo
client=standalone-client

# project configuration
config=Debug

# container registry name
acr=local

# A sensible image tag for the docker container
git_branch = $(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash = $(shell git log -1 --pretty=format:"%h")
image_tag = $(git_branch).$(git_latest_hash)

# project configuration
config=Debug

# container registry name
acr=local

# .NET commands
dotnet-build :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

dotnet-clean :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

dotnet-run :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

# Docker commands
docker-build :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

docker-push :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

docker-start-silo:
	$(MAKE) -f $(silo).Makefile docker-run

docker-start-client:
	$(eval silo_ident   := $(shell docker ps | awk '$$2 ~ /$(silo):$(image_tag)/ {print $$1}'))
	$(eval silo_address := $(shell docker container inspect $(silo_ident) --format "{{.NetworkSettings.IPAddress}}"))
	$(MAKE) -f $(client).Makefile silo_address=$(silo_address) docker-run

docker-image-explore:
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

docker-show:
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

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
