using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasketBallBooth : MonoBehaviour
{
    public Canon canon;
    public TMPro.TextMeshProUGUI SCORE_LBL;
    public TMPro.TextMeshProUGUI TIME_LBL;
    public GameObject EventLauncher;
    public GameObject EventLauncherFocusable;
    public Transform rewardSpawn;
    public GameObject nutExplodingPatch_Ref;
    public Transform gameBarrier;

    public bool eventIsActive = false;
    [Header("Tweaks")]
    public float canon_RoF = 2f;
    public float eventDuration = 30f;
    public float barrierYOffsetWhenOffGame = -1f;
    [Header("Internals")]
    public int score = 0;
    private float rest_time = 0f;
    private float event_elapsed_time = 0f;
    public List<GameObject> spawnedBalls;
    private Vector3 barrierInitPosition = Vector3.zero;// opened

    // Start is called before the first frame update
    void Start()
    {
        rest_time = 0f;
        event_elapsed_time = 0f;

        barrierInitPosition = gameBarrier.position;
        gameBarrier.position += new Vector3(0f, barrierYOffsetWhenOffGame, 0f); // start open

        updateTimeLbl();
        stopEvent();
    }

    // Update is called once per frame
    void Update()
    {
        if (eventIsActive)
        {
            event_elapsed_time += Time.deltaTime;
            updateTimeLbl();
            if (event_elapsed_time > eventDuration)
            {
                stopEvent();
            }

            rest_time += Time.deltaTime;
            if (rest_time > canon_RoF)
            {
                spawnedBalls.Add( canon.Fire() );
                rest_time = 0f;
            }
        }
    }

    public void scorePoints(int iPoints)
    {
        score += iPoints;
        updateScoreLbl();
    }

    public void startEvent()
    {
        if (eventIsActive)
            return;

        EventLauncher.SetActive(false);
        EventLauncherFocusable.SetActive(false);

        eventIsActive = true;
        event_elapsed_time = 0f;
        score = 0;
        spawnedBalls = new List<GameObject>(0);

        updateTimeLbl();
        updateScoreLbl();

        gameBarrier.position = barrierInitPosition;
    }

    public void stopEvent()
    {
        if (!eventIsActive)
            return;

        eventIsActive = false;
        StartCoroutine(waitEventEnd());
    }

    IEnumerator waitEventEnd()
    {
        spawnedBalls = spawnedBalls.Where(e => e != null).ToList();
        while(spawnedBalls.Count > 0)
        {
            spawnedBalls = spawnedBalls.Where(e => e != null).ToList();
            yield return null;
        }

        // Actual end
        rewardPlayer();

        EventLauncher.SetActive(true);
        EventLauncherFocusable.SetActive(true);

        gameBarrier.position = barrierInitPosition + new Vector3(0f, barrierYOffsetWhenOffGame, 0f);

    }

    public void rewardPlayer()
    {
        if (score <= 0)
            return;
        GameObject nuts = Instantiate(nutExplodingPatch_Ref);
        nuts.transform.position = rewardSpawn.position;
        NutExplodingPatch nep = nuts.GetComponent<NutExplodingPatch>();
        nep.n_nuts = score * 2;
        nep.forceStr = 10f;
        nep.triggerPatch();
    }

    public void updateTimeLbl()
    {
        TIME_LBL.text = "TIME : " + ((int)(eventDuration - event_elapsed_time)).ToString();
    }

    public void updateScoreLbl()
    {
        SCORE_LBL.text = score.ToString();
    }
}
