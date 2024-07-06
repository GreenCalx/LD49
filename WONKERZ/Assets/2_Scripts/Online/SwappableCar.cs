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

        car.transform.parent    = locPC.transform;
        boat.transform.parent   = locPC.transform;
        plane.transform.parent  = locPC.transform;

        locPC.car.transform.parent = transform;
        locPC.boat.transform.parent = transform;
        locPC.plane.transform.parent = transform;

        var oldCar = locPC.car;
        var oldBoat = locPC.boat;
        var oldPlane = locPC.plane;

        locPC.car = car;
        locPC.boat = boat;
        locPC.plane = plane;

        WkzCar wCar = car.GetComponent<WkzCar>();
        locPC.current.rb = wCar.rb;

        CameraManager CM = Access.CameraManager();
        CM.OnTargetChange(locPC.transform);

        // update swappable car with old car from player
        car = oldCar;
        boat = oldBoat;
        plane = oldPlane;

        cameraFocusable.OnPlayerUnfocus();

        WkzCar old_wCar = oldCar.GetComponent<WkzCar>();
        cameraFocusable.transform.parent = old_wCar.rb.transform;
        cameraFocusable.transform.localPosition = Vector3.zero;

        // Transfer AbstractCollector
        AbstractCollector AC = GetComponentInChildren<AbstractCollector>();
        AC.transform.parent = locPC.current.rb.transform;
        AC.transform.localPosition = Vector3.zero;
        AC.transform.rotation = Quaternion.identity;

        // TODO update from Bag
        OnlinePlayerController opc = locPC.GetComponent<OnlinePlayerController>();
        OnlineCollectibleBag bag = opc.bag;
        bag.InitStatRefValues();
        bag.UpdateCar();

    }

}
