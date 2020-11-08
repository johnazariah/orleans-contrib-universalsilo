# Customizing the WebApiConfigurator

## Folder Heirarchy
Let us assume you created a `webapi` project called "Cornflake".

This is what the top-level "Solution" folder looks like:

...\CORNFLAKE
│   Cornflake.sln
│   ...
│
├───.github
│   └───workflows
│           ci.yml
│
├───Cornflake
│       Cornflake.csproj
│       Program.cs
│
├───grain-controllers
│   │   grain-controllers.csproj
│   │
│   └───Controllers
│           CalculatorController.cs
│
├───grain-tests
│       grain-tests.csproj
│       GrainTests.cs
│
└───grains
    │   grains.csproj
    │
    ├───Contract
    │       ICalculatorGrain.cs
    │
    └───Implementation
            CalculatorGrain.cs

