# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

# project name
project:=Template
project-lc:=$(shell echo $(project) | tr A-Z a-z)
silo=Template.Silo
client=Template.Client

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

# Initialize
init :
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

# .NET commands
dotnet-test :
	dotnet test --no-build $(project).sln -c $(config)
	@echo Built DotNet projects

dotnet-build : dotnet-restore
	dotnet build --no-restore $(project).sln -c $(config)
	@echo Built DotNet projects

dotnet-restore :
	dotnet restore $(project).sln
	@echo Built DotNet projects

dotnet-clean:
	- rm -rf out/$(project)
	dotnet clean $(project).sln

dotnet-publish dotnet-run:
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

# Docker commands
docker-build docker-push docker-run docker-image-explore docker-show:
	$(MAKE) -f $(silo).Makefile   $@
	$(MAKE) -f $(client).Makefile $@

docker-start-silo:
	$(MAKE) -f $(silo).Makefile docker-run-hostlocal

docker-start-client:
	$(eval silo_ident   := $(shell docker ps | awk '$$2 ~ /$(silo):$(image_tag)/ {print $$1}'))
	$(eval silo_address := $(shell docker container inspect $(silo_ident) --format "{{.NetworkSettings.IPAddress}}"))
	$(MAKE) -f $(client).Makefile silo_address=$(silo_address) docker-run-hostlocal

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
