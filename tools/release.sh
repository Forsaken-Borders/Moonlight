#!/bin/bash

# Echo our actions before executing them
set -x

# Exit on error
set -e

TAG_NAME="$1"
NUGET_ORG_API_KEY="$2"
COMMIT_HASH=$(git rev-parse HEAD)
FILE=".github/data/commit_marks.ini"

# Build the project
mkdir build
dotnet pack -c Release -o build -p:Version="$TAG_NAME"

# Push to NuGet
dotnet nuget push "build/*" -k "$NUGET_ORG_API_KEY" -s https://api.nuget.org/v3/index.json

# Build the executables
for os in "win-x64" "osx-x64" "linux-x64" "linux-musl-x64" "win-arm64" "osx-arm64" "linux-arm64"; do
  # Invoke the shell command using the operating system value
  dotnet publish -c Release -r "$os" -o "build/$os/" -p:Version="0.2.0"
  zip -9r "build/moonlight-$os.zip" "build/$os/"
  rm -rf "build/$os/"
done

# Find the branches that contain the specific commit and iterate through them
git branch --contains "$COMMIT_HASH" | grep -v "detached" | awk '{print $1}' | while read -r branch_name; do
        # If the branch contains the commit, save the new commit hash to the file
        sed -i "s/^$branch_name=.*$/$branch_name=$COMMIT_HASH/" "$FILE"
    fi
done

# Sort the file alphabetically
sort -o "$FILE" "$FILE"

# Commit the new build hashes
git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"
git add .github/data/build_number
git commit -m "[ci-skip] Reset build hashes"
git push