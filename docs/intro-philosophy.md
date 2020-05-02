# Orleans UniversalSilo Design Philosophy

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
    * Deployment on Azure Kubernetes Service on Scale Sets