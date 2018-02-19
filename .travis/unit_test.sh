#!/bin/sh

log_file=$(pwd)/unit_test.xml

touch $log_file

echo "Running unit tests"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-editorTestsResultFile $log_file \
	-runEditorTests \
	-projectPath $(pwd)

res=$?

echo "Unit tests:"
cat $log_file

echo $res
ls
pwd
exit $res
