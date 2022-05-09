using UnityEngine;

public class Ground : MonoBehaviour
{
    public enum EType
    {
        ROAD,
        DESERT,
        WATER,
        NONE,
    };
    [System.Serializable]
    public class GroundInfos
    {
        // Define how much drag is applied on forces applied on the wheel depending on incoming direction.
        public Vector2 Friction;
        // Define the velocity of the ground. It is used to apply it to the wheel and become the ref velocity zero.
        public Vector3 Velocity;
        // Define how much we need to move from collider surface to actually collide.
        public Vector3 DepthPerturbation;
        // DEfine the type of ground, used to impact the car behavior.
        public EType Type;

        public GroundInfos()
        {
            Type = EType.ROAD;
            Friction = new Vector2(0.01f, 0.1f);
            Velocity = Vector3.zero;
            DepthPerturbation = Vector3.zero;
        }

        public GroundInfos(GroundInfos GI)
        {
            this.Copy(GI);
        }

        public void Copy(GroundInfos GI)
        {
            Friction = GI.Friction;
            Velocity = GI.Velocity;
            DepthPerturbation = GI.DepthPerturbation;
            Type = GI.Type;
        }

        static public GroundInfos Default = new GroundInfos();
    };
    [SerializeField]
    public GroundInfos GI;
    private GroundInfos GIDefault;

    private void Start()
    {
        GIDefault = new GroundInfos(GI);
    }

    private void ResetToDefault()
    {
        GI.Copy(GIDefault);
    }

    // NOTE toffa :
    // We dispatch at runtime to the right override to be able to attach only the "GRound" MonoBehavior
    // And apply the right behavior according to the type.
    // This had been done by inheritance, could have been made as custom functions.
    public GroundInfos GetGroundInfos(Vector3 HitPosition)
    {
        ResetToDefault();

        // TODO toffa :
        // Remove this and make it esier to use and more reliable
        var MovingObjectA = GetComponent<testphysx>();
        if (MovingObjectA)
        {
            GI.Velocity = MovingObjectA.Velocity;
        }
        var MovingObjectB = GetComponent<BridgePhysX>();
        if (MovingObjectB)
        {
            GI.Velocity = MovingObjectB.Velocity;
        }

        switch (GI.Type)
        {
            case EType.DESERT:
                GI.DepthPerturbation.y = -1 * (Mathf.Sin(HitPosition.x * 0.1f) - 1);
                // TODO toffa : remove this and make it better
                // find if we are in moving sands
                var Test = GetComponent<TestUpdateMeshCollider>();
                if (Test)
                {
                    var HP = new Vector2(HitPosition.x, HitPosition.z);
                    foreach (var Sand in Test.SandPositions)
                    {
                        var D = Vector3.Distance(HP, new Vector2(Sand.transform.position.x, Sand.transform.position.z)) / Test.MovingSandsRadius;
                        if (D <= 1)
                        {
                            // in sands we are having a velocity to the center
                            GI.Velocity = (1 - D) * (new Vector2(Sand.transform.position.x, Sand.transform.position.z) - HP).normalized * Test.ForceMultiplier;
                            //GI.Friction = Vector2.zero;
                        }
                    }
                }
                break;
            case EType.ROAD:
                break;
            case EType.WATER:
                break;
            case EType.NONE:
                break;
        };
        return GI;
    }
}
