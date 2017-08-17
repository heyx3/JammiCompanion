using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Test_AudioGraph))]
public class Editor_Test_AudioGraph : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(25.0f);

		int minF, maxF;
		Microphone.GetDeviceCaps(null, out minF, out maxF);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Min Hz: " + minF + "\tMax Hz: " + maxF);
		GUILayout.EndHorizontal();
	}
}