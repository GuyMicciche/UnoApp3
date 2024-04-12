using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.Logging;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UnoApp3.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NavigationRootPage : Page
{
    public IList<NavigationViewItem>? NavigationItems { get; set; }

    public NavigationView NavigationView
    {
        get { return NavigationViewControl; }
    }

    public Action NavigationViewLoaded { get; set; }

    public string AppTitleText
    {
        get
        {
#if DEBUG
            return "UnoApp3 Dev";
#else
                return "UnoApp3";
#endif
        }
    }

    public NavigationRootPage()
    {
        this.InitializeComponent();

        AddNavigationMenuItems();
    }

    private void AddNavigationMenuItems()
    {
        NavigationItems = new List<NavigationViewItem>
        {
            new NavigationViewItem { Name = "Main", Content = "Home Page", Tag = "UnoApp3.Presentation.MainPage", Icon = new SymbolIcon(Symbol.Home) },
            new NavigationViewItem { Name = "Second", Content = "Second Page", Tag = "UnoApp3.Presentation.SecondPage", Icon = new SymbolIcon(Symbol.Favorite) }
        };

        NavigationViewControl.MenuItemsSource = NavigationItems;
        NavigationViewControl.SelectedItem = NavigationItems.First();
    }

    public string GetAppTitleFromSystem()
    {
        //return Package.Current.DisplayName;
        return AppTitleText;
    }
    public void Navigate(
        Type pageType,
        object? targetPageArguments = null,
        Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo? navigationTransitionInfo = null)
    {
        NavigationRootPageArgs args = new NavigationRootPageArgs();
        args.NavigationRootPage = this;
        args.Parameter = targetPageArguments;
        rootFrame.Navigate(pageType, args, navigationTransitionInfo);
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
        {
            VisualStateManager.GoToState(this, "Top", true);
        }
        else
        {
            if (args.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                VisualStateManager.GoToState(this, "Compact", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Default", true);
            }
        }
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Delay necessary to ensure NavigationView visual state can match navigation
        Task.Delay(500).ContinueWith(_ => this.NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());

        var navigationView = sender as NavigationView;
        navigationView.RegisterPropertyChangedCallback(NavigationView.IsPaneOpenProperty, OnIsPaneOpenChanged);
    }

    private void OnIsPaneOpenChanged(DependencyObject sender, DependencyProperty dp)
    {
        var navigationView = sender as NavigationView;
        var announcementText = navigationView.IsPaneOpen ? "Navigation Pane Opened" : "Navigation Pane Closed";

        //UIHelper.AnnounceActionForAccessibility(navigationView, announcementText, "NavigationViewPaneIsOpenChangeNotificationId");
    }

    private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        NavigationViewItem item = args.SelectedItemContainer as NavigationViewItem;

        if (args.IsSettingsSelected)
        {
            if (rootFrame.CurrentSourcePageType != typeof(MainPage))
            {
                Navigate(typeof(MainPage));
            }
        }
        else if (item.Tag is string pageName)
        {
            if (rootFrame.CurrentSourcePageType != Type.GetType(pageName))
            {
                //Navigate(Type.GetType(pageName));
                Navigate(Type.GetType(pageName));
            }
        }

        // Set Header, SelectedItem, and IsBackEnabled properties
        NavigationViewControl.Header = item.Content;
        NavigationViewControl.SelectedItem = item;
        NavigationViewControl.IsBackEnabled = rootFrame.CanGoBack;
    }

    private void controlsSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null && args.ChosenSuggestion is NavigationViewItem)
        {
            var selectedItem = args.ChosenSuggestion as NavigationViewItem;
            var hasChangedSelection = EnsureItemIsVisibleInNavigation(selectedItem.Name);

            // In case the menu selection has changed, it means that it has triggered
            // the selection changed event, that will navigate to the page already
            if (!hasChangedSelection)
            {
                Navigate(Type.GetType(selectedItem.Tag.ToString()));
            }
        }
        else if (!string.IsNullOrEmpty(args.QueryText))
        {
            // GO TO A SEARCH RESULTS PAGE
            //Navigate(typeof(SearchResultsPage), args.QueryText); 
        }
    }

    private void controlsSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = new List<NavigationViewItem>();

            var querySplit = sender.Text.Split(" ");
            foreach (var item in NavigationItems)
            {
                // Check if any of the query tokens are in the item name
                bool matchesQuery = querySplit.All(queryToken =>
                    item.Name.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) >= 0);

                if (matchesQuery)
                {
                    suggestions.Add(item);
                }
            }

            if (suggestions.Count > 0)
            {
                // Order the suggestions based on whether they start with the query text
                controlsSearchBox.ItemsSource = suggestions
                    .OrderByDescending(i => i.Name.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase))
                    .ThenBy(i => i.Name);
            }
            else
            {
                controlsSearchBox.ItemsSource = new string[] { "No results found" };
            }
        }
    }

    private void rootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // TODO: Implement
    }

    private void rootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
    {
        // TODO: Implement
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        controlsSearchBox.Focus(FocusState.Programmatic);
    }

    public bool EnsureItemIsVisibleInNavigation(string name)
    {
        bool changedSelection = false;
        foreach (object rawItem in NavigationView.MenuItems)
        {
            // Check if we encountered the separator
            if (!(rawItem is NavigationViewItem))
            {
                // Skipping this item
                continue;
            }

            var item = rawItem as NavigationViewItem;

            // Check if we are this category
            if ((string)item.Content == name)
            {
                NavigationView.SelectedItem = item;
                changedSelection = true;
            }
            // We are not :/
            else
            {
                // Maybe one of our items is?
                if (item.MenuItems.Count != 0)
                {
                    foreach (NavigationViewItem child in item.MenuItems)
                    {
                        if ((string)child.Content == name)
                        {
                            // We are the item corresponding to the selected one, update selection!

                            // Deal with differences in displaymodes
                            if (NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                            {
                                // In Topmode, the child is not visible, so set parent as selected
                                // Everything else does not work unfortunately
                                NavigationView.SelectedItem = item;
                                item.StartBringIntoView();
                            }
                            else
                            {
                                // Expand so we animate
                                item.IsExpanded = true;
                                // Ensure parent is expanded so we actually show the selection indicator
                                NavigationView.UpdateLayout();
                                // Set selected item
                                NavigationView.SelectedItem = child;
                                child.StartBringIntoView();
                            }
                            // Set to true to also skip out of outer for loop
                            changedSelection = true;
                            // Break out of child iteration for loop
                            break;
                        }
                    }
                }
            }
            // We updated selection, break here!
            if (changedSelection)
            {
                break;
            }
        }
        return changedSelection;
    }

    public NavigationRootViewModel? ViewModel
    {
        get { return DataContext as NavigationRootViewModel; }
    }
}

public class NavigationRootPageArgs
{
    public NavigationRootPage? NavigationRootPage;
    public object? Parameter;
}
