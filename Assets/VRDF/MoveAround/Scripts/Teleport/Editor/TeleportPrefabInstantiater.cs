﻿using UnityEngine;
using UnityEditor;
using VRDF.Core.Utils;

namespace VRDF.MoveAround.Teleport
{
    public class TeleportPrefabInstantiater : Editor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuCommand"></param>
        [MenuItem("GameObject/VRDF/Move Around/Teleport/Step by Step", priority = 1)]
        [MenuItem("VRDF/Move Around/Teleport/Step by Step", priority = 1)]
        private static void AddStepByStepTeleporter(MenuCommand menuCommand)
        {
            var sbsTeleporter = VRDFPrefabReferencer.GetPrefab("StepByStepTeleporter");
            CreateGameObject(sbsTeleporter, menuCommand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuCommand"></param>
        [MenuItem("GameObject/VRDF/Move Around/Teleport/Curve Teleporter", priority = 1)]
        [MenuItem("VRDF/Move Around/Teleport/Curve Teleporter", priority = 1)]
        private static void AddCurveTeleporter(MenuCommand menuCommand)
        {
            var curveTeleporter = VRDFPrefabReferencer.GetPrefab("CurveTeleporter");
            CreateGameObject(curveTeleporter, menuCommand);
        }

        private static void CreateGameObject(GameObject pointerPrefab, MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject pointer = PrefabUtility.InstantiatePrefab(pointerPrefab) as GameObject;

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(pointer, "Create " + pointer.name);

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(pointer, menuCommand.context as GameObject);

            Selection.activeObject = pointer;
        }
    }
}