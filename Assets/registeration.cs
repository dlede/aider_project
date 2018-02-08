using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class registeration : MonoBehaviour {

    public InputField Email, Password, cfmPass, Username;//, FirstName, LastName;
    public Text console_status_text;
    public Button reg_button, reged_btn;


    // Use this for initialization
    void Start () {
        //for register button case
        Button reg_btn = reg_button.GetComponent<Button>();
        reg_btn.onClick.AddListener(Register);

        Button reg_ed_btn = reged_btn.GetComponent<Button>();
        reg_ed_btn.onClick.AddListener(backToHome);
    }
     
    public void backToHome()
    {
        SceneManager.LoadScene("Login_Screen");
    }

    public void Register()
    {
        var req_register = new RegisterPlayFabUserRequest();
        //slightly different due to naming convention - username is username and displayname is their displayname, but since we use displayname as username, this should work
        req_register.DisplayName = Username.text; 
        req_register.Email = Email.text;

        if(checkPasswordandCfmPass())
        {
            req_register.Password = Password.text;
        }
        else
        {
            DebugLog("The passwords does not match, Please enter the correct password ");
        }

        req_register.Password = Password.text;
        req_register.TitleId = "6449";
        //req_register.DisplayName = FirstName + " " + LastName;
        req_register.RequireBothUsernameAndEmail = false;

        if (checkInputField()) // if successfully mock register
        {
            PlayFabClientAPI.RegisterPlayFabUser(req_register, OnRegisterSuccess, OnRegisterFailure);

            DebugLog("You have registered via " + Email.text + " with password " + Password.text);
        }
        else
        {
            DebugLog("All field is compulsory, please fill in all the fields");
        }
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        DebugLog("Registeration Complete ");
        SceneManager.LoadScene("Login_Screen");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        DebugLog("Sorry there is an issue creating the user,");
        DebugLog(error.GenerateErrorReport());
    }

    private bool checkPasswordandCfmPass()
    {
        //if cfmPass != Password, reject
        if (Password.text == cfmPass.text)
        {
            return true;
        }
        return false;
    }

    private bool checkInputField()
    {
        if (string.IsNullOrEmpty(Email.text) || string.IsNullOrEmpty(Password.text))
        {
            //Error handling
            DebugLog("Empty!");
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

            if (numLines + 1 > 6)
            {
                console_status_text.text = "";
            }
        }
    }
}
