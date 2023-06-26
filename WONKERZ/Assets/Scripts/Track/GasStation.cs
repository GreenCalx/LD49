using UnityEngine;

public class GasStation : MonoBehaviour
{
    public Animator animator;

    public Color LightColorOnDeactivated;
    public Color LightColorOnActivated;
    public Light CPAnimatorLight;
    public Light CPAnimatorHalo;

    public float nutConversionInterval = 0.2f;
    private float nutConversionElapsed = 99f;

    private bool IsPumpingGas = false;
    private string animatorParm = "IsPumping";
    private string animatorStationActivationParm = "IsStationActivated";
    private string animatorActivationState = "ActivateStation";

    public bool bypassNutsCost = false;

    // Start is called before the first frame update
    void Start()
    {
        nutConversionElapsed = 99f;
        changeLightsColor(LightColorOnDeactivated);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void changeLightsColor(Color iColor)
    {
        if (CPAnimatorLight!=null)
        { CPAnimatorLight.color = iColor; }
        if (CPAnimatorHalo!=null)
        { CPAnimatorHalo.color = iColor; }
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            Access.CheckPointManager().notifyGasStation(this);
            animator.SetBool(animatorStationActivationParm, true);
            changeLightsColor(LightColorOnActivated);
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (bypassNutsCost)
            {
                Access.CollectiblesManager().currentTurbo = 1f;
                Access.UITurboAndSaves().updateTurboBar(Access.CollectiblesManager().currentTurbo);
                return;
            }
            nutConversionElapsed += Time.deltaTime;
            if (nutConversionElapsed > nutConversionInterval)
            {
                bool convertSuccess = Access.CollectiblesManager().tryConvertNutToTurbo();
                nutConversionElapsed = 0f;
                if (convertSuccess && !IsPumpingGas)
                {
                    animator.SetBool(animatorParm, true);
                    IsPumpingGas = true;
                }
                else if (!convertSuccess && IsPumpingGas)
                {
                    animator.SetBool(animatorParm, false);
                    IsPumpingGas = false;
                }
            }
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (IsPumpingGas)
            {
                IsPumpingGas = false;
                animator.SetBool(animatorParm, false);
            }

        }
    }
}
