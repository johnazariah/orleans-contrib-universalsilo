# client build container
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG config=Release
COPY . "/src"
RUN dotnet publish "/src/standalone-client/standalone-client.fsproj" -c ${config} -o /app

# container to run the server from
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS release
WORKDIR /app
#

# set up variables to allow the docker image to run by default with Azure clustering and Azure persistence on Azure Storage Emulator on the host
ENV ENV_CLUSTER_MODE           "Azure"
#
ENV ENV_CLUSTER_AZURE_STORAGE  "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;"
#
#

COPY --from=build /app .

ENTRYPOINT ["dotnet", "standalone-client.dll"]
