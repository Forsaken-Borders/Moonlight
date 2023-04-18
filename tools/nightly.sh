#!/bin/bash

# Echo our actions before executing them
set -x

# Exit on error
set -e

BRANCH_NAME="$1"
NUGET_ORG_API_KEY="$2"
PUSH_COMMIT=0

# The data file `.github/data/commit_marks` is in KEY=VALUE format where:
# KEY is the branch name
# VALUE is the last commit from the release workflow.
# If the branch name is not found in the file, the function saves the current commit to the branch name.
# The function returns the count between the saved commit and the current commit.
get_or_create() {
    local file=".github/data/commit_marks"
    local latest_commit=$(git rev-parse "$BRANCH_NAME")

    # Checkout the master branch as we're only interested in the data file stored in the master branch
    git checkout master

    # Create the data directory if it doesn't exist
    mkdir -p .github/data

    # Create the file if it doesn't exist
    touch "$file"

    # Search for the branch name in the file
    if ! grep -q -F "^$BRANCH_NAME=" "$file"; then
        echo "$BRANCH_NAME=$latest_commit" >> "$file"
        git config --global user.email "github-actions[bot]@users.noreply.github.com"
        git config --global user.name "github-actions[bot]"
        git add .github/data/build_number
        git commit -m "[ci-skip] Add build hash for '$BRANCH_NAME' branch"
        PUSH_COMMIT=1
    fi

    # Get the saved commit
    local existing_run_number=$(grep "^$BRANCH_NAME=" "$file" | cut -d'=' -f2)

    # Switch back to the branch we were on
    git checkout "$BRANCH_NAME"

    # Count the commits between the saved commit and the current commit (inclusive)
    echo $(git rev-list --count "$existing_run_number".."$latest_commit")
}

BUILD_NUMBER=$(printf "%0*d\n" 5 $(get_or_create))
echo $BUILD_NUMBER >> $GITHUB_ENV

# Build and package the project
mkdir build
dotnet pack -o build \
    --include-symbols \
    --include-source \
    -p:SymbolPackageFormat=snupkg \
    -p:VersionSuffix="nightly-$BUILD_NUMBER"

# Push if this is a commit to the master branch
if [[ "${$BRANCH_NAME##*/}" == "master" ]]; then
    dotnet nuget push "build/*" -k "$NUGET_ORG_API_KEY" -s https://api.nuget.org/v3/index.json

    # Run the Discord tool
    dotnet run --project ./tools/AutoUpdateChannelDescription -p:VersionSuffix="nightly-$BUILD_NUMBER"
fi

# Push the commit if we made one
if [[ $PUSH_COMMIT == 1 ]]; then
    git push
fi