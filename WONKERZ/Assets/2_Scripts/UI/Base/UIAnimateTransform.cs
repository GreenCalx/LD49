using UnityEngine;
using static Schnibble.Physics;

namespace Wonkerz
{
    public class UIAnimateTransform : MonoBehaviour
    {
        public enum Mode { FAIL, SUCCESS };
        public Mode mode;

        public float rotateMin;
        public float rotateMax;

        public float scaleMin;
        public float scaleMax;

        public void springCB_Animate(float value, SchSpring spring)
        {
            var rot = gameObject.transform.localRotation;
            var angles = rot.eulerAngles;
            if (mode == Mode.FAIL)
            angles.z = spring.FromSpace(rotateMin, rotateMax, value);
            else
            angles.z = 0;
            rot.eulerAngles = angles;

            gameObject.transform.localRotation = rot;

            var scale = gameObject.transform.localScale;
            scale.x = spring.FromSpace(scaleMin, scaleMax, value);
            scale.y = spring.FromSpace(scaleMin, scaleMax, value);

            gameObject.transform.localScale = scale;
        }
    }
}
