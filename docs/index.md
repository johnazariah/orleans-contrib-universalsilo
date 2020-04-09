# Orleans UniversalSilo
Orleans is a flexible platform for building distributed applications. Orleans, by itself, does not mandate a specific configuration or deployment strategy. This is how it should be, as Orleans needs to support the widest variety of use-cases and the highest degree of flexibility.

However, a beginner with Orleans can be practically swamped with choices - a great many of which require understanding a great deal about the platform _ab initio_.

This does not have to be the case at all. Orleans does, in reality, not require a steep learning curve at all, and this library attempts to provide a simple, boilerplate free foundation on which to get started with Orleans, with sensible defaults and extension points, allowing a developer to focus on grain design and testing whilst addressing the real-world concerns of configuration, packaging, service presentation and deployment.

The philosophy behind this library is as follows:

* Abstract away as much boilerplate as possible without sacrificing flexibility for evolution
* Provide sensible defaults without preventing the defaults being overridden
* Focus the experience of the developer on Grain Design
* Support Grain Testing as a first class concern
* Support CI/CD as a first class concern
* Provide opinionated but detailed packaging support
    * Packaging as an Azure App Service
    * Packaging a Docker Container
* Provide opinionated but detailed guidance to deployment on Azure. (Contributions for guidance on other platforms are welcome!)
    * Deployment with Azure App Service Site Plan and Scale Sets
    * Deployment on Azure Kubernetes Service

The documentation is structured as follows:

- [The Development Process](Development.md)
- [Configuration](Configuration.md)
- [Packaging](Packaging.md)
- [Deployment](Deployment.md)