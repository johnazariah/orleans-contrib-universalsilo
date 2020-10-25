# Simple Configuration

The following configuration settings are important, and commonly needed.

## Silo Options

The `ClusterId` and `ServiceId` of your Orleans Silo need be set to reasonable values.

The default values are:

1. `ClusterId` is set to the string `development`. Set the `ENV_CLUSTER_ID` environment variable to reflect the cluster identifier for the environment you are running in. If you are intending to run multiple clusters, set this to something unique to the cluster.

1. `ServiceId` is set by default to the name of your application in the entrypoint of your host application project. You can set the `ENV_SERVICE_ID` environment variable if you want to change this based on deployment.

You can also set the `SiloPort` and `GatewayPort` values by setting `ENV_SILO_PORT` and `ENV_GATEWAY_PORT` respectively. But don't do this unless you have good reason to.

## Clustering Options

You can set the clustering environment by setting the `ENV_CLUSTER_MODE` environment variable to one of:
1. `Azure` : Use clustering with Azure Tables. I _strongly_ recommend you use this clustering mode in both deployment and development environments. Set `ENV_CLUSTER_AZURE_STORAGE` to the connection string of an Azure Storage account. See the section on `Azure Storage Emulator` below.

1. `HostLocal` : Use this value if you will only run a single node in development. This is a valid configuration for development, but beware that your deployments need a different clustering strategy if you want to deploy a multi-node cluster. You won't need Azure Storage Emulator for this.

You can also configure custom clustering with providers like `Kubernetes` and others. This will involve more customization of code and dependencies. _Please submit ideas or contributions to add such functionality out-of-the-box._

## Persistence Options
You can set the persistence environment by setting the `ENV_PERSIST_MODE` environment variable to one of:

1. `AzureTable` : This is by far the most popular storage option. _Strongly recommend_. Set `ENV_PERSIST_AZURE_STORAGE` to the connection string of an Azure Storage account. See the section on `Azure Storage Emulator` below.

1. `InMemory` : Use this value if you will only run a single node in development. This is a valid configuration for development, but beware that your deployments need a different persistence strategy if you want to deploy a multi-node cluster.

1. `AzureBlob` : Use this value if you'd rather use Azure Blobs for grain storage than Azure Tables.

You can also configure custom persistence with providers like `CosmosDB` and others. _Please submit ideas or contributions to add such functionality out-of-the-box._

## Azure Storage Emulator

This is an ancient but still-useful emulator of Azure Storage that runs on your local machine. It's good enough for development and testing but not much more.

_I shouldn't have to say this, but please don't deploy a production topology with Azure Storage Emulator!_

You'll need to ensure that the emulator is running *before* you run your application. To use the emulator, you can use `"UseDevelopmentStorage=true"` as the connection string.

## Azure Storage Emulator with Docker 

If you wish to run a Docker container and want to refer to the Azure Storage Emulator instance on the host machine, do _not_ set the `ENV_CLUSTER_AZURE_STORAGE` and `ENV_PERSIST_AZURE_STORAGE` connection strings to `"UseDevelopmentStorage=true"`. Just leave them blank.

You will need to still set `ENV_CLUSTER_MODE` to `Azure` and `ENV_PERSIST_MODE` to `AzureTable` or `AzureBlob`.

The Dockerfile provides the correct default connection strings to access the host's Azure Storage Emulator from a running container in these cases.
