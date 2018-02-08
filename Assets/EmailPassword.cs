using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

public class EmailPassword : MonoBehaviour
{
    protected Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseAuth otherAuth;

    //private Firebase.Auth.FirebaseAuth auth;
    private DatabaseReference reference;
    public GameObject linstructionModal;
    public Button linstr_b_btn;
    //Firebase user 
    private Firebase.Auth.FirebaseUser user;
    private string displayName;
    public string emailAddress;
    private Uri photoUrl;

    public InputField Email, Password;
    private static string p_email, p_password; //static passing variable of users
    private static Boolean logined;
    public string passwordToEdit = ""; //TODO: change to private once beta
    public string userToEdit = ""; //TODO: change to private once beta
    public Button SignUpButton, LoginButton;//LogoutButton
    public Text ErrorText;
    public Text Logined_Email;
    private Boolean email_valid;
    private string realpass;
    private StringBuilder realpass_b;
    public int max_int_id;
    public Boolean pressed;
    public int prev_length;

    protected string email = "";
    protected string password = "";

    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    // Options used to setup secondary authentication object.
    private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions
    {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        Button login_btn = LoginButton.GetComponent<Button>();
        //Thread.Sleep(2000); // pause 2 secs
        linstructionModal.SetActive(true);
        CultureInfo culture = new CultureInfo("en-US"); // Singapore
        Thread.CurrentThread.CurrentCulture = culture;
        Screen.autorotateToPortrait = false;

        //button used in interaction
        //declare aed_button on top first
        //Button btn = aed_button.GetComponent<Button>();
        //btn.onClick.AddListener(ToggleOnClick);

        //UpdateErrorMessage("Before Init Firebase");
        //Firebase Initialization for Editor
        //InitializeFirebase();

        // Set up the Editor before calling into the realtime database.
        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://marfirstaid.firebaseio.com/");

        //UpdateErrorMessage("EDITOR default OK, commencing, reference");
        // Get the root reference location of the database.
        //DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        realpass_b = new StringBuilder(50);

        UpdateErrorMessage("button ok");
        //SignUpButton.onClick.AddListener(() => createAccount(Email.text, realpass_b.ToString()));
        //login_btn.onClick.AddListener(() => Login(Email.text, realpass_b.ToString()));

        //SignUpButton.onClick.AddListener(() => Signup_auth(Email.text, realpass_b.ToString()));
        //login_btn.onClick.AddListener(() => Login_auth(Email.text, realpass_b.ToString()));

        //CreateUserAsync
        SignUpButton.onClick.AddListener(() => CreateUserAsync());
        login_btn.onClick.AddListener(() => SigninAsync());

        linstr_b_btn.onClick.AddListener(() => linstr_back_button());
        //login_btn.onClick.AddListener(() => login_easy());
    }

    void login_easy()
    {
        SceneManager.LoadScene("MainMenu_Screen");
    }

    void Update()
    {
        email = Email.text;
        password = realpass_b.ToString();
        if (Input.anyKey && pressed==false)
        {
            if (!(string.IsNullOrEmpty(Password.text)))
            {
                passwordMask();
                pressed = true;
            }

        }
        else
        {
            Thread.Sleep(100); //pause the update to prevent many append
            pressed = false;
        }
    }

    void linstr_back_button()
    {
        linstructionModal.SetActive(false);
        //SceneManager.LoadScene("InstructionMenu");
    }

    public string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        md5.GetHashCode();

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    void passwordMask()
    {
        Debug.Log("start_realpass: " + realpass);
        Debug.Log("start_realpass_b: " + realpass_b);
        //if backspace all, clear stringbuilder
        if (Password.text.Length == 0)
        {
            realpass_b = new StringBuilder(50);
        }
        else if (Password.text.Length < prev_length)//if backspace reduce 1 letter
        {
            realpass_b.Length -= (prev_length - Password.text.Length);
            //TODO: another scenario, if the user backspace in different place?
        }
        realpass_b.Append(Password.text.ToCharArray());
        realpass_b.Replace("*", "");
        Debug.Log("realpass_b " + realpass_b);

        string mask_pass = "";
        
        for (int i = 0; i < Password.text.Length; i++)
        {
            mask_pass = mask_pass+"*";
        }
        realpass = realpass_b.ToString(); //string from previous
        Password.text = mask_pass;
        prev_length = Password.text.Length;
        //Debug.Log("passwordMask end");
    }

    public Task CreateUserAsync()
    {
        Debug.Log(String.Format("Attempting to create user {0}...", email));
        //DisableUI();

        // This passes the current displayName through to HandleCreateUserAsync
        // so that it can be passed to UpdateUserProfile().  displayName will be
        // reset by AuthStateChanged() when the new user is created and signed in.
        string newDisplayName = displayName;
        return auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWith((task) => {
              return HandleCreateUserAsync(task, newDisplayName: newDisplayName);
          }).Unwrap();
    }

    Task HandleCreateUserAsync(Task<Firebase.Auth.FirebaseUser> authTask,
                           string newDisplayName = null)
    {
        //EnableUI();
        if (LogTaskCompletion(authTask, "User Creation"))
        {
            if (auth.CurrentUser != null)
            {
                Debug.Log(String.Format("User Info: {0}  {1}", auth.CurrentUser.Email,
                                       auth.CurrentUser.ProviderId));
                return UpdateUserProfileAsync(newDisplayName: newDisplayName);
            }
        }
        // Nothing to update, so just return a completed Task.
        return Task.FromResult(0);
    }

    // Update the user's display name with the currently selected display name.
    public Task UpdateUserProfileAsync(string newDisplayName = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.Log("Not signed in, unable to update user profile");
            return Task.FromResult(0);
        }
        displayName = newDisplayName ?? displayName;
        Debug.Log("Updating user profile");
        //DisableUI();
        return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
        {
            DisplayName = displayName,
            PhotoUrl = auth.CurrentUser.PhotoUrl,
        }).ContinueWith(HandleUpdateUserProfile);
    }

    void HandleUpdateUserProfile(Task authTask)
    {
        //EnableUI();
        if (LogTaskCompletion(authTask, "User profile"))
        {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    // Display a more detailed view of a FirebaseUser.
    void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
    {
        DisplayUserInfo(user, indentLevel);
        Debug.Log("  Anonymous: " + user.IsAnonymous);
        Debug.Log("  Email Verified: " + user.IsEmailVerified);
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        if (providerDataList.Count > 0)
        {
            Debug.Log("  Provider Data:");
            foreach (var providerData in user.ProviderData)
            {
                DisplayUserInfo(providerData, indentLevel + 1);
            }
        }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }

    // Display user information.
    void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
        foreach (var property in userProperties)
        {
            if (!String.IsNullOrEmpty(property.Value))
            {
                Debug.Log(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    public Task SigninAsync()
    {
        Debug.Log(String.Format("Attempting to sign in as {0}...", email));
        //DisableUI();
        return auth.SignInWithEmailAndPasswordAsync(email, password)
          .ContinueWith(HandleSigninResult);
    }

    // This is functionally equivalent to the Signin() function.  However, it
    // illustrates the use of Credentials, which can be aquired from many
    // different sources of authentication.
    public Task SigninWithCredentialAsync()
    {
        Debug.Log(String.Format("Attempting to sign in as {0}...", email));
        //DisableUI();
        Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        return auth.SignInWithCredentialAsync(cred).ContinueWith(HandleSigninResult);
    }

    void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask)
    {
        //EnableUI();
        LogTaskCompletion(authTask, "Sign-in");
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            Debug.Log(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            Debug.Log(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                Debug.Log(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            Debug.Log(operation + " completed");
            complete = true;
        }
        return complete;
    }

    public void Login_auth(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync error: " + task.Exception);
                if (task.Exception.InnerExceptions.Count > 0)
                    UpdateErrorMessage(task.Exception.InnerExceptions[0].Message);
                return;
            }

            FirebaseUser user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                user.DisplayName, user.UserId);

            SceneManager.LoadScene("MainMenu_Screen");
        });
    }

    public void Signup_auth(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            //Error handling
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync error: " + task.Exception);
                if (task.Exception.InnerExceptions.Count > 0)
                    UpdateErrorMessage(task.Exception.InnerExceptions[0].Message);
                return;
            }

            FirebaseUser newUser = task.Result; // Firebase user has been created.
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            UpdateErrorMessage("Signup Success");
        });
    }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        // Specify valid options to construct a secondary authentication object.
        if (otherAuthOptions != null &&
            !(String.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
              String.IsNullOrEmpty(otherAuthOptions.AppId) ||
              String.IsNullOrEmpty(otherAuthOptions.ProjectId)))
        {
            try
            {
                otherAuth = Firebase.Auth.FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
                  otherAuthOptions, "Secondary"));
                otherAuth.StateChanged += AuthStateChanged;
                otherAuth.IdTokenChanged += IdTokenChanged;
            }
            catch (Exception)
            {
                Debug.Log("ERROR: Failed to initialize secondary authentication object.");
            }
        }
        AuthStateChanged(this, null);
    }

    //get User Token
    public void GetUserToken()
    {
        if (auth.CurrentUser == null)
        {
            Debug.Log("Not signed in, unable to get token.");
            return;
        }
        Debug.Log("Fetching user token");
        fetchingToken = true;
        auth.CurrentUser.TokenAsync(false).ContinueWith(HandleGetUserToken);
    }

    void HandleGetUserToken(Task<string> authTask)
    {
        fetchingToken = false;
        if (LogTaskCompletion(authTask, "User token fetch"))
        {
            Debug.Log("Token = " + authTask.Result);
        }
    }

    /*
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        //UpdateErrorMessage("default Init Firebase");
        //Thread.Sleep(2000); // pause 2 secs

        auth.StateChanged += AuthStateChanged;

        //UpdateErrorMessage("AuthStateChanged+=");
        //Thread.Sleep(2000); // pause 2 secs

        AuthStateChanged(this, null);
        //UpdateErrorMessage("AuthStateChanged this null");
        //Thread.Sleep(2000); // pause 2 secs

        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://marfirstaid.firebaseio.com/");
        FirebaseApp.DefaultInstance.SetEditorP12FileName("MARFirstAid-3a7df2a2ffb0.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("firebase-adminsdk-f2er0@marfirstaid.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
        

        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    */
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                emailAddress = user.Email ?? "";
                //String temp_uri = (user.PhotoUrl).ToString();
                //photoUrl = (temp_uri) ?? "";
            }
        }
    }

    public void email_valid_set(bool valid)
    {
        email_valid = valid;
    }

    public bool email_valid_get()
    {
        return email_valid;
    }

    public void Logout_pressed()
    {
        //auth.logout
        SceneManager.LoadScene("LoginMenu");
        //reset static email and password
        p_email = "";
        p_password = "";
        logined = false;
    }

    public void createAccount(string email, string password)
    {
        UpdateErrorMessage("in create account after signup pressed");
        //Thread.Sleep(2000); // pause 2 secs

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            //Error handling
            Debug.LogError("Empty!");
            UpdateErrorMessage("Empty!");
            return;
        }

        if (!(email.Contains("@")) || !(email.Contains(".com")))
        {
            //Error handling
            Debug.LogError("Email not correct!");
            UpdateErrorMessage("Email not correct!");
            Password.text = ""; //reset password field
            realpass_b = new StringBuilder(50); //reset stringbuilder too
            return;
        }

        FirebaseDatabase.DefaultInstance
          .GetReference("User")
          .GetValueAsync().ContinueWith(task => {
              if (task.IsFaulted)
              {
                  // Handle the error...
                  Debug.LogError("getSnapshot error: " + task.Exception);
                  if (task.Exception.InnerExceptions.Count > 0)
                      UpdateErrorMessage(task.Exception.InnerExceptions[0].Message);
                  return;
              }
              if (task.IsCompleted)
              {
                  //do something with the snapshot...
                  DataSnapshot snapshot = task.Result;
                  max_int_id = Convert.ToInt32(snapshot.ChildrenCount);

                  //TODO: generate hash function as well a unique ID token? save on both mobile device and the database - or just save a locked txt with his/her id inside
                  if (max_int_id == 0) //if first entry
                  {
                      Debug.Log("first account created");
                      User user = new User(max_int_id, email, Md5Sum(password));
                        string json = JsonUtility.ToJson(user);
                        reference.Child("User").Child((max_int_id).ToString()).SetRawJsonValueAsync(json);
                      Password.text = ""; //reset password field
                      realpass_b = new StringBuilder(50); //reset stringbuilder too
                      return;
                  }
                  else
                  {
                      string email_value;
                      for (int i = 0; i < max_int_id; i++)
                      {
                          email_value = snapshot.Child(i.ToString()).Child("email").ToString().Replace("DataSnapshot { key = email, value = ", "").Replace(" }", "").Trim();

                          if (email == email_value)
                          {
                              Debug.Log("email is being used");
                              Email.text = "";
                              Password.text = ""; //reset password field
                              realpass_b = new StringBuilder(50); //reset stringbuilder too
                              return;
                          }
                      }

                      Debug.Log("account created");

                      User user = new User(max_int_id, email, Md5Sum(password));
                      string json = JsonUtility.ToJson(user);
                      reference.Child("User").Child((max_int_id).ToString()).SetRawJsonValueAsync(json);
                      Password.text = ""; //reset password field
                      realpass_b = new StringBuilder(50); //reset stringbuilder too
                      return;
                  }
              }
          });
    }

    private void UpdateErrorMessage(string message)
    {
        ErrorText.text = message.ToString();
        //Invoke("ClearErrorMessage", 3);
    }

    void ClearErrorMessage()
    {
        ErrorText.text = "";
    }
    public void Login(string email, string password)
    {
        UpdateErrorMessage("trying to login account!");
        //Thread.Sleep(2000); // pause 2 secs

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            //Error handling
            Debug.LogError("Empty!");
            UpdateErrorMessage("Empty!");
            return;
        }

        if (!(email.Contains("@")) || !(email.Contains(".com")))
        {
            //Error handling
            Debug.LogError("Email not correct!");
            UpdateErrorMessage("Email not correct!");
            Password.text = ""; //reset password field
            realpass_b = new StringBuilder(50); //reset stringbuilder too
            return;
        }

        FirebaseDatabase.DefaultInstance
            .GetReference("User")
            .GetValueAsync().ContinueWith(task => {
                  if (task.IsCanceled)
                  {
                      Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                      UpdateErrorMessage("SignInWithEmailAndPasswordAsync was canceled.");
                      return;
                  }
                  if (task.IsFaulted)
                  {
                      Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                      UpdateErrorMessage("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                      return;
                  }
                  if (task.IsCompleted)
                  {
                        //Debug.Log("iscompleted");
                        DataSnapshot snapshot = task.Result;
                        // Do something with snapshot...
                        max_int_id = Convert.ToInt32(snapshot.ChildrenCount);
                        //Debug.Log("max_int_id count: " + (max_int_id).ToString());

                        //TODO: check if the id.txt have same value as the database
                        string email_value;
                        string pass_value;

                        for (int i = 0; i < max_int_id; i++)
                        {
                            email_value = snapshot.Child(i.ToString()).Child("email").ToString().Replace("DataSnapshot { key = email, value = ", "").Replace(" }", "").Trim();
                            pass_value = snapshot.Child(i.ToString()).Child("password").ToString().Replace("DataSnapshot { key = password, value = ", "").Replace(" }", "").Trim();
                            
                            if (string.IsNullOrEmpty(email) || email == email_value)
                                {
                                if(Md5Sum(password) != pass_value)
                                {
                                    Password.text = ""; //reset password field
                                    realpass_b = new StringBuilder(50); //reset stringbuilder too
                                    Debug.Log("invalid password, try again");
                                    return;
                                }
                                else
                                {
                                    //TODO: before clear, pass in variable to next scene
                                    //Logout.user_email;
                                    p_email = Email.text;
                                    p_password = Password.text;
                                    Email.text = "";
                                    Password.text = ""; //reset password field
                                    realpass_b = new StringBuilder(50); //reset stringbuilder too
                                    Debug.Log("logined!");
                                    logined = true;
                                    SceneManager.LoadScene("MainMenu_screen");
                                    return;
                                }
                            }
                        }
                    Password.text = ""; //reset password field
                    realpass_b = new StringBuilder(50); //reset stringbuilder too
                    Debug.Log("account with email " + Email.text + " does not exist");
                    Email.text = "";
                }
            });
    }
}

//User class for registering or login authentication
public class User
{
    public int id;
    public string email;
    public string password;
    public string date_created;
    public string date_updated;
    public string date_last_login;
    //null an array for both leaderboard scores and leaderboard dates. 
    public int[] leaderboard;
    public string[] leaderboard_date;

    //string date_created = t_date_created.ToString();

    public User()
    {

    }

    public User(int id, string email, string password)
    {
        this.id = id;
        this.email = email;
        this.password = password;

        //date creation and date updated as of today
        this.date_created = (DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ss.fff");
        this.date_updated = (DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ss.fff");
        this.date_last_login = (DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ss.fff"); //TODO: for when logout, change timestamp to last logout

        //this.date_created = (DateTime.Today).ToString();
        //this.date_updated = (DateTime.Today).ToString();
        //null an array for both leaderboard scores and leaderboard dates. 
        leaderboard = new int[] { 0 };
        leaderboard_date = new string[] { (DateTime.Today).ToString() };
    }
}