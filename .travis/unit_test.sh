#!/bin/sh

log_file=$(pwd)/unit_test.xml
unity=/Applications/Unity/Unity.app/Contents/MacOS/Unity

touch $log_file

ls -l $unity
ls -l $log_file

echo "Running unit tests"
$unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-editorTestsResultFile $log_file \
	-runEditorTests \
	-projectPath $(pwd)

res=$?

echo "Unit tests:"
cat $log_file

exit $res
