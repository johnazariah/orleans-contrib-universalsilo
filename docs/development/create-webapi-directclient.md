## Getting Started with an Orleans application with a WebAPI front-end

This is a popular and powerful configuration to start with, as it allows one to build a fully functional Silo-based Web-based application.

Orleans Silos form a cluster by communicating with each other through the `gateway` TCP port, and expose their functionality to Orleans Clients through the `silo` TCP port. This usually takes place in an environment like Azure where a server-side network allows nodes to communicate with each other. This server-side network is generally _not_ exposed to clients communicating from outside Azure. To keep the networking simple and safe, the general guidance is that clients not on the server-side networkshould communicate via a standard protocol such as HTTP/S, rather than expose the `silo` port to the outside world. To make this possible, we create a client that speaks HTTP/S to the outside world, and relays requests to the Orleans Silos over the server-side network.

There are several industry standard mechanisms for exposing an application's programming interface (API) over HTTP/S. We will use a popular one called `WebAPI` and expose the API using `Swagger`.

Additionally, to further simplify networking and improve runtime performance at the expense of a little scale performance, we will host both the Silo and Client into a single command-line application. Such a configuration is called a "Direct Client" configuration. It is normal to stand up multiple instances of this application, appropriately configured, where one can build an `n`-Silo cluster.

This template creates an application with such a configuration.

#### Install the templates

```shell
dotnet new --install Orleans.Contrib.UniversalSilo.Templates
```

This should print out the list of installed templates, including the following:

```shell
$ dotnet new --install Orleans.Contrib.UniversalSilo.Templates
  Restore completed in 660.35 ms.

...

Templates                                         Short Name                   Language          Tags
----------------------------------------------------------------------------------------------------------------------------------------------------
Console Application                               console                      [C#], F#, VB      Common/Console
Class library                                     classlib                     [C#], F#, VB      Common/Library
...
Orleans: Silo And Client                          orleans-silo-and-client      [C#], F#          Orleans/Contrib/Universal Silo/Silo And Client
Orleans: Standalone Client                        orleans-client               [C#], F#          Orleans/Contrib/Universal Silo/Standalone Client
Orleans: Standalone Silo                          orleans-silo                 [C#], F#          Orleans/Contrib/Universal Silo/Standalone Silo
Orleans: WebAPI Direct Client                     orleans-webapi               [C#], F#          Orleans/Contrib/Universal Silo/WebApi Direct Client
...
Solution File                                     sln                                            Solution
Protocol Buffer File                              proto                                          Web/gRPC

Examples:
    dotnet new mvc --auth Individual
    dotnet new react
    dotnet new --help

```

#### Create an application with a name like `HelloOrleansWorld`.

```shell
$ dotnet new orleans-webapi --name HelloOrleansWorld
The template "Orleans: WebAPI Direct Client" was created successfully.
```

This will create a fully-functional **C#** application in the `HelloOrleansWorld` folder.

You can also choose to generate the project in **F#** by using the following command:

```shell
$ dotnet new orleans-webapi --name HelloOrleansWorld --language F#
The template "Orleans: WebAPI Direct Client" was created successfully.
```

#### Inspect the sample app

```shell
$ cd HelloOrleansWorld
$ ls -al
total 31
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:50 ./
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:50 ../
-rw-r--r-- 1 johnaz 4096  124 Apr 30 09:50 .dockerignore
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:50 .github/
-rw-r--r-- 1 johnaz 4096 3266 Apr 30 09:50 .gitignore
-rw-r--r-- 1 johnaz 4096  206 Apr 30 09:50 docker-compose.yml
-rw-r--r-- 1 johnaz 4096 2119 Apr 30 09:50 Dockerfile
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:51 grain-controllers/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:51 grains/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:50 grain-tests/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 09:50 HelloOrleansWorld/
-rw-r--r-- 1 johnaz 4096 2578 Apr 30 09:50 HelloOrleansWorld.sln
-rw-r--r-- 1 johnaz 4096 2720 Apr 30 09:50 Makefile
-rw-r--r-- 1 johnaz 4096  223 Apr 30 09:50 tye.yaml
```

You will notice that it contains:
* A _console application_ project named `HelloOrleansWorld` which is the **host application**
* A _class library_ project for **grains** where the grain interfaces and implementations are held
* A _class library_ project for **grain-controllers** where controllers are provided to expose grain methods over WebAPI
* A _xunit test_ project where grains can be tested in a test cluster, with examples of how to do **unit-** and **property-based-** testing
* A _solution file_ to coordinate the projects together
* A `Makefile` script to help you with the incantations to use whilst developing. You do not need to know `make` to use it
* A `Dockerfile` script to package your application into a [Docker](https://www.docker.com/) container. You do not need to have Docker installed if you do not want to use it
* `.gitignore` and `.dockerignore` files to help keep your working set clean
* A `.github` folder which contains a simple CI pipeline ready to build your library if you commit it to a [GitHub](https://github.com/) repository

[Experimental]
* A `tye.yaml` script to build, run and deploy your application via [Tye](https://github.com/dotnet/tye)
* A `docker-compose.yml` script to orchestrate a multi-node cluster on your local machine using [Docker Compose](https://docs.docker.com/compose/)

#### Initialize a Git Repo

_You may receive some warnings on this command, and that is fine and normal. The key thing to ensure is that a git repo is eventually created._

```shell
$ make init
fatal: not a git repository (or any of the parent directories): .git
fatal: not a git repository (or any of the parent directories): .git
git init
Initialized empty Git repository in .../HelloOrleansWorld/.git/
git add .
warning: LF will be replaced by CRLF in .dockerignore.
The file will have its original line endings in your working directory
...
The file will have its original line endings in your working directory
git commit -m "Initial commit of HelloOrleansWorld"
[master (root-commit) bfecd13] Initial commit of HelloOrleansWorld
 17 files changed, 954 insertions(+)
 create mode 100644 .dockerignore
 create mode 100644 .github/workflows/ci.yml
 create mode 100644 .gitignore
 create mode 100644 Dockerfile
 create mode 100644 HelloOrleansWorld.sln
 create mode 100644 HelloOrleansWorld/HelloOrleansWorld.csproj
 create mode 100644 HelloOrleansWorld/Program.cs
 create mode 100644 Makefile
 create mode 100644 docker-compose.yml
 create mode 100644 grain-controllers/Controllers/CalculatorController.cs
 create mode 100644 grain-controllers/grain-controllers.csproj
 create mode 100644 grain-tests/GrainTests.cs
 create mode 100644 grain-tests/grain-tests.csproj
 create mode 100644 grains/Contract/ICalculatorGrain.cs
 create mode 100644 grains/Implementation/CalculatorGrain.cs
 create mode 100644 grains/grains.csproj
 create mode 100644 tye.yaml
```

This should be the state after the previous command completes.

```shell
$ git status
On branch master
nothing to commit, working tree clean
```

This step is necessary because any Docker images you build will automatically get tagged with the commit that the image is based on.

#### Build and Test the app

The simplest way to build and test the application is to run `make dotnet-build dotnet-test`

```shell
$ make dotnet-build dotnet-test
dotnet restore HelloOrleansWorld.sln
  Restore completed in 75.29 ms for ...\HelloOrleansWorld\grain-controllers\grain-controllers.csproj.
  Restore completed in 75.28 ms for ...\HelloOrleansWorld\grains\grains.csproj.
  Restore completed in 76.5 ms for ...\HelloOrleansWorld\HelloOrleansWorld\HelloOrleansWorld.csproj.
  Restore completed in 76.3 ms for ...\HelloOrleansWorld\grain-tests\grain-tests.csproj.
Built DotNet projects
dotnet build --no-restore HelloOrleansWorld.sln -c Debug
Microsoft (R) Build Engine version 16.5.0+d4cbfca49 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  grains -> ...\HelloOrleansWorld\grains\bin\Debug\netcoreapp3.1\grains.dll
  grain-controllers -> ...\HelloOrleansWorld\grain-controllers\bin\Debug\netcoreapp3.1\grain-controllers.dll
  grain-tests -> ...\HelloOrleansWorld\grain-tests\bin\Debug\netcoreapp3.1\grain-tests.dll
  HelloOrleansWorld -> ...\HelloOrleansWorld\HelloOrleansWorld\bin\Debug\netcoreapp3.1\HelloOrleansWorld.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.42
Built DotNet projects
dotnet test --no-build HelloOrleansWorld.sln -c Debug
Test run for ...\HelloOrleansWorld\grain-tests\bin\Debug\netcoreapp3.1\grain-tests.dll(.NETCoreApp,Version=v3.1)
Microsoft (R) Test Execution Command Line Tool Version 16.5.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

A total of 1 test files matched the specified pattern.

Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: 14.3104 Seconds
Built DotNet projects
```

As you make changes to the source code, you can choose to build and test your project with individual commands:

* To build your app, run `make dotnet-build`.
* To run tests on your app _without building it first_, run `make dotnet-test`

#### Run the app

Run the application you just built by invoking `make dotnet-publish dotnet-run`

```shell
$ make dotnet-publish dotnet-run
dotnet publish --no-build HelloOrleansWorld/HelloOrleansWorld.csproj -c Debug -o out/HelloOrleansWorld
Microsoft (R) Build Engine version 16.5.0+d4cbfca49 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  HelloOrleansWorld -> ...\HelloOrleansWorld\out\HelloOrleansWorld\
Built DotNet projects

powershell Start-Process 'out/HelloOrleansWorld/HelloOrleansWorld.exe' -WorkingDirectory 'out/HelloOrleansWorld'
Launched DotNet projects
```

The `dotnet-run` step launches a second `powershell` window to run the application. You'll see a lot of diagnostic output in that window which you will learn to use for diagnosis.

The first time you run this command, you _may_ get a Windows Security Alert saying that you will need allow the application to communicate over the network through your Windows Defender Firewall. **Ensure that you `Allow Access` on the dialog!**

#### Understanding the diagnostics

Inspect the diagnostic output and look for lines like the following:

```shell
...
info: SiloConfigurator[0]
      Configuring Endpoints and Silo Address for clustering mode HostLocal [127.0.0.1:(11111, 30000)]
info: SiloConfigurator[0]
      Configuring Persistence for InMemory []
...
```

These indicate the clustering and persistence settings that the application is configured to use.

```shell
...
-------------- Started silo S127.0.0.1:11111:326033669, ConsistentHashCode 2AB5ED45 --------------
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
...
```

These indicate that the WebAPI service is up and running, and listening on the specified ports.

```shell
...
    Grain class HelloOrleansWorld.Implementation.HelloOrleansWorld.Implementation.CalculatorGrain [1084273261 (0x40A0B26D)] from grains.dll implementing interfaces: HelloOrleansWorld.Contract.ICalculatorGrain [-407985760 (0xE7AEA1A0)]
...
```
These indicate that the  application grains have been found and included in the running application. Ensure all the grains you expect to see are loaded here.

You will see more grains than the ones in your project listed here. That is normal.

```shell
...
info: Orleans.Hosting.SiloHostedService[0]
      Orleans Silo started.
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: ...\HelloOrleansWorld\out\HelloOrleansWorld
...
```
These indicate that the Silo service has successfully started.

This window is where any failures or exceptions are displayed, so it's worth keeping it at hand to monitor. You can configure these messages to be sent to a Telemetry solution like Application Insights for better monitoring when deployed to production.

#### Accessing the API via Swagger

Once the application has been stood up, you can access the WebAPI interface with your browser. The application stands up a Swagger endpoint out of the box, so point your browser to https://localhost:5001 or whatever port your application says it's listening on in the diagnostics.

Once there, you will see a simple calculator service API exposed - a GET endpoint that adds two numbers. Play around with it and recognize that the functionality you are seeing is from the demo Calculator grain.

The Swagger endpoint generates a platform-agnostic description of the service, and there are several tools available to generate client-access libraries automatically from the endpoint, so exposing the endpoint enables you to create a wide variety of client applications that can interact with this service. [Read more here.](https://swagger.io/tools/)

#### Monitoring the application with the Orleans Dashboard

The application also stands up the Orleans Dashboard to be able to get oversight into the health and activity of the cluster. You can access it at http://localhost:8080 _note the **http**_.

Try submitting more requests from the swagger page and notice the grain count.

### Summary & Next Steps
This should give you a flavour of what it's like to work with Orleans. Next we'll write a grain and expose it to the Swagger API.
