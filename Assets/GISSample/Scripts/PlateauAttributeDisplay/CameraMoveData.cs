using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
{
    [CreateAssetMenu(fileName = "CameraMoveSpeedData", menuName = "PLATEAU GIS Sample/CameraMoveSpeedData")]
    public class CameraMoveData : ScriptableObject
    {
        public float walkerMoveSpeed;
        public float walkerOffsetYSpeed;
        public float walkerRotateScale;
    }
}