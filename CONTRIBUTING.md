# Contributing to Cody for Visual Studio

## Prerequisites

1. Install [node.js](https://nodejs.org/en/download/prebuilt-installer)
   1. Required for building and running Cody Agent
2. Install [Visual Studio Pro](https://visualstudio.microsoft.com/vs/professional/) with the required component:
   1. Visual Studio Extension
3. Install [git for Windows](https://gitforwindows.org)
   1. Configure it with `git config core.autocrlf false` to not change line endings
4. Install [.NET SDK](https://dotnet.microsoft.com/en-us/download)
5. Clone this repository: `git clone git@github.com:sourcegraph/cody-vs.git`

For Sourcegraph teammates:

- Install UTM and set up Windows 11 following the [Testing on Windows docs](https://sourcegraph.notion.site/Testing-on-Windows-f99bb11428234872a716f739271ac225)
- Ask in [#ask-it-tech-ops](https://sourcegraph.slack.com/archives/C01CSS3TC75) for a Windows 11 Pro and Visual Studio Pro license.

## Quick Start

To get started quickly, follow these steps:

### Open the Cody Project

1. In Visual Studio's Get started page, select `Open a project or solution`
2. Open the [Cody.sln](./src/Cody.sln) solution file

### Debugger

NOTE: You must build the agent before debugging for the first time.

1. Run `dotnet cake` inside the solution directory to build the agent.
2. In your Cody project, click `Debug` > `Start Debugging` (or press `F5`) to start the debugger
3. In the new window created by the debugger, open an existing project or create a new project
4. Now you can start setting breakpoints and debugging Cody!

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

1. Start the agent with the debugger enabled: `node --inspect --enable-source-maps index.js`
2. Open Chrome and navigate to `chrome://inspect/`
3. Click "Open dedicated DevTools for Node"
4. Wait for DevTools to detect the new debugging session

**Note:** Starting to debug the extension in Visual Studio will automatically start the agent with appropriate arguments.

Additional debugging options:

- Use `--inspect-brk` instead of `--inspect` to break before user code starts
- For more debugging options, refer to the [Node.js debugging documentation](https://nodejs.org/en/learn/getting-started/debugging#command-line-options)
