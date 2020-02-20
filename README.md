# SlnAggregate
[![CircleCI](https://circleci.com/gh/xaviersolau/SlnAggregate.svg?style=svg)](https://circleci.com/gh/xaviersolau/SlnAggregate)
[![Coverage Status](https://coveralls.io/repos/github/xaviersolau/SlnAggregate/badge.svg?branch=master)](https://coveralls.io/github/xaviersolau/SlnAggregate?branch=master)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet Beta](https://img.shields.io/nuget/vpre/SoloX.SlnAggregate.svg)](https://www.nuget.org/packages/SoloX.SlnAggregate)

Aggregate all C# projects from several solutions in a global solution replacing package references by
project references when possible. It is written in C# and thanks to .Net Core and the dotnet tools, it is cross platform.

Don't hesitate to post issue, pull request on the project or to fork and improve the project.

## License and credits

SlnAggregate project is written by Xavier Solau. It's licensed under the MIT license.

 * * *

## Installation

You can checkout this Github repository or you can use the NuGet package:

**Install editing your project file (csproj):**
```xml
<DotNetCliToolReference Include="SoloX.SlnAggregate" Version="1.0.0-alpha.5" />
```

Or using the dotnet tool install command:

**Install with dotnet:**
```bash
# Install globally:
dotnet tool install -g SoloX.SlnAggregate --version 1.0.0-alpha.5

# or in a specific 'target' folder
dotnet tool install SoloX.SlnAggregate --version 1.0.0-alpha.5 --tool-path target
```

## How to use it

In order to generate the shadow files, you will need to type:

```bash
SlnAggregate aggregate YourSolutionRootPath
or you can filter the folders you work on:
SlnAggregate aggregate YourSolutionRootPath --filters path1;path2;path3
```

Once your changes are done, you can push your changes back to the original project files:

```bash
SlnAggregate push YourSolutionRootPath
or you can filter the folders you work on:
SlnAggregate push YourSolutionRootPath --filters path1;path2;path3
```

### The use case

Let's say that your are working on a project that has got Nuget dependencies and you need to make
some changes related to your current project on one or several of those dependencies. In this situation 
this is some time convenient to work with a single solution containing all projects and replacing
package references by project references when possible instead of working on several solutions separately.
This tool will help you to aggregate all projects into one solution.

Let's take an example:

We are working on a project `MyProject` defined in `MyProjectSolution` that is using a Nuget `MyNuget` defined in
`MyNugetSolution`. Our working directory is `MyRoot`.

It gives us this working directory structure:

```bash
MyRoot
 | MyProjectSolution
 | | MyProjectSolution.sln
 | | MyProject
 | | | MyProject.csproj # Referencing the nuget MyNuget
 | MyNugetSolution
 | | MyNugetSolution.sln
 | | MyNuget
 | | | MyNuget.csproj # Defining the nuget MyNuget
```

The SlnAggregate tool will generate a global solution file `MyRoot.sln` referencing the projects through "shadow"
project files. Those "shadow" files are in fact the project images modified in a way that the package references
are replaced when possible by the corresponding project references.

The result of this generation will give us this structure:

```bash
MyRoot
 | MyRoot.sln (Generated)
 | MyProjectSolution
 | | MyProjectSolution.sln
 | | MyProject
 | | | MyProject.csproj # Referencing the nuget MyNuget
 | | | MyProject.Shadow.csproj # (Generated) Referencing the project MyNuget.Shadow.csproj
 | MyNugetSolution
 | | MyNugetSolution.sln
 | | MyNuget
 | | | MyNuget.csproj # Defining the nuget MyNuget and referenced by MyProject.csproj
 | | | MyNuget.Shadow.csproj # (Generated) Referenced by MyProject.Shadow.csproj
```

 It is now possible to open the `MyRoot.sln` and to work on the projects as if they were defined in the same solution
 from the beginning.

 Warning: If we add projects or if we change references in the shadow project files, we will have to manually report the changes
 into the original project files.