using UnityEngine;

public abstract class AbstractCameraPoint : MonoBehaviour
{
    public enum CameraMode
    {
        Follow,
        Fixed,
    };

    [System.Serializable]
    public class CameraDescriptor
    {
        public Vector3 position;
        public CameraMode mode;
        public Quaternion rotation;

    };

    [Header("AbstractCameraPoint")]
    public AbstractCameraPoint nextPoint;
    public AbstractCameraPoint prevPoint;
    public CameraDescriptor CamDescEnd;
    public CameraDescriptor CamDescStart;
}
