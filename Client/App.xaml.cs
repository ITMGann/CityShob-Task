using Client.Crud;
using Client.RealTime;
using Client.ViewModels;
using Microsoft.AspNet.SignalR.Client;
using System.Windows;
using Unity;
using Unity.Lifetime;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IUnityContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            base.OnStartup(startupEventArgs);

            Container = new UnityContainer().AddExtension(new Diagnostic());
            RegisterTypes(Container);

            MainWindow mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Show();
        }

        private void RegisterTypes(IUnityContainer container)
        {
            string baseUrl = "https://localhost";
            container.RegisterFactory<ITaskCrudService>((unityContainer) => new TaskCrudServiceRestApi(baseUrl),
                                                        new ContainerControlledLifetimeManager());

            HubConnection hubConnection = new HubConnection(baseUrl);
            container.RegisterInstance(hubConnection);
            container.RegisterSingleton<ITaskLockService, TaskLockServiceSignalR>();
            container.RegisterType<SignalRConnection>();
            container.RegisterType<MainWindowViewModel>();
            container.RegisterType<MainWindow>();
        }
    }
}
