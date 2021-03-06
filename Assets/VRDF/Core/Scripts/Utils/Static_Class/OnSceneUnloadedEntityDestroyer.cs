﻿using UnityEngine;
using Unity.Entities;

namespace VRDF.Core
{
    public static class OnSceneUnloadedEntityDestroyer
    {
        /// <summary>
        /// Check if the gameObject using a DestroyOnSceneUnload component isn't place on a gameObject that will be placed on a DontDestroyOnLoad scene
        /// </summary>
        public static void CheckDestroyOnSceneUnload(ref EntityManager entityManager, ref Entity entity, int gameObjectSceneIndex, string componentName)
        {
            // Need to check for index in case the object is placed in a DontDestroyOnLoad scene
            if (gameObjectSceneIndex == -1)
            {
                Debug.LogFormat("<Color=yellow><b>[VRDF] :</b> Placing a {0} component with 'Destroy On Scene Unloaded' checked and set as DontDestroyOnLoad is not adviced, as it may lead to unexpected behaviour.\n" +
                    "Please consider unchecking the 'Destroy On Scene Unloaded' checkbox or let this {0} component in the Active Scene or one of the Additive Scene.</Color>", componentName);
                try
                {
                    entityManager.AddComponentData(entity, new DestroyOnSceneUnloaded { SceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex });
                }
                catch (System.Exception e)
                {
                    Debug.LogError("<Color=red><b>[VRDF] :</b> An error has occured when trying to add a DestroyOnSceneUnloaded component. Please read the error below:\n</Color>" + e.ToString());
                }
            }
            else
            {
                entityManager.AddComponentData(entity, new DestroyOnSceneUnloaded { SceneIndex = gameObjectSceneIndex });
            }
        }
    }
}