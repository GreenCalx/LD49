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

    public class WaterCarPower : ICarPower
    {
        public string name { get; set; }
        public WaterCarPower()
        {
            name = "WaterPower";
        }
        public void applyDirectEffect()
        {
            //Access.Player().SetMode(CarController.CarMode.WATER);
        }
        public void onRefresh()
        {

        }
        public void onDisableEffect()
        {

        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log("Water Input effects");
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }

    public class PlaneCarPower : ICarPower
    {
        public string name { get; set; }
        public PlaneCarPower()
        {
            name = "PlanePower";
        }
        public void applyDirectEffect()
        {
            //        Access.Player().SetMode(CarController.CarMode.DELTA);
            //Access.Player().IsAircraft = true;
        }
        public void onRefresh()
        {

        }
        public void onDisableEffect()
        {
            //Access.Player().IsAircraft = false;
        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log("Plane Input effects");
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }

    public class SpiderCarPower : ICarPower
    {
        public string name { get; set; }
        public SpiderCarPower()
        {
            name = "SpiderPower";
        }
        public void applyDirectEffect()
        {
            //Access.Player().SetMode(CarController.CarMode.SPIDER);
        }
        public void onRefresh()
        {

        }
        public void onDisableEffect()
        {

        }
        public void applyEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            this.Log("Spider Input effects");
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }
}
