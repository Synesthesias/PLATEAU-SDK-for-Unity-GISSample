using UnityEngine;

namespace GISSample.Misc
{

    [CreateAssetMenu(fileName = "CameraMoveSpeedData", menuName = "PLATEAU GIS Sample/CameraMoveSpeedData")]
    public class CameraMoveData : ScriptableObject
    {
        public float horizontalMoveSpeed;
        public float verticalMoveSpeed;
        public float parallelMoveSpeed;
        public float zoomMoveSpeed;
        public float rotateSpeed;
        public float zoomLimit;
        public float heightLimitY;
        public float walkerMoveSpeed;
        public float walkerOffsetYSpeed;

    }

}