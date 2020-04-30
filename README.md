# Orleans UniversalSilo
Orleans is a flexible platform for building distributed applications.

Orleans, by itself, does not mandate or recommend a specific configuration or deployment strategy. This is how it should be, as Orleans needs to support the widest variety of use-cases and provide the highest degree of flexibility.

In practice, however, this flexibility may present a beginner with too many choices and too much ceremony - taking focus away from the joy of using Orleans!

This does not have to be the case at all! Orleans does _not_, in reality, require a steep learning curve.

This library attempts to provide a simple, boilerplate free foundation on which to get started with Orleans, with sensible defaults and extension points, allowing a developer to focus on grain design and testing whilst providing opinionated guidance around the real-world concerns of configuration, packaging, service presentation and deployment.

Here's how you can quickly get started with Orleans:

**1. Install the templates**

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

**2. Create an application with a name like `HelloOrleansWorld`.**

```shell
$ dotnet new orleans-webapi --name HelloOrleansWorld                                                                                                                                                                                         The template "Orleans: WebAPI Direct Client" was created successfully.
```

This will create a fully-functional sample application in the `HelloOrleansWorld` folder.

**3. Inspect the sample app**

```shell
$ cd HelloOrleansWorld/
$ ls -al
total 23
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 ./
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 ../
-rw-r--r-- 1 johnaz 4096  124 Apr 30 08:41 .dockerignore
-rw-r--r-- 1 johnaz 4096 3266 Apr 30 08:41 .gitignore
-rw-r--r-- 1 johnaz 4096  206 Apr 30 08:41 docker-compose.yml
-rw-r--r-- 1 johnaz 4096 1842 Apr 30 08:41 Dockerfile
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 grain-controllers/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 grains/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 grain-tests/
drwxr-xr-x 1 johnaz 4096    0 Apr 30 08:41 HelloOrleansWorld/
-rw-r--r-- 1 johnaz 4096 2578 Apr 30 08:41 HelloOrleansWorld.sln
-rw-r--r-- 1 johnaz 4096 3083 Apr 30 08:41 Makefile
-rw-r--r-- 1 johnaz 4096  223 Apr 30 08:41 tye.yaml

```

You will notice that it contains:
* A projects, a solution file, scripts to help you build and package your application, support for Docker containers, and a simple CI pipeline for github.

**4. Build, Test and Run the sample app.**

```shell
$ dotnet build HelloOrleansWorld.sln
Microsoft (R) Build Engine version 16.5.0+d4cbfca49 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restore completed in 1.61 sec for ...\HelloOrleansWorld\grains\grains.csproj.
  Restore completed in 1.61 sec for ...\HelloOrleansWorld\grain-controllers\grain-controllers.csproj.
  Restore completed in 2.43 sec for ...\HelloOrleansWorld\HelloOrleansWorld\HelloOrleansWorld.csproj.
  Restore completed in 2.45 sec for ...\HelloOrleansWorld\grain-tests\grain-tests.csproj.
  Orleans.CodeGenerator - command-line = SourceToSource ...\HelloOrleansWorld\grains\obj\Debug\netcoreapp3.1\grains.orleans.g.args.txt
  grains -> ...\HelloOrleansWorld\grains\bin\Debug\netcoreapp3.1\grains.dll
  grain-controllers -> ...\HelloOrleansWorld\grain-controllers\bin\Debug\netcoreapp3.1\grain-controllers.dll
  grain-tests -> ...\HelloOrleansWorld\grain-tests\bin\Debug\netcoreapp3.1\grain-tests.dll
  HelloOrleansWorld -> ...\HelloOrleansWorld\HelloOrleansWorld\bin\Debug\netcoreapp3.1\HelloOrleansWorld.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.81
```

```shell
$ dotnet test HelloOrleansWorld.sln
Test run for ...\HelloOrleansWorld\grain-tests\bin\Debug\netcoreapp3.1\grain-tests.dll(.NETCoreApp,Version=v3.1)
Microsoft (R) Test Execution Command Line Tool Version 16.5.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

A total of 1 test files matched the specified pattern.

Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: 16.8750 Seconds
```

Don't forget to _allow_ the firewall configuration in the next step.

```shell
$ dotnet run --project HelloOrleansWorld/HelloOrleansWorld.csproj
info: SiloConfigurator[0]
      Clustering mode from environment is: HostLocal
info: SiloConfigurator[0]
      Connection string from environment is:
info: SiloConfigurator[0]
      Blanking connection string because we are running with HostLocal clustering
info: SiloConfigurator[0]
      Finally configuring with clustering mode [HostLocal] with connection string []
info: SiloConfigurator[0]
      Persistence mode from environment is: InMemory
info: SiloConfigurator[0]
      Connection string from environment is:
info: SiloConfigurator[0]
      Finally configuring with persistence mode [InMemory] with connection string []
info: SiloConfigurator[0]
      Using the loopback address for [HostLocal]
info: SiloConfigurator[0]
      Development Clustering running on [127.0.0.1]:[11111]
info: SiloConfigurator[0]
      Configuring Endpoints and Silo Address for clustering mode HostLocal [127.0.0.1:(11111, 30000)]
info: SiloConfigurator[0]
      Configuring Persistence for InMemory []
info: SiloConfigurator[0]
      No dashboard available for clustering mode HostLocal
...
...
-------------- Started silo S127.0.0.1:11111:325957930, ConsistentHashCode 26F31827 --------------
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Orleans.Hosting.SiloHostedService[0]
      Starting Orleans Silo.
...
...
info: Orleans.Hosting.SiloHostedService[0]
      Orleans Silo started.
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: ...\HelloOrleansWorld\HelloOrleansWorld
...
...
```

Now fire up a browser and point it to https://localhost:5001/swagger/index.html and you will be presented with the API for a simple calculator which knows how to add two numbers.

Try it out. You are exercising a grain-based calculator!

You now have is a fully functional Orleans-based application exposing its functionality via a WebApi front end.

**5. Make it your own**

Take your time and look over the various projects in the solution. Add your own grains, tests and controllers. Rebuilding and running the application will extend it and make it your own!

**6. Learn more**

The application generated here is a sophisticated starting point. It has built-in support for configuration, extension, tests, CI/CD, packaging and deployment. Follow the [documentation](https://johnazariah.github.io/orleans-contrib-universalsilo/) to learn more.
