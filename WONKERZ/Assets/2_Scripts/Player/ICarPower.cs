using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Schnibble.Managers;
using Mirror;

namespace Wonkerz
{
    public interface ICarPower
    {
        // Itf attributes
        public string name { get; set; }
        public  float cooldown {get; set;}
        public float recovery_cooldown  {get; set;}
        public int baseDamage {get; set;}
        public OnlinePlayerController owner {get; set;}
        public PlayerPowerElement fullPowerDef {get; set;}
        public bool isOnline { get; set; }
        public  float elapsed_cooldown {get; set;}
        public float elapsed_recovery_cooldown {get; set;}
        public bool isArmed {get; set;}
        public bool isRecovering {get; set;}

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
        // Itf attributes
        public string name { get; set; }
        public  float cooldown {get; set;}
        public float recovery_cooldown  {get; set;}
        public int baseDamage {get; set;}
        public OnlinePlayerController owner {get; set;}
        public PlayerPowerElement fullPowerDef {get; set;}
        public bool isOnline { get; set; }
        public  float elapsed_cooldown {get; set;}
        public float elapsed_recovery_cooldown {get; set;}
        public bool isArmed {get; set;}
        public bool isRecovering {get; set;}

        // Power local
        public GameObject turboParticlesRef;
        public GameObject turboParticlesInst;
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

    // TODO : known bug : knight lance default damage doesnt update inbetween activation if 
    //      player picks up atk upgrade.
    [System.Serializable]
    public class KnightLanceCarPower : ICarPower
    {
        // Itf attributes
        public string name { get; set; }
        public  float cooldown {get; set;}
        public float recovery_cooldown  {get; set;}
        public int baseDamage {get; set;}
        public OnlinePlayerController owner {get; set;}
        public PlayerPowerElement fullPowerDef {get; set;}
        public bool isOnline { get; set; }
        public  float elapsed_cooldown {get; set;}
        public float elapsed_recovery_cooldown {get; set;}
        public bool isArmed {get; set;}
        public bool isRecovering {get; set;}
        
        //  Power Local
        public float thrustTime = 1f;
        public float thrustRotSpeed = 25f;
        public float preThrustZOffset = -1f;
        public float thrustZOffset = 5f;
        public float damageMulOnThrust = 1.5f;
        public GameObject KnightLanceObject_Ref;
        private GameObject KnightLanceObject_Inst;
        private Transform attachPoint;
        private bool thrusting = false;
        private OnlineWeapon selfOnlineWeapon;
        private OnlineDamager selfDamager;

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
            baseDamage = iPowerDef.baseDamage;
            recovery_cooldown = iPowerDef.recovery;

            isOnline = Access.managers.gameSettings.isOnline;
            elapsed_cooldown = 0f;
            elapsed_recovery_cooldown = 0f;
            isRecovering = false;
            isArmed = false;

            thrusting = false;
        }

        public void onEnableEffect()
        {
            owner = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer;
            WeightIndicator WI = owner.GetComponentInChildren<WeightIndicator>();
            attachPoint = WI.transform;

            if (!!WI)
            {
                KnightLanceObject_Inst = GameObject.Instantiate(KnightLanceObject_Ref, attachPoint);
                foreach(Transform child in KnightLanceObject_Inst.transform)
                {
                    Collider c = child.GetComponent<Collider>();
                    if (!!c)
                        Utils.IgnoreAllPlayerColliders(c, owner.transform);
                }
                if (isOnline)
                {
                    NetworkServer.Spawn(KnightLanceObject_Inst);
                    selfDamager = KnightLanceObject_Inst.GetComponentInChildren<OnlineDamager>();
                    selfDamager.damage = Utils.ApproxDamage(baseDamage * owner.atkMul);

                }
                KnightLanceObject_Inst.SetActive(true);
            }
        }

        public void onActivation()
        {
            isArmed = false;    
            selfOnlineWeapon = KnightLanceObject_Inst.GetComponent<OnlineWeapon>();
            if (selfOnlineWeapon!=null)
            {
                UnityEvent cbAfterThrust = new UnityEvent();
                cbAfterThrust.AddListener(OnWeaponFinish);
                selfDamager.damage = Utils.ApproxDamage(baseDamage * damageMulOnThrust * owner.atkMul);
                selfDamager.filteredOutDamageables.Add(owner.self_oDamageable);
                selfOnlineWeapon.Thrust(thrustTime, thrustRotSpeed, thrustZOffset, cbAfterThrust);
                thrusting = true;

                elapsed_cooldown = 0f;
                elapsed_recovery_cooldown = 0f;
                isRecovering = recovery_cooldown > 0f;
            }
            else
                this.LogError("ICarPower: Online Weapon Missing on KnightLance.");
        }

        public void OnWeaponFinish()
        {
            selfDamager.damage = Utils.ApproxDamage(baseDamage * owner.atkMul);
            thrusting = false;
        }


        public void onRefresh()
        {

            if (isRecovering)
            {
                elapsed_recovery_cooldown += Time.deltaTime;
                isRecovering = elapsed_recovery_cooldown < recovery_cooldown;
                return;
            }

            if (thrusting)
                return;

            if (isArmed)
                KnightLanceObject_Inst.transform.localPosition = new Vector3(0f,0f, preThrustZOffset);
            else
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
            if (thrusting)
                return;
            if (elapsed_cooldown < cooldown)
                return;
            if (isRecovering)
                return;
            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).down)
            {
                isArmed = true;
            }
            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).up)
                onActivation();
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
        // Itf attributes
        public string name { get; set; }
        public  float cooldown {get; set;}
        public float recovery_cooldown  {get; set;}
        public int baseDamage {get; set;}
        public OnlinePlayerController owner {get; set;}
        public PlayerPowerElement fullPowerDef {get; set;}
        public bool isOnline { get; set; }
        public  float elapsed_cooldown {get; set;}
        public float elapsed_recovery_cooldown {get; set;}
        public bool isRecovering {get; set;}
        public bool isArmed {get; set;}

        // Power local
        public GameObject PalletLauncher_Ref;
        private GameObject PalletLauncher_Inst;
        private Canon Canon_Handle;
        private Transform attachPoint;
        private Transform palletLoadingPoint;
        private Transform palletLaunchingPoint;


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
            baseDamage = iPowerDef.baseDamage;
            recovery_cooldown = iPowerDef.recovery;
            
            elapsed_cooldown = 0f;
            elapsed_recovery_cooldown = 0f;
            isOnline = Access.managers.gameSettings.isOnline;
            isArmed = false;
        }

        public void onEnableEffect()
        {
            owner = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer;
            WeightIndicator WI = owner.GetComponentInChildren<WeightIndicator>();
            attachPoint = WI.transform;

            if (!!WI)
            {
                PalletLauncher_Inst = GameObject.Instantiate(PalletLauncher_Ref, attachPoint);
                
                foreach(Transform child in PalletLauncher_Inst.transform)
                {
                    Collider c = child.GetComponent<Collider>();
                    if (!!c)
                        Utils.IgnoreAllPlayerColliders(c, owner.transform);
                }
                if (isOnline)
                {
                    NetworkServer.Spawn(PalletLauncher_Inst);
                }
                Canon_Handle = PalletLauncher_Inst.GetComponent<Canon>();
                palletLoadingPoint = Canon_Handle.loadingPoint;
                palletLaunchingPoint = Canon_Handle.spawnPoint;
                PalletLauncher_Inst.SetActive(true);
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
                as_damager.damage = Utils.ApproxDamage(baseDamage * owner.atkMul);
            }

            elapsed_cooldown = 0f;
            isArmed  = false;

            isRecovering = recovery_cooldown > 0f;
        }

        public void onRefresh()
        {
            PalletLauncher_Inst.transform.localPosition = Vector3.zero;
            if (isRecovering)
            {
                elapsed_recovery_cooldown += Time.deltaTime;
                isRecovering = elapsed_recovery_cooldown < recovery_cooldown;
                return;
            }

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
                NetworkServer.Destroy(PalletLauncher_Inst);
            else
                GameObject.Destroy(PalletLauncher_Inst);
        }
        public void applyEffectInInputs(GameController iEntry, PlayerController iCC)
        {
            //this.Log(name + " Input effects");
            if (elapsed_cooldown < cooldown)
                return;
            if (isRecovering)
                return;

            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).down)
            {
                isArmed = true;
            }
            if (iEntry.GetButtonState((int)PlayerInputs.InputCode.TriggerPower).up)
            {
                // launch if loaded
                if (isArmed)
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
