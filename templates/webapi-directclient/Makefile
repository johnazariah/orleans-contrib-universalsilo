# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`


# project name
project:=Template
project-lc:=$(shell echo $(project) | tr A-Z a-z)

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

# k8s namespace
k8s_namespace=green

# Initialize
init : git-init
	git status

git-init :
	git init
	git add .
	git commit -m "Initial commit of Template"

# .NET commands
dotnet-publish :
	dotnet publish --no-build $(project)/$(project)._PROJ_SUFFIX_ -c $(config) -o out/$(project)
	@echo Built DotNet projects

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

dotnet-run :
	powershell Start-Process 'out/$(project)/$(project).exe' -WorkingDirectory 'out/$(project)'
	@echo Launched DotNet projects

# Docker commands
docker-build :
	docker build . --rm --build-arg config=$(config) --file Dockerfile --tag $(container_name)
	@echo Built and tagged images

docker-push :
	docker push $(container_name)
	@echo Pushed images to container registry

docker-run :
	powershell Start-Process powershell -ArgumentList \
	'docker','run','--rm',\
	'-p','30000:30000',\
	'-p','11111:11111',\
	'-p','5000:80',\
	'-p','8080:8080',\
	'$(container_name)'

docker-run-hostlocal :
	powershell Start-Process powershell -ArgumentList \
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

# Local Kubernetes Commands
k8s-cleanup :
	kubectl delete namespace $(k8s_namespace)

k8s-deploy : k8s-create-namespace k8s-replace-image-tag
	kubectl apply -f local-k8s-deployment.yml

k8s-dashboard : k8s-deploy-dashboard k8s-setup-rbac
	kubectl proxy

k8s-create-namespace :
	kubectl create namespace $(k8s_namespace)
	kubectl config set-context --current --namespace $(k8s_namespace)

k8s-replace-image-tag :
	sed -e "s|{image-name}|$(container_name)|g" local-k8s-deployment-template.yml > local-k8s-deployment.yml

k8s-deploy-dashboard : 
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml

k8s-setup-rbac : 
	kubectl apply -f local-k8s-rbac.yml
	kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')
