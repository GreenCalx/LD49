using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BirdFleet : MonoBehaviour
{
    public Mesh boundaries;
    public GameObject birdRef;
    public Transform birdPoolHandle;

    public int fleetSizeMin, fleetSizeMax;
    public int birdScaleMin, birdScaleMax;
    public int maxBirds = 100;
    public float dispersionRadius;
    public float speed;
    public float spawnProbabilityTimeScale;
    public AnimationCurve spawnProbabilityOverTime;
    
    ///
    public List<GameObject> birds;
    private float elapsedTimeSinceLastSpawn;

    private float xmin;
    private float xmax;
    private float ymin;
    private float ymax;
    private float zmin;
    private float zmax;

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceLastSpawn = 0f;
        birds = new List<GameObject>(0);

        xmin = transform.position.x + boundaries.bounds.min.x * transform.lossyScale.x;
        xmax = transform.position.x + boundaries.bounds.max.x * transform.lossyScale.x;

        ymin = transform.position.y + boundaries.bounds.min.y * transform.lossyScale.y;
        ymax = transform.position.y + boundaries.bounds.max.y * transform.lossyScale.y;

        zmin = transform.position.z + boundaries.bounds.min.z * transform.lossyScale.z;
        zmax = transform.position.z + boundaries.bounds.max.z * transform.lossyScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTimeSinceLastSpawn += Time.deltaTime;
        if (birds.Count < maxBirds)
            pollForSpawn();
        
        //update birds
        if (birds.Count>0)
        {
            List<GameObject> birds_to_kill = new List<GameObject>(0);
            foreach( GameObject bird in birds)
            {
                bird.transform.Translate(bird.transform.forward * speed * Time.deltaTime);
                if ((bird.transform.position.x > xmax)||(bird.transform.position.x < xmin))
                {
                    birds_to_kill.Add(bird);
                }
                else if ((bird.transform.position.z > zmax)||(bird.transform.position.z < zmin))
                {
                    birds_to_kill.Add(bird);
                }
            }
            foreach( GameObject bird in birds_to_kill)
            {
                birds.Remove(bird);
                Destroy(bird);
            }
        }
    }

    private void pollForSpawn()
    {
        float prob = spawnProbabilityOverTime.Evaluate(elapsedTimeSinceLastSpawn * spawnProbabilityTimeScale);
        float rand_res = Random.Range(0f, 1f);
        if (rand_res<prob)
        {
            spawnFleet();
        }
    }

    private void spawnFleet()
    {
        // Determine spawn location
        // 4 possibilites : (+-Z==1)XY plane && (+-X==1)YZ plane

        Vector3 spawn_loc = Vector3.zero;
        Vector3 planeNormal = Vector3.zero;
        Vector3 destination = Vector3.zero;

        
        
        
        
        
        

        int rand_plane = Random.Range(0,3);
        switch(rand_plane)
        {
            case 0:
                spawn_loc = new Vector3(xmin, Random.Range(ymin,ymax), Random.Range(zmin,zmax));
                planeNormal = new Vector3(1,0,0);
                destination = new Vector3(xmax, spawn_loc.y, spawn_loc.z);
                break;
            case 1:
                spawn_loc = new Vector3(xmax, Random.Range(ymin,ymax), Random.Range(zmin,zmax));
                planeNormal = new Vector3(-1,0,0);
                destination = new Vector3(xmin, spawn_loc.y, spawn_loc.z);
                break;
            case 2:
                spawn_loc = new Vector3(Random.Range(xmin,xmax), Random.Range(ymin,ymax), zmin);
                planeNormal = new Vector3(0,0,1);
                destination = new Vector3(spawn_loc.x, spawn_loc.y, zmax);
                break;
            case 3:
                spawn_loc = new Vector3(Random.Range(xmin,xmax), Random.Range(ymin,ymax), zmax);
                planeNormal = new Vector3(0,0,-1);
                destination = new Vector3(spawn_loc.x, spawn_loc.y, zmax);
                break;
        }

        int maxsize = fleetSizeMax;
        if (birds.Count+fleetSizeMax > maxBirds)
        {
            maxsize = maxBirds - birds.Count;
        }
        int fleetSize = Random.Range(fleetSizeMin, maxsize);

        List<GameObject> new_fleet = new List<GameObject>(0);
        for (int i=0; i<fleetSize; i++)
        {
            Vector3 rand_pos = spawn_loc + (Random.insideUnitSphere * dispersionRadius);
            GameObject new_bird = Instantiate(birdRef, birdPoolHandle);
            new_bird.transform.position = rand_pos;

            int rand_scale = Random.Range(birdScaleMin, birdScaleMax);
            new_bird.transform.localScale = new Vector3(rand_scale, rand_scale, rand_scale);
            //new_bird.transform.forward = planeNormal;

            new_fleet.Add(new_bird);

        }

        birds.AddRange(new_fleet);
    }


    // GUI Gizmo
    static Color guizmo_col = new Color(0.0f, 0.7f, 1f, 1.0f);
    private void DrawGizmo(bool selected)
    {
        guizmo_col.a = selected ? 0.3f : 0.1f;
        Gizmos.color = guizmo_col;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        guizmo_col.a = selected ? 0.5f : 0.2f;
        Gizmos.color = guizmo_col;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    public void OnDrawGizmos()
    {
        DrawGizmo(false);
    }
    public void OnDrawGizmosSelected()
    {
        DrawGizmo(true);
    }    
}
