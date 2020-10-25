# Development/Deployment workflows

## Multiple Workflows
![Multiple Workflows](images/Workflows.png)



The simplest way to build and run your application is using the provided `Makefile`. You can inspect all the targets that you can run with `make` by inpecting the `Makefile` in your directory.

The `Makefile` has targets to help you build, test, package and run the application as a `dotnet` application as well as a `Docker` image.

#### `dotnet` targets
The standard targets you will use when interacting with your application as a dotnet command-line application are:

- `make dotnet-clean` - runs `dotnet clean` on your solution
- `make dotnet-build` - runs a build on your solution, automatically invoking the restore
- `make dotnet-test`  - runs all tests in your solution *without first building*
- `make dotnet-publish` - publishes your compiled application the `out` directory *without first building*
- `make dotnet-run` - runs your application in a separate shell window from the `out` directory
- `make dotnet-restore` - runs `dotnet restore` on your solution. Invoked from other targets.

You can chain targets together
- `make dotnet-clean dotnet-build dotnet-test` cleans, builds and tests your solution
- `make dotnet-publish dotnet-run` publishes and runs your application

These targets are the same ones that are used in the included `Github Actions` CI pipeline, so if you use them whilst developing, you will have greater confidence that the build will succeed on commit.


#### `Docker` targets
The standard targets you will use when interacting with your application as a Docker container are:

- `make docker-build` - builds the app into a docker image and tags it with the commit hash of the last commit in the active branch
- `make docker-run` - runs the docker image
- `make docker-push` - pushes the image to the container registry. You will need to modify the `acr` argument in the `Makefile`

#### Advanced targets
**THESE TARGETS AFFECT ALL CONTAINERS AND IMAGES ON YOUR SYSTEM**

**BE CAREFUL WITH THEM**
- `make docker-stop` stops _all_ running containers
- `make docker-kill` stops & removes _all_ running containers
- `make docker-clean` stops & removes _all_ running containers, and prunes the image catalog

## Using `dotnet` and `docker` commands

You can use the traditional `dotnet` and `docker` on the solution and various projects.