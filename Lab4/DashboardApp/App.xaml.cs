using System.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Lab4.DashboardApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var container = new ContainerConfiguration()
            .WithAssembly(typeof(App).Assembly)
            .CreateContainer();

        var dashboard = container.GetExport<Dashboard>();
        dashboard.Show();
    }
}
