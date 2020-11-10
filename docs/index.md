# Orleans UniversalSilo
Orleans is a flexible platform for building distributed applications.

Orleans, by itself, does not mandate or recommend a specific configuration or deployment strategy. This is how it should be, as Orleans needs to support the widest variety of use-cases and provide the highest degree of flexibility.

In practice, however, this flexibility may present a beginner with too many choices and too much ceremony - taking focus away from the joy of using Orleans!

This does not have to be the case at all! Orleans does _not_, in reality, require a steep learning curve.

This library attempts to provide a simple, boilerplate free foundation on which to get started with Orleans, with pragmatic defaults and extension points, allowing a developer to focus on grain design and testing whilst providing opinionated guidance around the real-world concerns of configuration, packaging, service presentation and deployment.

[Read more about the design philosophy here](intro-philosophy.md)

Using this library is best done by interacting with the working bits published to Nuget. This will allow you to focus on building Orleans applications in your choice of idiomatic C# or F# with the least ceremony.

Getting started is easy. [Follow the QuickStart Guide:](intro-quickstart.md)

**1. Install the templates**

```shell
dotnet new --install Orleans.Contrib.UniversalSilo.Templates
```

**2. Create an application with a name like `HelloOrleansWorld`.**

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

## Table of Contents
### Setup Environment
- [Setting up the Development Environment](setup-environment-setup.md)

### First Steps
- [Installing the Orleans UniversalSilo Templates](first-install-templates.md)
- [Creating a Silo with a WebAPI interface](first-create-application.md)
- [Simple Configuration](config-simple-configuration.md)

### The Development/Deployment Workflow
- [Development and Deployment Workflow Stages](development-workflow.md)
- [Makefile Target Reference](makefile-target-reference.md)

### Customizing and Configuring Your Application
- [Customizing the SiloConfiguration](customizing-silo.md)

### CI/CD
- On Github
