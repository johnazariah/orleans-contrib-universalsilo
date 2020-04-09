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

The templates installed are the following

#### WebAPI Direct Client
This template is the foundation for a popular and powerful application configuration.

It comprises a solution with :
- A single executable which hosts both a silo and a client which speaks directly with it
- A `grains` assembly which will contain your grains as you develop them
- A `grain-tests` testing project to help validate your grain logic
- A `grain-controllers` project which contains `WebAPI` controllers which are hosted for you to access over the web.

If you're starting out with Orleans, start here, as it will immediately showcase the power of the platform and help you get started with building real applications with as little ceremony as possible.

#### Standalone Silo
This template is the foundation for an independently hosted Silo application.

You will generally use this in conjunction with a client, with which you will have to share the `grains` assembly. It's not super useful by itself.

#### Standalone Client
This template is the foundation for an independently hosted Client application.

You will generally use this in conjunction with a silo, with which you will have to share the `grains` assembly. It's not super useful by itself.

#### Silo And Client
This template is the foundation for a solution with two independently hosted applications, with a shared `grains` assembly.

This is a popular, but advanced configuration, as it allows the silo and perhaps more than one client, to be developed concurrently but independently.

In this configuration, you are responsible for orchestrating the various components - like ensuring that the silos are available to the clients when the clients come up. Use this configuration when you're _very_ comfortable with how Orleans works.
