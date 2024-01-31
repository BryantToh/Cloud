using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace FMGames.PlayFab.login {
    public class PFUserMgt : MonoBehaviour
    {
        [SerializeField] TMP_InputField userEmail, userPassword, userName, Displayname;
        [SerializeField] TextMeshProUGUI Msg;
        PlayFabUserMgtTMP UserMGT;
        [SerializeField] TMP_InputField showPassword;

        bool Errors;

        public void ClickRegBtn()
        {
            if (SceneManager.GetActiveScene().name == "Login")
            {
                SceneManager.LoadScene("Register");
            }
            else if (SceneManager.GetActiveScene().name == "Register")
            {

                if (!Errors)
                {
                    SceneManager.LoadScene("Login");
                }
                else
                {
                    return;
                }
            }
        }

        public void OnButtonRegUser()
        {
            var regReq = new RegisterPlayFabUserRequest();
            regReq.Email = userEmail.text;
            regReq.Password = userPassword.text;
            regReq.Username = userName.text;
            regReq.DisplayName = Displayname.text;
            PlayFabClientAPI.RegisterPlayFabUser(regReq, OnRegSucc, OnError);

            Invoke("ClickRegBtn", 1);
        }

        public void ShowPassword()
        {
            if (showPassword.contentType == TMP_InputField.ContentType.Password)
            {
                showPassword.contentType = TMP_InputField.ContentType.Standard;
                showPassword.ForceLabelUpdate();
            }
            else
            {
                showPassword.contentType = TMP_InputField.ContentType.Password;
                showPassword.ForceLabelUpdate();   
            }
        }

        void OnRegSucc(RegisterPlayFabUserResult r)
        {
            Msg.text = "Register success. PlayFabID allocated:" + r.PlayFabId;
            // Reset errors flag on successful registration
            Errors = false;
        }

        void OnError(PlayFabError e)
        {
            // Set errors flag on error
            Errors = true;
        }


        public void OnButtonLogin()
        {
            string input = userEmail.text; // Assuming you use the same InputField for both email and username
            bool isEmail = IsEmail(input);

            if (isEmail)
            {
                var loginReq = new LoginWithEmailAddressRequest
                {
                    Email = input,
                    Password = userPassword.text
                };
                PlayFabClientAPI.LoginWithEmailAddress(loginReq, OnLoginSuccess, OnError);
            }
            else
            {
                var loginReq = new LoginWithPlayFabRequest
                {
                    Username = input,
                    Password = userPassword.text
                };
                PlayFabClientAPI.LoginWithPlayFab(loginReq, OnLoginSuccess, OnError);
            }
        }

        private bool IsEmail(string input)
        {
            // Use a simple check for email format
            return input.Contains("@");
        }

        void OnLoginSuccess(LoginResult r)
        {
            SceneManager.LoadScene("Menu");
        }

        public void DeviceLoginButton()
        {
            var req = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                TitleId = PlayFabSettings.TitleId,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(req, OnLoginSuccess, OnError);
        }

        public void GuestLoginButton()
        {
            var loginRequest = new LoginWithCustomIDRequest
            {
                CustomId = "Guest",
                CreateAccount = true, // This Will Create The Account If It Doesn't Exist
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(loginRequest, OnLoginSuccess, OnError);
        }

        public void PasswordReset()
        {
            SceneManager.LoadScene("FgtPassword");
        }

        public void PasswordResetRequest()
        {
            var passwordReq = new SendAccountRecoveryEmailRequest
            {
                Email = userEmail.text,
                TitleId = PlayFabSettings.TitleId
            };
            PlayFabClientAPI.SendAccountRecoveryEmail(passwordReq, OnPasswordReset, OnError);
        }
        void OnPasswordReset(SendAccountRecoveryEmailResult r)
        {
            Msg.text = "Password reset email sent.";
        }

        public void BackToLogin()
        {
            SceneManager.LoadScene("Login");
        }
    }
}


