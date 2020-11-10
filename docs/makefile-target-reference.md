# `make` Targets Reference

## General

You can run these targets from a terminal where the current directory is the project folder (where the `Makefile` resides).

You can run individual targets
- `make dotnet-clean` runs `dotnet clean` on your solution.

And you can chain targets together
- `make dotnet-clean dotnet-build dotnet-test` cleans, builds and tests your solution
- `make dotnet-publish dotnet-run` publishes and runs your application

## Multiple Target Families

The `Makefile` provided has several groups of targets, which are documented here, and in the `Makefile` itself:

- `make init` should be the first target you run _immediately after_ creating a new project.

## 1. "dotnet" commands

The following targets effectively package `dotnet` commands for ease of use. You can also use the traditional `dotnet` commands on the solution and various projects if you wish to do something not packaged in the `Makefile`.

- `make dotnet-clean` - runs `dotnet clean` on your solution
- `make dotnet-build` - runs a build on your solution, automatically invoking the restore
- `make dotnet-test`  - runs all tests in your solution *without first building*
- `make dotnet-publish` - publishes your compiled application the `out` directory *without first building*
- `make dotnet-run` - runs your application in a separate shell window from the `out` directory
- `make dotnet-restore` - runs `dotnet restore` on your solution. Invoked from other targets.

## 2. "docker" commands

In this stage, we will containerize the application so we can use **Docker**.

The following targets effectively package `docker` commands for ease of use. You can also use the traditional `docker` commands on the solution and various projects if you wish to do something not packaged in the `Makefile`.

- `make docker-build` - builds the app into a docker image and tags it with the commit hash of the last commit in the active branch.
- `make docker-push` - pushes the docker image to the specified container registry. Ensure that you have set up the remote Azure Container Registry and logged into it by following the steps outlined in [the Workflow document (Stage 5)](development-workflow.md).
- `make docker-run` - runs the docker image on the local machine. The default `Dockerfile` is set to run a single-silo cluster with `HostLocal` clustering
- `make docker-run-local-ase` - is an example of how to use the `-e ENV_xxx=yyy` syntax to run the single-silo cluster with Azure Storage Emulator running on the host. Make sure you start Azure Storage Emulator on the host before running this target.
- `make docker-image-explore` - is a useful target to ensure that the folder structure of the image is as you expect. This target gives you a shell script in the latest built image.
- `make docker-show` - will get the runtime IP address of a running container of the latest built image. Use this to get the IP address of a silo container before bringing up a client to talk to it.

The `Makefile` also has some powerful targets targets to help you manage Docker itself. Use these with great care.

**THESE TARGETS AFFECT ALL CONTAINERS AND IMAGES ON YOUR SYSTEM**

**BE CAREFUL WITH THEM**
- `make docker-stop` stops _all_ running containers
- `make docker-kill` stops & removes _all_ running containers
- `make docker-clean` stops & removes _all_ running containers, and prunes the image catalog

## 3. "kubectl" commands

### **Ensure you are in the 'docker-desktop' context before running these `make` targets, as they will operate against the active context. This is _by design_, as we will reuse the same targets when working against AKS but we will ensure that we have set the context appropriately before running them then.**

The following targets effectively package `kubectl` commands for ease of use. You can also use the traditional `kubectl` commands on the solution and various projects if you wish to do something not packaged in the `Makefile`.

- `make k8s-cleanup` - deletes the namespace the application is currently deployed into.
- `make k8s-deploy` - creates a namespace, generates a `k8s-deployment.yaml` file and deploys it to the current k8s context. Use this when deploying an application to a new cluster.
- `make k8s-upgrade` - uses the current namespace, generates a `k8s-deployment.yaml` file and deploys it to the current k8s context. Use this when upgrading an existing cluster with a new version.
- `make k8s-dashboard` - configures the *local* k8s cluster to host a dashboard to see the status of the  cluster on a web page.
- `make k8s-status` - runs `kubectl get all` against the current context

## 5. "az cli" commands

One of the preferred ways of interacting with Azure programmatically is to use the Azure CLI. 

You can install Azure CLI on your local machine, but I find that it tends to snowflake your dev environment unnecessarily. A much cleaner way is to run the Azure CLI from within the official Docker image provided.

I find it is useful, from here on out, to have _two_ terminal windows open: 
1. The terminal which will run the Azure CLI from within Docker. The active directory should be the main project directory ( _i.e._ with `Makefile` in it). We'll call this the "Azure" terminal.
1. The terminal which will run all the other `make` targets. The active directory should be the main project directory ( _i.e._ with `Makefile` in it). We'll call this the "Project" terminal.

You will generally need to run the following commands _every_ time you fire up the "Azure" terminal.
- `make az-start` will fire up an interactive `bash` shell with the application directory mounted and active. This allows us to run `make` from this interactive `bash` shell with other targets!
- `make az-login` should be the first command you run. This will trigger a device login flow.
- `make az-sub-set sub=<your desired subscription guid>` should be the next command you run, replacing the placeholder with the subscription id to use for the other commands.
- `make az-sub-show` will show the active subscription details. Ensure this is the subscription you want to use to create resources with.

**Please note that `Setup.cfg` is not source controlled as it contains secrets.**

You will have to ensure that `Setup.cfg` has proper values in it for the "organization" and "project" names, so you will have to do this every time you clone your project repo.

- `make az-new-org` should only be run _once per organization name_. It sets up a resource group and an Azure Container Registry that can be shared against many projects. Additional project-shareable resources like an Analytics Workspace are also set up here. 
- `make az-new-proj` should only be run _once per project_. It sets up a resource group, an AKS cluster, and a storage account for use _only_ by this project. When this target is run, the script will emit two values:
    1. A storage connection string. Please edit `Setup.cfg` and put this value _carefully_ as the value of `paks_storage_connection_string`.
    1. A token to be used to login to the Azure Container Registery. Please edit `Setup.cfg` and put this value _carefully_ as the value of `oaks_acr_login_token`. This is a _long_ string with no line breaks.

You can run the following targets in the "Project" terminal, as they do not have any `az cli` dependencies.
- `make aks-prepare` will: 
    - `docker login` into the ACR using the login token embedded in `Setup.cfg` so that deployments can pull the images from the ACR
    - switch the context on your local kubernetes environment to point to the newly created AKS cluster
    - create a namespace on the AKS cluster
    - set up the storage connection string as a `kubectl secret` so that deployments can properly configure the silos to use Azure Storage for Clustering and Persistence

Now you can run _all_ the `k8s-` targets listed above against the AKS cluster.

_e.g._
- `make k8s-deploy` - creates a namespace, generates a `k8s-deployment.yaml` file and deploys it to the current k8s context. Use this when deploying an application to a new cluster.
- `make k8s-upgrade` - uses the current namespace, generates a `k8s-deployment.yaml` file and deploys it to the current k8s context. Use this when upgrading an existing cluster with a new version.
