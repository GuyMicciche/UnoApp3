using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace UnoApp3.Presentation;

public partial record NavigationRootViewModel
{
    private INavigator _navigator;

    public NavigationRootViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        //Title = "Main";
        //Title += $" - {localizer["ApplicationName"]}";
        //Title += $" - {appInfo?.Value?.Environment}";
    }

    //public string? Title { get; }

    //public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task Navigate()
    {
        await _navigator.NavigateViewModelAsync<MainViewModel>(this);
    }
}
