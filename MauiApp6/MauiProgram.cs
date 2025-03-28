﻿using MauiApp6.Model;
using MauiApp6.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace MauiApp6
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Services.AddMudServices(); 
            builder.Logging.AddDebug();

#endif

            return builder.Build();
        }
    }
}
