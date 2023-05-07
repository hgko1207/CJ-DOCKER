using CJ_DOCKER.Services;
using CJ_DOCKER.Views;
using DevExpress.Xpf.Core;
using Prism.Ioc;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WPF.Common.Infrastructure;
using WPF.Common.Mvvm;

namespace CJ_DOCKER
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 중복실행 방지
            bool createNew = false;
            new Mutex(true, "CJ-DOCKER", out createNew);
            if (!createNew)
            {
                Shutdown();
                return;
            }

            //var theme = new Theme("IVM_Theme");
            //theme.AssemblyName = "DevExpress.Xpf.Themes.IVM_Theme.v21.1";
            //Theme.RegisterTheme(theme);
            //ApplicationThemeHelper.ApplicationThemeName = "IVM_Theme";

            Application.Current.DispatcherUnhandledException += UnhandledException;

            base.OnStartup(e);
        }

        /// <summary>
        /// 서비스 등록
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<DataManager>();

            containerRegistry.RegisterSingleton<ILogger, Logger>();
        }

        /// <summary>
        /// ConfigureViewModelLocator
        /// </summary>
        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(new ViewModelResolver(() => Container).UseDefaultConfigure().ResolveViewModelForView);
        }

        /// <summary>
        /// 종료
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            (Container.Resolve<ILogger>() as IDisposable)?.Dispose();
            base.OnExit(e);
        }

        /// <summary>
        /// 비정상 종료되었을 때 로그 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string log = "./log/crash/";
            if (Directory.Exists(log) == false)
                Directory.CreateDirectory(log);

            DateTime now = DateTime.Now;

            using (StreamWriter file = new StreamWriter(log + string.Format("log_{0:yyyyMMdd}.log", now), true))
            {
                file.WriteLine(string.Format("[ {0:yyyy-MM-dd hh:mm s} ] ", now));
                file.WriteLine("Module : " + e.Exception.Source);
                file.WriteLine("Message : " + e.Exception.Message);
                file.WriteLine(e.Exception.StackTrace.ToString());
                file.WriteLine("\n\n");
                file.Flush();
            }

            // msg box
            string msg = string.Format("An unhandled exception: {0}\r\n{1}", e.Exception.Message, e.Exception.StackTrace);

            MessageBox.Show(msg, "ERROR - CJ-DOCKER", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
