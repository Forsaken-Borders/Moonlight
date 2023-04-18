#!/bin/bash

# Echo our actions before executing them
set -x

# Exit on error
set -e

# Build the project
mkdir build
dotnet pack -o build \
    -c Release \
    -p:Version=${{ github.event.release.tag_name }}

# Push to NuGet
dotnet nuget push "build/*" -k "${{ secrets.NUGET_ORG_API_KEY }}" -s https://api.nuget.org/v3/index.json

## Update the build hashes
# Get the commit hash
commit_hash=$(git rev-parse HEAD)

# Find the branches that contain the specific commit and iterate through them
git branch --contains "$commit_hash" | grep -v "detached" | awk '{print $1}' | while read -r branch; do
        # If the branch contains the commit, save the new commit hash to the file
        sed -i "s/^$branch_name=.*$/$branch_name=$commit_hash/" "$file"
    fi
done

# Sort the file alphabetically
sort -o "$file" "$file"

# Commit the new build hashes
git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"
git add .github/data/build_number
git commit -m "[ci-skip] Reset build hashes"
git push