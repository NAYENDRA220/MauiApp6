
using MauiApp6.Model;
using Microsoft.AspNetCore.Components;

namespace MauiApp6.Components.Pages
{
    public partial class Index 
    {

        [CascadingParameter]
        private GlobalState _globle {  get; set; }
        protected override void OnInitialized()
        {
            if (_globle != null) 
            {
                Nav.NavigateTo("/login");
            }
          else
            {
                Nav.NavigateTo("/home");
            }

        }
    }
}
