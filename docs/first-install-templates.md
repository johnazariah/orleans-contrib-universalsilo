## Install Orleans UniversalSilo Templates

Run the following in a new command shell:

```
dotnet new --install Orleans.Contrib.UniversalSilo.Templates
```

This will download the latest version of the templates from [nuget](https://www.nuget.org/packages/Orleans.Contrib.UniversalSilo.Templates/) and install them on your machine.


When you are done, you will get something like this printed on your console:

```
Templates                                         Short Name                   Language          Tags
----------------------------------------------------------------------------------------------------------------------------------------------------
Console Application                               console                      [C#], F#, VB      Common/Console
Class library                                     classlib                     [C#], F#, VB      Common/Library
...
...
Orleans: Silo And Client                          orleans-silo-and-client      [C#]              Orleans/Contrib/Universal Silo/Silo And Client
Orleans: Standalone Client                        orleans-client               [C#]              Orleans/Contrib/Universal Silo/Standalone Client
Orleans: Standalone Silo                          orleans-silo                 [C#]              Orleans/Contrib/Universal Silo/Standalone Silo
Orleans: WebAPI Direct Client                     orleans-webapi               [C#]              Orleans/Contrib/Universal Silo/WebApi Direct Client
...
...
```

This tells you that the templates from the template pack have been successfully installed.

The templates installed are the following:
1. WebAPI DirectClient
1. Standalone Silo
1. Standalone Client
1. Silo and Client

_These templates will evolve as more functionality is published - specifically all the support for Kubernetes is forthcoming. So it will be necessary to uninstall and reinstall the dotnet templates as new versions become available_

Here are some more details about the templates:

#### WebAPI Direct Client
This template is the foundation for a popular and powerful application configuration.

It comprises a solution with:
- A single executable which hosts both a silo and a client which speaks directly with it
- A `grains` assembly which will contain your grains as you develop them
- A `grain-tests` testing project to help validate your grain logic
- A `grain-controllers` project which contains `WebAPI` controllers which are hosted for you to access over the web.

Additionally, it currently contains:
- A `Dockerfile` script to package the application as a Docker container
- A `Makefile` script with all the incantations to build and run the app, which can be used for CI/CD as well if your pipeline allows it

If you're starting out with Orleans, start here, as it will immediately showcase the power of the platform and help you get started with building real applications with as little ceremony as possible.

#### Standalone Silo
This template is the foundation for an independently hosted Silo application.

It comprises a solution with:
- A single executable which hosts a silo
- A `grains` assembly which will contain your grains as you develop them
- A `grain-tests` testing project to help validate your grain logic

Additionally, it currently contains:
- A `Dockerfile` script to package the application as a Docker container
- A `Makefile` script with all the incantations to build and run the app, which can be used for CI/CD as well if your pipeline allows it

You will generally use this in conjunction with a client, with which you will have to share the `grains` assembly. It's not super useful by itself.

#### Standalone Client
This template is the foundation for an independently hosted Client application.

It comprises a solution with:
- A single executable which hosts a client
- A `grains` assembly which will contain your grains as you develop them
- A `grain-tests` testing project to help validate your grain logic

Additionally, it currently contains:
- A `Dockerfile` script to package the application as a Docker container
- A `Makefile` script with all the incantations to build and run the app, which can be used for CI/CD as well if your pipeline allows it

You will generally use this in conjunction with a silo, with which you will have to share the `grains` assembly. It's not super useful by itself.

#### Silo And Client
This template is the foundation for a solution with two independently hosted applications, with a shared `grains` assembly.

It comprises a solution with:
- A single executable which hosts a silo
- A single executable which hosts a client
- A `grains` assembly which will contain your grains as you develop them
- A `grain-tests` testing project to help validate your grain logic

Additionally, it currently contains:
- A `Dockerfile` script for each application to package is as a Docker container
- A `Makefile` script with all the incantations to build and run the app, which can be used for CI/CD as well if your pipeline allows it
- A sample `docker-compose` script to bring up the silo and client in an orchestrated fashion

This is a popular, but advanced configuration, as it allows the silo and perhaps more than one client, to be developed concurrently but independently.

In this configuration, you are responsible for orchestrating the various components - like ensuring that the silos are available to the clients when the clients come up. Use this configuration when you're _very_ comfortable with how Orleans works.
