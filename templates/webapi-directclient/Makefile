# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

-include Setup.cfg

# project name
project   ?=Cheerio
project-lc:=$(shell echo $(project) | tr A-Z a-z)

# project configuration
config    ?=Debug

#
#

# container name
acr             ?=local
git_branch      ?=$(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash ?=$(shell git log -1 --pretty=format:"%h")
image_tag       ?=$(git_branch).$(git_latest_hash)

container_name  ?=$(acr)/$(project-lc):$(image_tag)

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
	- kubectl delete namespace $(k8s_namespace)

k8s-deploy : k8s-create-namespace k8s-upgrade
	@echo Clean-deployed to $(k8s_namespace)

k8s-upgrade : k8s-replace-image-tag k8s-apply
	@echo Upgraded $(k8s_namespace)

k8s-apply :
	kubectl apply -f local-k8s-deployment.yml

k8s-status :
	kubectl get all

k8s-dashboard : k8s-deploy-dashboard k8s-setup-rbac
	kubectl proxy

k8s-create-namespace :
	kubectl create namespace $(k8s_namespace)
	kubectl config set-context --current --namespace $(k8s_namespace)

k8s-replace-image-tag :
	sed -e "s|{image-name}|$(container_name)|g;s|t~em|tem|g" local-k8s-deployment.ymlt > local-k8s-deployment.yml

k8s-deploy-dashboard :
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml

k8s-setup-rbac :
	kubectl apply -f local-k8s-rbac.yml
	kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')

# Az CLI
az-start:
	MSYS_NO_PATHCONV=1 docker run --rm --volume $(shell pwd):/code --workdir /code --interactive --tty mcr.microsoft.com/azure-cli

# These commands are to be accessed from within the docker image started above
az-login:
	az login
	@echo Ensure you set your default subscription in 'Setup.cfg'

az-sub-set:
	az account set --subscription $(sub)

az-sub-show:
	az account show

az-show-regions:
	az account list-locations --query "sort_by([].{Location:name}, &Location)" -o table

get-random-number:
	@echo $$RANDOM

oaks-list-vars:
	@echo
	@echo Modify these values by editing 'Setup.cfg' and ensure that 'Makefile' has not overriden values unnecessarily.
	@echo
	@echo "sub                 : "[$(sub)]
	@echo "region              : "[$(region)]
	@echo "oaks_name           : "[$(oaks_name)]
	@echo "oaks_resource_group : "[$(oaks_resource_group)]
	@echo "oaks_keyvault       : "[$(oaks_keyvault)]
	@echo "oaks_acr            : "[$(oaks_acr)]
	@echo "oaks_acr_login      : "[$(oaks_acr_login)]
	@echo "acr                 : "[$(acr)]
	@echo "container_name      : "[$(container_name)]

oaks-setup: oaks-setup-rg oaks-setup-kv oaks-setup-acr
	@echo Completed setting up Orleans-on-AKS core resources

oaks-setup-rg :
	@echo Starting to set up OAKS resource group [$(oaks_resource_group)]
	- az group create --name $(oaks_resource_group) --location $(region)

oaks-setup-kv :
	@echo Starting to set up OAKS key vault [$(oaks_keyvault)]
	- az keyvault create --name $(oaks_keyvault) --resource-group $(oaks_resource_group) --location $(region)

oaks-setup-acr :
	@echo Starting to set up OAKS ACR [$(oaks_acr)]
	- az acr create --resource-group $(oaks_resource_group) --name $(oaks_acr) --sku Basic
