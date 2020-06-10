# Contributing Guidelines

The `F0.Analyzers` open source project is hosted on GitHub, so we start this document off with the [GitHub Community Guidelines](https://help.github.com/en/github/site-policy/github-community-guidelines).

## Coding Style
See [.editorconfig](./source/.editorconfig) file.

See also
* [EditorConfig.org](https://editorconfig.org/)
* [Visual Studio Docs](https://docs.microsoft.com/en-us/visualstudio/ide/editorconfig-code-style-settings-reference)

## Development Techniques
Apply _test-driven development_ when adding features or fixing bugs.
Follow the rules of _Clean Code_ for the entire codebase.

## Version Scheme
See [Semantic Versioning](https://semver.org/).

Although .NET analyzers do not declare a public API, `MAJOR`, `MINOR` and `PATCH` changes for each release are observed from the user's point of view, consumed by tools like _Microsoft Visual Studio_, _JetBrains Rider_ or _MSBuild_.

## Branching Workflow
Based on the [GitHub flow](https://guides.github.com/introduction/flow/).

The [master](https://github.com/Flash0ver/F0.Analyzers) branch is the _default_ branch.
It includes all verified changes being published in the next release.
New _topic branches_ are created off of `master`.
Tags target the `master` branch.

The [publish](https://github.com/Flash0ver/F0.Analyzers/tree/publish) branch is the second long-running branch.
It represents the state of the _latest release_.
On each release published, the `publish` branch is updated from `master` and pushed to.

Every change to the repository is committed to short-lived _topic branches_.
New _topic branches_ are based on `master`.
Their work in progress is status checked, reviewed and discussed through _pull requests_.
When approved, the _topic branch_ may be merged into the `master` branch while maintaining a linear history.
After a successful merge, the topic branch should be deleted.

Topic branch naming conventions:
* all lowercase
* start with `commit type` (see [Commit Messages](#commit-messages))
* followed by a _slash_: `/`
* end with a short description, use _hyphen_ as word divider: `-`

Format: `{type}/{short-description}`\
Example: `docs/contributing-guidelines`

## Commit Messages
Based on [Conventional Commits](https://www.conventionalcommits.org/),
where a _major_/_breaking change_ is indicated by a `!` between the `type/scope` and the `:`,
and may optionally include a `BREAKING CHANGE:` footer with additional information.

Another common footer is `Co-authored-by: name <name@example.com>` to [attribute a commit to more than one author](https://help.github.com/en/github/committing-changes-to-your-project/creating-a-commit-with-multiple-authors). Such _trailers_ should be provided after the `BREAKING CHANGE:` footer.

Write both the _description_ and the optional _body_ of commit messages in _present tense imperative_.

## Changelog
Based on [Keep a Changelog](https://keepachangelog.com/),
where the `Unreleased` section is called `vNext`,
and where changes are grouped by product scope,
and where changes within a group begin with a _type_.

## Release Checklist
The ultimate requisite for each release is that all tests pass and the production codebase is both fully covered and asserted.

- bump _SemVer_ version
  - [NuGet package](./source/production/F0.Analyzers/F0.Analyzers.csproj): `/Project/PropertyGroup/Version`
  - [Visual Studio Extension](./source/extension/F0.Analyzers.Vsix/source.extension.vsixmanifest): `/PackageManifest/Metadata/Identity[@Version]`
- update Release Notes
  - [Changelog](./CHANGELOG.md): move the `vNext` section changes into a new release version section
  - [NuGet package](./source/production/F0.Analyzers/F0.Analyzers.csproj): `/Project/PropertyGroup/PackageReleaseNotes`
- ensure that the [Documentation](./documentation/) is updated
- commit changes
  - merge into `master` branch
  - update `publish` branch from `master` branch
- publish the release
  - upload to [NuGet.org](https://www.nuget.org/)
  - publish to [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
  - create [GitHub Release](https://github.com/Flash0ver/F0.Analyzers/releases)
    - with new _tag_ @ `master` (prefix with the letter `v`)
    - with _release title_ `F0.Analyzers {Version}` (without `v` prefix)
    - with description
