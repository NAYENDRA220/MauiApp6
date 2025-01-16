using MauiApp6.Base;
using MauiApp6.Model;
using MauiApp6.Services;
using Microsoft.AspNetCore.Components;


namespace MauiApp6.Components.Pages
{
    public partial class Login
    {
        [CascadingParameter]
        private GlobalState _globalState { get; set; }
        private bool _ShowDefaultCredentials { get; set; }
        private Currency _selectedCurrency { get; set; }
        private List<Currency> _currencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().ToList();
        private string _username { get; set; }
        private string _password { get; set; }
        private string _errorMessage = "";

        protected override void OnInitialized()
        {
            try
            {
                var user = UserService.Login(UserService.SeedUsername, UserService.SeedPassword);
                _ShowDefaultCredentials = user.HasInitialPassword;
                _selectedCurrency = _currencies.First(); // Default to first currency
            }
            catch { }
        }

        private void LoginHandler()
        {
            try
            {
                _errorMessage = "";
                _globalState.CurrentUser = UserService.Login(_username, _password);
                if (_globalState.CurrentUser != null)
                {
                    Nav.NavigateTo(_globalState.CurrentUser.HasInitialPassword ? "/change-password" : "/dashboard");
                }
            }
            catch (Exception e)
            {
                _errorMessage = e.Message;
                Console.WriteLine(e);
            }
        }
    }

}

