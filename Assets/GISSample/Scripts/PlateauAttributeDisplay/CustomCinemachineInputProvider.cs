using UnityEngine;
using Cinemachine;

namespace GISSample.PlateauAttributeDisplay
{
    public class CustomCinemachineInputProvider : CinemachineInputProvider
    {
        public float SensitivityMultiplier = 1.0f;

        // Replace the incorrect override with GetAxisValue override.
        public override float GetAxisValue(int axis)
        {
            float baseValue = base.GetAxisValue(axis);
            return baseValue * SensitivityMultiplier;
        }
    }
}
