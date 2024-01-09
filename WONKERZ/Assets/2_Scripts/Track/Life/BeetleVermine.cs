using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.AI;

public class BeetleVermine : SchAIAgent
{

    public GameObject splatterDecal;
    public float splatterDuration = 5f;

    public bool isActive = true;
    public float changeDirectionTimeScale = 0.1f;
    public AnimationCurve changeDirectionLikehood;
    private float changeDirectionElapsed =0f;
    private Vector3 currDirection = Vector3.zero;

    public void splatter()
    {
        if (!isActive)
            return;

        ai_kill();

        Vector3 curr_scale = transform.localScale;
        transform.localScale = new Vector3(curr_scale.x, 0.01f, curr_scale.z);
        GameObject splatter_decal = Instantiate(splatterDecal);
        splatter_decal.transform.position = transform.position;
        Destroy(gameObject, 0.2f);
        Destroy(splatter_decal, splatterDuration);

        
        isActive = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        ai_init();
        changeDirectionElapsed =0f;
        isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;
        changeDirectionElapsed += Time.deltaTime;
        
        float val = changeDirectionLikehood.Evaluate(changeDirectionElapsed * changeDirectionTimeScale);
        if ( Random.Range(0f,1f) < val )
        {
            agent.SetDestination(AINavigation.GoInRandomDirection(transform, 50f, navAgentMaskName));
            changeDirectionElapsed = 0f;
        } else {
            //agent.SetDestination(AINavigation.GetNextMoveInDirection(transform, currDirection, 50f, navAgentMaskName));
        }
        
    }

}
