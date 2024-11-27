# Visual Studio Cody client versioning

## Distribution channels
- **Nightly** Every day, a version of the extension is built containing the latest changes from the main branch and using the latest stable version of the agent. The version built from this channel is not distributed anywhere, so it is not assigned a version number. It can be downloaded from the GitHub Actions artifacts for each run.
- **Preview** This version is intended for QA testing and is also available to insiders who are interested in early versions. The version is distributed to GitHub Release and to a private feed (https://sourcegraph.github.io/cody-vs/feed.xml)
- **Release (production)** Stable version available in GitHub Release and in Visual Studio Marketplace.

## Versioning
Visual Studio extensions do not support semantic versioning. Only version numbers containing digits are accepted. Therefore, it became necessary to separate versioning for preview and release versions. The **preview version has an odd minor number**, while the **release version has an even minor number**. The numbering is shared between the preview and release versions. Therefore, it is important for subsequently numbered versions of the extension to have a semantically larger version number.
Examples:
 - ❌1.23.3-beta1
 - ❌v0.3.3
 - ❌3.0
 - ✅3.0.0
 - ✅0.1.24

## Branch Naming Convention
Each branch from which the next version is to be created should follow the format: vs-vMAJOR.MINOR.x
where **MINOR must be an even positive number**. MAJOR can be any positive number.
Examples:
- ✅vs-v0.12.x
- ✅vs-v2.22.x
- ❌vs-0.12.x
- ❌vs-v0.12.0
- ❌vs-v0.3.x

## Version auto-numbering
During the publishing process, a new version number is generated automatically. The numbering is determined based on the branch name and whether it is a preview or production version. Version follow the format: MAJOR.MINOR.PATH

### For the preview version
MAJOR - remains the same as the MAJOR from the branch\
MINOR - the MINOR from the branch minus one\
PATCH - the next available number starting from 0 (determined based on existing tags)\
Example: branch `vs-v0.12.x`  -> version `0.11.0` (next version `0.11.1`, next next version `0.11.2`)

Special case:
When MINOR is 0, e.g., the branch name is vs-v2.0.x. In this case:\
MAJOR - the MAJOR from the branch minus one\
MINOR - is set to 999\
PATCH - the next available number starting from 0\
Example: branch `vs-v2.0.x`  -> version `1.999.0` (next version `1.999.1`, next next version `1.999.2`)

### For a release version
MAJOR - is the same as the MAJOR from the branch\
MINOR - is the same as the MINOR from the branch\
PATCH - the next available number starting from 0 (determined based on existing tags)\
Example: branch `vs-v0.12.x`  -> version `0.12.0` (next version `0.12.1`, next next version `0.12.2`)
> [!NOTE]
> PATH for each branch and version (preview and release) is calculated separately!

## Tags
An element of the publication process is the automatic addition of a tag indicating the version being published.
 - preview version e.g.  `vs-insiders-v0.1.0`
 - release version e.g.  `vs-v.0.2.1`

## Setting the agent version
The agent version can be set independently from the extension version. This is done by editing the `agent/agent.version` file in the repository. Acceptable values include the branch name, tag or commit hash from the Cody repository. It is recommended to use only tags, as they indicate a specific stable version of the agent.

## Release Captain responsibilities
- Upgrate agent version if needed
- Create and push branch with proper naming
- Run Publish workflow with the selected branch and type of publication (preview or release)

## Example release scenario
You are the release captain, and your task is to publish a new version of Cody for Visual Studio. To do this, you select the latest commit from the repository that will be included in the new version and create a release branch. The name of this branch is important as it will be used to generate the version number. Upon checking the tags, you found that the last stable version is `0.8.1`. Therefore, the name of the new release branch will be `vs-v0.10.x`. Before publishing the final version, you always conduct QA tests. For this, a preview version is required. You create it by running the 'Publish' workflow in GitHub Actions (select branch `vs-v0.10.x` and type 'Preview'). The published preview version will have the number `0.9.0`. It will be tagged in the repository as `vs-insiders-v0.9.0`. You ask the QA team to conduct the tests.

One of the QA team members discovers a issue. You verify the information, and it is indeed a bug that needs fixing. The developers will take care of this. After preparing the fix, another verification is necessary, so you publish a new preview version. You ensure that the commit containing the fix is on the `vs-v0.10.x` branch and, just like the first time, run the 'Publish' workflow. The new preview version will be numbered `0.9.1` and will be tagged as `vs-insiders-v0.9.1`. You pass the version information to the QA team. This time, the tests go smoothly, and the team does not report any issues. We are ready to publish the stable version.

You inform the involved teams that you intend to publish the new version. It will be widely available in the Visual Studio Marketplace. To do this, you run the 'Publish' workflow, similar to before. You select the branch `vs-v0.10.x`, but this time you choose 'Release' as the type. The new stable version will be numbered `0.10.0` and will be tagged in the repository as `vs-v0.10.0`. After publishing, you verify its availability in the Visual Studio Marketplace. You know that sometimes the published version appears with a delay of several minutes.

A few days after the stable release, one of the extension's users reports a issue. You verify the report. It is not a serious issue, but you don't want the user to wait for a fix until the next major release. You assign the fix. Before publishing, you ensure that the commit with the fix is on the `vs-v0.10.x` branch. You can now publish another preview version (which would be numbered `0.9.2`) and pass it for QA, or if the change was minimal and low-risk, you can immediately publish a stable version. You choose the latter. The new stable version will be numbered `0.10.1`.

By following these scenario, a structured approach is taken to release management, ensuring quality and timely updates for the users.