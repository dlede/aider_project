using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject instruction_modal, ldrboard_modal;
    public Button logout_btn, play_btn, ldr_btn, ldr_b_btn, instr_btn, instr_b_btn;
    public persistent_obj re_persistent_obj_script;
    public Text highscore, yourrank, rankscore;
    private List<PlayerLeaderboardEntry> ple_list;
    private List<PlayerStatisticVersion> statistic_version;
    private int highestscore, rank;
    private uint temp_max;

    // Use this for initialization
    void Start () {
        var req_login = new LoginWithPlayFabRequest();
        var req_upstats = new UpdatePlayerStatisticsRequest();
        ple_list = new List<PlayerLeaderboardEntry>();
        statistic_version = new List<PlayerStatisticVersion>();

        temp_max = 999;
        highestscore = -1;
        rank = -1; 

        instruction_modal.SetActive(false);
        ldrboard_modal.SetActive(false);

        logout_btn.onClick.AddListener(() => logout_button());
        play_btn.onClick.AddListener(() => play_button());

        ldr_btn.onClick.AddListener(() => ldr_button());
        ldr_b_btn.onClick.AddListener(() => ldr_back_button());

        instr_btn.onClick.AddListener(() => instr_button());
        instr_b_btn.onClick.AddListener(() => instr_back_button());

        var re_persistent_obj = GameObject.Find("PersistenObj");
        re_persistent_obj_script = re_persistent_obj.GetComponent<persistent_obj>();

        //Debug.Log("playfab_id: " + re_persistent_obj_script.get_pfi());
        GameObject.DontDestroyOnLoad(GameObject.Find("PersistenObj"));
    }

    private void OnUpdateUsernameSuccess(AddUsernamePasswordResult result)
    {
        Debug.Log("update successful");
    }

    private void OnUpdateUsernameFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnGetStatisticVersionSuccess(GetPlayerStatisticVersionsResult result)
    {
        statistic_version = result.StatisticVersions;

        foreach (PlayerStatisticVersion i in statistic_version)
        {
            //for the first time, take in temp max as a value regardless
            if (temp_max == 999)
            {
                temp_max = i.Version;
            }
            
            //temp max lesser thn version, hence temp max become max
            if (temp_max < i.Version)
            {
                temp_max = i.Version;
            }
        }
    }

    private void OnGetStatisticVersionFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        ple_list = result.Leaderboard;

        if (rankscore.text.Length > 1) // if there is more than 1 character in the text ui, clear
        {
            rankscore.text = "";
        }
       
        Debug.Log("START Full Leaderboard Iteration");
        foreach (PlayerLeaderboardEntry i in ple_list)
        {
            if (ple_list.Count < 0) // if the list in the score board is empty
            {
                rankscore.text = "Noone have yet to have a high score!";
                break;
            }

            rankscore.text = rankscore.text + (i.Position+1).ToString() + "\t\t\t\t"+ i.DisplayName +"\t\t\t\t\t\t\t" + i.StatValue.ToString() + "\n";
        }
        Debug.Log("END Full Leaderboard Iteration");

        Debug.Log("Leaderboard gotten successful");
    }

    private void OnGetLeaderboardFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnGetLAPSuccess(GetLeaderboardAroundPlayerResult result)
    {
        ple_list = result.Leaderboard;

        Debug.Log("START Partial Leaderboard Iteration, Specifically User only");
        foreach (PlayerLeaderboardEntry i in ple_list)
        {
            //insert highest score and rank of player
            highscore.text = "Your High Score: " + i.StatValue.ToString();
            yourrank.text = "Your Rank: " + (i.Position + 1).ToString();

        }
        Debug.Log("END Partial Leaderboard Iteration, Specifically User only");

        Debug.Log("Get User Leaerboard Position successful");
    }

    private void OnGetLAPFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnUpdateStatisticSuccess(UpdatePlayerStatisticsResult result)
    {
        /*
        Debug.Log("update palyer statistic result1: " + result.ToString()); // returning result string as statement, acceptable

        if (result.CustomData != null)
        {
            Debug.Log("update palyer statistic result2: " + result.CustomData.ToString()); //returning null, acceptable
        }        
        Debug.Log("update palyer statistic result3: " + result.Request.ToString());
        */
        Debug.Log("User statistics updated");
    }

    private void OnUpdateStatisticFailure(PlayFabError error)
    {
        Debug.Log("Error on updating player statistic");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnAccountInfoSuccess(GetAccountInfoResult result)
    {
        Debug.Log("Retrieval Complete ");
    }

    private void OnAccountInfoFailure(PlayFabError error)
    {
        Debug.Log("Retrieval Incomplete ");
        Debug.Log(error.GenerateErrorReport());
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Complete ");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    void logout_button()
    {
        SceneManager.LoadScene("Login_Screen");
    }
    void play_button()
    {
        SceneManager.LoadScene("MarMdeicalAR");
    }
    void ldr_button()
    {
        highscore.text = "Your High Score: Retrieving...";
        yourrank.text = "Your Ranking: Retrieving...";
        rankscore.text = "Retrieving Information...";
        ldrboard_modal.SetActive(true);

        //getting user's high score and rank
        GetLeaderboardAroundPlayerRequest glapr = new GetLeaderboardAroundPlayerRequest();
        glapr.PlayFabId = re_persistent_obj_script.get_pfi();
        glapr.MaxResultsCount = 1;
        glapr.StatisticName = "score";
        PlayFabClientAPI.GetLeaderboardAroundPlayer(glapr, OnGetLAPSuccess, OnGetLAPFailure);

        //get full leaderboard scores and rank
        GetLeaderboardRequest glr = new GetLeaderboardRequest();
        glr.StartPosition = 0;
        glr.MaxResultsCount = 10;
        glr.StatisticName = "score";
        PlayFabClientAPI.GetLeaderboard(glr, OnGetLeaderboardSuccess, OnGetLeaderboardFailure);
    }
    void ldr_back_button()
    {
        ldrboard_modal.SetActive(false);
        //rankscore.text = "Retrieving Information";
    }
    void instr_button()
    {
        instruction_modal.SetActive(true);
    }
    void instr_back_button()
    {
        instruction_modal.SetActive(false);
    }
}
