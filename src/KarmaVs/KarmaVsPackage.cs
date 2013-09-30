using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Process = EnvDTE.Process;

namespace devcoach.Tools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidKarmaVsPkgString)]
    public sealed class KarmaVsPackage : Package
    {
        private static readonly object _s_applicationLock = new object();
        public static DTE2 Application { get; private set; }
        private static OutputWindow outputWindow;
        private static OutputWindowPane karmaOutputWindowPane;
        private static System.Diagnostics.Process karmaProcess = null;
        private static System.Diagnostics.Process webServerProcess = null;
        public static Events Events { get; private set; }
        public static DTEEvents DTEEvents { get; private set; }
        public static DocumentEvents DocumentEvents { get; private set; }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public KarmaVsPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

            lock (_s_applicationLock)
            {
                Application = (DTE2)GetService(typeof(SDTE));
                Events = Application.Application.Events;
                DTEEvents = Events.DTEEvents;
                DocumentEvents = Events.DocumentEvents;

                var win = Application.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                outputWindow = win.Object as OutputWindow;
                karmaOutputWindowPane = outputWindow.OutputWindowPanes.Add("Karma");
                DTEEvents.OnBeginShutdown += ShutdownKarma;
                Events.SolutionEvents.Opened += SolutionEventsOpened;

            }

            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID1 = new CommandID(GuidList.guidToggleKarmaVsUnitCmdSet, (int)PkgCmdIDList.cmdidToggleKarmaVsUnit);
                MenuCommand menuItem1 = new MenuCommand(KarmaVsUnit, menuCommandID1);
                mcs.AddCommand(menuItem1);

                // Create the command for the menu item.
                CommandID menuCommandID2 = new CommandID(GuidList.guidToggleKarmaVsE2eCmdSet, (int)PkgCmdIDList.cmdidToggleKarmaVsE2e);
                MenuCommand menuItem2 = new MenuCommand(KarmaVsE2e, menuCommandID2);
                mcs.AddCommand(menuItem2);
            }
        }



        private void SolutionEventsOpened()
        {
            RunKarmaVS();
        }

        private void RunKarmaVS(string config = "unit")
        {
            karmaOutputWindowPane.Clear();

            ShutdownKarma();

            var nodeFilePath = GetNodeJsPath();
            if (nodeFilePath == null)
            {
                karmaOutputWindowPane.OutputString(
                    "ERROR: Node not found. Download and " +
                    "install from: http://www.nodejs.org");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                return;
            }

            karmaOutputWindowPane.OutputString(
                "INFO: Node installation found: " + nodeFilePath);
            karmaOutputWindowPane.OutputString(Environment.NewLine);

            var karmaFilePath = GetKarmaPath();
            if (karmaFilePath == null)
            {
                karmaOutputWindowPane.OutputString(
                    "ERROR: Karma was not found. Run \"npm install -g karma\"...");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                return;
            }

            karmaOutputWindowPane.OutputString(
                "INFO: Karma installation found: " + karmaFilePath);
            karmaOutputWindowPane.OutputString(Environment.NewLine);

            var chromePath = GetChromePath();
            if (chromePath != null)
            {
                karmaOutputWindowPane.OutputString(
                    "INFO: Found Google Chrome : " + chromePath);
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                Environment.SetEnvironmentVariable("CHROME_BIN", chromePath);
            }

            var mozillaPath = GetMozillaPath();
            if (mozillaPath != null)
            {
                karmaOutputWindowPane.OutputString(
                    "INFO: Found Mozilla Firefox: " + mozillaPath);
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                Environment.SetEnvironmentVariable("FIREFOX_BIN", mozillaPath);
            }


            if (Application.Solution.Projects.Count < 1)
            {
                karmaOutputWindowPane.OutputString("ERROR: No projects loaded");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                return;
            }


            var projects =
                Application.Solution.Projects.Cast<Project>().
                    ToDictionary(project => project.Name, project => project);
            const string webApplication = "{349C5851-65DF-11DA-9384-00065B846F21}";
            const string webSite = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

            Project karmaProject = null;
            string karmaConfigFilePath = null;
            string projectDir = null;
            foreach (var project in projects.Values)
            {
                try
                {
                    var projectGuids = project.GetProjectTypeGuids(GetService);
                    if (projectGuids.Contains(webApplication) ||
                        projectGuids.Contains(webSite))
                    {
                        karmaOutputWindowPane.OutputString(
                            "INFO: Web project found: " + project.Name);
                        karmaOutputWindowPane.OutputString(Environment.NewLine);

                        projectDir =
                            Path.GetDirectoryName(project.FileName);
                        karmaConfigFilePath =
                            Path.Combine(projectDir, "karma." + config + ".conf.js");

                        if (File.Exists(karmaConfigFilePath))
                        {
                            karmaOutputWindowPane.OutputString(
                                "INFO: Configuration found: " + karmaConfigFilePath);
                            karmaOutputWindowPane.OutputString(Environment.NewLine);
                            karmaProject = project;
                            break;
                        }
                    }
                }
                catch
                {
                }
            }

            if (karmaProject == null ||
                projectDir == null)
            {
                karmaOutputWindowPane.OutputString(
                    "INFO: No web project found with a file " +
                    "named \"karma." + config + ".conf.js\" " +
                    "in the root directory.");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                return;
            }

            var nodeServerFilePath = Path.Combine(projectDir, "server.js");

            if (File.Exists(nodeServerFilePath))
            {
                karmaOutputWindowPane.OutputString("INFO: server.js found.");
                karmaOutputWindowPane.OutputString(Environment.NewLine);

                webServerProcess =
                    new System.Diagnostics.Process
                    {
                        StartInfo =
                        {
                            CreateNoWindow = true,
                            FileName = nodeFilePath,
                            Arguments = nodeServerFilePath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        },
                    };

                karmaOutputWindowPane.OutputString(
                    "INFO: Starting node server...");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                webServerProcess.Start();
            }


            karmaProcess =
                new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        CreateNoWindow = true,
                        FileName = nodeFilePath,
                        Arguments =
                            "\"" +
                            karmaFilePath +
                            "\" start \"" +
                            karmaConfigFilePath +
                            "\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    },
                };
            karmaProcess.ErrorDataReceived += OutputReceived;
            karmaProcess.OutputDataReceived += OutputReceived;
            karmaProcess.Exited += KarmaProcessOnExited;
            karmaProcess.EnableRaisingEvents = true;

            try
            {
                karmaOutputWindowPane.OutputString("INFO: Starting karma server...");
                karmaOutputWindowPane.OutputString(Environment.NewLine);
                karmaProcess.Start();
                karmaProcess.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                karmaOutputWindowPane.OutputString("ERROR: " + ex);
                karmaOutputWindowPane.OutputString(Environment.NewLine);
            }
        }

        #endregion

        #region KarmaProcessOnExited()
        private void KarmaProcessOnExited(object sender, EventArgs eventArgs)
        {
            ShutdownKarma();
        }
        #endregion

        #region ShutdownKarma()
        private void ShutdownKarma()
        {
            try
            {
                foreach (System.Diagnostics.Process proc in
                    System.Diagnostics.Process.GetProcessesByName("phantomjs"))
                {
                    karmaOutputWindowPane.OutputString("KILL: phantomjs");
                    karmaOutputWindowPane.OutputString(Environment.NewLine);
                    proc.Kill();
                }
            }
            catch
            {
            }

            try
            {
                foreach (System.Diagnostics.Process proc in
                    System.Diagnostics.Process.GetProcessesByName("phantomjs.exe"))
                {
                    karmaOutputWindowPane.OutputString("KILL: phantomjs.exe");
                    karmaOutputWindowPane.OutputString(Environment.NewLine);
                    proc.Kill();
                }
            }
            catch
            {
            }

            if (karmaProcess != null)
            {
                try
                {
                    karmaProcess.Kill();
                }
                catch { }
                karmaProcess = null;
            }
            if (webServerProcess != null)
            {
                try
                {
                    webServerProcess.Kill();
                }
                catch { }
                webServerProcess = null;
            }
        }
        #endregion

        #region GetNodeJsPath()
        private string GetNodeJsPath()
        {
            using (var softwareKey = Registry.CurrentUser.OpenSubKey("Software"))
            {
                if (softwareKey == null) return null;
                using (var nodeJsKey = softwareKey.OpenSubKey("Node.js"))
                {
                    if (nodeJsKey == null) return null;
                    var nodeJsFilePath = Path.Combine(
                      (string)nodeJsKey.GetValue("InstallPath"),
                      "node.exe");
                    if (!File.Exists(nodeJsFilePath))
                    {
                        return null;
                    }
                    return nodeJsFilePath;
                }
            }
        }
        #endregion

        #region GetKarmaPath()
        private string GetKarmaPath()
        {
            var karmaFilePath =
              Path.Combine(
                Environment.GetFolderPath(
                  Environment.SpecialFolder.ApplicationData),
                "npm\\node_modules\\karma\\bin\\karma");

            if (File.Exists(karmaFilePath))
            {
                return karmaFilePath;
            }
            return null;
        }
        #endregion

        #region GetChromePath()
        private string GetChromePath()
        {
            var chromeFilePath =
              Path.Combine(
                Environment.GetFolderPath(
                  Environment.SpecialFolder.CommonProgramFiles),
                "Google\\Chrome\\Application\\chrome.exe");
            if (File.Exists(chromeFilePath))
            {
                return chromeFilePath;
            }

            chromeFilePath =
              Path.Combine(
                Environment.GetFolderPath(
                  Environment.SpecialFolder.CommonProgramFilesX86),
                "Google\\Chrome\\Application\\chrome.exe");

            if (File.Exists(chromeFilePath))
            {
                return chromeFilePath;
            }

            chromeFilePath =
              Path.Combine(
                Environment.GetFolderPath(
                  Environment.SpecialFolder.LocalApplicationData),
                "Google\\Chrome\\Application\\chrome.exe");

            if (File.Exists(chromeFilePath))
            {
                return chromeFilePath;
            }
            return null;
        }
        #endregion

        #region GetMozillaPath()
        private string GetMozillaPath()
        {
            var mozillaFilePath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonProgramFiles),
                    "Mozilla Firefox\\firefox.exe");
            if (File.Exists(mozillaFilePath))
            {
                return mozillaFilePath;
            }
            mozillaFilePath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonProgramFilesX86),
                    "Mozilla Firefox\\firefox.exe");
            if (File.Exists(mozillaFilePath))
            {
                return mozillaFilePath;
            }
            mozillaFilePath =
             Path.Combine(
               Environment.GetFolderPath(
                 Environment.SpecialFolder.LocalApplicationData),
               "Mozilla Firefox\\firefox.exe");

            if (File.Exists(mozillaFilePath))
            {
                return mozillaFilePath;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void KarmaVsUnit(object sender, EventArgs e)
        {
            if (karmaProcess != null)
            {
                ShutdownKarma();

                karmaOutputWindowPane.Clear();
                karmaOutputWindowPane.OutputString("INFO: Karma has shut down!");
                karmaOutputWindowPane.OutputString(Environment.NewLine);

                return;
            }

            RunKarmaVS("unit");
        }

        private void KarmaVsE2e(object sender, EventArgs e)
        {
            if (karmaProcess != null)
            {
                ShutdownKarma();

                karmaOutputWindowPane.Clear();
                karmaOutputWindowPane.OutputString("INFO: Karma has shut down!");
                karmaOutputWindowPane.OutputString(Environment.NewLine);

                return;
            }

            RunKarmaVS("e2e");
        }

        private void OutputReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            try
            {
                karmaOutputWindowPane.Activate();
                karmaOutputWindowPane.OutputString(FixData(dataReceivedEventArgs.Data));
                karmaOutputWindowPane.OutputString(Environment.NewLine);
            }
            catch { }
        }

        Regex _outputDir = new Regex(@"\[[\d]{1,6}m{1}", RegexOptions.Compiled);
        Regex _browserInfo = new Regex(@"[a-z|A-Z]{4,20}\s([\d]{1,5}\.{0,1})*\s\([a-z|A-Z|\s|\d]{4,20}\)(\:\s|\s|\]\:\s)", RegexOptions.Compiled);
        Regex _details = new Regex(@"\[\d{1,2}[A-Z]{1}|\[\d{1,2}m{1}", RegexOptions.Compiled);

        private string FixData(string data)
        {
            if (data == null) return null;
            data = _outputDir.Replace(data, string.Empty);
            data = _browserInfo.Replace(data, string.Empty);
            data = _details.Replace(data, string.Empty);
            data = data.TrimStart((char)27);
            if (data.StartsWith("xecuted"))
            {
                data = "E" + data;
            }
            return data;
        }
    }
}
