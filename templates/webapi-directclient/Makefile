# WINDOWS
#
# We'll use some Unix commands here: install git bash with all Unix commands on the PATH
# We'll need `make`: run `choco install make`

-include Setup.cfg

# project name
project    ?=GeneratedProjectName
project-lc :=$(shell echo $(project) | tr A-Z a-z)

# project configuration
config ?=Debug

#
#

# container name
oaks_acr_login_server ?=local
git_branch            ?=$(subst /,--,$(shell git rev-parse --abbrev-ref HEAD))
git_latest_hash       ?=$(shell git log -1 --pretty=format:"%h")
image_tag             ?=$(git_branch).$(git_latest_hash)
container_name        ?=$(oaks_acr_login_server)/$(project-lc):$(image_tag)

# k8s namespace
k8s_namespace         ?=green

# Initialize
init : git-init
	git status

git-init :
	git init
	git add .
	git commit -m "Initial commit of GeneratedProjectName"

# .NET commands
dotnet-publish :
	dotnet publish --no-build $(project)/$(project)._PROJ_SUFFIX_ -c $(config) -o out/$(project)
	@echo Published DotNet projects

dotnet-test : dotnet-build
	dotnet test --no-build $(project).sln -c $(config)
	@echo Tested DotNet projects

dotnet-build : dotnet-restore
	dotnet build --no-restore $(project).sln -c $(config)
	@echo Built DotNet projects

dotnet-restore :
	dotnet restore $(project).sln
	@echo Restored DotNet projects

dotnet-clean:
	- rm -rf out/$(project)
	dotnet clean $(project).sln

dotnet-run :
	dotnet exec 'out/$(project)/$(project).dll'
	@echo Launched DotNet projects

dotnet-all: dotnet-clean dotnet-test dotnet-test dotnet-publish dotnet-run
	@echo

# Docker commands
docker-list :
	docker images

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

docker-run-local-ase :
	powershell Start-Process powershell -ArgumentList \
	'docker','run','--rm',\
	'-e','ENV_CLUSTER_MODE=Azure',\
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

# Local/Remote Kubernetes Commands
# For Local Use, just run these targets
# For Remote Use, ensure that you have deployed the OAKS and PAKS resources, switch context, and then run these targets
k8s-cleanup :
	- kubectl delete namespace $(k8s_namespace)

k8s-deploy : k8s-create-namespace k8s-upgrade
	@echo Clean-deployed to $(k8s_namespace)

k8s-upgrade : k8s-replace-image-tag k8s-apply
	@echo Upgraded $(k8s_namespace)

k8s-apply :
	kubectl apply -f k8s-deployment.yml

k8s-status :
	kubectl get all

k8s-dashboard : k8s-deploy-dashboard k8s-setup-rbac
	kubectl proxy

k8s-create-namespace :
	- kubectl create namespace $(k8s_namespace)
	kubectl config set-context --current --namespace $(k8s_namespace)

k8s-replace-image-tag :
	sed -e \
		"s|{image-name}|$(container_name)|g;\
		 s|{project-name}|$(project-lc)|g;\
		 s|{storage-secret-name}|$(paks_storage_secret)|g"\
		k8s-deployment.ymlt > k8s-deployment.yml

k8s-deploy-dashboard :
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml

k8s-setup-rbac :
	kubectl apply -f local-k8s-rbac.yml
	kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')

# Utilities
get-random-number:
	@echo $$RANDOM

list-config:
	@echo
	@echo Modify these values by editing 'Setup.cfg' and ensure that 'Makefile' has not overriden values unnecessarily.
	@echo
	@echo "sub                            : "[$(sub)]
	@echo "region                         : "[$(region)]
	@echo "org                            : "[$(org)]
	@echo "container_name                 : "[$(container_name)]
	@echo
	@echo "oaks_name                      : "[$(oaks_name)]
	@echo "oaks_resource_group            : "[$(oaks_resource_group)]
	@echo "oaks_keyvault                  : "[$(oaks_keyvault)]
	@echo "oaks_lawks                     : "[$(oaks_lawks)]
	@echo "oaks_acr                       : "[$(oaks_acr)]
	@echo "oaks_acr_login_server          : "[$(oaks_acr_login_server)]
	@echo
	@echo "paks_resource_group            : "[$(paks_resource_group)]
	@echo "paks_keyvault                  : "[$(paks_keyvault)]
	@echo "paks_storage                   : "[$(paks_storage)]
	@echo "paks_storage_connection_string : "[$(paks_storage_connection_string)]
	@echo "paks_storage_secret            : "[$(paks_storage_secret)]
	@echo "paks_cluster                   : "[$(paks_cluster)]
	@echo

# Az CLI - when you have not installed AZ CLI locally, and do not want to, you can run it from inside a docker container
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

az-new-all : az-new-org az-new-proj
	@echo

az-new-org : oaks-setup
	@echo Completed setting up new organization [$(org)]]

az-new-proj :  paks-setup paks-prepare-aks oaks-login-acr
	@echo Completed setting up new project [$(project)] in organization [$(org)]

oaks-cleanup:
	- az group delete --name $(oaks_resource_group) --yes
	@echo Completed cleaning up Orleans-on-AKS core resources

oaks-setup: oaks-setup-defaults oaks-setup-rg oaks-setup-kv oaks-setup-acr oaks-setup-lawks
	@echo Completed setting up Orleans-on-AKS core resources

oaks-setup-defaults :
	@echo Setting default subscription, location and acr
	- az account set --subscription $(sub)
	- az configure --defaults location=$(region) acr=$(oaks_acr)
	@echo

oaks-setup-rg :
	@echo Starting to set up OAKS resource group [$(oaks_resource_group)]
	- az group create\
		--name $(oaks_resource_group)\
		--location $(region)
	@echo

oaks-setup-kv :
	# @echo Starting to set up OAKS key vault [$(oaks_keyvault)]
	# - az keyvault create\
	# 	--name $(oaks_keyvault)\
	# 	--resource-group $(oaks_resource_group)\
	# 	--location $(region)\
	# 	--enabled-for-template-deployment
	@echo

oaks-setup-acr :
	@echo Starting to set up OAKS ACR [$(oaks_acr)]
	- az acr create\
		--name $(oaks_acr)\
		--resource-group $(oaks_resource_group)\
		--sku Basic
	@echo

oaks-login-acr :
	@echo Logging in to OAKS ACR [$(oaks_acr)]
	- az acr login\
		--name $(oaks_acr)\
		--expose-token\
		--query accessToken -o tsv
	@echo

oaks-setup-lawks :
	@echo Starting to setup OAKS LA workspace [$(oaks_lawks)]
	- az monitor log-analytics workspace create\
		--workspace-name $(oaks_lawks)\
		--resource-group $(oaks_resource_group)
	@echo

paks-cleanup:
	- az group delete --name $(paks_resource_group) --yes
	@echo Completed cleaning up project resources

paks-setup : paks-setup-rg paks-setup-kv paks-setup-stg paks-setup-aks
	@echo Completed setting up project resources

paks-setup-rg :
	@echo Starting to set up project resource group [$(paks_resource_group)]
	- az group create\
		--name $(paks_resource_group)\
		--location $(region)
	@echo

paks-setup-kv :
	# @echo Starting to set up project key vault [$(paks_keyvault)]
	# - az keyvault create\
	# 	--name $(paks_keyvault)\
	# 	--resource-group $(paks_resource_group)\
	# 	--location $(region)\
	# 	--enabled-for-template-deployment
	@echo

paks-setup-stg : paks-create-storage paks-get-storage-connection-string
	@echo

paks-create-storage :
	@echo Starting to set up project storage account [$(paks_storage)]
	- az storage account create\
		--name $(paks_storage)\
		--resource-group $(paks_resource_group)\
		--kind StorageV2
	@echo

paks-get-storage-connection-string :
	@echo #Modify your Setup.cfg and put this as the value of 'paks_storage_connection_string'
	az storage account show-connection-string --name $(paks_storage) --resource-group $(paks_resource_group) --query connectionString -o tsv

paks-setup-aks :
	@echo Starting to set up project AKS cluster [$(paks_cluster)]
	- az aks create\
		--name $(paks_cluster)\
		--resource-group $(paks_resource_group)\
		--location $(region)\
		--node-vm-size Standard_DS2_v2\
		--node-count 2\
		--workspace-resource-id /subscriptions/$(sub)/resourcegroups/$(oaks_resource_group)/providers/microsoft.operationalinsights/workspaces/$(oaks_lawks)\
		--attach-acr $(oaks_acr)\
		--enable-addons monitoring\
		--enable-managed-identity\
		--generate-ssh-keys
	@echo

paks-prepare-aks : paks-register-provider paks-install-cli paks-get-credentials paks-export-config

paks-register-provider :
	@echo Registering OperationsManagement and OperationalInsights
	- az provider register --namespace Microsoft.OperationsManagement
	- az provider register --namespace Microsoft.OperationalInsights
	@echo

paks-install-cli :
	- az aks install-cli

paks-get-credentials :
	- az aks get-credentials --resource-group $(paks_resource_group) --name $(paks_cluster)

paks-export-config :
	- cp /root/.kube/config .aks_kube_config

aks-switch-context :
	cp --force $$HOME/.kube/config $$HOME/.kube/config.backup
	KUBECONFIG=$$HOME/.kube/config:.aks_kube_config kubectl config view --merge --flatten > ~/.kube/merged_kubeconfig && mv ~/.kube/merged_kubeconfig ~/.kube/config
	kubectl config use-context $(paks_cluster)

aks-acr-login :
	- docker login $(oaks_acr_login_server) -u 00000000-0000-0000-0000-000000000000 -p $(oaks_acr_login_token)

aks-set-secrets : k8s-create-namespace aks-set-storage-secret
	@echo Completed setting up secrets

aks-set-storage-secret :
	@echo Starting to set up storage connection string as a secrets
	kubectl create secret generic $(paks_storage_secret)\
		--namespace $(k8s_namespace)\
		--from-literal=connection-string=$(paks_storage_connection_string)

aks-prepare : aks-acr-login aks-switch-context aks-set-secrets
	@echo Prepared context to run against remote AKS!
