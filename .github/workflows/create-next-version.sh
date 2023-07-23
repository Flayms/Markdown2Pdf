#!/bin/bash

FILE_NAME=$1
LAST_TAG=$(git ls-remote --tags --sort=committerdate | grep -o 'v.*' | sort -r | head -1)

MAJOR_INDEX=0
MINOR_INDEX=1
PATCH_INDEX=2
INDEX_TO_INCREASE=$PATCH_INDEX

echo "Last Tag: $LAST_TAG"

# Create file
git log $LAST_TAG..HEAD --no-merges --oneline > $FILE_NAME

echo "Created file '$FILE_NAME' with following content:"
echo "$(<$FILE_NAME)"
echo "----"

# loop over each commit to determine new version
while IFS="" read -r p || [ -n "$p" ]
do
  COMMIT_HASH=${p:0:6}
  echo "parsing $COMMIT_HASH"

  COMMIT_MSG=`git log --format=%B -n 1 $COMMIT_HASH`

  # determine major changes
  # todo: by conventional commits specification the BREAKING CHANGE is only allowed in the footer
  if [[ "$COMMIT_MSG" == *"BREAKING CHANGES"* ]]; then
    INDEX_TO_INCREASE=$MAJOR_INDEX
    break
  fi
  
  MSG_HEADER=`echo "${COMMIT_MSG}" | head -1`

  # todo: check if this really works
  # header containing '!' after the type/scope
  if [[ "$MSG_HEADER" =~ ^(\w.)+(\(.*\))?!: ]]; then
    INDEX_TO_INCREASE=$MAJOR_INDEX
    break
  fi

  # todo: check if this really works
  # determine minor changes
  # header like 'feat[optional scope]:'
  if [[ "$MSG_HEADER" =~ ^(feat)+(\(.*\))?!?: ]]; then
    INDEX_TO_INCREASE=$MINOR_INDEX
  fi

  # don't need to increase patch because we always increase patch by default
  #if [[ "$MSG_HEADER" =~ ^(fix)+(\(.*\))?!?: ]]; then
  #  INDEX_TO_INCREASE=$MINOR_INDEX
  #fi

done < $FILE_NAME

#----DEBUG----
#LAST_TAG="v3.4.5"
#INDEX_TO_INCREASE=2
#-------------

# remove v letter
LAST_TAG_TRIMMED="${LAST_TAG:1}"
MAJOR=$(echo $LAST_TAG_TRIMMED | awk -F \. {'print $1'})
MINOR=$(echo $LAST_TAG_TRIMMED | awk -F \. {'print $2'})
PATCH=$(echo $LAST_TAG_TRIMMED | awk -F \. {'print $3'})

# increase major
if [[ $INDEX_TO_INCREASE = $MAJOR_INDEX ]]; then
  echo "increasing major"
  let "MAJOR=MAJOR+1"
  MINOR=0
  PATCH=0
fi

# increase minor
if [[ $INDEX_TO_INCREASE = $MINOR_INDEX ]]; then
  echo "increasing minor"
  let "MINOR=MINOR+1"
  PATCH=0
fi

# increase patch
if [[ $INDEX_TO_INCREASE = $PATCH_INDEX ]]; then
  echo "increasing patch"
  let "PATCH=PATCH+1"
fi

VERSION_NAME="$MAJOR.$MINOR.$PATCH"
TAG_NAME="v$NEXT_VERSION"
RELEASE_NAME="Version $TAG_NAME"

# informational output
echo "Tag_Name:      $TAG_NAME"
echo "Release_Name:  $RELEASE_NAME"
echo "Nuget_Version: $VERSION_NAME"

# write to github enviornment variables
TEST_VALUE="Hello Variable!"
echo "Tag_Name=$TAG_NAME" >> $GITHUB_ENV
echo "Release_Name=$RELEASE_NAME" >> $GITHUB_ENV
echo "Nuget_Version=$VERSION_NAME" >> $GITHUB_ENV