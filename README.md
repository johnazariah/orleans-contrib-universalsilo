# orleans-contrib-universalsilo
This is an opinionated library + template pack designed to help beginners to build Orleans applications.

Orleans is a flexible, un-opinionated platform for building distributed applications. This is how it should be, to support the widest variety of use-cases and the highest degree of flexibility.

However, with such flexibility, a beginner with Orleans is practically swamped with choices - a great many of which require understanding a great deal about the platform _ab initio_. In reality, there is no need to have a steep learning curve with Orleans at all, as we can provide guidance in the form of sensible defaults which allows for a developer to focus on grain design. This is an attempt to do specifically that.

The philosophy behind this library is as follows:

* Abstract away as much boilerplate as possible without sacrificing flexibility for evolution
* Provide sensible defaults without preventing the defaults being overridden
* Focus the experience of the developer on Grain Design
* Support Grain Testing as a first class concern
* Support CI/CD as a first class concern
* Provide a sensible default deployment experience without preventing other ways of deploying

