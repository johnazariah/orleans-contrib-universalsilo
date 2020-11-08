## Setting up your Development Environment on Windows
These instructions are primarily for development on Windows, but attempt to be as cross-platform as possible.

This looks like a lot of work but (1) I'm assuming you're starting on a clean machine or VM, and (2) you do it only once! If you're using a machine on which you're already developing, you likely have a few of these already set up.

### (XPlat) Visual Studio Code
We'll try to use [Visual Studio Code](https://git-scm.com/downloads) for the majority of our development work. This is a capable cross-platform editor with the ability to handle multiple languages and dialects, and has support for debugging, Docker, and Kubernetes as well.

You can install [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) in addition to Visual Studio Code if you like.

I haven't tested [Rider](https://www.jetbrains.com/rider/), but I've heard good things about it, and you may want to install it and give it a whirl. 

### (Windows) Git for Windows
Install [Git for Windows](https://git-scm.com/downloads) on your machine.
We will make use of `bash` and other linux tools during development, so ensure you install the Linux tools _in your PATH_ when installing Git for Windows. You can set the default editor to be `Visual Studio Code` if you don't know [how to exit `vim`!](https://stackoverflow.com/questions/11828270/how-do-i-exit-the-vim-editor) :)

### (XPlat) PowerShell Core
Follow instructions to install [PowerShell Core](https://github.com/PowerShell/PowerShell) on your machine.

### (Windows) Chocolatey
Follow [these instructions](https://chocolatey.org/install) on an *Administrative* instance of PowerShell you just installed to install `Chocolatey` on your machine. Ensure you have a working install.

### (Windows) Make
We will make use of `make` during development, so you will need to install this on Windows. 

*Don't worry, you won't need to write any `make` scripts. You'll need `make` to run the provided scripts, though!*

Open a new command shell window after installing Chocolatey to run `choco install make`. This will install `make` on your windows environment.

### (XPlat) DotNet Core
You'll need an SDK install of [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core) or newer. For your own sanity, try to stick with the recommended version and not the bleeding-edge version.

### (Windows/Mac) Docker Desktop
If you want to explore the possibilities of packaging your application in Docker Containers, or want to consider Kubernetes for deployment, install [Docker Desktop](https://www.docker.com/products/docker-desktop). Ensure you have a working installation by following the instructions for [windows](https://docs.docker.com/docker-for-windows/) or [mac](https://docs.docker.com/docker-for-mac/).

This will come with a version of Kubernetes that runs on your desktop as well, so ensure you enable it.

### Azure Storage Emulator and Azure Storage Explorer
We'll use the Azure Storage Emulator for a local version of Azure storage. You'll find the [installer](https://go.microsoft.com/fwlink/?LinkId=717179&clcid=0x409) and [documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator).

Ensure you start up the Azure Storage Emulator before you run your application.

We'll also use Azure Storage Explorer to inspect the contents of an Azure storage account. You'll find the [installer](https://azure.microsoft.com/en-us/features/storage-explorer/) and [documentation](https://docs.microsoft.com/en-us/azure/vs-azure-tools-storage-manage-with-storage-explorer).

## Setting up your Development Environment on Mac or Linux

**_Contributions for specific instructions for setup on Mac and Linux are welcome_**

The instructions listed for a Windows development environment attempt to be as cross-platform as possible.

Try to set up the analogous packages on your machine and reach out if you need help.