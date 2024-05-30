using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

namespace Wonkerz
{

    public class UISpeedAndLifePool : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI speedText;
        public Image speedBar;
        public TextMeshProUGUI lifePool;

        public Image cpImageFilled;
        public TextMeshProUGUI nAvailablePanels;
        public TextMeshProUGUI idOfLastCPTriggered;

        private PlayerController player;
        private CollectiblesManager CM;

        void Start()
        {
            player = Access.Player();
            CM = Access.CollectiblesManager();

            updateLastCPTriggered("x");
            cpImageFilled.fillAmount = 0f;

            updateSpeedCounter();
            updateLifePool();
        }

        void Update()
        {
            updateSpeedCounter();
            updateLifePool();
        }

        public void updateSpeedCounter()
        {
            #if false
            PlayerController pc = Access.Player();
            float playerVelocity = 0.0f;
            float max_speed = 0.0f;
            if (pc.car_old) {
                CarController cc = pc.car_old;
                playerVelocity = cc.GetCurrentSpeed();
                max_speed = cc.maxTorque;
            } else {
                SchCarController cc = pc.car_new;
                playerVelocity = cc.GetCurrentSpeed();
                max_speed = cc.maxTorque;
            }

            // Update Bar
            float bar_percent = Mathf.Clamp(playerVelocity / max_speed, 0f, max_speed);
            speedBar.fillAmount = bar_percent;

            // Update Text
            float velocityInKmH = playerVelocity * 3.6f;
            string lbl = ((int)velocityInKmH).ToString();
            lbl += " KMH";
            speedText.SetText(lbl);
            #endif

            PlayerController pc = Access.Player();
            var speed = (float)pc.car.car.GetCurrentSpeedInKmH_FromWheels();
            // TODO: compute max theoretical speed from car specs.
            var maxSpeed = 300.00f;
            var ratio = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);
            speedBar.fillAmount = ratio;
            speedText.SetText(((int)speed).ToString());
        }

        public void updateLifePool()
        {
            int n_nuts = CM.getCollectedNuts();
            lifePool.text = (n_nuts > 0) ? n_nuts.ToString() : "0";
        }

        public void updateAvailablePanels(int iVal)
        {
            int val = (iVal > 0) ? iVal : 0;

            string str = (val < 999) ? val.ToString() : "inf";

            nAvailablePanels.text = str;
        }

        public void updateLastCPTriggered(string iTxt)
        {
            idOfLastCPTriggered.text = iTxt;
        }

        public void updateCPFillImage(float iVal)
        {
            cpImageFilled.fillAmount = iVal;
        }
        public void updatePanelOnRespawn()
        {

        }
    }
}
