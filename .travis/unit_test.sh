#!/bin/sh

res_file=$(pwd)/unit_test.xml
log_file=$(pwd)/unit_test.log
unity=/Applications/Unity/Unity.app/Contents/MacOS/Unity

touch "$log_file"

ls -l $unity
ls -l "$log_file"

echo "Running unit tests"
$unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-editorTestsResultFile "$res_file" \
	-logFile "$log_file" \
	-runEditorTests \
	-projectPath "$(pwd)"

res=$?

echo "Unit tests:"
cat "$res_file"

echo "\nEditor logs:"
cat "$log_file"

exit $res
