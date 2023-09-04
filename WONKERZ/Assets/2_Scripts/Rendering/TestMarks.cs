using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMarks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate () {
        RaycastHit hitInfo;
        if(Physics.Raycast(transform.position, -transform.up, out hitInfo)){
            //get material
            var mat = hitInfo.collider.material;
            // todo toffa : come on this replace is sooo ugly ;_;
            var hash = mat.name.Replace(" (Instance)", "").GetHashCode();
            var schMat = PhysicsMaterialManager.instance.GetMat(hash);
            //add skid marks at point
            var point = hitInfo.point;
            var basis = Schnibble.SchMathf.GetBasis(transform.up);
            //schMat.AddGroundMark(point, basis, 1);
        };

}
    // Update is called once per frame
    void Update()
    {

    }
}
