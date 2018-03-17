#!/bin/sh

log_file=$(pwd)/performance.log

echo "Running performance test"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-executeMethod "ProceduralWorlds.Editor.PerformanceTestsRunner.Run" \
	-projectPath $(pwd) \
	-quit

res=$?

echo "Results:"
cat $log_file

exit $res
