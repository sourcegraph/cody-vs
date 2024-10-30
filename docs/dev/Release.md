
# Releases

Version number format follows [Semantic Versioning](https://semver.org/) of <major>.<minor>.<patch>.

## Stable release

NOTE: All releases are currently published automatically via GitHub Actions as Preview version.

Follow these steps to publish a new release to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).

1. **Coordinate with Marketing**: Contact the Marketing team in the Cody Slack channel approximately 2 days before the release to ensure a blog post is prepared.
2. **Update Extension Package Version**: Increment the version in [source.extension.vsixmanifest](../../src/Cody.VisualStudio/source.extension.vsixmanifest) and [CHANGELOG.md](../../CHANGELOG.md).
3. **Commit the Version Changes**: Commit the version increment with a message with `git commit -m Release vX.Y.Z`
4. **Create Pull Request**: Open a PR with the updated version.
5. **Tag the Release**: After the PR is merged (stable release only), create a git tag: `git tag vX.Y.Z`
6. **Push the Tag**: Push the tag to the remote repository to trigger the [Release workflow](https://github.com/sourcegraph/cody-vs/actions/workflows/release-preview.yml): `git push --tags`
7. **Monitor Publication**: Once the workflow run is complete:
   - The new version of the extension will be published to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).
   - A release will be created on GitHub with the release notes.

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

A patch release is necessary when a critical bug is discovered in the latest stable release that requires an immediate fix.

To publish a **patch** release:

1. Ensure all the changes for the patch are already committed to the `main` branch.
2. Check out the `main` branch and update the tags: `git fetch --tags`
3. Create a new branch for the patch release based on the tag of the latest release: `git checkout -b v<patch> v<latest>`
4. Replace <latest> with the latest stable release version, and <patch> should be the latest version incremented by 1: `git checkout -b v1.2.4 v1.2.3`
5. Cherry-pick the commits for the patch release into the patch branch: `git cherry-pick <commit-hash>`
6. Follow the steps for a stable release starting from step 2 to publish the patch release.

IMPORTANT: You do not need to merge the patch branch back into `main` as it is a temporary branch. However, you will need to update the version number in the `main` branch after the patch release is published.

## Nightly build

Nightly build is currently not supported.

## Running a release build locally

It can be helpful to build and run the packaged extension locally to replicate a typical user flow.

To do this:

1. Set the target branch / commit in build.cake.
2. Run `cd src; dotnet cake`.
3. Open the [src/Cody.sln](src/Cody.sln) file in Visual Studio.
4. Right click on the [Cody.Visual Studio](src/Cody.VisualStudio/) project and select `Rebuild Project`.
5. Once the project is built, it will create a `Cody.VisualStudio.vsix` file in the `src/Cody.VisualStudio/bin/Debug` folder, which you can double-click to install the extension.