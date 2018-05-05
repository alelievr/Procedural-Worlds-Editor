#!/bin/sh

pwd

unity_pkg='https://netstorage.unity3d.com/unity/d4d99f31acba/MacEditorInstaller/Unity-2018.1.0f2.pkg'

echo "Downloading from $unity_pkg:"
curl -o Unity.pkg $unity_pkg

echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /
