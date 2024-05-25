using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using static Schnibble.Physics;
public class SchTestObject : MonoBehaviour
{
    [System.Serializable]
    public class SchTestSchObject : SchObject {}
    [System.Serializable]
    public class SchTestSchObjectWithUnity : SchObjectWithUnityParent {}

    [SerializeReference]
    SchObjectWithUnityParent obj = new SchTestSchObjectWithUnity();

    void OnDrawGizmosSelected()
    {
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            this.Log("==== begin ====");

            var v = new Vector3(10, 20, 30);
            obj.SetPosition(v);
            transform.position = v;

            this.Log("[Local ] position : " + obj.GetPositionLocal() + " rotation : " + obj.GetRotationLocal().eulerAngles + " scale : " + obj.GetScaleLocal());
            this.Log("[Local Unity] position : " + transform.localPosition + " rotation : " + transform.localRotation.eulerAngles + " scale : " + transform.localScale);
            this.Log("[Global] position : " + obj.GetPosition() + " rotation : " + obj.GetRotation().eulerAngles + " scale : " + obj.GetScale());
            this.Log("[Global Unity] position : " + transform.position + " rotation : " + transform.rotation.eulerAngles + " scale : " + transform.lossyScale);

            #if false
            this.Log("[Local ] position : " + obj.GetPositionLocal() + " rotation : " + obj.GetRotationLocal().eulerAngles + " scale : " + obj.GetScaleLocal());
            this.Log("[Global] position : " + obj.GetPosition() + " rotation : " + obj.GetRotation().eulerAngles + " scale : " + obj.GetScale());
            this.Log("[Global Unity] position : " + transform.position + " rotation : " + transform.rotation.eulerAngles + " scale : " + transform.lossyScale);

            this.Log("[Unity] GetWorld : " + transform.TransformPoint(new Vector3(10, 20, 30)));
            this.Log("[Sch  ] GetWorld : " + obj.GetWorldPoint(new Vector3(10, 20, 30)));

            this.Log("[Unity] GetLocal : " + transform.InverseTransformPoint(new Vector3(10, 20, 30)));
            this.Log("[Sch  ] GetLocal : " + obj.GetLocalPoint(new Vector3(10, 20, 30)));

            var v_current = obj.GetPosition();

            var v = new Vector3(10, 20, 30);
            obj.SetPosition(v);
            transform.position = (v);

            this.Log("[Global] position : " + obj.GetPosition() + " rotation : " + obj.GetRotation().eulerAngles + " scale : " + obj.GetScale());
            this.Log("[Global Unity] position : " + transform.position + " rotation : " + transform.rotation.eulerAngles + " scale : " + transform.lossyScale);

            obj.SetPosition(v);
            transform.position = (v);

            this.Log("[Local ] position : " + obj.GetPositionLocal() + " rotation : " + obj.GetRotationLocal().eulerAngles + " scale : " + obj.GetScaleLocal());
            this.Log("[Global] position : " + obj.GetPosition() + " rotation : " + obj.GetRotation().eulerAngles + " scale : " + obj.GetScale());
            this.Log("[Global Unity] position : " + transform.position + " rotation : " + transform.rotation.eulerAngles + " scale : " + transform.lossyScale);

            this.Log(obj.GetTransform().ToString());
            this.Log(transform.localToWorldMatrix.ToString());

            obj.SetPosition(v_current);
            transform.position = v_current;

            this.Log("[Local ] position : " + obj.GetPositionLocal() + " rotation : " + obj.GetRotationLocal().eulerAngles + " scale : " + obj.GetScaleLocal());
            this.Log("[Global] position : " + obj.GetPosition() + " rotation : " + obj.GetRotation().eulerAngles + " scale : " + obj.GetScale());
            this.Log("[Global Unity] position : " + transform.position + " rotation : " + transform.rotation.eulerAngles + " scale : " + transform.lossyScale);

            #endif

            this.Log("==== begin ====");
        }
    }
}
