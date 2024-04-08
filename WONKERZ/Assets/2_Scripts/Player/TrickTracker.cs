using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.Physics;

// should be in namespace or sm shit but access to tricks not working for some reason ?
public enum WHEEL_LOCATION
{
    FRONT_RIGHT = 0,
    FRONT_LEFT = 1,
    BACK_RIGHT = 2,
    BACK_LEFT = 3
}

public class TrickTracker : MonoBehaviour
{

    [Header("MANDATORY")]
    public bool activate_tricks;
   
   // Two purposes :
   // 1 : less mess in the editor + full UI can be easily hidden
   // 2 : Avoid ui instantiation in old scene (the loading scene most likely) 
   // as this Start method will be be called before LoadingScene is fully unloaded
   // and there is no way to overload Instantiate method with an explicit scene name
   // Unity shits.
    public GameObject UIHandle;

    public GameObject trickUIRef;
    private GameObject trickUIInst;
    public TrickUI trickUI;
    public Transform player_transform;
    [HideInInspector]
    public int storedScore = 0;

    [Header("TWEAK PARAMS")]
    public float combo_multiplier = 1f;
    public float rot_epsilon = 2f;
    public float line_cooldown = 0.4f;
    public float hold_time_start_flat_trick = 0.4f;
    public int MIN_SCORE_FOR_DISPLAY = 10;

    [Header("DEBUG")]
    public bool[] wheels_statuses;
    private double time_trick_started;


    [HideInInspector]
    public TrickLine trick_line;


    //[HideInInspector]
    public float init_rot_x, init_rot_y, init_rot_z;
    public Quaternion initQ;

    //[HideInInspector]
    private List<float> rec_rot_x, rec_rot_y, rec_rot_z;

    public Vector3 rotations;   

    [HideInInspector]
    public float time_waited_after_line;
    [HideInInspector]
    public float recorded_time_trick;
    private KeyValuePair<Trick, float> flat_trick_starter;

    private SchCarController CC_new;
    //private CarController CC_old;

    public bool ready_to_rec_line;

    private Coroutine trickRecordCo;
    void Start()
    {
        //init();
    }

    public void init(GameObject iUIHandle)
    {
        if (!activate_tricks)
            return;

        //   CC_old = Access.Player().car_old;
        //   if (!CC_old)
        //   {
        //       activate_tricks = false;
        //       return;
        //   }

        wheels_statuses = new bool[4];
        for (int i = 0; i < wheels_statuses.Length; i++)
            wheels_statuses[i] = true;

        trickUIInst = Instantiate(trickUIRef, iUIHandle.transform);
        trickUI =  trickUIInst.GetComponent<TrickUI>();

        trick_line = new TrickLine();
        time_trick_started = 0;
        flat_trick_starter = new KeyValuePair<Trick, float>();

        init_rot_x = 0f;
        init_rot_y = 0f;
        init_rot_z = 0f;

        initQ = Quaternion.identity;

        storedScore = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!activate_tricks || (trick_line == null))
            return;

        updateWheelStatuses();

        // Look for trickline cooldown
        ready_to_rec_line = ((Time.time - time_waited_after_line) > line_cooldown) /*&& carIsOnGround()*/;

        if (ready_to_rec_line || trick_line.is_opened)
        {
            if (!trick_line.is_opened)
            {
                if (tryOpenLine())
                {
                    // TODO : New record system wip
                    // if (trickRecordCo!=null)
                    //     StopCoroutine(trickRecordCo);
                    // trickRecordCo = StartCoroutine(TrickRecordCo(0.1f));

                    this.Log("open line");
                    trickUI.recordingTrick();
                    initRotationsRecord();
                    recordRotations();
                    time_trick_started = Time.time;
                }
            }
            else
            {
                recordRotations();
                if (tryContinueLine())
                { /*continue line..*/ }
                else
                    end_line();
            }
        }
        updateUI();
    }

    IEnumerator TrickRecordCo(float frequency)
    {
        // Rigidbody car_rb = CC_old.GetComponent<Rigidbody>();
        // if (car_rb==null)
        //     yield break;
        
        // 0.5 circle is pi, our quanta is 180deg
        float threshold = Mathf.PI;

        // in rad?
        float accumulated_x = 0f;
        float accumulated_y = 0f;
        float accumulated_z = 0f;
        
        float previous_rpf_x = 0f;
        float previous_rpf_y = 0f;
        float previous_rpf_z = 0f;

        while (trick_line.is_opened)
        {
            yield return new WaitForSeconds(frequency);

            //recordRotations();
            //Vector3 angVel = car_rb.angularVelocity; // rad/sec
            Vector3 angVel = Vector3.zero;

            float rpf_x =  angVel.x * (60f/(2*Mathf.PI)) *frequency;
            float rpf_y =  angVel.y * (60f/(2*Mathf.PI)) *frequency;
            float rpf_z =  angVel.z * (60f/(2*Mathf.PI)) *frequency; 
            
            // rpf_x -= previous_rpf_x;
            // rpf_y -= previous_rpf_y;
            // rpf_z -= previous_rpf_z;

            // rpf_x = 1f / rpf_x;
            // rpf_y = 1f / rpf_y;
            // rpf_z = 1f / rpf_z;

            accumulated_x += rpf_x;
            accumulated_y += rpf_y;
            accumulated_z += rpf_z;

            if (accumulated_x >= threshold)
            {
                this.Log("trick on x");
            }
            if (accumulated_y >= threshold)
            {
                this.Log("trick on y");
            }
            if (accumulated_z >= threshold)
            {
                this.Log("trick on z");
            }

            previous_rpf_x = rpf_x;
            previous_rpf_y = rpf_y;
            previous_rpf_z = rpf_z;
        }

        this.Log("v RECORD RESULT v");
        this.Log("acc x " + accumulated_x);
        this.Log("acc y " + accumulated_y);
        this.Log("acc z " + accumulated_z);

        yield return null;
    }

    public void recordRotations()
    {
        //Vector3 curr_PYR = player_transform.rotation.eulerAngles;
        Vector3 curr_PYR = player_transform.rotation.eulerAngles;

        float angle =0f;
        Vector3 axis = Vector3.zero;

        player_transform.rotation.ToAngleAxis(out angle, out axis);
        


        // add to rec values
        //if (!rec_rot_x.Contains(curr_PYR.x))
        rec_rot_x.Add(curr_PYR.x);
        //if (!rec_rot_y.Contains(curr_PYR.y))
        rec_rot_y.Add(curr_PYR.y);
        //if (!rec_rot_z.Contains(curr_PYR.z))
        rec_rot_z.Add(curr_PYR.z);

        updateRotations();
    }

    private float rotDiff(float iInit, float iCurrent)
    {
        //float ret = iCurrent - iInit;
        //return (ret+180) % 360 - 180;
        return Mathf.DeltaAngle(iInit, iCurrent);
    }

    public void updateRotations()
    {
        int xcount = rec_rot_x.Count;
        if (xcount > 0)
            rotations.x = rotDiff(rec_rot_x[0], rec_rot_x[xcount - 1]);
        int ycount = rec_rot_y.Count;
        if (ycount > 0)
            rotations.y = rotDiff(rec_rot_y[0], rec_rot_y[ycount - 1]);
        int zcount = rec_rot_z.Count;
        if (zcount > 0)
            rotations.z = rotDiff(rec_rot_z[0], rec_rot_z[zcount - 1]);

    }

    public void initRotationsRecord()
    {
        init_rot_x = player_transform.eulerAngles.x;
        init_rot_y = player_transform.eulerAngles.y;
        init_rot_z = player_transform.eulerAngles.z;

        rec_rot_x = new List<float>();
        rec_rot_y = new List<float>();
        rec_rot_z = new List<float>();
    }

    // consume rotation
    public void updateConsumedRotations(TrickCondition tc)
    {
        //Vector3 curr_PYR = MathUtils.getPYR(player_transform.rotation);
        if (tc.x_rot != 0)
        { rec_rot_x.Clear(); }
        if (tc.y_rot != 0)
        { rec_rot_y.Clear(); }
        if (tc.z_rot != 0)
        { rec_rot_z.Clear(); }
        //updateRotations();
    }

    public bool tryOpenLine()
    {
        Trick opener1 = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.NEUTRAL);
        Trick opener2 = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.FLAT);

        if (opener1 != null)
        {
            trick_line.open(opener1);
            return true;
        }
        else if (opener2 != null)
        {

            if (flat_trick_starter.Equals(default(KeyValuePair<Trick, float>)))
            { flat_trick_starter = new KeyValuePair<Trick, float>(opener2, 0f); }

            Trick t = flat_trick_starter.Key;
            if (t.name == opener2.name)
            {
                flat_trick_starter = new KeyValuePair<Trick, float>(opener2, flat_trick_starter.Value + Time.deltaTime);
            }
            else
            {
                flat_trick_starter = new KeyValuePair<Trick, float>();
            }

            if (flat_trick_starter.Value > hold_time_start_flat_trick)
            {
                trick_line.open(opener2);
                return true;
            }
        }
        return false;
    }

    public void OnCollisionEnter(Collision iCol)
    {
        if (!carIsOnGround() && activate_tricks)
            end_line();
    }

    public bool tryContinueLine()
    {
        // NEW TRICK / continuing trick
        Trick tbasic    = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.BASIC);
        Trick tflat     = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.FLAT);
        Trick tneutral  = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.NEUTRAL);
        Trick tignore   = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.IGNORE);

        double trick_duration = Time.time - time_trick_started;
        if (tbasic != null)
        {
            trick_line.add(tbasic, trick_duration);
            updateConsumedRotations(tbasic.condition);
            time_trick_started = Time.time;
            return true;
        }
        else if (tflat != null)
        {
            if (trick_duration > hold_time_start_flat_trick)
            {
                trick_line.add(tflat, trick_duration);
                time_trick_started = Time.time;
                return true;
            }
        }
        else if (tneutral != null)
        {
            time_trick_started = Time.time;
            trick_line.add(tneutral, trick_duration);
            return true;
        }
        else if (tignore != null)
        {
            return true;
        }

        return false;
    }

    public void end_line(bool iForceFail = false)
    {
        if (trick_line==null)
            return;

        if (carIsOnGround() && !iForceFail)
        {
            int trick_score = trick_line.getLineScore(combo_multiplier);

            storedScore += trick_score;

            if (!!trickUI)
            {
                trickUI.displayTricklineScore(storedScore);
                trickUI.displayTricklineTricks(trick_line.getTrickList());
                trickUI.validateTrick();
            }
        }
        else
        {
            if (!!trickUI)
            {
                trickUI.failTrick();
                trickUI.displayTricklineScore(storedScore);
                trickUI.displayTricklineTricks(new List<Trick>(0));
            }
        }

        if (trick_line.is_opened)
        {
            trick_line.close();

            rec_rot_x.Clear();
            rec_rot_y.Clear();
            rec_rot_z.Clear();
        }

        time_waited_after_line = Time.time;
    }

    public void updateUI()
    {
        if (trickUI == null)
        { return; }

        if (!trick_line.is_opened && ready_to_rec_line)
        {
            trickUI.displayTrick("");
            trickUI.displayScore(0);
            return;
        }

        if (trick_line.getLineScore(combo_multiplier) < MIN_SCORE_FOR_DISPLAY)
        {
            return;
        }

        string tricks = "";
        using var enumerator = trick_line.full_line.GetEnumerator();
        var last = !enumerator.MoveNext();
        TrickLine.TrickTimePair ttp;

        while (!last)
        {
            ttp = enumerator.Current;
            tricks += ttp.trick.name;
            last = !enumerator.MoveNext();

            if (!last)
                tricks += " + ";
        }

        trickUI.displayTrick(tricks);
        trickUI.displayScore(trick_line.getLineScore(combo_multiplier));

    }

    public void updateWheelStatuses()
    {
        #if false
        // update wheels
        Axle front = CC.axles[(int)AxleType.front];
        Axle rear = CC.axles[(int)AxleType.rear];

        wheels_statuses[(int)WHEEL_LOCATION.FRONT_LEFT] = front.left.isGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.FRONT_RIGHT] = front.right.isGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.BACK_LEFT] = rear.left.isGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.BACK_RIGHT] = rear.right.isGrounded;
        #endif
    }

    public bool carIsOnGround()
    {
        foreach (bool w in wheels_statuses)
        {
            if (w)
            { return true; }
        }
        return false;
    }
}
