# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

SHELL = CMD

# project name
project:=standalone-client

# project configuration
config:=Debug

# silo's runtime address
silo_address:=172.17.0.2

# container name
acr=local
git_branch = $(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash = $(shell git log -1 --pretty=format:"%h")
image_tag = $(git_branch).$(git_latest_hash)

container_name:= $(acr)/$(project):$(image_tag)

# .NET commands
dotnet-clean:
	- rm -rf out/$(project)
	dotnet clean $(project)/$(project).csproj

dotnet-build :
	- rm -rf out/$(project)
	dotnet publish $(project)/$(project).csproj -c $(config) -o out/$(project)
	@echo Built DotNet projects

dotnet-run : dotnet-build
	powershell Start-Process cmd -ArgumentList '/k','$(project).exe' -WorkingDirectory 'out/$(project)'
	@echo Launched DotNet projects

# Docker commands
docker-build :
	docker build . --rm --build-arg config=$(config) --file $(project).Dockerfile --tag $(container_name)
	@echo Built and tagged images

docker-push :
	docker push $(container_name)
	@echo Pushed images to container registry

docker-run :
	powershell Start-Process cmd -ArgumentList '/k',\
	'docker','run','--rm',\
	'$(container_name)'

docker-run-hostlocal :
	powershell Start-Process cmd -ArgumentList '/k',\
	'docker','run','--rm',\
	'-e','ENV_CLUSTER_MODE=HostLocal',\
	'-e','silo-address=$(silo_address)',\
	'$(container_name)'

docker-image-explore :
	@echo Showing the insides of the latest container
	docker run -it --entrypoint sh $(container_name)

docker-show:
	$(eval container_ident   := $(shell docker ps | awk '$$2 ~ "$(container_name)" {print $$1}'))
	$(eval container_address := $(shell docker container inspect $(container_ident) --format "{{.NetworkSettings.IPAddress}}"))
	@echo Running the docker image at $(container_ident) @ $(container_address)
