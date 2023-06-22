#!/bin/bash

OWNER_AND_REPO_NAME=$1
CURRENT_TAG=$2
LAST_TAG=$(git describe --tags --abbrev=0)
FILE_NAME=release-notes.md

echo "Creating $FILE_NAME"

git log $LAST_TAG..HEAD --no-merges --oneline > $FILE_NAME
echo "" >> $FILE_NAME
echo "---" >> $FILE_NAME
echo "**Full Changelog**: https://github.com/$OWNER_AND_REPO_NAME/compare/$LAST_TAG...$CURRENT_TAG" >> $FILE_NAME