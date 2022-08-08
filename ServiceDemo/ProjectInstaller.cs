using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

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
                ServiceController sc = new ServiceController(serviceInstaller.ServiceName);
                sc.Start();
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
