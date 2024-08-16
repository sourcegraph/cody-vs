
# Releases

Version number format follows [Semantic Versioning](https://semver.org/) of <major>.<minor>.<patch>.

## Stable release

Follow these steps to publish a new release to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).

1. **Coordinate with Marketing**: Contact the Marketing team in the Cody Slack channel approximately 2 days before the release to ensure a blog post is prepared.
2. TBC

### Release checklist

Include the following checklist in the PR description when creating a new release.

```markdown
Release Checklist:
    - [ ] Update version number
    - [ ] Update [CHANGELOG.md](./CHANGELOG.md)
    - [ ] Link to PR for the release blog post (if any)
```

Note: Ensure all checklist items are completed before merging the release PR.

## Patch release

To publish a **patch** release

1. Make sure all the changes for the patch are already committed to the `main` branch.
2. TBC

## Insiders builds

Insiders builds are nightly (or more frequent) builds with the latest from `main`. They're less stable but have the latest changes. Only use the insiders build if you want to test the latest changes.

### Using the insiders build

To use the Cody insiders build:

1. Install the extension from the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).

### Publishing a new insiders build

To manually trigger an insiders build:

1. TBC

## Running a release build locally

It can be helpful to build and run the packaged extension locally to replicate a typical user flow.

To do this:

1. Set the target branch / commit in build.cake.
2. Run `cd src && dotnet cake`.
3. Open the [src/Cody.sln](src/Cody.sln) file in Visual Studio.
4. Right click on the [Cody.Visual Studio](src/Cody.VisualStudio/) project and select `Rebuild Project`.
5. Once the project is built, it will create a `Cody.VisualStudio.vsix` file in the `src/Cody.VisualStudio/bin/Debug` folder, which you can double-click to install the extension.