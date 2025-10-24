
# Releases

Version number format: MAJOR.MINOR.PATH. Version numbering is automatic and uses the branch naming convention. For more details read [versioning.md](./versioning.md)
- preview version has an odd MINOR number
- release version has an even MINOR number

## Preview release

Follow these steps to publish a new preview to the [Private feed](https://sourcegraph.github.io/cody-vs/feed.xml).

1. **Create release branch**: Branch name must follow convention `vs-vMAJOR.MINOR.x` (e. g. `vs-v0.2.x`) where MINOR must be **even** number `git checkout -b vs-vMAJOR.MINOR.x`. NOTE: Although the MINOR number will be even in the branch name the preview version of the extension will have an odd number (MINOR -1) (e. g. `vs-v0.1.x`)
2. **Start publication**: Go to https://github.com/sourcegraph/cody-vs/actions/workflows/publish.yml and run workflow manually using following parameters: 
    - Use workflow from: release branch
    - Publish type: Preview
4. **Monitor Publication**: Once the workflow run is complete:
   - The new version of the extension will be published to the [Private feed](https://sourcegraph.github.io/cody-vs/feed.xml).
   - A pre-release will be created on GitHub.

4. **Visual Studio auto-updating with the latest Preview version**: Use preview gallery feed:
    - Tools -> Options -> Extensions
    - In the `Additional Extension Galleries` select `Add` and use https://sourcegraph.github.io/cody-vs/feed.xml as `URL`

## Stable release

NOTE: All releases are currently published automatically via GitHub Actions as Preview version.

Follow these steps to publish a new release to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).

1. **Coordinate with Marketing**: Contact the Marketing team in the Cody Slack channel approximately 2 days before the release to ensure a blog post is prepared.
2. **Create release branch**: Branch name must follow convention `vs-vMAJOR.MINOR.x` (e. g. `vs-v0.2.x`) where MINOR must be **even** number `git checkout -b vs-vMAJOR.MINOR.x`. Consider that the branch may already exist and be used to publish a preview version. In this case, you don't need to create a new branch just use an existing one.
3. **Update changelog file** Add changes to [CHANGELOG.md](../../CHANGELOG.md). Commit and push changes to the release branch
3. **Start publication**: Go to https://github.com/sourcegraph/cody-vs/actions/workflows/publish.yml and run workflow manually using following parameters: 
    - Use workflow from: release branch
    - Publish type: Release
4. **Monitor Publication**: Once the workflow run is complete:
   - The new version of the extension will be published to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=sourcegraph.cody-vs).
   - A release will be created on GitHub.

## Patch release

A patch release is necessary when a critical bug is discovered in the latest stable release that requires an immediate fix.

To publish a **patch** release:

1. Ensure all the changes for the patch are already committed to the latest release branch.
2. **Start publication**: Go to https://github.com/sourcegraph/cody-vs/actions/workflows/publish.yml and run workflow manually using following parameters: 
    - Use workflow from: release branch
    - Publish type: Release



