using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{
    [Serializable]
    public struct CarBankInfo
    {
        public CAR_TYPE carType;
        public GameObject carPrefab;
        public GameObject OnDeathPrefab;
        public GameObject swappableCarPrefab;
    }

    [ExecuteInEditMode]
    public class WkzCarBanks : MonoBehaviour
    {
        [SerializeField]
        public List<CarBankInfo> carInfos;

        public GameObject GetDeathClonePrefab(CAR_TYPE iCarType)
        {
            return carInfos.Where(e => e.carType == iCarType).ToList()?[0].OnDeathPrefab;
        }

        public GameObject GetCarClone(CAR_TYPE iCarType)
        {
            return carInfos.Where(e => e.carType == iCarType).ToList()?[0].carPrefab;
        }

        public GameObject GetCarAsSwappable(CAR_TYPE iCarType)
        {
            return carInfos.Where(e => e.carType == iCarType).ToList()?[0].swappableCarPrefab;
        }

        #if UNITY_EDITOR
        public bool reset = false;

        void Update()
        {
            if (reset)
            {
                carInfos = new List<CarBankInfo>();
                foreach (CAR_TYPE ctype in Enum.GetValues(typeof(CAR_TYPE)))
                {
                    CarBankInfo new_info = new CarBankInfo();
                    new_info.carType = ctype;
                    carInfos.Add(new_info);
                }
                reset = false;
            }
        }
        #endif
    }


}