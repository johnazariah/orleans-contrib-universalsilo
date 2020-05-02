# Running the application in Visual Studio

Running the application in Visual Studio is no different from any traditional console app.

You can hit F5, set up breakpoints and step through code.

## Configuration

You can configure your application's environment in the development context using a variety of ways:

* `launchSettings.json` - setting environment variables in the project will also edit this file.
* `dotnet user-secrets` - a project-specific secret store for your local development environment.
* ~~`appSettings.json`~~ - only because you want to build [_beautiful snowflakes_](https://martinfowler.com/bliki/SnowflakeServer.html)!

I strongly prefer the first two options, because when your application is deployed in a container or elsewhere in the cloud, it's nice to be able to just change the values of environment variables and even integrate with KeyVault or other secret-managements in a CI/CD context *uniformly* with your development environment.

I rarely find the need to use `appSettings.json`, and prefer "in-code" configuration for global defaults.

See [Simple Configuration](config-simple-configuration.md) for ways to configure your application.
