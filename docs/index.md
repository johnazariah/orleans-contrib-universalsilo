# Orleans UniversalSilo
Orleans is a flexible platform for building distributed applications.

Orleans, by itself, does not mandate or recommend a specific configuration or deployment strategy. This is how it should be, as Orleans needs to support the widest variety of use-cases and provide the highest degree of flexibility.

In practice, however, this flexibility may present a beginner with too many choices and too much ceremony - taking focus away from the joy of using Orleans!

This does not have to be the case at all! Orleans does _not_, in reality, require a steep learning curve.

This library attempts to provide a simple, boilerplate free foundation on which to get started with Orleans, with sensible defaults and extension points, allowing a developer to focus on grain design and testing whilst providing opinionated guidance around the real-world concerns of configuration, packaging, service presentation and deployment.

The philosophy behind this library is to:

* **Abstract away as much boilerplate as possible** without sacrificing flexibility for evolution

* **Provide sensible defaults** without preventing the defaults being overridden

* **Make grain design the focus** of the development experience

* **Support Grain Testing** as a first class concern

* **Support Cross-Platform** as a first class concern
    * Build on .net core

* **Support CI/CD** as a first class concern
    * Provide uniform building instructions for local development _and_ build pipelines
    * Provide uniform packaging instructions for application executables _and_ Docker images

* **Support Azure Deployment** as opinionated guidance.

    **_Contributions for guidance on other platforms are welcome!_**
    * Deployment as Azure App Service with a Site Plan on Scale Sets
    * Deployment on Azure Kubernetes Service on Scale Sets

The documentation is structured as follows:

- [The Development Process](development.md)
- [Configuration](configuration.md)
- [Building and Packaging](building-and-packaging.md)
- [Deployment](deployment.md)