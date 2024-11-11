using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Wonkerz;
using Schnibble;

public class SwappableCar : NetworkBehaviour
{
    public WkzCarController car;
    public WkzBoatController boat;
    public WkzGliderController plane;
    public CameraFocusable cameraFocusable;


    public void SwapCar()
    {
        PlayerController locPC = Access.Player();

        // Unequip Power
        if (locPC.self_PowerController)
        {
            locPC.self_PowerController.UnequipPower();
        }

        // get needed ref before swap occures
        Vector3 playerPosAtSwapStart    = locPC.GetTransform().position;
        Quaternion playerRotAtSwapStart = locPC.GetTransform().rotation;

        // Swap car
        car.transform.parent    = locPC.transform;
        locPC.car.transform.parent = transform;
        locPC.car.transform.localPosition = Vector3.zero;
        locPC.car.transform.rotation = Quaternion.identity;
        
        var oldCar = locPC.car;
        locPC.car = car;
        car = oldCar;

        // swap boat if exists, keep old one otherwise
        if (boat!=null)
        {
            boat.transform.parent   = locPC.transform;
            locPC.boat.transform.parent = transform;
            var oldBoat = locPC.boat;
            locPC.boat = boat;

            boat = oldBoat;
        }


        // swap plane if exists, keep old one otherwise
        if (plane!=null)
        {
            plane.transform.parent  = locPC.transform;
            locPC.plane.transform.parent = transform;
            var oldPlane = locPC.plane;
            locPC.plane = plane;

            plane = oldPlane;
        }


        // Update References after swap
        // local car/boat/plane are now old ones
        // new ones are on locPC

        WkzCar wCar = locPC.car.GetComponent<WkzCar>();
        locPC.current.rb = wCar.rb;

        CameraManager CM = Access.managers.cameraMgr;
        CM.OnTargetChange(locPC.transform);

        // update swappable car with old car from player
        
        
        cameraFocusable.OnPlayerUnfocus();

        WkzCar old_wCar = oldCar.GetComponent<WkzCar>();
        cameraFocusable.transform.parent = old_wCar.rb.transform;
        cameraFocusable.transform.localPosition = Vector3.zero;

        /// Update online player refs
        OnlinePlayerController opc = locPC.GetComponent<OnlinePlayerController>();

        // transfer bag to new car & apply
        OnlineCollectibleBag bag = opc.bag;
        bag.InitStatRefValues();
        bag.UpdateCar();

        // Transfer AbstractCollector
        // AbstractCollector AC = GetComponentInChildren<AbstractCollector>();
        // AC.transform.parent = locPC.current.rb.transform;
        // AC.transform.localPosition = Vector3.zero;
        // AC.transform.rotation = Quaternion.identity;
        AbstractCollector AC = locPC.gameObject.GetComponentInChildren<AbstractCollector>();
        AC.attachedOnlinePlayer = opc;

        // set damagers/damageable on new car and refresh refs
        opc.InitPlayerDamagers();
        opc.InitPlayerDamageable();

        car.transform.localPosition = Vector3.zero;
        car.transform.localRotation = Quaternion.identity;
        transform.position = playerPosAtSwapStart;
        transform.rotation = playerRotAtSwapStart;
        
        //Destroy(this);
    }

}
