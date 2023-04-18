#!/bin/bash

# Echo our actions before executing them
set -x

# Exit on error
set -e

# The data file `.github/data/commit_marks` is in KEY=VALUE format where:
# KEY is the branch name
# VALUE is the last commit from the release workflow.
# If the branch name is not found in the file, the function saves the current commit to the branch name.
# The function returns the count between the saved commit and the current commit.
get_or_create() {
    local branch_name="$1"
    local file="$2"
    local latest_commit=$(git rev-parse "$branch_name")

    # Checkout the master branch as we're only interested in the data file stored in the master branch
    git checkout master

    # Create the data directory if it doesn't exist
    mkdir -p .github/data

    # Create the file if it doesn't exist
    touch "$file"

    # Search for the branch name in the file
    grep -q -F "^$branch_name=" "$file" || echo "$branch_name=$latest_commit" >> "$file"

    # Get the saved commit
    local existing_run_number=$(grep "^$branch_name=" "$file" | cut -d'=' -f2)

    # Switch back to the branch we were on
    git checkout "$branch_name"

    # Count the commits between the saved commit and the current commit (inclusive)
    echo $(git rev-list --count "$existing_run_number".."$latest_commit")
}

BUILD_NUMBER=$(printf "%0*d\n" 5 $(get_or_create "${{ github.ref_name }}" ".github/data/commit_marks"))
echo $BUILD_NUMBER >> $GITHUB_ENV

# Build and package the project
mkdir build
dotnet pack -o build \
    --include-symbols \
    --include-source \
    -p:SymbolPackageFormat=snupkg \
    -p:VersionSuffix="nightly-$BUILD_NUMBER"

# Push if this is a commit to the master branch
if [[ "${GITHUB_REF##*/}" == "master" ]]; then
    dotnet nuget push "build/*" -k "${{ secrets.NUGET_ORG_API_KEY }}" -s https://api.nuget.org/v3/index.json

    # Run the Discord tool
    DISCORD_TOKEN="${{ secrets.DISCORD_TOKEN }}"
    DISCORD_GUILD_ID="${{ secrets.DISCORD_GUILD_ID }}"
    DISCORD_CHANNEL_ID="${{ secrets.DISCORD_CHANNEL_ID }}"
    DISCORD_CHANNEL_TOPIC="${{ secrets.DISCORD_CHANNEL_TOPIC }}"
    NUGET_URL="${{ secrets.NUGET_URL }}"
    GITHUB_URL="${{ github.server_url }}/${{ github.repository }}"
    LATEST_STABLE_VERSION=$(git describe --tags $(git rev-list --tags --max-count=1))
    dotnet run --project ./tools/AutoUpdateChannelDescription -p:VersionSuffix="nightly-$BUILD_NUMBER"
fi