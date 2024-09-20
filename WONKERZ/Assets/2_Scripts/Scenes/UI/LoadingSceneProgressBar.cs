using UnityEngine;
using UnityEngine.UI;

namespace Wonkerz
{

    [RequireComponent(typeof(Image))]
    public class LoadingSceneProgressBar : MonoBehaviour
    {
        Image progressBar;

        void Start()
        {
            progressBar = GetComponent<Image>();
        }


        void Update()
        {
            progressBar.fillAmount = Access.managers.sceneMgr.operationProgress;
        }
    }
}
