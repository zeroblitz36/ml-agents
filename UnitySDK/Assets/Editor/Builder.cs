#if UNITY_CLOUD_BUILD

using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MLAgents
{
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
					Debug.Log("Switched brain " + brain.name + " to Control Mode");
				}
			}
	        
	        Debug.Log("The current scene " + EditorSceneManager.GetActiveScene().name + " has switched its all of its brain to Control Mode");
        }
    }
}

#endif