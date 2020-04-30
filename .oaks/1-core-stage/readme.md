## Core Resource Group

These resources can be shared across multiple applications. 

It is not recommended to have one of these per-application, as it will mean that all the build-pipeline plumbing, along with service principal management, will be difficult.

The following resources are present in the `oak-core-rg` resource group:

1. The `oak-core-kv` key vault. This needs to be created first as we will need to store the password of `oak-aks-sp` in it
1. The `oak-aks-sp` service principal. The password for this service principal is stored in `oak-core-kv`
1. The `oakacr` azure container registry. This will be wired up to serve all your Orleans-on-AKS applications.

