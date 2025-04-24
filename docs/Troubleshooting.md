## Troubleshooting Cody for Visual Studio extension

### Access Cody logs

Visual Studio logs can be accessed via the **Output** panel. To access logs:

- Open `View -> Output` from the main menu
- Select `Cody` output pane

![Cody Default Output](./images/Cody-ouput.png)

### Autocomplete

Cody autocomplete for Visual Studio is using underlaying VS API to get it displayed in the editor, and it's turned on by default in VS (`Tools -> Options -> IntelliCode -> Show Inline Completions`). Without it, it won't work so this is the first thing to check:

![Inline completions](./images/Inline-completions.png)

Also make sure that `Tools -> Options -> Cody -> Automatically trigger completions`, is turned on (it is by default). 

![Autocomplete](./images/Autocomplete.png)

The autocomplete is supported from Visual Studio 17.6+, and includes support for languages:

1. C/C++/C#
2. Python
3. JavaScript/TypeScript/TSX
4. HTML
5. CSS
6. JSON

#### Non-trusted certificates 

When autocomplete still doesn't work (or the Cody Chat), you could try **turning on** accepting `non-trusted certificates` option (requires Visual Studio restart). It should help especially in the enterprise settings, if someone is behind a firewall:

![Non-trusted-certificates](./images/Non-trusted-certificates.png)

### Detailed debugging logs

The detailed logging configuration can be turned on by adding `CODY_VS_DEV_CONFIG` environment variable containing the full path to [the configuration file](https://github.com/sourcegraph/cody-vs/blob/main/src/CodyDevConfig.json) placed somewhere in the filesystem:

![Detailed logs](./images/Detailed-logs.png)

Two additional output panes, `Cody Agent` and `Cody Notifications` will be created with more detailed logs. More information how to [configure them](https://github.com/sourcegraph/cody-vs/blob/main/CONTRIBUTING.md#developer-configuration-file)

![Cody output panes](/images/Cody-output-panes.png)
