# Contributing to Cody for Visual Studio

## Prerequisites

1. Install [Visual Studio Pro](https://visualstudio.microsoft.com/vs/professional/) with the required components:
   1. node.js
   2. Visual Studio Extension
2. Install [git for Windows](https://gitforwindows.org)
   1. Configure it with `git config core.autocrlf false` to not change line endings
3. Clone this repository: `git clone git@github.com:sourcegraph/cody-vs.git`

For Sourcegraph teammates:

- Install UTM and set up Windows 11 following the [Testing on Windows docs](https://sourcegraph.notion.site/Testing-on-Windows-f99bb11428234872a716f739271ac225)
- Ask in [#ask-it-tech-ops](https://sourcegraph.slack.com/archives/C01CSS3TC75) for a Windows 11 Pro and Visual Studio Pro license.

## Quick Start

### Open the Cody Project

1. In Visual Studio's Get started page, select `Open a project or solution`
2. Open the [Cody.sln](./src/Cody.sln) file

### Debugger

1. In your Cody project, click `Debug` > `Start Debugging` (or press `F5`) to start the debugger
2. In the new window created by the debugger, open an existing project or create a new project
3. Now you can start setting breakpoints and debugging Cody!
