## Application Resource Group

These resources are specific to a single application. It is assumed that the Core Resource Group has been built already.

In the interest of security and maintainability, do not put resources in this group that are shared with other Orleans-on-AKS applications.

Specifically, do *not* share the storage account with another application!

For the purposes of this document, we will assume your application name is `cornflake`.

The following resources are present in the `oak-cornflake-rg` resource group:

1. The `oakcornflakestg` storage account. This storage account will be used both for Orleans clustering and grain persistence.
1. The `oak-cornflake-aks` AKS cluster, along with the associated virtual networks and the app-insights workspace

