#!/bin/sh

log_file=$(pwd)/perf.log

touch $log_file

echo "Running performance test"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-executeMethod \
	-logfile $log_file \
	-projectPath $(pwd)

res=$?

echo "Performance tests:"
cat $log_file

curl -X POST https://api.loadimpact.com/v2/data-stores \
	-u "$API_TOKEN:" {"id": TEST_RUN_ID_OF_STARTED_TEST} \
	-H "Accept: application/json" \
	-H "Content-Type: multipart/form-data" \
	-F "file=@$log_file" \
	-F "name=perf" \
	-F "fromline=1" \
	-F "separator=comma" \
	-F "delimiter=double" \

exit $res
