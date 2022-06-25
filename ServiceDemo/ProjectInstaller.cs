using System.ComponentModel;
using System.Diagnostics;

namespace ServiceDemo
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.AfterInstall += (sender, args) =>
            {
                if (EventLog.SourceExists("ServiceDemo"))
                {
                    EventLog.DeleteEventSource("ServiceDemo");
                    EventLog.CreateEventSource("ServiceDemo", "ServiceDemoLog");
                }
            };

            this.AfterUninstall += (sender, args) =>
            {
                if (EventLog.SourceExists("ServiceDemo"))
                {
                    EventLog.DeleteEventSource("ServiceDemo");
                }
            };
        }
    }
}
