using ThunderMC.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace ThunderMC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Splash>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
