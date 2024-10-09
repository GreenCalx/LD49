using UnityEngine;
using Schnibble;
using Schnibble.Managers;
using Mirror;

namespace Wonkerz
{
    public interface ICarPower
    {
        public PlayerPowerElement fullPowerDef {get; set;}
        public string name { get; set; }
        public bool isOnline { get; set; }
        public  float cooldown {get; set;}
        public  float elapsed_cooldown {get; set;}

        // build itself from playerpowerelement explicitely
        public void init(PlayerPowerElement iPowerElement);
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

    // TODO : Broken ATM old logic
    public class TurboCarPower : ICarPower
    {
        public GameObject turboParticlesRef;
        public GameObject turboParticlesInst;
        public PlayerPowerElement fullPowerDef {get; set;}
        public  float cooldown {get; set;}
        public  float elapsed_cooldown {get; set;}
        public string name { get; set; }
        public bool isOnline { get; set; }
        public TurboCarPower()
        {

        }

        public void init(PlayerPowerElement iPowerDef)
        {
            name = iPowerDef.name;
            cooldown = iPowerDef.cooldown;
            fullPowerDef = iPowerDef;
            
            turboParticlesRef = iPowerDef.prefabToAttachOnPlayer;
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

    [System.Serializable]
    public class KnightLanceCarPower : ICarPower
    {
        public OnlinePlayerController owner;
        public PlayerPowerElement fullPowerDef {get; set;}
        public  float cooldown {get; set;}
        public  float elapsed_cooldown {get; set;}
        public GameObject KnightLanceObject_Ref;
        private GameObject KnightLanceObject_Inst;
        private Transform attachPoint;

        public string name { get; set; }
        public bool isOnline { get; set; }

        public KnightLanceCarPower() {}
        public KnightLanceCarPower(PlayerPowerElement iPowerDef)
        {
            init(iPowerDef);
        }

        public void init(PlayerPowerElement iPowerDef)
        {
            fullPowerDef = iPowerDef;
            name = iPowerDef.name;
            KnightLanceObject_Ref = iPowerDef.prefabToAttachOnPlayer;
            cooldown = iPowerDef.cooldown;

            isOnline = Access.managers.gameSettings.isOnline;
            elapsed_cooldown = 0f;
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
            elapsed_cooldown = 0f;
        }

        public void onRefresh()
        {
            KnightLanceObject_Inst.transform.localPosition = Vector3.zero;
            if (elapsed_cooldown < cooldown)
            {
                elapsed_cooldown += Time.deltaTime;
                return;
            }
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
    [System.Serializable]
    public class PalletLauncherCarPower : ICarPower
    {
        public PlayerPowerElement fullPowerDef {get; set;}
        public  float cooldown {get; set;}
        public  float elapsed_cooldown {get; set;}
        public OnlinePlayerController owner;

        public GameObject PalletLauncher_Ref;
        private GameObject PalletLauncher_Inst;


        private Canon Canon_Handle;

        private Transform attachPoint;
        private Transform palletLoadingPoint;
        private Transform palletLaunchingPoint;

        private bool palletRdyToLaunch = false;
        private bool canonArmed = false;

        public string name { get; set; }
        public bool isOnline { get; set; }

        public PalletLauncherCarPower() {}
        public PalletLauncherCarPower(PlayerPowerElement iPowerDef)
        {
            init(iPowerDef);
        }

        public void init(PlayerPowerElement iPowerDef)
        {
            fullPowerDef = iPowerDef;
            name = iPowerDef.name;
            PalletLauncher_Ref = iPowerDef.prefabToAttachOnPlayer;
            cooldown = iPowerDef.cooldown;

            
            canonArmed = false;
            palletRdyToLaunch = true;
            elapsed_cooldown = 0f;
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

            elapsed_cooldown = 0f;
            canonArmed  = false;
            palletRdyToLaunch = false;
        }

        public void onRefresh()
        {
            PalletLauncher_Inst.transform.localPosition = Vector3.zero;

            if (!palletRdyToLaunch)
            {
                if (elapsed_cooldown < cooldown)
                {
                    elapsed_cooldown += Time.deltaTime;
                    return;
                }

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
