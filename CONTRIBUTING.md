# Contributing to Cody for Visual Studio

This is the POC (proof of concept) branch for developing purpose, created off the v0.0.0 of Cody.

Do not use this branch for production purposes.

## Prerequisites

1. Install [chocolatey](https://chocolatey.org/install) - Package Manager for Windows
   1. `Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))`
2. Install [node.js](https://nodejs.org/en/download/prebuilt-installer), required for building and running Cody Agent
   1. `choco install pnpm --version=8.6.7`
3. Install [Visual Studio Pro](https://visualstudio.microsoft.com/vs/professional/) with the required component:
   1. Visual Studio Extension
4. Install [git for Windows](https://gitforwindows.org)
   1. Configure it with `git config core.autocrlf false` to not change line endings
5. Install [.NET SDK](https://dotnet.microsoft.com/en-us/download)
6. Clone this repository: `git clone git@github.com:sourcegraph/cody-vs.git`

For Sourcegraph teammates:

- Install UTM and set up Windows 11 following the [Testing on Windows docs](https://sourcegraph.notion.site/Testing-on-Windows-f99bb11428234872a716f739271ac225)
- Ask in [#ask-it-tech-ops](https://sourcegraph.slack.com/archives/C01CSS3TC75) for a Windows 11 Pro and Visual Studio Pro license.

## Quick Start

To get started quickly, follow these steps:

### Open the Cody Project

1. In Visual Studio's Get started page, select `Open a project or solution`
2. Open the [Cody.sln](./src/Cody.sln) solution file

### Debugger

#### Visual Studio

NOTE: You must build the agent before debugging for the first time.

1. Run `dotnet cake` inside the `src` directory to build the agent.
2. In your Cody project, click `Debug` > `Start Debugging` (or press `F5`) to start the debugger
3. In the new window created by the debugger, open an existing project or create a new project
4. Now you can start setting breakpoints and debugging Cody!

#### Visual Studio with Agent running in VS Code locally

1. Download and install VS Code on your machine
   Clone the main Cody repository: `git clone git@github.com:sourcegraph/cody.git`
   1. Makes sure the `cody` repository is in the same directory as the `cody-vs` repository
2. `cd` into the `cody` repository and run `pnpm install`
3. Open the `cody` repository in VS Code
4. After you have set the breakpoints, open the debug panel from selecting `View > Run`
5. In the drop down menu next to `RUN AND DEBUG`, select `Launch Agent port 3113` to start the debugger for Agent
6. To enable Visual Studio listening to the Agent running on Port 3113, set CODY_VS_DEV_PORT with `setx CODY_VS_DEV_PORT 3113`
7. After the Agent is built and launched, start the debugger on the Visual Studio side following the steps above

### Running VS Integration Tests + Playwright

1. Open Cody.sln in the Visual Studio
2. Select Visual Studio menu Test->Test Explorer
3. Right-click on the `Cody.VisualStudio.Tests`, select Run

## Runtime Requirements

This project uses different runtimes for various components:

### Visual Studio Extension

- **Runtime**: .NET Framework 4.7.2
- **Platform**: Windows only
- **Note**: This runtime is typically included with Windows and Visual Studio installations.

### Agent

- **Runtime**: Node.js
- **Usage**: Used for build and run processes only
- **Note**: Not required for Visual Studio functionality

### Build Scripts

- **Runtime**: .NET 8
- **Platform**: Cross-platform
- **Usage**: Exclusively used by Cake build automation system

Please ensure you have the appropriate runtimes installed for the components you intend to work with.

## Cake Build Automation

We use [Cake](https://cakebuild.net/) as our build automation system. The build script can be found in the `build.cake` file. Our building and publishing process includes the following steps:

1. Downloading the agent repository
2. Selecting the commit for building the agent
3. Building the agent
4. Copying agent files to the VS extension folder
5. Downloading node binary files (x64 and arm64 versions)
6. Copying node files to the VS extension folder
7. Building the VS extension using MSBuild
8. Publishing to the marketplace

## Build Commands

Execute these commands from the directory containing the `build.cake` file:

| Command                        | Description                                                             |
| ------------------------------ | ----------------------------------------------------------------------- |
| `dotnet cake`                  | Download and build agent, download required node files, build extension |
| `dotnet cake --target Build`   | Same as above                                                           |
| `dotnet cake --target Publish` | Build extension and publish it to the marketplace                       |

## Access Token

Create an account and go to https://sourcegraph.com/user/settings/tokens to create a token.

During development, you can use your own Cody access token by setting an environment variable. This eliminates the need to register and create a new token for each session.

To set your access token:

```bash
setx SourcegraphCodyToken YOUR_TOKEN
```

To display your token:

```bash
echo $env:SourcegraphCodyToken
```

**Note:** After setting an environment variable, you may need to restart Visual Studio and any open command prompts for the changes to take effect.

The token from the environment variable always overrides the value from the user settings and is never saved in the user settings.

## Debugging the Agent

To debug the agent:

1. Start the agent with the debugger enabled: `node --inspect --enable-source-maps ../cody-dist/agent/dist/index.js api jsonrpc-stdio`
2. Open Chrome and navigate to `chrome://inspect/`
3. Click "Open dedicated DevTools for Node"
4. Wait for DevTools to detect the new debugging session

**Note:** Starting to debug the extension in Visual Studio will automatically start the agent with appropriate arguments.

Additional debugging options:

- Use `--inspect-brk` instead of `--inspect` to break before user code starts
- For more debugging options, refer to the [Node.js debugging documentation](https://nodejs.org/en/learn/getting-started/debugging#command-line-options)
