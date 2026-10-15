using System.Windows;

namespace Desktop_Gremlin
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                MainWindow = new DesktopGremlin.MainWindow();
                MainWindow.Show();
            }
            catch (CharacterPackException exception)
            {
                PackDiagnostics.ReportOnce(exception.Message);
                if (Settings.AllowErrorMessages)
                {
                    MessageBox.Show(exception.Message + "\nDiagnostics: " + PackDiagnostics.LogPath,
                        "Cannot start character pack", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Shutdown(1);
            }
        }
    }
}
