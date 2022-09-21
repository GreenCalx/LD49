using UnityEngine;

// https://answers.unity.com/questions/1372346/onbecameinvisible-in-the-editor.html?childToView=1372636#answer-1372636
public enum EFrustumIntersection
{
    Outside,
    Inside,
    Intersecting
}

public struct Frustum
{
    public Vector4 l;
    public Vector4 r;
    public Vector4 b;
    public Vector4 t;
    public Vector4 n;
    public Vector4 f;

    public Plane left { get { return new Plane { normal = l, distance = l.w }; } }
    public Plane right { get { return new Plane { normal = r, distance = r.w }; } }
    public Plane bottom { get { return new Plane { normal = b, distance = b.w }; } }
    public Plane top { get { return new Plane { normal = t, distance = t.w }; } }
    public Plane near { get { return new Plane { normal = n, distance = n.w }; } }
    public Plane far { get { return new Plane { normal = f, distance = f.w }; } }
    public Vector4 this[int aIndex]
    {
        get
        {
            switch (aIndex)
            {
                case 0: return l;
                case 1: return r;
                case 2: return b;
                case 3: return t;
                case 4: return n;
                case 5: return f;
                default: throw new System.ArgumentOutOfRangeException("aIndex", "value must be between 0 and 5");
            }
        }
    }

    public EFrustumIntersection TestBounds(Bounds aBounds)
    {
        var min = aBounds.min;
        var max = aBounds.max;
        var x = new Vector2(min.x, max.x);
        var y = new Vector2(min.y, max.y);
        var z = new Vector2(min.z, max.z);
        var res = EFrustumIntersection.Inside;

        for (int i = 0; i < 6; i++)
        {
            var plane = this[i];
            int xi = plane.x > 0 ? 1 : 0;
            int yi = plane.y > 0 ? 1 : 0;
            int zi = plane.z > 0 ? 1 : 0;
            if (plane.x * x[xi] + plane.y * y[yi] + plane.z * z[zi] + plane.w < 0)
                return EFrustumIntersection.Outside;
            if (plane.x * x[1 - xi] + plane.y * y[1 - yi] + plane.z * z[1 - zi] + plane.w < 0)
                res = EFrustumIntersection.Intersecting;
        }
        return res;
    }
    public EFrustumIntersection TestPoint(Vector3 aPoint)
    {
        for (int i = 0; i < 6; i++)
        {
            var plane = this[i];
            float d = plane.x * aPoint.x + plane.y * aPoint.y + plane.z * aPoint.z + plane.w;
            if (d < 0)
                return EFrustumIntersection.Outside;
        }
        return EFrustumIntersection.Inside;
    }
}


public static class VisibilityTools
{
    public static Frustum GetFrustum(this Camera aCam)
    {
        var m = aCam.projectionMatrix * aCam.worldToCameraMatrix;
        Frustum res;
        var x = m.GetRow(0);
        var y = m.GetRow(1);
        var z = m.GetRow(2);
        var w = m.GetRow(3);

        res.l = GetNormalizedPlane(w + x);
        res.r = GetNormalizedPlane(w - x);

        res.b = GetNormalizedPlane(w + y);
        res.t = GetNormalizedPlane(w - y);

        res.n = GetNormalizedPlane(w + z);
        res.f = GetNormalizedPlane(w - z);
        return res;
    }

    private static Vector4 GetNormalizedPlane(Vector4 aVec)
    {
        return aVec * (1f / Mathf.Sqrt(aVec.x * aVec.x + aVec.y * aVec.y + aVec.z * aVec.z));
    }
}

public class DebugShowCulling : MonoBehaviour
{
    Bounds b;
    Renderer r;
    bool init = false;

    Bounds TransformBounds(Bounds B)
    {
        var center = transform.TransformPoint(B.center);

        var extents = B.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    void doinit()
    {

        b = GetComponent<MeshFilter>().sharedMesh.bounds;
        r = GetComponent<MeshRenderer>();
        init = true;

    }
    void OnDrawGizmos()
    {
        // Gizmos.color = new Color(1, 0, 0, 0.5f);      
        //Gizmos.DrawCube(b.center, b.size);

    }
    void Update()
    {
        if (!init) doinit();


        if (!gameObject.isStatic)
            b = TransformBounds(GetComponent<MeshFilter>().sharedMesh.bounds);
        Frustum f = Access.CameraManager().active_camera.GetComponent<Camera>().GetFrustum();
        if (f.TestBounds(b) == EFrustumIntersection.Outside)
        {
            r.enabled = false;
        }
        else
        {
            r.enabled = true;
        }
    }
}
