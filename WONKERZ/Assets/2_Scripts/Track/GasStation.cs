using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class GasStation : MonoBehaviour, IControllable
    {
        [Header("BellRefs")]
        public Transform bell_t;
        public ParticleSystem bell_PS;
        public AudioClip bell_SFX;

        [Header("TourniquetRef")]
        public HingeJoint tourniquetHinge;
        [Header("PumpistRefs")]
        public ParticleSystem sleeping_PS;

        [Header("NutConvertAnim")]
        public Transform convertNutsDestination;
        public GameObject nutsRef;
        public AnimationCurve heightCurve;

        [Header("Params")]
        public Animator animator;

        public Color LightColorOnDeactivated;
        public Color LightColorOnActivated;
        public Light CPAnimatorLight;
        public Light CPAnimatorHalo;

        public float nutConversionInterval = 0.2f;
        private float nutConversionElapsed = 99f;

        private bool IsPumpingGas = false;
        private string animatorParm = "IsPumping";
        private string animatorStationActivationParm = "IsStationActivated";

        public bool bypassNutsCost = false;
        private bool convertNuts = false;

        public UIGasStationAskCoinz askCoinz;

        // Start is called before the first frame update
        void Start()
        {
            nutConversionElapsed = 99f;
            changeLightsColor(LightColorOnDeactivated);
            convertNuts = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (convertNuts)
            {
                if (bypassNutsCost)
                {
                    Access.Player().turbo.current = 1f;
                    //Access.UITurboAndSaves().updateTurboBar();
                    return;
                }
                nutConversionElapsed += Time.deltaTime;
                if (nutConversionElapsed > nutConversionInterval)
                {
                    bool convertSuccess = Access.CollectiblesManager().tryConvertNutToTurbo();
                    nutConversionElapsed = 0f;
                    if (convertSuccess)
                    {
                        if (!IsPumpingGas)
                        {
                            animator.SetBool(animatorParm, true);
                            IsPumpingGas = true;
                            //Access.UITurboAndSaves().startTurboRefilAnim(true);
                        }
                        StartCoroutine(nutsConvertAnim(Instantiate(nutsRef)));
                    }
                    else if (!convertSuccess && IsPumpingGas)
                    {
                        animator.SetBool(animatorParm, false);
                        IsPumpingGas = false;
                        //Access.UITurboAndSaves().startTurboRefilAnim(false);
                    }
                }
            }
        }

        IEnumerator nutsConvertAnim(GameObject convertedNut)
        {
            convertedNut.transform.position = Access.Player().transform.position;
            Vector3 start = convertedNut.transform.position;
            Vector3 end = convertNutsDestination.transform.position;
            Vector3 initScale = convertedNut.transform.localScale;

            for (float time = 0f; time < 1f; time += Time.deltaTime)
            {
                convertedNut.transform.position  = Vector3.Lerp( start, end, time)
                                                   + Vector3.up * heightCurve.Evaluate(time);
                convertedNut.transform.localScale  = Vector3.Lerp( initScale, Vector3.zero, time);
                yield return null;
            }

            Destroy(convertedNut.gameObject);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            //convertNuts = (Entry[(int) PlayerInputs.InputCode.GiveCoinsForTurbo] as GameInputButton).GetState().down;
        }

        void OnDestroy()
        {
            try{
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
                #pragma warning disable CS0168
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
            #pragma warning restore CS0168
        
        }

        void changeLightsColor(Color iColor)
        {
            if (CPAnimatorLight!=null)
            { CPAnimatorLight.color = iColor; }
            if (CPAnimatorHalo!=null)
            { CPAnimatorHalo.color = iColor; }
        }

        void OnTriggerEnter(Collider iCollider)
        {
            if (Utils.isPlayer(iCollider.gameObject))
            {
                tourniquetHinge.useMotor = true;
            }
        }

        void OnTriggerStay(Collider iCollider)
        {
            if (Utils.isPlayer(iCollider.gameObject))
            {
                //UICheckpoint uicp = Access.UICheckpoint();

                //Access.PlayerInputsManager().player1.Attach(this as IControllable);
                Access.CheckPointManager().playerInGasStation = true;
                askCoinz.gameObject.SetActive(true);
                askCoinz.animate = true;
            }
        }

        void OnTriggerExit(Collider iCollider)
        {
            if (Utils.isPlayer(iCollider.gameObject))
            {
                //Access.PlayerInputsManager().player1.Detach(this as IControllable);
                Access.CheckPointManager().playerInGasStation = false;

                if (IsPumpingGas)
                {
                    IsPumpingGas = false;
                    animator.SetBool(animatorParm, false);
                    //Access.UITurboAndSaves().startTurboRefilAnim(false);
                }

                askCoinz.gameObject.SetActive(false);
                askCoinz.animate = false;
            }
        }

        public void ActivateGasStation()
        {
            GetComponent<CheckPoint>()?.triggerCheckPoint();
            animator.SetBool(animatorStationActivationParm, true);
            changeLightsColor(LightColorOnActivated);
        }

        public void RingBell()
        {
        
            Schnibble.Utils.SpawnAudioSource( bell_SFX, bell_t);
            bell_PS.Play();
            sleeping_PS.Stop();
            ActivateGasStation();
        }
    }
}
