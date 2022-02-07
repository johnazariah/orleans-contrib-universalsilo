# server build container
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG config=Release
COPY . "/src"
RUN dotnet restore "/src/GeneratedProjectName/GeneratedProjectName._PROJ_SUFFIX_"
RUN dotnet build --no-restore "/src/GeneratedProjectName/GeneratedProjectName._PROJ_SUFFIX_" -c ${config}
RUN dotnet test --no-build "/src/GeneratedProjectName/GeneratedProjectName._PROJ_SUFFIX_" -c ${config}
RUN dotnet publish --no-build  "/src/GeneratedProjectName/GeneratedProjectName._PROJ_SUFFIX_" -c ${config} -o /app

# container to run the server from
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS release
WORKDIR /app
EXPOSE 30000 11111 8080 80

# The following ENV settings are the build-time defaults.
# You can pass different run-time values for any/all of them by using the '-e ENV_xxx=value' when invoking 'docker run'.

# This string sets up the name of the cluster. 
# It is _strongly_ recommended that you supply a run-time value here for each unique deployment.
ENV ENV_CLUSTER_ID                "development"

# This string sets up the name of the service
ENV ENV_SERVICE_ID                "GeneratedProjectName"

# These numbers are required for members of the cluster to properly communicate with each other.
# Do not change these without convincing reason.
# Leaving the environment variables unset will supply these defaults from the code.
# Changing these will require exposing those ports as well
# ENV ENV_SILO_PORT                 11111
# ENV ENV_GATEWAY_PORT              30000

# ENV_CLUSTER_MODE sets the clustering mode.
# * 'HostLocal' for single-silo configurations typically required in dev-test scenarios. ENV_CLUSTER_AZURE_STORAGE and ENV_CLUSTER_DEV_STORAGE are ignored in this case. 
# * 'Azure' for Azure-Table based clustering. Set ENV_CLUSTER_AZURE_STORAGE or ENV_CLUSTER_DEV_STORAGE as desired below.
ENV ENV_CLUSTER_MODE              "HostLocal"

# These environment variables only make sense for Azure-based clustering.
# * To run against Azure Storage Emulator running on the host machine, 
#       * leave ENV_CLUSTER_AZURE_STORAGE blank - do not specify it either here or at run-time.
#       * uncomment ENV_CLUSTER_DEV_STORAGE below. The connection string specified refers to the host machine's Azure Storage Emulator
# ENV ENV_CLUSTER_DEV_STORAGE    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;"
#
# * To run against Azure Storage.
#       * set ENV_CLUSTER_AZURE_STORAGE to the connection-string of the account to be used. 
#         Typically it is more convenient to specify this at run-time with '-e ENV_CLUSTER_AZURE_STORAGE=DefaultEndpointsProtocol=https;...'
#         Setting this at build-time in this Dockerfile is discouraged, as this will result in exposing the key in the source-control system
#
# If ENV_CLUSTER_AZURE_STORAGE is set (either at build or run-time), ENV_CLUSTER_DEV_STORAGE is ignored

# ENV_PERSIST_MODE sets the persistence mode for grains.
# * 'InMemory' for volatile storage typically required in dev-test scenarios. ENV_PERSIST_AZURE_STORAGE and ENV_PERSIST_DEV_STORAGE are ignored in this case.
# * 'AzureTable' for Azure-Table based persistence. Set ENV_PERSIST_AZURE_STORAGE or ENV_PERSIST_DEV_STORAGE as desired below.
# * 'AzureBlob' for Azure-Blob based persistence. Set ENV_PERSIST_AZURE_STORAGE or ENV_PERSIST_DEV_STORAGE as desired below.
ENV ENV_PERSIST_MODE              "InMemory"

# These environment variables only make sense for Azure (Table or Blob)-based persistence.
# * To run against Azure Storage Emulator running on the host machine, 
#       * leave ENV_PERSIST_AZURE_STORAGE blank - do not specify it either here or at run-time.
#       * uncomment ENV_PERSIST_DEV_STORAGE below. The connection string specified refers to the host machine's Azure Storage Emulator
# ENV ENV_PERSIST_DEV_STORAGE   "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;"
#
# * To run against Azure Storage.
#       * set ENV_PERSIST_AZURE_STORAGE to the connection-string of the account to be used. 
#         Typically it is more convenient to specify this at run-time with '-e ENV_PERSIST_AZURE_STORAGE=DefaultEndpointsProtocol=https;...'
#         Setting this at build-time in this Dockerfile is discouraged, as this will result in exposing the key in the source-control system
#
# If ENV_PERSIST_AZURE_STORAGE is set (either at build or run-time), ENV_PERSIST_DEV_STORAGE is ignored

# This is the app-insights key used to log diagnostics.
# It is _strongly_ recommended that you supply a run-time value here when running in Azure.
ENV ENV_APPINSIGHTS_KEY           ""

COPY --from=build /app .

ENTRYPOINT ["dotnet", "GeneratedProjectName.dll"]
