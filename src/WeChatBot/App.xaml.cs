using Prism.Ioc;
using System.Windows;
using WeChatBot.Views;

namespace WeChatBot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }

    /// <inheritdoc />
    protected override Window CreateShell() => Container.Resolve<MainView>();
}