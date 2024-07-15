# Cody for Visual Studio

Visual Studio extension for Cody, an AI coding assistant that uses search and codebase context to help you understand, write, and fix code faster.

## Setup Instructions for Sourcegraph Teammates

- Ask [#ask-it-tech-ops](https://sourcegraph.slack.com/archives/C01CSS3TC75) for a Windows 11 Pro and Visual Studio Pro license.
- [Install UTM and set up Windows 11](https://sourcegraph.notion.site/Testing-on-Windows-f99bb11428234872a716f739271ac225)
- Install [Visual Studio Pro](https://visualstudio.microsoft.com/vs/professional/) with the node.js and Visual Studio Extension components
- Install [git for Windows](https://gitforwindows.org/) and set up git to not change line endings (`git config core.autocrlf false`).
- Clone `git@github.com:sourcegraph/cody-vs.git`

See [CONTRIBUTING.md](./CONTRIBUTING.md) for more information.

## Used runtimes

 - visual studio extension use .NET Framework 4.7.2. This is Windows only runtime and it comes with the system itself and the Visual Studio installation.
 - agent use Node for build and run only. Visual Studio does not require this runtime.
 - build scripts use .NET runtime 8. This runtime is multiplatform and is only used by cake 

## Cake build automation
We use [Cake](https://cakebuild.net/) as build automation system. The building script can be found in the `build.cake` file. During the building and publishing process, we perform the following steps:
 1. Downloading a repository with an agent
 2. Selecting the commit used to build the agent
 3. Building agent
 4. Copying agent files to the folder used by the VS extension
 5. Downloading node binary files (x64 and arm64 versions)
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

## Access token
During development, you don't have to register and create a new token every time. You can use your own access token for Cody by using environment variables. To do this, open the command prompt and type:

`setx SourcegraphCodyToken YOUR_TOKEN`

To display your token, use the command
`echo $env:SourcegraphCodyToken`

After setting an environment variable, it is not always visible to already running applications. Therefore, it may be necessary to restart Visual Studio. This also applies to the command line.
Token from the environment variable always overrides the value from the user settings and is never saved in the user settings.

## Debug agent

 1. Start agent with debugger enabled (starting to debug extension in VS will automatically start the agent with appropriate arguments)
> node --inspect --enable-source-maps index.js
 2. Open Chrome and type `chrome://inspect/` in address bar. Hit enter.
 3. Click `Open dedicated DevTools for Node`
 4. Wait a moment for DevTools to detect a new debugging session

Besides argument `--inspect` you can also use `--inspect-brk` to break before user code starts. More debugging options https://nodejs.org/en/learn/getting-started/debugging#command-line-options
