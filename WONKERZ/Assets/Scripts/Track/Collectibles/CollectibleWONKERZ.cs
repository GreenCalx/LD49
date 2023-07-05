using UnityEngine;

public class CollectibleWONKERZ : AbstractCollectible
{
    public enum LETTERS { W, O, N, K, E, R, Z }

    public LETTERS currLetter;
    public float yRotationSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        collectibleType = COLLECTIBLE_TYPE.UNIQUE;

    }

    // Update is called once per frame
    void Update()
    {
        animate();
    }

    private void animate()
    {
        transform.Rotate(new Vector3(0, yRotationSpeed, 0), Space.World);
    }

    protected override void OnCollect()
    {
        gameObject.SetActive(false);
        //TODO : persist collected status
        Access.CollectiblesManager().applyCollectEffect(this);
        Access.UIWonkerzBar().display();
    }
}
