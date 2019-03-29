#if UNITY_CLOUD_BUILD

using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MLAgents
{
	public class Builder
	{
		public static void PreExport()
		{
			EditorSceneManager.OpenScene("Assets/ML-Agents/Examples/3DBall/Scenes/3DBall.unity");
			var aca = SceneAsset.FindObjectOfType<Academy>();

			if (aca == null)
			{
				Debug.Log("Academy is null");
			}

			if (aca.broadcastHub == null)
			{
				Debug.Log("broadcastHub is null");
			}
	
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