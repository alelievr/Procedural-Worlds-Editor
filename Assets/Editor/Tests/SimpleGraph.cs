using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using System.IO;

public class SimpleGraph {

	[Test]
	public void SimpleGraphSimplePasses() {

		string fileName = "TestGraph";
		string path = PWGraphManager.PWMainGraphPath + "/" + fileName + ".asset";
		PWGraphManager.CreateMainGraph(fileName);
		Assert.That(File.Exists(path));
		ScriptableObject.DestroyImmediate(AssetDatabase.LoadAssetAtPath< PWMainGraph >(path), true);
	}

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
/*	[UnityTest]
	public IEnumerator SimpleGraphWithEnumeratorPasses() {
		// Use the Assert class to test conditions.
		// yield to skip a frame
		yield return null;
	}*/
}
