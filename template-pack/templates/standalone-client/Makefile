# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

SHELL = CMD

# project name
client=standalone-client

# project configuration
config:=Debug

# container registry name
acr:=local

# container name
git_branch = $(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash = $(shell git log -1 --pretty=format:"%h")
image_tag = $(git_branch).$(git_latest_hash)
container_name = $(acr)/$(client):$(image_tag)

# silo's runtime address
silo_address:=172.17.0.2

# .NET commands
dotnet-clean dotnet-build dotnet-run:
	$(MAKE) -f $(client).Makefile project=$(client) config=$(config) $@

# Docker commands
docker-build :
	$(MAKE) -f $(client).Makefile project=$(client) config=$(config) container_name=$(container_name) $@

docker-run:
	$(MAKE) -f $(client).Makefile silo_address=$(silo_address) container_name=$(container_name) docker-run

docker-push docker-image-explore docker-show:
	$(MAKE) -f $(client).Makefile container_name=$(container_name) $@

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
