# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

SHELL = CMD

lc = $(subst A,a,$(subst B,b,$(subst C,c,$(subst D,d,$(subst E,e,$(subst F,f,$(subst G,g,$(subst H,h,$(subst I,i,$(subst J,j,$(subst K,k,$(subst L,l,$(subst M,m,$(subst N,n,$(subst O,o,$(subst P,p,$(subst Q,q,$(subst R,r,$(subst S,s,$(subst T,t,$(subst U,u,$(subst V,v,$(subst W,w,$(subst X,x,$(subst Y,y,$(subst Z,z,$1))))))))))))))))))))))))))

# project name
project:=webapi-directclient
project-lc:=$(call lc,$(project))

# project configuration
config:=Debug

#
#

# container name
acr=local
git_branch = $(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash = $(shell git log -1 --pretty=format:"%h")
image_tag = $(git_branch).$(git_latest_hash)

container_name:= $(acr)/$(project-lc):$(image_tag)

# .NET commands
dotnet-clean:
	- rm -rf out/$(project)
	dotnet clean $(project)/$(project).fsproj

dotnet-build :
	- rm -rf out/$(project)
	dotnet publish $(project)/$(project).fsproj -c $(config) -o out/$(project)
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
	'-p','30000:30000',\
	'-p','11111:11111',\
	'-p','5000:80',\
	'-p','8080:8080',\
	'$(container_name)'

docker-run-hostlocal :
	powershell Start-Process cmd -ArgumentList '/k',\
	'docker','run','--rm',\
	'-e','ENV_CLUSTER_MODE=HostLocal',\
	'-p','30000:30000',\
	'-p','11111:11111',\
	'-p','5000:80',\
	'-p','8080:8080',\
	'$(container_name)'

docker-image-explore :
	@echo Showing the insides of the latest container
	docker run -it --entrypoint sh $(container_name)

docker-show:
	$(eval container_ident   := $(shell docker ps | awk '$$2 ~ "$(container_name)" {print $$1}'))
	$(eval container_address := $(shell docker container inspect $(container_ident) --format "{{.NetworkSettings.IPAddress}}"))
	@echo Running the docker image at $(container_ident) @ $(container_address)

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
