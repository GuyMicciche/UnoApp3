namespace UnoApp3;

public class App : Application
{
    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
#if __IOS__ || __ANDROID__
			Uno.UI.FeatureConfiguration.Style.ConfigureNativeFrameNavigation();
#endif
        var services = new ServiceCollection();
        ServiceProvider = services.BuildServiceProvider();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);

                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                // Register Json serializers (ISerializer and ISerializer)
                .UseSerialization((context, services) => services
                    .AddContentSerializer(context)
                    .AddJsonTypeInfo(WeatherForecastContext.Default.IImmutableListWeatherForecast))
                .UseHttp((context, services) => services
                    // Register HttpClient
#if DEBUG
                    // DelegatingHandler will be automatically injected into Refit Client
                    .AddTransient<DelegatingHandler, DebugHttpHandler>()
#endif
                    .AddSingleton<IWeatherCache, WeatherCache>()
                    .AddRefitClient<IApiClient>(context))
                .ConfigureServices((context, services) =>
                {
                    // TODO: Register your services
                    //services.AddSingleton<IMyService, MyService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
                // Add navigation support for toolkit controls such as TabBar and NavigationView
                .UseToolkitNavigation()
            );
        MainWindow = builder.Window;
        MainWindow.ExtendsContentIntoTitleBar = true;

#if DEBUG
        MainWindow.EnableHotReload();
#endif

        Host = await builder.NavigateAsync<Shell>();

        //EnsureWindow();
    }

    private void EnsureWindow(Windows.ApplicationModel.Activation.IActivatedEventArgs args = null)
    {
        Type targetPageType = typeof(MainPage);
        string targetPageArguments = string.Empty;

        if (args != null)
        {
            if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.Launch)
            {
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //try
                    //{
                    //    await SuspensionManager.RestoreAsync();
                    //}
                    //catch (SuspensionManagerException)
                    //{
                    //    //Something went wrong restoring state.
                    //    //Assume there is no state and continue
                    //}
                }

                targetPageArguments = ((Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)args).Arguments;
            }
        }

        NavigationRootPage rootPage = MainWindow.Content as NavigationRootPage;
        rootPage.Navigate(targetPageType, targetPageArguments);

        if (targetPageType == typeof(MainPage))
        {
            ((Microsoft.UI.Xaml.Controls.NavigationViewItem)((NavigationRootPage)MainWindow.Content).NavigationView.MenuItems[0]).IsSelected = true;
        }

        // Ensure the current window is active
        MainWindow.Activate();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<NavigationRootPage, NavigationRootViewModel>(),
            new ViewMap<MainPage, MainViewModel>(),
            new DataViewMap<SecondPage, SecondViewModel, Entity>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
            Nested: new RouteMap[]
            {
                new RouteMap("Root", View: views.FindByViewModel<NavigationRootViewModel>(),
                    Nested: new RouteMap[]
                    {
                        new RouteMap("Main", View: views.FindByViewModel<MainViewModel>()),
                        new RouteMap("Second", View: views.FindByViewModel<SecondViewModel>())
                    })
            }));
    }
}
