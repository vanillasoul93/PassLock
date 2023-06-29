using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace PwManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static User currentUser = new User();
        
        System.Windows.Threading.DispatcherTimer WelcomeMessageTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        System.Windows.Threading.DispatcherTimer GeneralMessageTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        System.Windows.Threading.DispatcherTimer LoginErrorMessageTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        System.Windows.Threading.DispatcherTimer EmptyFieldMessageTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        System.Windows.Threading.DispatcherTimer ClipBoardTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        System.Windows.Threading.DispatcherTimer InactivityTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Render);
        Account newAccount = new Account();
        Account selectedAccount;
        bool updateAccountInProgress = false;

        string updateAccountInitialDisplayName = "";
        string updateAccountInitialUserName = "";
        string updateAccountInitialPassword = "";
        string updateAccountInitialAssociatedEmail = "";
        string updateAccountInitialURL = "";
        


        ObservableCollection<Account> Accounts = new ObservableCollection<Account>();
        ObservableCollection<AccountGroup> AccountGroups = new ObservableCollection<AccountGroup>();


        public MainWindow()

        {
            InitializeComponent();
            ListViewAccounts.ItemsSource = Accounts;
            ComboBoxSelectedTheme.Items.Add("Default");
            ComboBoxSelectedTheme.Items.Add("Light");
            ComboBoxSelectedTheme.Items.Add("Green");
            ComboBoxSelectedTheme.Items.Add("Purple");
            ComboBoxAccountCategory.Items.Add("All");
            ComboBoxAccountCategory.Items.Add("Email");
            ComboBoxAccountCategory.Items.Add("Banking");
            ComboBoxAccountCategory.Items.Add("Network");
            ComboBoxAccountCategory.Items.Add("Social Media");
            ComboBoxAccountCategory.Items.Add("Wifi");
            ComboBoxAccountCategory.SelectedIndex = 0;



        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutUser();
        }

        public void LogoutUser()
        {
            try 
            {
                //Encrypt User Data Before Logging Out
                AesGcmService aesGcm = new AesGcmService(currentUser.UserPassword);
                aesGcm.SerializeAndEncryptJson(currentUser);
            }
            catch(Exception le)
            {
                MessageBox.Show("Error Encrypting User");
            }
            
            
            //Clear the Observable Collection of accounts
            Accounts.Clear();
            //Set The Account List View to the now empty Accounts Observable Collection
            ListViewAccounts.ItemsSource = Accounts;
            //Clear the Observable collection of Account Groups
            AccountGroups.Clear();
            //Set the Account Group List View to the now empty Observable Collection
            ListViewAccountGroups.ItemsSource = AccountGroups;
            // Hide all windows on user logout
            FadeOutAnimation(GridAccountListView);
            FadeOutAnimation(GridDiscardChangesWarning);
            FadeOutAnimation(GridOverwriteUserWarning);
            FadeOutAnimation(GridPasswordSettings);
            FadeOutAnimation(GridRegistrationWarning);
            FadeOutAnimation(GridUserSettings);
            FadeOutAnimation(WelcomeMessage);
            FadeOutAnimation(GeneralMessage);
            FadeOutAnimation(AccountCreator);
            FadeOutAnimation(AccountView);
            //Fade into the Login Screen
            FadeInAnimation(GridLoginView);
            //Set the Login button as the default Button
            ButtonLogin.IsDefault = true;
            //Remove the Open Account Button From Default Button
            ButtonOpenAccount.IsDefault = false;
            //Set the Theme Back to default
            var rd = new ResourceDictionary();
            string pathofskin;
            pathofskin = @"Skins\Default.xaml";
            rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources = rd;
            //Set the current User to null
            //currentUser = new User();
        }

        private void ButtonRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            //Check if there is a valid selected item
            if (ListViewAccounts.SelectedIndex >= 0)
            {
                //
                //
                //
                //
                //
                //Add code for confirm option when user is deleting an account
                //
                //
                //
                //
                //
                //Get the index of the selected account group
                int selectedAccountGroupIndex = ListViewAccountGroups.SelectedIndex;
                
                //Get the index of the selected item
                int selectedAccountIndex = ListViewAccounts.SelectedIndex;
                //Delete the account from the Users accounts
                currentUser.accounts = Accounts;
                //Remove The Selected Item from the Observable Collection
                Accounts.Remove((Account)ListViewAccounts.SelectedItem);
                //Order the Observable Collection alphabetically and set the ListViewItemSource to the new ObservableCollection
                Accounts = new ObservableCollection<Account>(Accounts.OrderBy(a => a.DisplayName).ToList());
                ListViewAccounts.ItemsSource = Accounts;
                //Set the selected index to the previous selected index if it is in bounds
                if (selectedAccountIndex <= Accounts.Count && Accounts.Count > 0)
                {
                    ListViewAccounts.SelectedIndex = selectedAccountIndex - 1;
                }
                if (selectedAccountIndex == 0 && Accounts.Count > 0)
                {
                    ListViewAccounts.SelectedIndex = 0;
                }
                ListViewAccountGroups.SelectedIndex = 0;
                ListViewAccountGroups.SelectedIndex = selectedAccountGroupIndex;
                
                
            }
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            //bool updateAccountInProgress = false;
            //Set the Account creation date
            TextBlockDateCreated.Text = DateTime.Now.ToString();
            //Show Account Creation Form using Fade In Animation
            TextBoxPasswordLength.Text = currentUser.DefaultPasswordLength.ToString();
            newAccount.passwordLength = currentUser.DefaultPasswordLength;
            //MessageBox.Show("users default password length = " + currentUser.DefaultPasswordLength.ToString());
            ListViewAccountGroups.SelectedIndex = 0;
            
            FadeInAnimation(AccountCreator);

        }

        private void ButtonOpenAccount_Click(object sender, RoutedEventArgs e)
        {
            //Check if the selected account is a valid selection
            if (ListViewAccounts.SelectedIndex >= 0)
            {
                //Populate the form with the account information using the selected Account
                LoadAccountViewForm((Account)ListViewAccounts.SelectedItem);

                //Hide the GridAccountListView
                FadeOutAnimation(GridAccountListView);
                //Make the View Account form visible using fade in animation
                FadeInAnimation(AccountView);
            }
        }

        private void LoadAccountViewForm(Account selectedAcc)
        {
            //Get the selected account
            selectedAccount = selectedAcc as Account;
            //Fill the form based on the selected account
            //Make the fields read only until user selects to update them
            TextBoxDisplayNameView.Text = selectedAccount.DisplayName;
            TextBoxDisplayNameView.IsReadOnly = true;
            TextBoxDisplayNameView.Focusable = false;
            TextBoxDisplayNameView.IsHitTestVisible = false;
            TextBoxUserNameView.Text = selectedAccount.UserName;
            TextBoxUserNameView.IsReadOnly = true;
            TextBoxUserNameView.Focusable = false;
            TextBoxUserNameView.IsHitTestVisible = false;
            TextBoxPasswordView.Password = selectedAccount.Password;
            TextBoxPasswordView.Focusable = false;
            TextBoxPasswordView.IsHitTestVisible = false;
            TextBoxAssociatedEmailView.Text = selectedAccount.AssociatedEmail;
            TextBoxAssociatedEmailView.IsReadOnly = true;
            TextBoxAssociatedEmailView.Focusable = false;
            TextBoxAssociatedEmailView.IsHitTestVisible = false;
            TextBoxURLView.Text = selectedAccount.URL;
            TextBoxURLView.IsReadOnly = true;
            TextBoxURLView.Focusable = false;
            TextBoxURLView.IsHitTestVisible = false;
            TextBlockDateCreatedView.Text = selectedAccount.Created.ToString();
            TextBlockDateLastUpdatedView.Text = selectedAccount.LastUpdated.ToString();
            ComboBoxAccountCategory.SelectedItem = selectedAccount.Group;
        }

        private void ButtonGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            if (updateAccountInProgress == true)
            {
                TextBoxPassword.Text = PasswordGenerator.GetRandomAlphanumericString(selectedAccount.passwordLength, selectedAccount.SpecialCharacters);
            }
            else 
            {
                TextBoxPassword.Text = PasswordGenerator.GetRandomAlphanumericString(newAccount.passwordLength, newAccount.SpecialCharacters);
            }
        }

        private void ButtonSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            FieldValidateAndSaveAccount();
        }

        public void FieldValidateAndSaveAccount()
        {
            //Set all values to false before checking if they are valid
            bool validDisplayName = false;
            bool validUserName = false;
            bool validPassword = false;
            bool urlEntered = false;
            bool validURL = false;
            //Create a Timespan on how long you would like the Error messages to be displayed
            EmptyFieldMessageTimer.Interval = new TimeSpan(0, 0, 0, 1, 500);

            //Check if a Display Name has been entered
            if (TextBoxDisplayName.Text.Length > 0)
            {
                validDisplayName = true;
            }
            else
            {
                //Show error message for Display Name
                FadeInAnimation(TextBoxDisplayNameError);
                //Attatch Timer Event for The Error Message
                EmptyFieldMessageTimer.Tick += (s, args) =>
                {
                    //After the Message timer ends, Fade out the Message and fade in the Account List View
                    FadeOutAnimation(TextBoxDisplayNameError);
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    EmptyFieldMessageTimer.Stop();
                };
                //Start the Timer
                EmptyFieldMessageTimer.Start();
            }
            //Check if a User Name has been entered
            if (TextBoxUserName.Text.Length > 0)
            {
                validUserName = true;
            }
            else
            {
                //Show error message for User Name
                FadeInAnimation(TextBoxUserNameError);
                //Attatch Timer Event for The Error Message
                EmptyFieldMessageTimer.Tick += (s, args) =>
                {
                    //After the Message timer ends, Fade out the Message and fade in the Account List View
                    FadeOutAnimation(TextBoxUserNameError);
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    EmptyFieldMessageTimer.Stop();
                };
                //Start the Timer
                EmptyFieldMessageTimer.Start();
            }
            //Check if a Password has been entered
            if (TextBoxPassword.Text.Length > 0)
            {
                validPassword = true;
            }
            else
            {
                //Show error message for Password
                FadeInAnimation(TextBoxPasswordError);
                //Attatch Timer Event for The Error Message
                EmptyFieldMessageTimer.Tick += (s, args) =>
                {
                    //After the Message timer ends, Fade out the Message and fade in the Account List View
                    FadeOutAnimation(TextBoxPasswordError);
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    EmptyFieldMessageTimer.Stop();
                };
                //Start the Timer
                EmptyFieldMessageTimer.Start();
            }
            //Check if a URL has been entered
            if (TextBoxURL.Text.Length > 0)
            {
                //A URL has been entered
                urlEntered = true;
                //Check if the entered URL is a valid URL
                Uri uriResult;
                validURL = Uri.TryCreate(TextBoxURL.Text, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                //If the entered URL returns invalid then show the error message
                if (validURL == false)
                {
                    //Show error message for Invalid URL
                    FadeInAnimation(TextBoxURLError);
                    //Attatch Timer Event for The Error Message
                    EmptyFieldMessageTimer.Tick += (s, args) =>
                    {
                        //After the Message timer ends, Fade out the Message and fade in the Account List View
                        FadeOutAnimation(TextBoxURLError);
                        //Stop the dispatch timer from repeating these actions (only fires once)
                        EmptyFieldMessageTimer.Stop();
                    };
                    //Start the Timer
                    EmptyFieldMessageTimer.Start();
                }
            }
            //If no URL was enetered allow the user to save the account with no URL
            else
            {
                validURL = true;
            }

            if (validDisplayName && validUserName && validPassword && validURL)
            {
                SaveAccount();
            }
        }
        public void SaveAccount()
        {
            //Check to see if we are saving a new account or an existing account
            if(updateAccountInProgress == true)
            {
                //Get the index of the selected Account in the Observable Collection
                int observableCollectionAccountIndex = Accounts.IndexOf(selectedAccount);
                //Get the index of the selected Account in the users Account List
                int userAccountIndex = currentUser.accounts.IndexOf(selectedAccount);

                //set the new variables of the account that has been updated
                selectedAccount.DisplayName = TextBoxDisplayName.Text;
                selectedAccount.UserName = TextBoxUserName.Text;
                selectedAccount.Password = TextBoxPassword.Text;
                selectedAccount.AssociatedEmail = TextBoxAssociatedEmail.Text;
                selectedAccount.URL = TextBoxURL.Text;
                selectedAccount.LastUpdated = DateTime.Now;
                selectedAccount.passwordLength = int.Parse(TextBoxPasswordLength.Text);
                selectedAccount.SpecialCharacters = TextBoxSpecialCharacters.Text;
                selectedAccount.Group = ComboBoxAccountCategory.SelectedItem.ToString();

                //save the updates to that account
                Accounts[observableCollectionAccountIndex] = selectedAccount;
                currentUser.accounts[userAccountIndex] = selectedAccount;
                //Order the Observable Collection alphabetically
                Accounts = new ObservableCollection<Account>(Accounts.OrderBy(a => a.DisplayName).ToList());
                ListViewAccounts.ItemsSource = Accounts;
                //Set the selection of the listview to the newly created Account
                ListViewAccounts.SelectedItem = selectedAccount;
                //Clear the Account Creation Form
                ClearAccountCreatorForm();
                //Update Logged In Users Account and Encrypt User
                AesGcmService encryptUser = new AesGcmService(currentUser.UserPassword);
                encryptUser.SerializeAndEncryptJson(currentUser);
                //Check logged in user credentials ****TEMPORARY****
                //MessageBox.Show("Added account for user: " + currentUser.Name + "\nPassword: " + currentUser.UserPassword + "\nTotal accounts for user: " + currentUser.accounts.Count);
                selectedAccount = new Account();
                ListViewAccountGroups.SelectedIndex = 0;
                //Hide Account using Fade Out Animation
                FadeOutAnimation(AccountCreator);
                //Show Message that account has been updated
                TextBlockGeneralMessageTop.Text = "";
                TextBlockGeneralMessageMiddle.Text = "Account has been updated";
                TextBlockGeneralMessageBottom.Text = "";
                FadeInAnimation(GeneralMessage);
                GeneralMessageTimer.Tick += (s, args) =>
                {
                    //After the Message timer ends, Fade out the Message and fade in the Account List View
                    FadeOutAnimation(GeneralMessage);
                    FadeInAnimation(GridAccountListView);
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    GeneralMessageTimer.Stop();
                };
                //Start the Timer
                GeneralMessageTimer.Start();
                updateAccountInProgress = false;
            }
            //if it is a new account then save it as a new account
            else
            {
                //When the user clicks Save account, create a new account and save it to the Users Accounts
                newAccount.DisplayName = TextBoxDisplayName.Text;
                newAccount.UserName = TextBoxUserName.Text;
                newAccount.Password = TextBoxPassword.Text;
                newAccount.Notes = "Notes Here";
                newAccount.AssociatedEmail = TextBoxAssociatedEmail.Text;
                newAccount.URL = TextBoxURL.Text;
                newAccount.Created = DateTime.Now;
                newAccount.LastUpdated = DateTime.Now;
                newAccount.passwordLength = int.Parse(TextBoxPasswordLength.Text);
                newAccount.SpecialCharacters = TextBoxSpecialCharacters.Text;
                newAccount.Group = ComboBoxAccountCategory.SelectedItem.ToString();

                //Add the account to the Observable Collection
                Accounts.Add(newAccount);
                //Add the account to the current user
                currentUser.accounts = Accounts;
                //Order the Observable Collection alphabetically
                Accounts = new ObservableCollection<Account>(Accounts.OrderBy(a => a.DisplayName).ToList());
                ListViewAccounts.ItemsSource = Accounts;
                //Set the selection of the listview to the newly created Account
                ListViewAccounts.SelectedItem = newAccount;
                //Clear the Account Creation Form
                ClearAccountCreatorForm();
                //Update Logged In Users Account and Encrypt User
                AesGcmService encryptUser = new AesGcmService(currentUser.UserPassword);
                encryptUser.SerializeAndEncryptJson(currentUser);
                //Check logged in user credentials ****TEMPORARY****
                //MessageBox.Show("Added account for user: " + currentUser.Name + "\nPassword: " + currentUser.UserPassword + "\nTotal accounts for user: " + currentUser.accounts.Count);
                newAccount = new Account();
                ListViewAccountGroups.SelectedIndex = 0;
                //Hide Account using Fade Out Animation
                FadeOutAnimation(AccountCreator);
            }
            
        }

        private void ButtonCancelAccount_Click(object sender, RoutedEventArgs e)
        {
            //check if updating an account or creating a new account
            if (updateAccountInProgress)
            {
                //If any changes are detected on the fields when a user clicks cancel, prompt the user if they would like to continue without saving
                if (
                    updateAccountInitialDisplayName != TextBoxDisplayName.Text ||
                    updateAccountInitialUserName != TextBoxUserName.Text ||
                    updateAccountInitialPassword != TextBoxPassword.Text ||
                    updateAccountInitialAssociatedEmail != TextBoxAssociatedEmail.Text ||
                    updateAccountInitialURL != TextBoxURL.Text)
                {
                    //MessageBox.Show("Changes Detected");
                    FadeOutAnimation(AccountCreator);
                    FadeInAnimation(GridDiscardChangesWarning);
                }
                else
                {
                    //If no changes are detected then show the Account List View
                    //MessageBox.Show("Changes not Detected");
                    FadeOutAnimation(AccountCreator);
                    FadeInAnimation(GridAccountListView);
                    updateAccountInProgress = false;
                    ClearAccountCreatorForm();
                }
            }
            else
            {
                //Fade out the Account Creator Form
                FadeOutAnimation(AccountCreator);
                //Fade in the Account List View
                FadeInAnimation(GridAccountListView);
                //Clear the user defined special characters for for the account a
                newAccount.SpecialCharacters = "";
                //Set the user defined password length back to the default password length (defined in user settings)
                newAccount.passwordLength = currentUser.DefaultPasswordLength;
                //Clear the account Creator form
                ClearAccountCreatorForm();
            }



            

            
            
            

           
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Allow the user to click and drag the window around.
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
            Keyboard.ClearFocus();
        }

        public void FadeInAnimation(FrameworkElement element)
        {
            //Sets the visibilty of the form to visible but keeps it transparent
            element.Visibility = Visibility.Visible;
            //Create opacity animation on the selected element
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            da.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            da.AutoReverse = false;
            //Attach Animation Completed Event to animation
            da.Completed += (s, e) =>
            {
                //This runs once the animation is complete
                //Ensure the form is visible after fading in.
                element.Visibility = Visibility.Visible;

            };
            //Start the fade in animation
            element.BeginAnimation(OpacityProperty, da);
        }
       



        public void FadeOutAnimation(FrameworkElement element)
        {
            //Create opacity animation on the selected element
            DoubleAnimation da = new DoubleAnimation();
            da.From = 1;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            da.AutoReverse = false;
            //Attach Animation Completed Event to animation
            da.Completed += (s, e) =>
            {
                element.Visibility = Visibility.Hidden;

            };
            //Start the animation
            element.BeginAnimation(OpacityProperty, da);
        }

        private void ButtonCopyUserNameView_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TextBoxUserNameView.Text);
            //Set the amount of time the clipboard will store the copied password
            ClipBoardTimer.Interval = new TimeSpan(0, 0, 0, currentUser.CopyTimeout);
            ClipBoardTimer.Tick += (s, args) =>
            {
                //Clear the clipboard after the set interval
                Clipboard.SetText("");
                Clipboard.Clear();
                //Stop the dispatch timer from repeating these actions (only fires once)
                ClipBoardTimer.Stop();
            };
            //Start the Clipboard Timer
            ClipBoardTimer.Start();
        }

        private void ButtonCopyUserPasswordView_Click(object sender, RoutedEventArgs e)
        {
           
            Clipboard.SetText(TextBoxPasswordView.Password);
            //Set the amount of time the clipboard will store the copied password
            ClipBoardTimer.Interval = new TimeSpan(0, 0, 0, currentUser.CopyTimeout);
            ClipBoardTimer.Tick += (s, args) =>
            {
                //Clear the clipboard after the set interval
                Clipboard.SetText("");
                Clipboard.Clear();
                //Stop the dispatch timer from repeating these actions (only fires once)
                ClipBoardTimer.Stop();
            };
            //Start the Clipboard Timer
            ClipBoardTimer.Start();
        }

        private void ButtonUpdateAccountView_Click(object sender, RoutedEventArgs e)
        {
            //Use the Account Creator when a user is updating an account
            FadeOutAnimation(AccountView);
            FadeInAnimation(AccountCreator);
            updateAccountInProgress = true;
            UpdateAccount(selectedAccount);
        }

        private void UpdateAccount(Account selectedAcc)
        {
            //Populate the field of Account Creator with the current Selected Account
            TextBoxDisplayName.Text = selectedAcc.DisplayName;
            TextBoxUserName.Text = selectedAcc.UserName;
            TextBoxPassword.Text = selectedAcc.Password;
            TextBoxAssociatedEmail.Text = selectedAcc.AssociatedEmail;
            TextBoxURL.Text = selectedAcc.URL;
            TextBlockDateCreated.Text = selectedAcc.Created.ToString();
            TextBoxPasswordLength.Text = selectedAcc.passwordLength.ToString();
            TextBoxSpecialCharacters.Text = selectedAcc.SpecialCharacters;

            //Save the initial values of the Textboxes to variable so we can compare them to the input when a user clicks cancel to see
            //if they would like to continue without saving changes
            updateAccountInitialDisplayName = selectedAcc.DisplayName;
            updateAccountInitialUserName = selectedAcc.UserName;
            updateAccountInitialPassword = selectedAcc.Password;
            updateAccountInitialAssociatedEmail = selectedAcc.AssociatedEmail;
            updateAccountInitialURL = selectedAcc.URL;
        }

        private void ButtonExitAccountView_Click(object sender, RoutedEventArgs e)
        {
            //When exiting the Account View, show the Account List View
            FadeOutAnimation(AccountView);
            FadeInAnimation(GridAccountListView);
        }

        

        private void ClearAccountCreatorForm()
        {
            //Clear the Account Creation Form
            TextBoxDisplayName.Text = "";
            TextBoxUserName.Text = "";
            TextBoxPassword.Text = "";
            TextBoxAssociatedEmail.Text = "";
            TextBlockDateCreated.Text = "";
            TextBoxURL.Text = "";
            TextBoxPasswordLength.Text = "";
            TextBoxSpecialCharacters.Text = "";
            ComboBoxAccountCategory.SelectedIndex = 0;
        }

        private void ButtonOpenWebpageView_Click(object sender, RoutedEventArgs e)
        {
            //Attempt to open the URL for the account, launches the default web browser
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = TextBoxURLView.Text,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch(Exception badURL)
            {
                MessageBox.Show("Invalid URL, please enter a valid one\n" + badURL.Message);
            }
            
            
        }

        private void ButtonExitApplication_Click(object sender, RoutedEventArgs e)
        {
            //Ensure the clipboard is empty when the user exits the application
            Clipboard.SetText("");
            //Exit the Application
            this.Close();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Initialize AesGcmService with the password input
            AesGcmService decryptUser = new AesGcmService(TextBoxLogInPassword.Password);
            // IF the user exists continue
            if (File.Exists(@$"Users/{TextBoxLogInUserName.Text}.dbxt"))
            {
                try
                {
                    //Get the text value of the users .dbxt file if it exists
                    string cypher = System.IO.File.ReadAllText(@$"Users/{TextBoxLogInUserName.Text}.dbxt");
                    //Attempt to verify authetication tag of Aes-Gcm with the entered password
                    currentUser = decryptUser.Decrypt(cypher);
                    //If a successfull password was entered Display a welcome message to the user
                    TextBlockLoginMessage.Text = "Welcome back\n" + TextBoxLogInUserName.Text;
                    //Load the users last selected Skin
                    
                    //Fade out the login view
                    FadeOutAnimation(GridLoginView);
                    //Get a random quote to be displayed
                    TipsAndQuotes newQuote = new TipsAndQuotes();
                    TextBlockLoginQuote.Text = newQuote.getRandomQuote();
                    // Fade in the welcome message
                    FadeInAnimation(WelcomeMessage);
                    //Check if the user has existing accounts
                    if(currentUser.accounts != null)
                    {
                        //Iterate through all of the users accounts
                        foreach (Account account in currentUser.accounts)
                        {
                            //Add each account to the Observable Collection
                            Accounts.Add(account);
                        }
                        //Order the Observable Collection alphabetically by the accounts Display Name 
                        Accounts = new ObservableCollection<Account>(Accounts.OrderBy(a => a.DisplayName).ToList());
                        //Set the List View's item source to the observable collection
                        ListViewAccounts.ItemsSource = Accounts;
                        //If accounts exists for a user, set the list selection to the first index.
                        if (Accounts.Count > 0)
                        {
                            ListViewAccounts.SelectedIndex = 0;
                        }
                        
                    }
                    if(currentUser.accountGroups != null)
                    {
                        //MessageBox.Show("Searching for account groups");
                        //Iterate through all of the users account groups
                        foreach (AccountGroup accountGroup in currentUser.accountGroups)
                        {
                            //Add each account group to the observable collection
                            AccountGroups.Add(accountGroup);
                            //MessageBox.Show("Account group found: " + accountGroup.GroupName);
                        }
                        ListViewAccountGroups.ItemsSource = AccountGroups;
                        //If any account groups exist set the list selection to the first index
                        if (AccountGroups.Count > 0)
                        {
                            ListViewAccountGroups.SelectedIndex = 0;
                        }
                    }
                    
                    //Create a Dispatch timer to allow the Welcome message to display for a set period of time.
                    WelcomeMessageTimer.Tick += dispatcherTimer_Tick;
                    //Set the time span for the welcome message to be displayed
                    WelcomeMessageTimer.Interval = new TimeSpan(0, 0, 0, 2, 500);
                    //Start the timer for welcome message
                    WelcomeMessageTimer.Start();
                    //If the user logs in succesfully clear the text fields on the Login View
                    TextBoxLogInPassword.Password = "";
                    TextBoxLogInUserName.Text = "";
                    TextBoxLogInVerifyPassword.Password = "";
                    //Change the Default button if login is successful
                    ButtonLogin.IsDefault = false;
                    ButtonOpenAccount.IsDefault = true;
                    //Set the theme to the users selected theme
                    var rd = new ResourceDictionary();
                    string pathofskin;
                    pathofskin = @$"Skins\{currentUser.Theme}.xaml";
                    rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
                    Application.Current.Resources = rd;
                    //Set the Selected Theme combobox to the users saved theme
                    ComboBoxSelectedTheme.SelectedItem = currentUser.Theme;
                }
                catch(Exception nv)
                {
                    //This error throws if the authentication tag doesnt resolve with the password entered.

                    //Set the text of the login error message and the position of it on the grid
                    TextBlockLoginErrorMessage.Text = "Invalid Username or Password";
                    Grid.SetRow(TextBlockLoginErrorMessage, 7);
                    //Fade in a invalid username/password message
                    FadeInAnimation(TextBlockLoginErrorMessage);
                    //Create a Dispatch timer to allow the Error message to display for a set period of time.
                    LoginErrorMessageTimer.Tick += dispatcherTimerErrorMessage_Tick;
                    //Set the time span for the welcome message to be displayed
                    LoginErrorMessageTimer.Interval = new TimeSpan(0, 0, 0, 2);
                    //Start the timer for welcome message
                    LoginErrorMessageTimer.Start();
                    //If the user logs in succesfully clear the text fields on the Login View
                }

            }
            else
            {
                //If there was no database file corresponding to the username then this block runs

                //Set the text of the login error message and the position of it on the grid
                TextBlockLoginErrorMessage.Text = "Invalid Username or Password";
                Grid.SetRow(TextBlockLoginErrorMessage, 7);
                //Fade in a invalid username/password message
                FadeInAnimation(TextBlockLoginErrorMessage);
                //Create a Dispatch timer to allow the Error message to display for a set period of time.
                LoginErrorMessageTimer.Tick += dispatcherTimerErrorMessage_Tick;
                //Set the time span for the error message to be displayed
                LoginErrorMessageTimer.Interval = new TimeSpan(0, 0, 0, 2);
                //Start the timer for error message
                LoginErrorMessageTimer.Start();
                
            }

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Timer for the welcome message
            //This will run when the specified time has completed
            FadeOutAnimation(WelcomeMessage);
            FadeInAnimation(GridAccountListView);
            WelcomeMessageTimer.Stop();
        }
        private void dispatcherTimerErrorMessage_Tick(object sender, EventArgs e)
        {
            //Timer for the login error message
            //This will run when the specified time has completed
            FadeOutAnimation(TextBlockLoginErrorMessage);
            LoginErrorMessageTimer.Stop();
        }
        private void dispatcherTimerGeneralMessage_Tick(object sender, EventArgs e, FrameworkElement element)
        {
            //Timer for the General Message to be displayed
            //This will run when the specified time has completed
            FadeOutAnimation(GeneralMessage);
            //Fades in the view that gets passed in with the parameters
            FadeInAnimation(element);

            GeneralMessageTimer.Stop();
        }
        private void dispatcherTimerEmptyFieldMessage_Tick(object sender, EventArgs e, FrameworkElement element)
        {
            //Timer for the Empty Field Error Message to be displayed
            //This will run when the specified time has completed
            FadeOutAnimation(GeneralMessage);
            //Fades in the view that gets passed in with the parameters
            FadeInAnimation(element);

            GeneralMessageTimer.Stop();
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            //Change the UI to show the Registration form when the user selects register
            ButtonRegister.IsEnabled = false;
            ButtonRegister.Visibility = Visibility.Hidden;
            ButtonRegisterConfirm.IsEnabled = true;
            ButtonRegisterConfirm.Visibility = Visibility.Visible;
            ButtonExitApplication.IsEnabled = false;
            ButtonExitApplication.Visibility = Visibility.Hidden;
            ButtonRegisterCancel.IsEnabled = true;
            ButtonRegisterCancel.Visibility = Visibility.Visible;
            ButtonLogin.IsEnabled = false;
            ButtonLogin.Visibility = Visibility.Hidden;
            TextBoxLogInVerifyPassword.Visibility = Visibility.Visible;
            TextBlockLogInVerifyPassword.Visibility = Visibility.Visible;
        }

        private void ButtonRegisterConfirm_Click(object sender, RoutedEventArgs e)
        {
            RegisterUser registerNewUser = new RegisterUser();
            //newUser.DeriveKeyFromPassword(TextBoxLogInPassword.Text);

            //Check to ensure no registration fields are left blank and that the two passwords match
            if (TextBoxLogInPassword.Password == TextBoxLogInVerifyPassword.Password && TextBoxLogInUserName.Text != "" && TextBoxLogInPassword.Password.Length >= 10)
            {
                // if the registration fields pass then check if the user exists
                // if the user already exists then ask if they would like to overwrite the user and lose all of thier data
                if (registerNewUser.UserExists(TextBoxLogInUserName.Text))
                {
                    //Show warning message that user already exists, if they click overwrite then overwrite that user
                    FadeOutAnimation(GridLoginView);
                    FadeInAnimation(GridOverwriteUserWarning);

                }
                else
                {
                    //The User does not already exist so create the User
                    RegisterUser();
                }



            }
            else
            {
                if (TextBoxLogInUserName.Text == null || TextBoxLogInUserName.Text.Length == 0)
                {
                    //Set the text of the registration error message
                    //Login error message for an empty username
                    TextBlockLoginErrorMessage.Text = "Username cannot be empty";
                }
                else if (TextBoxLogInPassword.Password == "" || TextBoxLogInPassword.Password.Length < 10)
                {
                    //Set the text of the registration error message
                    //Registration error message for passwords under the required length.
                    TextBlockLoginErrorMessage.Text = "Minimum password length is 10";
                }
                else if (TextBoxLogInVerifyPassword.Password != TextBoxLogInPassword.Password)
                {
                    //Set the text of the registration error message
                    //Registration error message for non matching passwords.
                    TextBlockLoginErrorMessage.Text = "Passwords do not match";
                }
                //Set the location of the error message to be displayed
                Grid.SetRow(TextBlockLoginErrorMessage, 9);
                //Fade in a invalid username/password message
                FadeInAnimation(TextBlockLoginErrorMessage);
                //Create a Dispatch timer to allow the Error message to display for a set period of time.
                LoginErrorMessageTimer.Tick += dispatcherTimerErrorMessage_Tick;
                //Set the time span for the welcome message to be displayed
                LoginErrorMessageTimer.Interval = new TimeSpan(0, 0, 0, 2);
                //Start the timer for welcome message
                LoginErrorMessageTimer.Start();
                //If the user logs in succesfully clear the text fields on the Login View
            }

        }

        private void RegisterUser()
        {
            RegisterUser registerNewUser = new RegisterUser();
            registerNewUser.CreateUserFile(TextBoxLogInUserName.Text);
            User newUser = new User();
            newUser.Name = TextBoxLogInUserName.Text;
            newUser.UserPassword = TextBoxLogInPassword.Password;
            newUser.Created = DateTime.Now;
            ObservableCollection<AccountGroup> accountGroups = new ObservableCollection<AccountGroup>();
            AccountGroup all = new AccountGroup();
            all.GroupName = "All";
            all.GroupIconPath = "Images/GroupIcons/All.png";
            AccountGroup email = new AccountGroup();
            email.GroupName = "Email";
            email.GroupIconPath = "Images/GroupIcons/Email.png";
            AccountGroup banking = new AccountGroup();
            banking.GroupName = "Banking";
            banking.GroupIconPath = "Images/GroupIcons/Banking.png";
            AccountGroup network = new AccountGroup();
            network.GroupName = "Network";
            network.GroupIconPath = "Images/GroupIcons/Network.png";
            AccountGroup socialMedia = new AccountGroup();
            socialMedia.GroupName = "Social Media";
            socialMedia.GroupIconPath = "Images/GroupIcons/SocialMedia.png";
            AccountGroup wifi = new AccountGroup();
            wifi.GroupName = "Wifi";
            wifi.GroupIconPath = "Images/GroupIcons/Wifi.png";
            accountGroups.Add(all);
            accountGroups.Add(email);
            accountGroups.Add(banking);
            accountGroups.Add(network);
            accountGroups.Add(socialMedia);
            accountGroups.Add(wifi);

            newUser.accountGroups = accountGroups;
            newUser.TotalAccountGroups = accountGroups.Count;
            AesGcmService encryptUser = new AesGcmService(newUser.UserPassword);
            encryptUser.SerializeAndEncryptJson(newUser);
            if (newUser.accountGroups != null)
            {
                //MessageBox.Show("Searching for account groups");
                //Iterate through all of the users account groups
                foreach (AccountGroup accountGroup in newUser.accountGroups)
                {
                    //Add each account group to the observable collection
                    AccountGroups.Add(accountGroup);
                    //MessageBox.Show("Account group found: " + accountGroup.GroupName);
                }
                
                //If any account groups exist set the list selection to the first index
                if (AccountGroups.Count > 0)
                {
                    ListViewAccountGroups.ItemsSource = AccountGroups;
                    ListViewAccountGroups.SelectedIndex = 0;
                }
            }
            currentUser = newUser;
            //MessageBox.Show("Account Created Succesfully \nWarning: If you forget your password, there is no way to retrieve it. YOU WILL LOSE ALL ASSOCIATED ACCOUNTS AND PASSWORDS.\n It is recommended to make backups of your user file in -directoryhere- in the case of data corruption");
            TextBoxLogInPassword.Password = "";
            TextBoxLogInUserName.Text = "";
            TextBoxLogInVerifyPassword.Password = "";
            ButtonRegister.IsEnabled = true;
            ButtonRegister.Visibility = Visibility.Visible;
            ButtonRegisterConfirm.IsEnabled = false;
            ButtonRegisterConfirm.Visibility = Visibility.Hidden;
            ButtonExitApplication.IsEnabled = true;
            ButtonExitApplication.Visibility = Visibility.Visible;
            ButtonRegisterCancel.IsEnabled = false;
            ButtonRegisterCancel.Visibility = Visibility.Hidden;
            ButtonLogin.IsEnabled = true;
            ButtonLogin.Visibility = Visibility.Visible;
            TextBoxLogInVerifyPassword.Visibility = Visibility.Hidden;
            TextBlockLogInVerifyPassword.Visibility = Visibility.Hidden;
            ButtonLogin.IsDefault = false;
            ButtonConfirmRegistrationWarning.IsDefault = true;
            FadeOutAnimation(GridOverwriteUserWarning);
            FadeOutAnimation(GridLoginView);
            FadeInAnimation(GridRegistrationWarning);
        }

        private void ButtonRegisterCancel_Click(object sender, RoutedEventArgs e)
        {
            //Change the UI back to the Login UI if the user selects cancel registration
            TextBoxLogInVerifyPassword.Password = "";
            ButtonRegister.IsEnabled = true;
            ButtonRegister.Visibility = Visibility.Visible;
            ButtonRegisterConfirm.IsEnabled = false;
            ButtonRegisterConfirm.Visibility = Visibility.Hidden;
            ButtonExitApplication.IsEnabled = true;
            ButtonExitApplication.Visibility = Visibility.Visible;
            ButtonRegisterCancel.IsEnabled = false;
            ButtonRegisterCancel.Visibility = Visibility.Hidden;
            ButtonLogin.IsEnabled = true;
            ButtonLogin.Visibility = Visibility.Visible;
            TextBoxLogInVerifyPassword.Visibility = Visibility.Hidden;
            TextBlockLogInVerifyPassword.Visibility = Visibility.Hidden;
        }

        private void ButtonPasswordOptions_Click(object sender, RoutedEventArgs e)
        {
            //When a user is accessing the password settings for an account, check to see if the user is updating an existing account of creating a new one
            if(updateAccountInProgress == true)
            {
                //If the user is updating an existing account set the fields to reflect that accounts current Password Options
                TextBoxPasswordLength.Text = selectedAccount.passwordLength.ToString();
                TextBoxSpecialCharacters.Text = selectedAccount.SpecialCharacters;
            }
            else
            {
                //When adding a new account and changing the password setting parameters for the password to generate set the users default values.
                TextBoxPasswordLength.Text = currentUser.DefaultPasswordLength.ToString();
                TextBoxSpecialCharacters.Text = "";
            }
            //Display the password options for the account
            FadeInAnimation(GridPasswordSettings);
        }

        private void ButtonSavePasswordSettings_Click(object sender, RoutedEventArgs e)
        {
            //When a user is saving the password settings for an account, check to see if the user is updating an existing account of creating a new one
            if (updateAccountInProgress == true)
            {
                //If updating an account, save the settings to that account
                selectedAccount.SpecialCharacters = TextBoxSpecialCharacters.Text;
                selectedAccount.passwordLength = int.Parse(TextBoxPasswordLength.Text);
            }
            else
            {
                //save the settings password generation settings to the account being created for future use.
                newAccount.SpecialCharacters = TextBoxSpecialCharacters.Text;
                newAccount.passwordLength = int.Parse(TextBoxPasswordLength.Text);
            }
            
            //fade out the password settings to reveal the Account Creator again
            FadeOutAnimation(GridPasswordSettings);
        }

        private void ButtonCancelPasswordSettings_Click(object sender, RoutedEventArgs e)
        {
            //Fade out the password settings if the user cancels
            FadeOutAnimation(GridPasswordSettings);
        }

        private void TextBoxPasswordLength_KeyDown(object sender, KeyEventArgs e)
        {
            //Only allow integers to be typed into password length textbox
            if (e.Key < Key.D0 || e.Key > Key.D9 && e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
            {
                //Allow Backspace
                if(e.Key == Key.Back)
                {
                    
                }
                //Anything other than numbers gets handled
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void TextBoxPasswordLength_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //if a user types while text is highlighted, first delete the highlighted text
            if(((TextBox)sender).SelectedText != null)
            {
                //save the textbox text to a string
                string textboxText = ((TextBox)sender).Text;
                //Save the highlighted text to a string
                string highlightedText = ((TextBox)sender).SelectedText;
                //Remove the highlighted Text
                ((TextBox)sender).Text = textboxText.Remove(textboxText.IndexOf(highlightedText), highlightedText.Length);
                //Set The Caret to the end of the highlited text before Parsing the Value
                ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
            }

            //Only allow a maximum of 25 for the password length
            //Parse the value currently being entered into the Password Length Text box
            int passwordValue = int.Parse(((TextBox)sender).Text + e.Text);

            //if the value is 0 then dont allow 0 as input
            if(passwordValue == 0)
            {
                e.Handled = true;
            }

            //if the value is greater than 25 set the text to 25 (The maximum length acceptable for the password)
            if(passwordValue > 25)
            {
                e.Handled = true;
                ((TextBox)sender).Text = "25";
                ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
            }
            
        }

        private void TextBoxSpecialCharacters_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //check if the current character being input is a special character and do not allow it to be input if it is not.
            e.Handled = IsSpecialCharacter(e.Text, @"[\p{P}\p{S}]");
            
            //check if the Special Character textbox already contains the entered special character
            if (TextBoxSpecialCharacters.Text.Contains(e.Text))
            {
                //if the special character already exists, do not allow a duplicate to be entered
                e.Handled = true;
            }
        }

        //Check if the special character is in the allowed character set
        private static bool IsSpecialCharacter(string Text, string AllowedRegex)
        {
            try
            {
                var regex = new Regex(AllowedRegex);
                return !regex.IsMatch(Text);
            }
            catch
            {
                return true;
            }
        }

        private void ButtonConfirmRegistrationWarning_Click(object sender, RoutedEventArgs e)
        {
            //Display a welcome message to new users
            TextBlockLoginMessage.Text = "Welcome to PassLock";
            //Create a Dispatch timer to allow the Welcome message to display for a set period of time.
            WelcomeMessageTimer.Tick += dispatcherTimer_Tick;
            //Set the time span for the welcome message to be displayed
            WelcomeMessageTimer.Interval = new TimeSpan(0, 0, 0, 2, 500);
            //Start the timer for welcome message
            WelcomeMessageTimer.Start();
            //If the user logs in succesfully clear the text fields on the Login View
            TextBoxLogInPassword.Password = "";
            TextBoxLogInUserName.Text = "";
            TextBoxLogInVerifyPassword.Password = "";
            //Change the Default button
           
            ButtonConfirmRegistrationWarning.IsDefault = false;
            ButtonOpenAccount.IsDefault = true;
            FadeOutAnimation(GridRegistrationWarning);
            //Get a random quote to be displayed
            TipsAndQuotes newQuote = new TipsAndQuotes();
            TextBlockLoginQuote.Text = newQuote.getRandomQuote();
            FadeInAnimation(WelcomeMessage);
        }


        private void MenuItemCopyUsername_Click(object sender, RoutedEventArgs e)
        {
            if(ListViewAccounts.SelectedItem != null)
            {
                Account selectedAccount = (Account)ListViewAccounts.SelectedItem;
                Clipboard.SetText(selectedAccount.UserName);

                //Set the amount of time the clipboard will store the copied username
                ClipBoardTimer.Interval = new TimeSpan(0, 0, 0, currentUser.CopyTimeout);
                ClipBoardTimer.Tick += (s, args) =>
                {
                    //Clear the clipboard after the set interval
                    Clipboard.SetText("");
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    ClipBoardTimer.Stop();
                };
                //Start the Clipboard Timer
                ClipBoardTimer.Start();
            }
            
        }

        private void MenuItemCopyPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewAccounts.SelectedItem != null)
            {
                Account selectedAccount = (Account)ListViewAccounts.SelectedItem;
                Clipboard.SetText(selectedAccount.Password);

                //Set the amount of time the clipboard will store the copied password
                ClipBoardTimer.Interval = new TimeSpan(0, 0, 0, currentUser.CopyTimeout);
                ClipBoardTimer.Tick += (s, args) =>
                {
                    //Clear the clipboard after the set interval
                    Clipboard.SetText("");
                    //Stop the dispatch timer from repeating these actions (only fires once)
                    ClipBoardTimer.Stop();
                };
                //Start the Clipboard Timer
                ClipBoardTimer.Start();
            }
        }

        private void ButtonUserSettings_Click(object sender, RoutedEventArgs e)
        {
            //populate the timeout for a copy/paste of a username or password defined in the currentUsers Settings
            TextBoxCopyTimeout.Text = currentUser.CopyTimeout.ToString();
            //Populate the Default Password Length defined in a the currentUsers Settings
            TextBoxDefaultPasswordLength.Text = currentUser.DefaultPasswordLength.ToString();
            //Populate the Auto Logout Timer defined in the user settings
            if(currentUser.AutoLogoutTimeInSeconds == null || currentUser.AutoLogoutTimeInSeconds == 0)
            {
                TextBoxAutoLogoutTime.Text = "0";
            }
            else
            {
                TextBoxAutoLogoutTime.Text = currentUser.AutoLogoutTimeInSeconds.ToString();
            }
            
            //Fade out the AccountListView and Show the User Settings View
            FadeOutAnimation(GridAccountListView);
            FadeInAnimation(GridUserSettings);
        }

        private void TextBoxLogInUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBoxLogInUserName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBoxLogInPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void TextBoxLogInVerifyPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void AllowIntegersOnly(object sender, KeyEventArgs e)
        {
            //Only allow integers to be typed into into the sender(usually textbox)
            if (e.Key < Key.D0 || e.Key > Key.D9 && e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
            {
                if (e.Key == Key.Back)
                {

                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void ButtonSaveUserSettings_Click(object sender, RoutedEventArgs e)
        {
            //Verify the fields are not blank
            if(TextBoxDefaultPasswordLength.Text.Length > 0 && TextBoxCopyTimeout.Text.Length > 0)
            {
                try
                {
                    //Save the parameters entered
                    currentUser.Theme = (string)ComboBoxSelectedTheme.SelectedItem;
                    currentUser.DefaultPasswordLength = int.Parse(TextBoxDefaultPasswordLength.Text);
                    currentUser.CopyTimeout = int.Parse(TextBoxCopyTimeout.Text);
                    currentUser.AutoLogoutTimeInSeconds = int.Parse(TextBoxAutoLogoutTime.Text);
                }
                catch (Exception notValid)
                {
                    MessageBox.Show("Copy Timeout or Default Password Length is not valid");
                }
            }
            else
            {
                //Inform the user of the minimum password length and the minimum timeout length
            }

            //Set up the message to be displayed to the user (Settings Have Been Saved)
            TextBlockGeneralMessageTop.Text = "";
            TextBlockGeneralMessageMiddle.Text = "Settings have been saved.";
            TextBlockGeneralMessageBottom.Text = "";
            //Fade out the User Settings Form and Fade in the Message
            FadeOutAnimation(GridUserSettings);
            FadeInAnimation(GeneralMessage);
            //Create a Timespan on how long you would like the message to be displayed
            GeneralMessageTimer.Interval = new TimeSpan(0, 0, 0, 2);
            GeneralMessageTimer.Tick += (s, args) =>
            {
                //After the Message timer ends, Fade out the Message and fade in the Account List View
                FadeOutAnimation(GeneralMessage);
                FadeInAnimation(GridAccountListView);
                //Stop the dispatch timer from repeating these actions (only fires once)
                GeneralMessageTimer.Stop();
            };
            //Start the Timer
            GeneralMessageTimer.Start();
        }

        private void ButtonUserSettingsReturn_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAnimation(GridUserSettings);
            FadeInAnimation(GridAccountListView);
            //Set the ComboBox to the users theme
            ComboBoxSelectedTheme.SelectedItem = currentUser.Theme;
            //If the user was previewing themes set the theme to the last saved theme for the user
            var rd = new ResourceDictionary();
            string pathofskin;
            pathofskin = @$"Skins\{currentUser.Theme}.xaml";
            rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources = rd;

        }

        private void ButtonReturnToAccountUpdateForm_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAnimation(GridDiscardChangesWarning);
            FadeInAnimation(AccountCreator);
        }

        private void ButtonDiscardAccountChanges_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAnimation(GridDiscardChangesWarning);
            FadeInAnimation(GridAccountListView);
            updateAccountInProgress = false;
            ClearAccountCreatorForm();
        }

        private void ButtonReturnToUserRegistration_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAnimation(GridOverwriteUserWarning);
            FadeInAnimation(GridLoginView);
        }

        private void ButtonOverwriteUser_Click(object sender, RoutedEventArgs e)
        {
            RegisterUser();
        }

        private void ComboBoxSelectedTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)ComboBoxSelectedTheme.SelectedItem == "Default")
            {
                var rd = new ResourceDictionary();
                string pathofskin;
                pathofskin = @"Skins\Default.xaml";
                rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
                Application.Current.Resources = rd;
                //Set the theme for the user
                
            }
            if ((string)ComboBoxSelectedTheme.SelectedItem == "Light")
            {

            }
            if((string)ComboBoxSelectedTheme.SelectedItem == "Purple")
            {
                var rd = new ResourceDictionary();
                string pathofskin;
                pathofskin = @"Skins\Purple.xaml";
                rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
                Application.Current.Resources = rd;
                //Set the theme for the user
                
            }
            if ((string)ComboBoxSelectedTheme.SelectedItem == "Green")
            {
                var rd = new ResourceDictionary();
                string pathofskin;
                pathofskin = @"Skins\Green.xaml";
                rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(pathofskin, UriKind.Relative)) as ResourceDictionary);
                Application.Current.Resources = rd;
                
            }
            
        }


        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //Check to see if a user is logged in
            if (currentUser != null)
            {
                //If the mouse leaves the frame of the program and a user is logged in, set timer interval to the users AutoLogoutTime
                InactivityTimer.Interval = new TimeSpan(0, 0, currentUser.AutoLogoutTimeInSeconds);
                //Attach the event handler
                InactivityTimer.Tick += InactiveUser_Tick;
                //Start the Timer
                InactivityTimer.Start();
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //When the mouse enters the window, stop the inactivity timer
            InactivityTimer.Stop();
            //Dispose of the inactivity event handler
            InactivityTimer.Tick -= InactiveUser_Tick;
        }
        private void InactiveUser_Tick(Object sender, EventArgs e)
        {
            //Dispose of the event handler
            InactivityTimer.Tick -= InactiveUser_Tick;
            //Stop the timer
            InactivityTimer.Stop();
            //Logout the user
            LogoutUser();
        }

        private void ButtonChangeUserPassword_Click(object sender, RoutedEventArgs e)
        {
            //Open Password Change And Verify View
            //User must enter current password, and new password, and new password verify

            //Update the password for the currentUser

            //Update PasswordLastUpdated DateTime in currentUser
        }
        private double DaysSinceUserPasswordUpdate()
        {
            TimeSpan DaysSinceLastUpdate = DateTime.Now.Subtract(currentUser.Created);
            return DaysSinceLastUpdate.TotalDays;
        }

        private void TextBoxSearchAccounts_GotFocus(object sender, RoutedEventArgs e)
        {
            if(TextBoxSearchAccounts.Text == "Search")
            {
                TextBoxSearchAccounts.Text = "";
                //SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
                TextBoxSearchAccounts.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void TextBoxSearchAccounts_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(ListViewAccountGroups.Items.Count > 0)
                ListViewAccountGroups.SelectedIndex = 0;
            ListViewAccountGroups.ScrollIntoView(ListViewAccountGroups.SelectedItem);

            if (TextBoxSearchAccounts.Text == "Search")
            {
                TextBoxSearchAccounts.Text = "";
                //SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
                TextBoxSearchAccounts.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void TextBoxSearchAccounts_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ListViewAccountGroups.Items.Count > 0)
                ListViewAccountGroups.SelectedIndex = 0;
            ListViewAccountGroups.ScrollIntoView(ListViewAccountGroups.SelectedItem);

            if (TextBoxSearchAccounts.Text == "")
            {
                TextBoxSearchAccounts.Text = "Search";
                TextBoxSearchAccounts.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void TextBoxSearchAccounts_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextBoxSearchAccounts.Text == "")
            {
                TextBoxSearchAccounts.Text = "Search";
                TextBoxSearchAccounts.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void SearchAccounts(string searchTerm)
        {
            //ListViewMovies.Items.Clear();
            //List<MovieListItem> movieSearch = new List<MovieListItem>();
            ObservableCollection<Account> filteredAccounts = new ObservableCollection<Account>();
            foreach (Account account in Accounts)
            {
                if (account.DisplayName.ToLower().Contains(searchTerm.ToLower()))
                {
                    filteredAccounts.Add(account);
                    
                }
                ListViewAccounts.ItemsSource = filteredAccounts;
            }
        }

        private void TextBoxSearchAccounts_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(TextBoxSearchAccounts.Text != "Search")
                SearchAccounts(TextBoxSearchAccounts.Text);
        }

        private void ComboBoxAccountCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ListViewAccountGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                AccountGroup selectedAccountGroup = ((AccountGroup)ListViewAccountGroups.SelectedItem);
                if(ListViewAccountGroups.SelectedIndex == 0)
                {
                    ListViewAccounts.ItemsSource = Accounts;
                    if(Accounts.Count > 0)
                        ListViewAccounts.SelectedIndex = 0;
                }
                else
                {
                    ObservableCollection<Account> filteredAccounts = new ObservableCollection<Account>();
                    foreach (Account account in Accounts)
                    {

                        if (account.Group != null && selectedAccountGroup != null)
                        {
                            //MessageBox.Show($@"Checking if selected account group of - " + account.Group + "contains the selected filter of - " + selectedAccountGroup.GroupName);
                            if (account.Group.ToLower().Contains(selectedAccountGroup.GroupName.ToLower()))
                            {
                                filteredAccounts.Add(account);

                            }
                            ListViewAccounts.ItemsSource = filteredAccounts;
                            if(filteredAccounts.Count > 0)
                                ListViewAccounts.SelectedIndex = 0;
                        }

                    }
                }
                
            }
            catch(Exception noGroup)
            {

            }
            
        }

        private void ListViewAccountGroups_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ListView listBox = sender as ListView;
            ScrollViewer scrollviewer = FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;

        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

    }
}
