#if UNITY_CLOUD_BUILD

using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Builder
{
	public static void PreExport()
	{
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


#endif