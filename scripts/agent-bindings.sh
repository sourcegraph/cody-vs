#!/bin/bash

# Function to determine the operating system
get_os() {
    case "$(uname -s)" in
    Darwin*) echo "mac" ;;
    MINGW* | CYGWIN* | MSYS*) echo "windows" ;;
    *) echo "unknown" ;;
    esac
}

# Get the current operating system
OS=$(get_os)

# Function to execute commands based on the OS
execute_command() {
    if [ "$OS" = "windows" ]; then
        powershell -Command "$1"
    else
        eval "$1"
    fi
}

# Check if cody-dist directory exists
if [ ! -d "cody-dist" ]; then
    echo "cody-dist directory not found. Cloning repository..."
    execute_command "git clone https://github.com/sourcegraph/cody.git cody-dist"

else
    echo "cody-dist directory found."
fi

echo "Installing dependencies and building..."
execute_command "cd cody-dist && pnpm install && cd agent && pnpm build && cd .."

# Navigate to cody-dist directory and run the ts-node command
execute_command "pnpm exec ts-node agent/src/cli/scip-codegen/command.ts --output \"agent/bindings/csharp\" --language \"csharp\" --kotlin-package \"Cody.Core.Agent.Protocol\""

# Navigate back to repo root
execute_command "cd .."

execute_command "pwd"

# Move files from cody-dist/agent/bindings/csharp to src/Cody.Core/Agent/Protocol
if [ "$OS" = "windows" ]; then
    execute_command "Move-Item -Force cody-dist/agent/bindings/csharp/* src/Cody.Core/Agent/Protocol/"
else
    execute_command "mv -f cody-dist/agent/bindings/csharp/* src/Cody.Core/Agent/Protocol/"
fi

echo "Script execution completed."
