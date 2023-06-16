using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class ClippingPlane : MonoBehaviour
{
    public List<Material> mats;
    private float triggerPlaneClipping;
    public bool clipAbovePlane = true;

    private Plane plane;
    private Vector4 planeRepresentation;

    private MeshRenderer MR;
    private Material selfMat;

    // Start is called before the first frame update
    void Start()
    {
        triggerPlaneClipping = 0;

        plane = new Plane(transform.up, transform.position);
        planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        MR = GetComponent<MeshRenderer>();
        selfMat = MR.material;

        refresh();
    }

    // Update is called once per frame
    private void refresh()
    {
        // refresh for clipping
        foreach (Material mat in mats)
        {
            mat.SetVector("_Plane", planeRepresentation);
            mat.SetFloat("_TriggerPlaneClipping", triggerPlaneClipping);
            mat.SetFloat("_ClipAbovePlane", (clipAbovePlane ? 1f : 0f));
        }
    }

    private void refreshPortalTrigger(bool iStatus)
    {
        PortalTrigger pt = GetComponentInChildren<PortalTrigger>();
        if (!!pt)
        { pt.isActive = iStatus; }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!!iCol.GetComponent<PlayerController>())
        {
            HUBPortal hub_p = GetComponentInParent<HUBPortal>();
            if (!!hub_p)
            {
                if (!!hub_p.activeClippingPortal)
                    return;
                else
                {
                    hub_p.setActiveClippingPortal(this.gameObject);
                    triggerPlaneClipping = 1;
                    refresh();
                    refreshPortalTrigger(true);
                }
            }

        }
    }

    void OnTriggerStay(Collider iCol)
    {
        PlayerController cc = iCol.GetComponent<PlayerController>();
        if (!!cc && (triggerPlaneClipping > 0))
        {
            // refresh self ripple
            Vector3 rippleOrig = Vector3.ProjectOnPlane(cc.transform.position, new Vector3(plane.normal.x, plane.normal.y, plane.normal.z));
            selfMat.SetVector("_RippleOrigin",
                new Vector4(rippleOrig.x,
                             rippleOrig.y,
                             rippleOrig.z,
                             0
            ));
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        if (!!iCol.GetComponent<PlayerController>())
        {
            HUBPortal hub_p = GetComponentInParent<HUBPortal>();
            if (!!hub_p && (hub_p.activeClippingPortal == this.gameObject))
            {
                hub_p.activeClippingPortal = null;
                triggerPlaneClipping = 0;
                refresh();
                refreshPortalTrigger(false);
            }
        }
    }
}
