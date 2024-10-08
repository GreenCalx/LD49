using UnityEngine;
using Schnibble;
using Schnibble.Managers;
using Mirror;

namespace Wonkerz
{

    public interface ICarPower
    {
        public string name { get; set; }
        public bool isOnline { get; set; }
        // called in update
        public void onRefresh();

        // To handle inputs specific to the power, unused atm ?
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC);

        // called on update and desequips power if returns true
        public bool turnOffTriggers();
        
        // called when a power is equiped
        public void onEnableEffect();
        // Called when power is activated
        public void onActivation();
        // called when a power is unequiped
        public void onDisableEffect();
    }

    public class NeutralCarPower : ICarPower
    {
        public string name { get; set; }
        public bool isOnline { get; set; }
        public NeutralCarPower()
        {
            name = "NeutralPower";
            isOnline = Access.managers.gameSettings.isOnline;
        }
        public void onEnableEffect()
        {
            //Access.Player().SetMode(CarController.CarMode.NONE);
        }

        public void onActivation()
        {

        }
        public void onRefresh()
        {
            // no power : nothing to do here 
        }
        public void onDisableEffect()
        {

        }
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
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
        public bool isOnline { get; set; }
        public TurboCarPower(GameObject iTurboParticles)
        {
            name = "TurboPower";
            turboParticlesRef = iTurboParticles;
            isOnline = Access.managers.gameSettings.isOnline;
        }
        public void onEnableEffect()
        {
            PlayerController player = Access.Player();

            turboParticlesInst = GameObject.Instantiate(turboParticlesRef);
            turboParticlesInst.transform.position = player.transform.position;
            turboParticlesInst.transform.rotation = player.transform.rotation;

            turboParticlesInst.GetComponent<ParticleSystem>()?.Stop();
        }

        public void onActivation()
        {
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

        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
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
        public bool isOnline { get; set; }
        public SpinCarPower(GameObject iSpinPowerObject_Ref)
        {
            name = "SpinPower";
            SpinPowerObject_Ref = iSpinPowerObject_Ref;
            isOnline = Access.managers.gameSettings.isOnline;
        }
        public void onEnableEffect()
        {
            // SPAWN spin hurtbox mesh SpinPowerObject

        }

        public void onActivation()
        {
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
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
        {
            this.Log("Spin Power Input effects");
        }
        public bool turnOffTriggers()
        {
            return false;
        }
    }

    public class KnightLanceCarPower : ICarPower
    {
        public OnlinePlayerController owner;
        public GameObject KnightLanceObject_Ref;
        private GameObject KnightLanceObject_Inst;
        private Transform attachPoint;

        public string name { get; set; }
        public bool isOnline { get; set; }
        public KnightLanceCarPower(GameObject iKLance_Ref)
        {
            name = "Knight Lance";
            KnightLanceObject_Ref = iKLance_Ref;
            isOnline = Access.managers.gameSettings.isOnline;
        }

        public void onEnableEffect()
        {
            owner = owner = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer;
            WeightIndicator WI = owner.GetComponentInChildren<WeightIndicator>();
            attachPoint = WI.transform;

            if (!!WI)
            {
                KnightLanceObject_Inst = GameObject.Instantiate(KnightLanceObject_Ref, attachPoint);
                KnightLanceObject_Inst.SetActive(true);
                if (isOnline)
                {
                    NetworkServer.Spawn(KnightLanceObject_Inst);
                }
            }
        }

        public void onActivation()
        {
            
        }

        public void onRefresh()
        {
            KnightLanceObject_Inst.transform.localPosition = Vector3.zero;
        }
        public void onDisableEffect()
        {
            // DESPAWN spin hurtbox mesh SpinPowerObject
            if (isOnline)
                NetworkServer.Destroy(KnightLanceObject_Inst);
            else
                GameObject.Destroy(KnightLanceObject_Inst);
        }
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
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

    public class PalletLauncherCarPower : ICarPower
    {
        // TODO : Make me tweakable from outside this place
        public const float cooldown = 1f;
        public OnlinePlayerController owner;

        public GameObject PalletLauncher_Ref;
        private GameObject PalletLauncher_Inst;
        private GameObject Pallet_Ref;
        private GameObject Pallet_Inst;
        private Canon Canon_Handle;

        private Transform attachPoint;
        private Transform palletLoadingPoint;
        private Transform palletLaunchingPoint;

        private bool palletRdyToLaunch = false;
        private bool canonArmed = false;

        private float elapsedTime;

        public string name { get; set; }
        public bool isOnline { get; set; }

        public PalletLauncherCarPower(GameObject iPLauncher_Ref, GameObject iPalletRef)
        {
            name = "Pallet Launcher";
            canonArmed = false;
            palletRdyToLaunch = true;

            PalletLauncher_Ref = iPLauncher_Ref;
            Pallet_Ref = iPalletRef;

            elapsedTime = 0f;
            isOnline = Access.managers.gameSettings.isOnline;
        }

        public void onEnableEffect()
        {
            owner = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer;
            WeightIndicator WI = owner.GetComponentInChildren<WeightIndicator>();
            attachPoint = WI.transform;

            if (!!WI)
            {
                PalletLauncher_Inst = GameObject.Instantiate(PalletLauncher_Ref, attachPoint);
                PalletLauncher_Inst.SetActive(true);
                if (isOnline)
                {
                    NetworkServer.Spawn(PalletLauncher_Inst);
                }

                Pallet_Inst = GameObject.Instantiate(Pallet_Ref);
                if (isOnline)
                {
                    NetworkServer.Spawn(Pallet_Inst);
                }

                Pallet_Inst.transform.parent = palletLoadingPoint;
                Pallet_Inst.transform.localPosition = Vector3.zero;
                Pallet_Inst.transform.rotation = Quaternion.identity;

                palletRdyToLaunch = true;

                Canon_Handle = PalletLauncher_Inst.GetComponent<Canon>();
                palletLoadingPoint = Canon_Handle.loadingPoint;
                palletLaunchingPoint = Canon_Handle.spawnPoint;
            }
        }

        public void onActivation()
        {
            if (Canon_Handle==null)
            {
                this.LogError("Canon reference missing on PalletLauncherCarPower");
                Canon_Handle = PalletLauncher_Inst.GetComponent<Canon>();
                return;
            }

            GameObject projectile = Canon_Handle.Fire();
            if (isOnline)
                NetworkServer.Spawn(projectile);
            OnlineProjectile as_projectile = projectile.GetComponent<OnlineProjectile>();
            if (!!as_projectile)
            {
                as_projectile.lifeTime = Canon_Handle.projectileDuration;
            }
            OnlineDamager as_damager = projectile.GetComponent<OnlineDamager>();
            if (!!as_damager)
            {
                as_damager.filteredOutDamageables.Add(owner.self_oDamageable);
            }

            elapsedTime = 0f;
            canonArmed  = false;
            palletRdyToLaunch = false;
        }

        public void onRefresh()
        {
            PalletLauncher_Inst.transform.localPosition = Vector3.zero;

            if (!palletRdyToLaunch)
            {
                if (elapsedTime < cooldown)
                {
                    elapsedTime += Time.deltaTime;
                    return;
                }
                Pallet_Inst.SetActive(true);
                palletRdyToLaunch = true;
            }

        }
        public void onDisableEffect()
        {
            // DESPAWN spin hurtbox mesh SpinPowerObject
            if (isOnline)
                NetworkServer.Destroy(PalletLauncher_Inst);
            else
                GameObject.Destroy(PalletLauncher_Inst);
        }
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
        {
            //this.Log(name + " Input effects");
            if (!palletRdyToLaunch)
                return;

            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).down)
            {
                // Load
                Pallet_Inst.SetActive(false);
                canonArmed = true;
            }
            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).up)
            {
                // launch if loaded
                if (canonArmed)
                {
                    onActivation();
                }
            }
        }
        public bool turnOffTriggers()
        {
            //if (Access.Player().currentSpeed < 1f)
            //    return true;
            return false;
        }
    }

}
