## Prepare your development environment
Install the following components:
 - Visual Studio 2022 (at least 17.10.2 version) with Visual Studio SDK (Visual Studio extension development)
 - Node.js (at least 20.14.0 version)
 - pnpm (version 8.6.7)
 - .NET SDK (at least 8.0.302 version)
 - git
 
## Commands
We use Cake as build automation system. The billing script can be found in the `build.cake` file. 
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