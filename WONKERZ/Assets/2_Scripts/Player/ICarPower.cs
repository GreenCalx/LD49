using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz
{

    public interface ICarPower
    {
        public string name { get; set; }

        public void applyDirectEffect();

        public void onRefresh();
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC);

        public bool turnOffTriggers();
        public void onDisableEffect();
    }

    public class NeutralCarPower : ICarPower
    {
        public string name { get; set; }
        public NeutralCarPower()
        {
            name = "NeutralPower";
        }
        public void applyDirectEffect()
        {
            //Access.Player().SetMode(CarController.CarMode.NONE);
        }
        public void onRefresh()
        {
            // no power : nothing to do here 
        }
        public void onDisableEffect()
        {

        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {

        }
        public bool turnOffTriggers()
        {
            return false;
        }
    }

    public class TurboCarPower : ICarPower
    {
        public GameObject turboParticlesRef;
        public GameObject turboParticlesInst;

        public string name { get; set; }

        public TurboCarPower(GameObject iTurboParticles)
        {
            name = "TurboPower";
            turboParticlesRef = iTurboParticles;
        }
        public void applyDirectEffect()
        {
            PlayerController player = Access.Player();

            turboParticlesInst = GameObject.Instantiate(turboParticlesRef);
            turboParticlesInst.transform.position = player.transform.position;
            turboParticlesInst.transform.rotation = player.transform.rotation;

            turboParticlesInst.GetComponent<ParticleSystem>()?.Play();
        }
        public void onRefresh()
        {
            PlayerController cc = Access.Player();
            cc.transform.position = turboParticlesInst.transform.position;
            cc.transform.rotation = turboParticlesInst.transform.rotation;
        }

        public void onDisableEffect()
        {
            GameObject.Destroy(turboParticlesInst);
        }

        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log("Turbo Input effects");
            // No motor
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }

    public class SpinCarPower : ICarPower
    {
        public GameObject SpinPowerObject_Ref;
        private GameObject SpinPowerObject_Inst;

        public float duration = 0.5f;
        private float elapsed = 0f;
        public string name { get; set; }
        public SpinCarPower(GameObject iSpinPowerObject_Ref)
        {
            name = "SpinPower";
            SpinPowerObject_Ref = iSpinPowerObject_Ref;
        }
        public void applyDirectEffect()
        {
            // SPAWN spin hurtbox mesh SpinPowerObject
            SpinPowerObject_Inst = GameObject.Instantiate(SpinPowerObject_Ref, Access.Player().GetTransform());
            SpinPowerObject_Inst.SetActive(true);

            elapsed = 0f;
        }
        public void onRefresh()
        {
            elapsed += Time.deltaTime;
            if (elapsed > duration)
            {
                onDisableEffect();
            }
        }
        public void onDisableEffect()
        {
            // DESPAWN spin hurtbox mesh SpinPowerObject
            GameObject.Destroy(SpinPowerObject_Inst.gameObject);
        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log("Spin Power Input effects");
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }

    public class KnightLanceCarPower : ICarPower
    {
        public GameObject KnightLanceObject_Ref;
        private GameObject KnightLanceObject_Inst;
        private Transform attachPoint;

        public string name { get; set; }

        public KnightLanceCarPower(GameObject iKLance_Ref)
        {
            name = "Knight Lance";
            KnightLanceObject_Ref = iKLance_Ref;
        }

        public void applyDirectEffect()
        {
            PlayerController PC = Access.Player();
            WeightIndicator WI = PC.GetComponentInChildren<WeightIndicator>();
            attachPoint = WI.transform;

            if (!!WI)
            {
                KnightLanceObject_Inst = GameObject.Instantiate(KnightLanceObject_Ref, attachPoint);
                KnightLanceObject_Inst.SetActive(true);
            }
        }
        public void onRefresh()
        {
            KnightLanceObject_Inst.transform.localPosition = Vector3.zero;
        }
        public void onDisableEffect()
        {
            // DESPAWN spin hurtbox mesh SpinPowerObject
            GameObject.Destroy(KnightLanceObject_Inst);
        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log(name + " Input effects");
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }
}
