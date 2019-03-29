#if UNITY_CLOUD_BUILD

using System;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MLAgents
{
	public class Builder
	{
		public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
		{
			var envPath = Environment.GetEnvironmentVariable("ENV_PATH");
			EditorSceneManager.OpenScene(envPath);
			var aca = SceneAsset.FindObjectOfType<Academy>();
	
			var learningBrains = aca.broadcastHub.broadcastingBrains.Where(
				x => x != null && x is LearningBrain);
		
			foreach (Brain brain in learningBrains)
			{
				if (!aca.broadcastHub.IsControlled(brain))
				{
					aca.broadcastHub._brainsToControl.Add(brain);
				}
			}
		}
	}
}

#endif