using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

public class Menu_test : MonoBehaviour
{
    //Dictionary<string, string> post_dict;// = new Dictionary<int, string>();
    private bool registeredPressed;

    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;
    protected string displayName = "";

    //asets, variables
    public Button login_button, register_button, linstr_b_btn;
    public Text console_status_text;
    public GameObject linstructionModal;
    public persistent_obj re_persistent_obj_script;
    private System.Action onComplete;

    private double update_ticks;
    private int rounded_ticks;

    public InputField Email, Password;

    void Start()
    {
        PlayFabSettings.TitleId = "144"; // Please change this value to your own titleId from PlayFab Game Manager
        update_ticks = 0.0;
        update_ticks = 0;
        //TODO: check this part cause without this, the flow works on phone
        //linstructionModal.SetActive(true);
        Text console_text = console_status_text.GetComponent<Text>();

        //for login button case
        Button login_btn = login_button.GetComponent<Button>();
        login_btn.onClick.AddListener(Login);

        //for register button case
        Button reg_btn = register_button.GetComponent<Button>();
        reg_btn.onClick.AddListener(Register);

        linstr_b_btn.onClick.AddListener(() => linstr_back_button());
        var re_persistent_obj = GameObject.Find("PersistenObj");
        re_persistent_obj_script = re_persistent_obj.GetComponent<persistent_obj>();
        //persistentScript = re_persistent_obj.GetComponent();

        GameObject.DontDestroyOnLoad(GameObject.Find("PersistenObj"));
    }

    private void Update()
    {
        update_ticks += Time.deltaTime;
        rounded_ticks = Convert.ToInt32(Math.Round(update_ticks, MidpointRounding.AwayFromZero));
        if (rounded_ticks >= 5) // 5 seconds per update
        {
            DebugLog("latitude:" + re_persistent_obj_script.get_lat() + " & longitude: " + re_persistent_obj_script.get_long());
            update_ticks = 0;
            rounded_ticks = 0;
        }
    }

    //*******************************
    // start of playerFab API
    //*******************************

    private void OnLoginSuccess(LoginResult result)
    {
        re_persistent_obj_script.set_session(result.SessionTicket);
        re_persistent_obj_script.set_llr(result.LastLoginTime.ToString());
        re_persistent_obj_script.set_pfi(result.PlayFabId);

        DebugLog("Logining...");
        SceneManager.LoadScene("MainMenu_Screen");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        DebugLog(error.GenerateErrorReport());
        SceneManager.LoadScene("Login_Screen");
    }

    //*******************************
    // end of playerFab API
    //*******************************


    void linstr_back_button()
    {
        linstructionModal.SetActive(false);
        //SceneManager.LoadScene("InstructionMenu");
    }

    public void Login()
    {
        var req_login = new LoginWithEmailAddressRequest();

        //TODO: make a facebook login
        if (checkInputField()) // if successfully mock login
        {
            //TODO: check the email and pass from somewhere
            req_login.Email = Email.text;
            req_login.Password = Password.text;
            req_login.TitleId = "6449";
            //PlayFabClientAPI.acc
            PlayFabClientAPI.LoginWithEmailAddress(req_login, OnLoginSuccess, OnLoginFailure);

            re_persistent_obj_script.set_email(Email.text);
            re_persistent_obj_script.set_password(Password.text);

            Debug.Log("You have logined via " + Email.text + " with password " + Password.text);
        }
    }

    public void Register()
    {
        SceneManager.LoadScene("Register_Screen");
    }

    public void DebugLog(string log)
    {
        if (log.Contains("Empty"))
        {
            Debug.LogError(log);
        }
        else
        {
            Debug.Log(log);
        }

        if (console_status_text)
        {
            string temp = console_status_text.text;
            int numLines = temp.Split('\n').Length;
            console_status_text.text = console_status_text.text + "\n" + log + " ";// + numLines.ToString();

            if(numLines+1 > 6)
            {
                console_status_text.text = "";
            }
        }
    }

    private bool checkInputField()
    {
        //TODO: create a alert modal for errors: errorModal.SetActive(true); etc.

        if (string.IsNullOrEmpty(Email.text))
        {
            //Error handling
            DebugLog("Email Field is Empty!");
            return false;
        }

        if (string.IsNullOrEmpty(Password.text))
        { 
            //Error handling
            DebugLog("Password Field is Empty!");
            return false;
        }

        if (!(Email.text.Contains("@")) || !(Email.text.Contains(".")))
        {
            DebugLog("Email not correct!");
            Password.text = ""; //reset password field
            return false;
        }

        return true;
    }
}
