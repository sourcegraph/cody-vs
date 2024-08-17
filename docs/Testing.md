# Testing Cody for Visual Studio

This test plan outlines the comprehensive testing procedures for Cody in Visual Studio.

It covers various features including Chat functionality, Context handling, LLM selection, and Command execution. The plan details specific steps to verify the Chat View's opening, closing, and basic interactions, as well as testing context-awareness and LLM selection for different user tiers. It also includes instructions for testing built-in commands like Explain Code and Find Code Smells, along with instructions for creating and verifying custom commands, and prompts from the prompt library.

## Checklist

- Autocomplete
  - [ ] Not available
- Chat
  - [ ] [Chat View](#chat-view)
  - [ ] [Context](#context)
  - [ ] [LLM Selection](#llm-selection)
- Commands
  - [ ] [Explain](#explain)
  - [ ] [Smell](#smell)
  - [ ] [Prompt Library](#prompt-library)
  - [ ] [Custom Commands](#custom-commands)
- Edit
  - [ ] Not available

## Chat

### Chat View

#### Opening and Closing

- [ ] Open Cody chat window: Go to `View` > `Other Windows` > `Cody Chat`
- [ ] Verify the chat window opens with the `Chat` tab selected
- [ ] Close the chat window using the `X` button in the upper right corner
- [ ] Verify that pressing `Alt + L` reopens the chat window

#### Basic Functionality

- [ ] Submit a chat prompt by pressing `Enter` or clicking the `Send` button
- [ ] Clear the current chat to start a new session using the `New Chat` button
- [ ] Ask Cody a question that includes a request for code generation
- [ ] Verify that Cody displays a loading state while generating a response
- [ ] Confirm the ability to stop Cody's response generation mid-process

#### Interaction and Follow-ups

- [ ] Ask a follow-up question in the same chat window and verify Cody's response
- [ ] Edit a previous chat prompt and confirm that Cody generates a new answer

### Additional Tests

- [ ] Test multi-line input in the chat prompt
- [ ] Check for proper handling of special characters and code snippets
- [ ] Ensure chat history persists between sessions

#### Known Limitations

- The Copy button does not copy code to the clipboard
- The Insert button under code blocks does not insert code into the editor
- The New File button under code blocks does not create a new file in the editor

### Context

- [ ] Open a solution project in your editor.
- [ ] Open the Cody chat window and verify that the project directory name appears as an @-mention in the chat input.
- [ ] Open a file from your solution and select some code. Verify that the file name appears as an @-mention in the chat input.
- [ ] Test the @-mention functionality: Type a file name followed by '@' and verify that you can select from a list of files.

#### Known Limitations

- There is no progress indicator in the Chat UI when Cody is indexing the codebase.

### LLM Selection

#### Free User Experience

- [ ] Sign in as a Free user and open a new chat.
- [ ] Verify that the default LLM is Claude 3.5 Sonnet.
- [ ] Check that you can switch between LLM options not marked as Pro.
- [ ] Confirm that clicking on a Pro-marked LLM option redirects to the Pro subscription page.

#### Account Management

- [ ] Open the "Account" tab and verify it displays "Cody Free" plan.
- [ ] Confirm the "Manage Subscription" button links to the subscription page.
- [ ] Test the "Sign Out" button in the "Account" tab.
- [ ] Verify that signing out redirects to the sign-in page.

#### Pro User Experience

- [ ] Sign in as a Pro user using manual credential entry.
- [ ] Verify all LLM options in the dropdown list are available and switchable.

#### Enterprise User Experience

- [ ] Sign in as an enterprise user.
- [ ] Confirm that LLM selection is not available (cannot be changed).

## Commands

### Explain Code

- [ ] Verify that the `Explain Code` command is available in the following locations:
  - [ ] Sidebar Chat home page under the `Prompts & Commands` section
  - [ ] `Prompts & Commands` tab under the `Commands` section
- [ ] Test the `Explain Code` command:
  1. Highlight a section of code in a file
  2. Run the `Explain Code` command
  3. Verify that Cody provides an explanation of the selected code in a new chat window
  4. Confirm that the chat is added to the `History` tab

### Find Code Smells

- [ ] Verify that the `Find Code Smells` command is available in the following locations:
  - [ ] Sidebar Chat home page under the `Prompts & Commands` section
  - [ ] `Prompts & Commands` tab under the `Commands` section
- [ ] Test the `Find Code Smells` command:
  1. Place the cursor within a function in a file
  2. Run the `Find Code Smells` command
  3. Verify that Cody provides suggestions for improving the function in a new chat window
  4. Confirm that the chat is added to the `History` tab

### General Command Verification

- [ ] Ensure that all commands are easily accessible and clearly labeled
- [ ] Verify that command results are displayed consistently in new chat windows
- [ ] Check that all command-generated chats are properly logged in the relevant history or chat sections

## Custom Commands

### Creating User Custom Commands

To create a user-specific custom command:

1. Create a new JSON file named `commands.json` in the `.cody` directory of your home folder.
2. Add your custom command to the file. For example:

```json
{
  "commands": {
    "summarize": {
      "prompt": "Summarize the selected code in 3-5 sentences",
      "context": {
        "currentFile": true,
        "selection": true
      }
    }
  }
}
```

### Creating Workspace Custom Commands

To create a workspace-specific custom command:

1. Create a new JSON file named `commands.json` in the `.cody directory` at the root of your project.
2. Add your custom command to the file. For example:

```json
{
  "commands": {
    "checker": {
      "prompt": "Check for spelling mistakes in the selected code",
      "context": {
        "selection": true
      }
    }
  }
}
```

### Verifying Custom Commands

- [ ] Ensure custom commands appear in the new chat view and the "Prompts & Commands" tab.
- [ ] Verify that summarize and checker are listed in the Commands section above the default commands.
- [ ] Test the newly created commands by selecting and running them.
- [ ] Confirm that responses appear in a new chat window and align with the command prompts.

## Prompt Library

- [ ] Verify the availability of the `Prompt Library` command in:
  - [ ] The sidebar Chat home page under the `Prompts & Commands` section.
  - [ ] The `Prompts & Commands` tab above the `Commands` section.
- [ ] Confirm that selecting `+ New` redirects to the Sourcegraph instance for prompt creation.
- [ ] Verify that your created prompts are visible in the `Prompt Library` tab.
- [ ] Ensure that clicking `Manage` redirects to the Sourcegraph instance for prompt management.
- [ ] Verify that selecting a prompt inserts it into the chat input box.
- [ ] Confirm that you can run the prompt by pressing Enter or clicking the `Send` button.

### Known Limitations

- The `Manage` button in the Command section is not functional.
