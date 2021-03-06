﻿using Unity.Entities;
using UnityEngine;

namespace VRDF.Core.Simulator
{
    /// <summary>
    /// Components to calculate the horizontal movements of the simulator using the arrow keys or WASD/ZQSD
    /// </summary>
    public struct SimulatorMovements : IComponentData
    {
        /// <summary>
        /// Base speed for moving on the horizontal axis
        /// </summary>
        public float WalkSpeed;

        /// <summary>
        /// The boost effect to apply when the user press one of the shift key
        /// </summary>
        public float ShiftBoost;

        /// <summary>
        /// Should the player always stay on the floor ? Use raycasting to check for collider under the Camera.
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// The layers we can check to ground the user
        /// </summary>
        public LayerMask GroundLayerMask;
    }
}