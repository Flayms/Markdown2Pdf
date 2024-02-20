#!/bin/bash

OWNER_AND_REPO_NAME=$1
CURRENT_TAG=$2
FILE_NAME=$3
declare -A SYMBOLS=(
    [feat]=🚀
    [feature]=🚀
    [fix]=🐛
    [bug]=🐛
    [docs]=📚
    [documentation]=📚
    [style]=🎨
    [refactor]=🔧
    [refac]=🔧
    [perf]=⚡
    [test]=🔬
    [build]=🏗
    [ci]=🤖
    [chore]=🧹
    [revert]=⏪
)

LAST_TAG=$(git ls-remote --tags --sort=committerdate | grep -o 'v.*' | sort -r | head -1)

echo "Creating $FILE_NAME"

log=$(git log $LAST_TAG..HEAD --no-merges --oneline)

# add emojis
for key in ${!SYMBOLS[@]}
do
  value=${SYMBOLS[$key]}

  reg="^(\w{7}\s)($key(\((\w)+\))?:)((.)+)$"
  log=$(echo "$log" | sed -r "s/$reg/\1$value\2\5/")
done

echo "$log" > $FILE_NAME
echo "" >> $FILE_NAME
echo "---" >> $FILE_NAME
echo "**Full Changelog**: https://github.com/$OWNER_AND_REPO_NAME/compare/$LAST_TAG...$CURRENT_TAG" >> $FILE_NAME