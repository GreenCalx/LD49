using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {

    public class JunkyardLock : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CollectiblesManager cm = Access.CollectiblesManager();
            bool hasAllGaragers = true;

            hasAllGaragers =    cm.hasGaragist(Constants.SN_DESERT_TOWER)
                                && cm.hasGaragist(Constants.SN_GROTTO_TRACK)
                                && cm.hasGaragist(Constants.SN_WATERWORLD_TRACK)
                                && cm.hasGaragist(Constants.SN_SKYCASTLE_TRACK);
            if (hasAllGaragers)
            Destroy(gameObject);
        }
    }
}
