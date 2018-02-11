using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public class Interaction : MonoBehaviour
{
    public GameObject setting_modal, gameOverModal, youWinModal, instruction_modal; // setting_modal.SetActive
    public GameObject body1, body2, body3;//, body4;
    public GameObject bodyaesg_1, bodyaesg_2, bodyaesg_3;

    private float range = 10000f;
    //public float damage = 10f;
    //public float impact = 100f;

    public Camera fpsCamera;
    private GameObject impactSpark;
    public GameObject imageTarget, imageTarget2;
    public GameObject aed_patch1, aed_patch2, aed_patch3;

    private Vector3 force;

    //patches variables
    public Button aed_button, amb_button, cpr_button, fia_button, set_button;
    public Button resume_buton, forfeit_button, instruction_button, instruction_back_button;
    public Button gameOver_backToHome_button, youWin_backToHome_button;
    private int Speed = 10;

    private bool aed_switch, cpr_switch, firstaid_switch, call_switch;

    //public GameObject patchPrefab;
    public GameObject patchSpawn1, patchSpawn2, patchSpawn3;
    public Text clock, amb_timer, score_text, debug_scoretext, deathtimer;

    public Text resp_vital; //breathing
    public Text puls_vital; //heart
    public Text temp_vital; //temperature

    //torque, for subtle shock movement
    private float torque = 100.0f;

    //create victim
    private Victim victim;
    private bool pause;

    //add calltime variable for call ambulance response time 
    private float calltime, deathtime = 0;
    private float vital_ticks, offscreen_ticks, death_ticks = 0;
    private bool track_spawned, track_spawned2;
    private int score = 0;
    private int action_count = 0, body_rand = 0;

    private persistent_obj re_persistent_obj_script;
    private UpdatePlayerStatisticsRequest upsr;
    private List<StatisticUpdate> su_list;
    private StatisticUpdate su;

    void Start()
    {
        pause = false;
        setting_modal.SetActive(false);
        gameOverModal.SetActive(false);
        youWinModal.SetActive(false);

        //clear all patch to false
        aed_patch1.SetActive(false);
        aed_patch2.SetActive(false); 
        aed_patch3.SetActive(false);

        patchSpawn1.SetActive(false);
        patchSpawn2.SetActive(false);
        patchSpawn3.SetActive(false);

        instruction_modal.SetActive(false);
        victim = new Victim();
        aed_switch = false;
        cpr_switch = false;
        firstaid_switch = false;
        call_switch = false;
        action_count = 0;
        //(11.40min +/- 4.88 minutes) - convertion from realtime to relative time -> Random.Range((6.52f/10) - (16.28f/10)), number derive from (11.4 + 4.88) and (4.88 + 4.88)
        calltime = UnityEngine.Random.Range((6.52f * 2), (16.28f * 2)); // play with the variable time, *2 for longer playing time
        //on comparison to stroke death time, it takes about 4minutes or less for your body without oxygen and blood causing death
        deathtime = UnityEngine.Random.Range((3.00f * 2), (4.00f * 2)); // play with the variable time, 3 to 4 seconds for this case
        body_rand = UnityEngine.Random.Range(1, 12); //number of bodies

        //change accordingly based on the force imitation of a CPR or AED accordingly and can be split eventually
        force = new Vector3(0.0f, 1000.0f, 0.0f);

        //start - persistent obj passing
        var re_persistent_obj = GameObject.Find("PersistenObj");
        re_persistent_obj_script = re_persistent_obj.GetComponent<persistent_obj>();

        Debug.Log(re_persistent_obj_script.get_email().ToString());
        Debug.Log(re_persistent_obj_script.get_password().ToString());
        GameObject.DontDestroyOnLoad(GameObject.Find("PersistenObj"));
        //end - persistent obj passing

        //id aed is pressed
        Button aed_btn = aed_button.GetComponent<Button>();
        aed_btn.onClick.AddListener(aed_toggle);

        //ambulance call button, where timer will start
        Button amb_btn = amb_button.GetComponent<Button>();
        amb_btn.onClick.AddListener(call_toggle);

        //cpr button
        Button cpr_btn = cpr_button.GetComponent<Button>();
        cpr_btn.onClick.AddListener(cpr_toggle);

        //first aid button
        Button fia_btn = fia_button.GetComponent<Button>();
        fia_btn.onClick.AddListener(firstaid_toggle);

        //setting button, pause all, all transition of screen, no save
        Button set_btn = set_button.GetComponent<Button>();
        set_btn.onClick.AddListener(setting_toggle);

        //setting button, pause all, all transition of screen, no save
        //public Button instruction_button;
        Button res_btn = resume_buton.GetComponent<Button>();
        res_btn.onClick.AddListener(resume_toggle); //resume_toggle

        Button forf_btn = forfeit_button.GetComponent<Button>();
        forf_btn.onClick.AddListener(forfeit_toggle); //forfeit_toggle

        Button ins_btn = instruction_button.GetComponent<Button>();
        ins_btn.onClick.AddListener(instruction_toggle); //instruction_toggle

        Button ins_back_btn = instruction_back_button.GetComponent<Button>();
        ins_back_btn.onClick.AddListener(instruction_back_toggle); //instruction_toggle

        Button youwin_bth_btn = youWin_backToHome_button.GetComponent<Button>();
        youwin_bth_btn.onClick.AddListener(backtoHome_toggle); //backtoHome_toggle

        Button gameover_bth_btn = gameOver_backToHome_button.GetComponent<Button>();
        gameover_bth_btn.onClick.AddListener(backtoHome_toggle); //backtoHome_toggle

        upsr = new UpdatePlayerStatisticsRequest();
        su_list = new List<StatisticUpdate>();
        su = new StatisticUpdate();
    }

    private void OnUpdateStatisticSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("User statistics updated");
    }

    private void OnUpdateStatisticFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    // Update is called once per frame
    void Update()
    {
        track_spawned = imageTarget.GetComponent<trackable>().get_spawned();
        track_spawned2 = imageTarget2.GetComponent<trackable>().get_spawned();
        clock.text = clock_time();
        score_text.text = "Score: " + score.ToString();
        debug_scoretext.text = "Score: " + score.ToString();
        //if body appear else don't drop the vital or show
        if ((track_spawned || track_spawned2) && !pause)
        {
            if (track_spawned)
            {
                if (body_rand == 1 || body_rand == 4 || body_rand == 7 || body_rand == 10)
                {
                    //spawn macolm
                    body1.SetActive(true);
                }
                else if (body_rand == 2 || body_rand == 5 || body_rand == 8 || body_rand == 11)
                {
                    //spawn ant
                    //body2.SetActive(true);
                    body3.SetActive(true);
                }
                else
                {
                    //spawn body4
                    //body4.SetActive(true);
                    //for now body3 is max
                    body2.SetActive(true);
                }
            }
            else if (track_spawned2) // if got 2 markers
            {
                if (body_rand == 1)
                {
                    //spawn macolm
                    bodyaesg_1.SetActive(true);
                }
                else if (body_rand == 2)
                {
                    //spawn ant
                    bodyaesg_2.SetActive(true);
                }
                else if (body_rand == 3)
                {
                    //spawn body4
                    //body4.SetActive(true);
                    //for now body3 is max
                    bodyaesg_3.SetActive(true);
                }
            }

            resp_vital.text = victim.ill_type.vital_respi.ToString(); //breathing
            puls_vital.text = victim.ill_type.vital_pulse.ToString(); //heart
            temp_vital.text = Math.Round(victim.ill_type.vital_tempa, 2).ToString() + " °C"; //temperature

            vital_ticks += Time.deltaTime;
            if (vital_ticks > 1) // every 1 ticks/ 1 seconds
            {
                victim.drop_vital();
                vital_ticks = 0; //reset vital ticks
            }
        }
        else
        {
            offscreen_ticks += Time.deltaTime;
            //An internal counter starts if the target is not found.Counter set at 1min. Or if the user win the thing.He or she can will have a new victim after X minutes...hard to implement spawning rate
            //Reset victim = new victim();
            if (offscreen_ticks > 60)//reset after 60 seconds
            {
                victim = new Victim();
            }
            //masking the vitals to prevent users from "cheating"
            resp_vital.text = "0"; //breathing
            puls_vital.text = "0"; //heart
            temp_vital.text = "0 °C"; //temperature
        }
        if (call_switch && !pause) // if the button is pressed
        {
            calltime -= Time.deltaTime;
            amb_timer.text = "Timer: " + Math.Round(calltime).ToString();
            if (calltime < 0)
            {
                //amb_timer.text = "amb arrived"; // or the text can be you win!
                //pop up a modal and collate the scores for the session and store into the respective user database
                //score_text.text = score.ToString();
                su.StatisticName = "firstaid_score";
                su.Value = score;
                su_list.Add(su);
                upsr.Statistics = su_list;
                PlayFabClientAPI.UpdatePlayerStatistics(upsr, OnUpdateStatisticSuccess, OnUpdateStatisticFailure);

                youWinModal.SetActive(true);
                pause = true;
            }
        }

        if (victim.get_status())
        {
            death_ticks += Time.deltaTime;
            deathtimer.text = "Death in " + (deathtime - Math.Floor(death_ticks));
            Debug.Log("deathtimer: " + death_ticks);
            if (death_ticks > deathtime)
            {
                gameOverModal.SetActive(true);
                pause = true;

                //update the scores based on the session etc.
                PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
                {
                    // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
                    Statistics = new List<StatisticUpdate> {
                        new StatisticUpdate { StatisticName = "firstaid_score", Value = 0 },
                    }
                },
                result => { Debug.Log("User statistics updated"); },
                error => { Debug.LogError(error.GenerateErrorReport()); });
            }
            else
            {
                //reset death ticks
                //death_ticks = 0;
            }
        }

        double rand_add = UnityEngine.Random.Range((0.0f), (2.00f));

        if (Input.GetButtonDown("Fire1"))
        {
            //Shoot();
            if (aed_switch)
            {
                score += victim.add_vital(3, rand_add);
                Shoot(); // apply torque
                //applyTorque();
            }
            else if (cpr_switch)
            {
                score += victim.add_vital(2, rand_add);
                Shoot(); // apply torque
                //applyTorque();
            }
            else if (firstaid_switch)
            {
                score += victim.add_vital(1, rand_add);
            }
            else
            {
                int rand_plusminus = Convert.ToInt32(UnityEngine.Random.Range((0.0f), (1.00f)));
                bool boolValue = rand_plusminus != 0;
                /*if (boolValue)
                {
                    score += 1;
                }
                else
                {
                    score -= 1;
                }*/
            }

            //for debugging purposes
            if (score < 0)
            {
                score = 0;
            }
            else if (score > 100)
            {
                score = 100;
            }
        }
    }

    public string clock_time()
    {
        var str = "";

        DateTime time = DateTime.Now;
        String hour = time.Hour.ToString().PadLeft(2, '0');
        String minute = time.Minute.ToString().PadLeft(2, '0');
        String second = time.Second.ToString().PadLeft(2, '0');

        str = hour + ":" + minute + ":" + second;

        return str;
    }

    public void applyTorque()
    {
        RaycastHit hit;

        //float turn = Input.GetAxis("Horizontal");
        //.AddTorque(transform.up * torque * turn);

        //range of raycast
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            //if object have rigibody, push
            if (hit.rigidbody != null)
            {
                //hit.rigidbody.AddTorque(transform.up * torque * turn);

                hit.rigidbody.AddForce((-hit.normal), ForceMode.Impulse);//* impact
            }
        }
    }

    void Shoot()
    {
        RaycastHit hit;

        //flipping function, range of raycast
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(force, ForceMode.Impulse);
                //hit.rigidbody.AddTorque(new Vector3(10000.0f, 10000.0f, 10000.0f), ForceMode.Impulse);
            }

        }
    }

    public void backtoHome_toggle()
    {
        SceneManager.LoadScene("MainMenu_screen");
    }

    public void cpr_toggle()
    {
        aed_switch = false;
        cpr_switch = !cpr_switch;
        firstaid_switch = false;
        action_count += 1;
        //when cpr button is pressed, any action 
        // increase more respiration
        // increase some pulse
        //tap as apply first aid
    }

    public void aed_toggle()
    {
        aed_switch = !aed_switch;

        if (aed_switch)
        {
            if (track_spawned) //when test aed marker is seen
            {
                if (body_rand == 1)
                {
                    //set a patch of body spawned botx patch
                    patchSpawn1.SetActive(true);
                }
                else if (body_rand == 2)
                {
                    //set a patch of body spawned andromeda patch
                    patchSpawn2.SetActive(true);
                }
                else
                {
                    //set a patch of body spawned malcolm patch
                    patchSpawn3.SetActive(true);
                }
            }

            if (track_spawned2) //when test aed marker is seen
            {
                if (body_rand == 1)
                {
                    //set a patch of body spawned botx patch
                    aed_patch1.SetActive(true);
                }
                else if (body_rand == 2)
                {
                    //set a patch of body spawned andromeda patch
                    aed_patch2.SetActive(true);
                }
                else
                {
                    //set a patch of body spawned malcolm patch
                    aed_patch3.SetActive(true);
                }
            }

            
        }
        else
        {
            patchSpawn1.SetActive(false);
            patchSpawn2.SetActive(false);
            patchSpawn3.SetActive(false);
            aed_patch1.SetActive(false);
            aed_patch2.SetActive(false);
            aed_patch3.SetActive(false);
        }
        action_count += 1;
        cpr_switch = false;
        firstaid_switch = false;
        //probably same as ToggleOnClick
        // increase more pulse
        // increase some temperature
        //tap as apply aed shock
    }

    public void firstaid_toggle()
    {
        aed_switch = false;
        cpr_switch = false;
        firstaid_switch = !firstaid_switch;
        action_count += 1;
        //when first aid applied, add 
        // increase more temperature
        // increase some pulse
        //tap as apply first aid
    }

    public void call_toggle()
    {
        //when pressed, the timer for game end will start
        aed_switch = false;
        cpr_switch = false;
        firstaid_switch = false;
        call_switch = true;

        if (action_count == 0)
        {
            this.score += 10;
        }
        else
        {
            this.score = 10 - action_count;
            if (action_count == 10)
            {
                this.score = 1;
            }
        }
        action_count += 1;
    }

    public void setting_toggle()
    {
        //pause();
        pause = true;
        setting_modal.SetActive(true);
    }

    public void resume_toggle()
    {
        //pause();
        pause = false;
        setting_modal.SetActive(false);
    }

    public void forfeit_toggle()
    {
        //pause();
        //setting_modal.SetActive(false);
        //TODO: do something like an extra warning before going back to main menu
        SceneManager.LoadScene("MainMenu_Screen");
    }

    public void instruction_toggle()
    {
        //pause();
        //SceneManager.LoadScene("InstructionMenu");
        instruction_modal.SetActive(true);
    }

    public void instruction_back_toggle()
    {
        //pause();
        //SceneManager.LoadScene("InstructionMenu");
        instruction_modal.SetActive(false);
    }
}

public class Illness
{
    public int type; //give a range illness based on real life 
    public string desc;
    public int vital_respi;
    public int vital_pulse;
    public double vital_tempa;

    public Illness()
    {
        type = UnityEngine.Random.Range(0, 1);

        //normal adults, in general
        vital_respi = UnityEngine.Random.Range(12, 18);
        vital_pulse = UnityEngine.Random.Range(60, 100);
        vital_tempa = UnityEngine.Random.Range(35.7f, 37.4f);

        if (type == 1) // no pulse or breathing
        {
            desc = "no pulse or breathing";
            vital_respi = UnityEngine.Random.Range(0, 4);
            vital_pulse = UnityEngine.Random.Range(0, 50);
            vital_tempa = UnityEngine.Random.Range(35.7f, 37.4f); // temp same
        }
        else // fever
        {
            desc = "fever";
            vital_respi = UnityEngine.Random.Range(18, 28); // breathing harder and more times
            vital_pulse = UnityEngine.Random.Range(80, 140); // 
            vital_tempa = UnityEngine.Random.Range(37.5f, 39.8f); // temp high
        }
    }
}

//Victim class for accessing and changing value of vitals
public class Victim
{
    /*
     Can further be evaluated using min-max normalisation for each vitals to make it more smooth and realistic in value, changing all the value from 100-120 to 0-1
     After normalisation, the value can be used to involve in age variables, illness type variables and many other possible variable such as weather/current temperature e.g.
    */
    public string[] v_name_arr;// = new string[] { "Ali", "Bob", "Cally", "Edd", "Francis", "Robert" };
    public string illness; //numeric value of type
    public string v_name;
    public int age;
    public Illness ill_type;
    public bool dead;
    static double ROOM_TEMP = 32.00; //change accordingly for setting
    static double HIGH_TEMP = 43.00;
    //string date_created = t_date_created.ToString();
    //random age, illness type, vital respi, 
    public Victim()
    {

        this.v_name_arr = new string[] { "Ali", "Bob", "Cally", "Edd", "Francis", "Robert" };

        //random illness
        this.ill_type = new Illness();
        this.illness = ill_type.desc;
        dead = false;

        //random name
        int rand_name_i = UnityEngine.Random.Range(0, 5);
        this.v_name = v_name_arr[rand_name_i];

        //random age
        this.age = UnityEngine.Random.Range(20, 50);
    }

    //overload constructor, set own name, age and illness
    public Victim(string name, int age)
    {
        this.v_name = name;
        this.age = age;
    }

    private int add_tempa(double value_tempa)
    {
        int score = 0;
        //vital_tempa = UnityEngine.Random.Range(35.7f, 37.4f);
        if (this.ill_type.vital_tempa < 36) // if tempature below 36
        {
            this.ill_type.vital_tempa += value_tempa;
            score += 1;
            //add score
        }
        else if (this.ill_type.vital_tempa > 37.4)
        {
            this.ill_type.vital_tempa -= value_tempa;
            score += 1;
            //add score
        }
        else
        {
            int rand_plusminus = Convert.ToInt32(UnityEngine.Random.Range((0.0f), (1.00f)));
            bool boolValue = rand_plusminus != 0;
            if (boolValue)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }
            //minus score if they spam with 50% chance
        }
        return score;
    }

    public bool get_status() //get whether dead or not
    {
        if (this.ill_type.vital_pulse <= 0 || this.ill_type.vital_respi <= 0 || this.ill_type.vital_tempa >= HIGH_TEMP || this.ill_type.vital_tempa <= ROOM_TEMP)
        {
            return true;
        }

        return false;
    }

    private int add_pulse(int value_pulse)
    {
        int score = 0;
        //vital_pulse = UnityEngine.Random.Range(60, 100);
        if (this.ill_type.vital_pulse < 60) // if pulse below 60
        {
            this.ill_type.vital_pulse += value_pulse;
            score += 1;
            //add score
        }
        else if (this.ill_type.vital_pulse > 100) // if pulse above 100
        {
            this.ill_type.vital_pulse -= value_pulse;
            score += 1;
            //add score
        }
        else
        {
            int rand_plusminus = Convert.ToInt32(UnityEngine.Random.Range((0.0f), (1.00f)));
            bool boolValue = rand_plusminus != 0;
            if (boolValue)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }
            //minus score if they spam with 50% chance
        }
        return score;
    }

    private int add_breath(int value_breath)
    {
        int score = 0;
        //vital_respi = UnityEngine.Random.Range(12, 18);
        if (this.ill_type.vital_respi < 12) // if breath rate below 12
        {
            this.ill_type.vital_respi += value_breath;
            score += 1;
            //add score
        }
        else if (this.ill_type.vital_respi > 18) // if breath rate above 18
        {
            this.ill_type.vital_respi -= value_breath;
            score += 1;
            //add score
        }
        else
        {
            int rand_plusminus = Convert.ToInt32(UnityEngine.Random.Range((0.0f), (1.00f)));
            bool boolValue = rand_plusminus != 0;
            if (boolValue)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }
            //minus score if they spam with 50% chance
        }
        return score;
    }

    public int add_vital(int type, double value)
    {
        int score = 0;
        int value_breath = Convert.ToInt32(Math.Round(value));
        //add_breath(value_breath);
        int value_pulse = Convert.ToInt32(Math.Round(value));
        //add_pulse(value_pulse);
        double value_tempa = Math.Round(value, 2);
        //add_tempa(value_tempa);

        switch (type)
        {
            case 1: // first aid, add_vital(1, 0);
                Console.WriteLine("first aid applied");
                //Add tempa;
                score += add_tempa(value_tempa);
                //Add pulse;
                score += add_pulse(value_pulse);
                //first aid applied
                break;
            case 2: //cpr , add_vital(2, 0);
                Console.WriteLine("CPR applied");
                //Add respirtory;
                score += add_breath(value_breath);
                //Add pulse;
                score += add_pulse(value_pulse);
                //cpr_applied
                break;
            case 3: // AED, add_vital(3, 0);
                Console.WriteLine("AED applied");
                //Add pulse;
                score += add_pulse(value_pulse);
                //Add temperature;
                score += add_tempa(value_tempa);
                //aed_applied
                break;
            default:
                Console.WriteLine("null");
                break;
        }
        return score;
    }

    public void drop_vital() //every 3 seconds, call
    {
        //This is the area where you set the dropping variable to make it more realistic
        float rand_t = UnityEngine.Random.Range(0.1f, 0.4f); // drop slower, temperature not easy to drop in real life
        int rand_r = UnityEngine.Random.Range(0, 2);
        int rand_p = UnityEngine.Random.Range(0, 2);

        if (this.ill_type.vital_tempa > ROOM_TEMP) //room temperature, state of death
        {
            this.ill_type.vital_tempa = this.ill_type.vital_tempa - rand_t;
        }
        else
        {
            this.ill_type.vital_tempa = 0;
        }

        if (this.ill_type.vital_respi > 0)
        {
            this.ill_type.vital_respi = this.ill_type.vital_respi - rand_r;
        }
        else
        {
            this.ill_type.vital_respi = 0;
        }

        if (this.ill_type.vital_pulse > 0)
        {
            this.ill_type.vital_pulse = this.ill_type.vital_pulse - rand_p;
        }
        else
        {
            this.ill_type.vital_pulse = 0;
        }
    }
}