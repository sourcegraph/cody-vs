
## Prepare your development environment
Install the following components:
 - Visual Studio 2022 (at least 17.10.2 version) with Visual Studio SDK (Visual Studio extension development)
 - Node.js (at least 20.14.0 version)
 - pnpm (version 8.6.7)
 - .NET SDK (at least 8.0.302 version)
 - git

During the first launch, you must first build the agent and only then run the extension in Visual Studio.

## Used runtimes

 - visual studio extension use .NET Framework 4.8. This is Windows only runtime and it comes with the system itself and the Visual Studio installation.
 - agent use Node for build and runtime
 - build scripts use .NET runtime 8. This runtime is multiplatform and is only used by cake 

## Cake build automation
We use [Cake](https://cakebuild.net/) as build automation system. The building script can be found in the `build.cake` file. During the building and publishing process, we perform the following steps:
 1. Downloading a repository with an agent
 2. Selecting the commit used to build the agent
 3. Building agent
 4. Copying agent files to the folder used by the VS extension
 5. Downloading node files (x64 and arm64 versions)
 6. Copying node files to the folder used by the VS extension
 7. Building VS extension using MSBuild
 8. Publishing to the marketplace 
 
## Commands
The following commands assume you are in the  directory where `build.cake` file exists:
|Command| What  |
|--|--|
| dotnet cake |Download and build agent, download required node files, build extension |
| dotnet cake --target Build | Same as above |
| dotnet cake --target Publish| Build extension and publish it in marketplace|

## Debug agent

 1. Start agent with debugger enabled (starting to debug extension in VS will automatically start the agent with appropriate arguments)
> node --inspect --enable-source-maps index.js
 2. Open Chrome and type `chrome://inspect/` in address bar. Hit enter.
 3. Click `Open dedicated DevTools for Node`
 4. Wait a moment for DevTools to detect a new debugging session

Besides argument `--inspect` you can also use `--inspect-brk` to break before user code starts. More debugging options https://nodejs.org/en/learn/getting-started/debugging#command-line-options