using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace devcoach.Tools
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
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
    private static OutputWindow _outputWindow;
    private static OutputWindowPane _karmaOutputWindowPane;
    private static System.Diagnostics.Process _karmaProcess;
    private static System.Diagnostics.Process _webServerProcess;
    private static Events _events;
    private static DTEEvents _dteEvents;
    private static DocumentEvents _documentEvents;

    readonly Regex _outputDir =
        new Regex(
          @"\[[\d]{1,6}m{1}",
          RegexOptions.Compiled);

    readonly Regex _browserInfo =
        new Regex(
            @"[a-z|A-Z]{4,20}\s([\d]{1,5}\.{0,1})*\s\" +
            @"([a-z|A-Z|\s|\d]{4,20}\)(\:\s|\s|\]\:\s)",
            RegexOptions.Compiled);

    readonly Regex _details =
        new Regex(
          @"\[\d{1,2}[A-Z]{1}|\[\d{1,2}m{1}",
          RegexOptions.Compiled);

    #region Initialize()
    protected override void Initialize()
    {
      lock (_s_applicationLock)
      {
        Application = (DTE2)GetService(typeof(SDTE));
        _events = Application.Application.Events;
        _dteEvents = _events.DTEEvents;
        _documentEvents = _events.DocumentEvents;

        var win =
          Application.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
        _outputWindow = win.Object as OutputWindow;
        if (_outputWindow != null)
        {
          _karmaOutputWindowPane =
            _outputWindow.OutputWindowPanes.Add("Karma");
        }
        _dteEvents.OnBeginShutdown += ShutdownKarma;
        _events.SolutionEvents.Opened += SolutionEventsOpened;

      }

      base.Initialize();

      // Add our command handlers for menu (commands must exist in .vsct file)
      var mcs =
        GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (null == mcs) return;

      // Create the command for the menu item.
      var menuCommandId1 =
        new CommandID(
          GuidList.guidToggleKarmaVsUnitCmdSet,
          (int)PkgCmdIDList.cmdidToggleKarmaVsUnit
        );
      var menuItem1 = new MenuCommand(KarmaVsUnit, menuCommandId1);
      mcs.AddCommand(menuItem1);

      // Create the command for the menu item.
      var menuCommandId2 =
        new CommandID(
          GuidList.guidToggleKarmaVsE2eCmdSet,
          (int)PkgCmdIDList.cmdidToggleKarmaVsE2e
        );
      var menuItem2 = new MenuCommand(KarmaVsE2e, menuCommandId2);
      mcs.AddCommand(menuItem2);
    }
    #endregion

    #region SolutionEventsOpened()
    private void SolutionEventsOpened()
    {
      RunKarmaVs();
    }
    #endregion

    #region RunKarmaVs()
    private void RunKarmaVs(string config = "unit")
    {
      _karmaOutputWindowPane.Clear();

      ShutdownKarma();

      var nodeFilePath = GetNodeJsPath();
      if (nodeFilePath == null)
      {
        _karmaOutputWindowPane.OutputString(
            "ERROR: Node not found. Download and " +
            "install from: http://www.nodejs.org");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        return;
      }

      _karmaOutputWindowPane.OutputString(
          "INFO: Node installation found: " + nodeFilePath);
      _karmaOutputWindowPane.OutputString(Environment.NewLine);

      var karmaFilePath = GetKarmaPath();
      if (karmaFilePath == null)
      {
        _karmaOutputWindowPane.OutputString(
            "ERROR: Karma was not found. Run \"npm install -g karma\"...");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        return;
      }

      _karmaOutputWindowPane.OutputString(
          "INFO: Karma installation found: " + karmaFilePath);
      _karmaOutputWindowPane.OutputString(Environment.NewLine);

      var chromePath = GetChromePath();
      if (chromePath != null)
      {
        _karmaOutputWindowPane.OutputString(
            "INFO: Found Google Chrome : " + chromePath);
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        Environment.SetEnvironmentVariable("CHROME_BIN", chromePath);
      }

      var mozillaPath = GetMozillaPath();
      if (mozillaPath != null)
      {
        _karmaOutputWindowPane.OutputString(
            "INFO: Found Mozilla Firefox: " + mozillaPath);
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        Environment.SetEnvironmentVariable("FIREFOX_BIN", mozillaPath);
      }

      if (Application.Solution.Projects.Count < 1)
      {
        _karmaOutputWindowPane.OutputString("ERROR: No projects loaded");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        return;
      }


      var projects = GetProjects().
        ToDictionary(project => project.Name, project => project);

      const string webApplication = "{349C5851-65DF-11DA-9384-00065B846F21}";
      const string webSite = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
      const string testProject = "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}";

      Project karmaProject = null;
      string karmaConfigFilePath = null;
      string projectDir = null;
      foreach (var project in projects.Values)
      {
        try
        {
          var projectGuids = project.GetProjectTypeGuids(GetService);

          _karmaOutputWindowPane.OutputString(
              "DEBUG: project '" + project.Name + "' found; GUIDs: " + projectGuids);

          if (projectGuids.Contains(webApplication) ||
              projectGuids.Contains(webSite) ||
              projectGuids.Contains(testProject))
          {
            _karmaOutputWindowPane.OutputString(
                "INFO: Web / Test project found: " + project.Name);
            _karmaOutputWindowPane.OutputString(Environment.NewLine);

            projectDir =
                Path.GetDirectoryName(project.FileName);

            karmaConfigFilePath =
                Path.Combine(projectDir, "karma." + config + ".conf.js");

            if (File.Exists(karmaConfigFilePath))
            {
              _karmaOutputWindowPane.OutputString(
                  "INFO: Configuration found: " + karmaConfigFilePath);
              _karmaOutputWindowPane.OutputString(Environment.NewLine);
              karmaProject = project;
              break;
            }
          }
        }
        catch (Exception ex)
        {

        }
      }

      if (karmaProject == null ||
          projectDir == null)
      {
        _karmaOutputWindowPane.OutputString(
            "INFO: No web project found with a file " +
            "named \"karma." + config + ".conf.js\" " +
            "in the root directory.");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        return;
      }

      var nodeServerFilePath = Path.Combine(projectDir, "server.js");

      if (File.Exists(nodeServerFilePath))
      {
        _karmaOutputWindowPane.OutputString("INFO: server.js found.");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);

        _webServerProcess =
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

        _karmaOutputWindowPane.OutputString(
            "INFO: Starting node server...");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        _webServerProcess.Start();
      }


      _karmaProcess =
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
      _karmaProcess.ErrorDataReceived += OutputReceived;
      _karmaProcess.OutputDataReceived += OutputReceived;
      _karmaProcess.Exited += KarmaProcessOnExited;
      _karmaProcess.EnableRaisingEvents = true;

      try
      {
        _karmaOutputWindowPane.OutputString("INFO: Starting karma server...");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
        _karmaProcess.Start();
        _karmaProcess.BeginOutputReadLine();
      }
      catch (Exception ex)
      {
        _karmaOutputWindowPane.OutputString("ERROR: " + ex);
        _karmaOutputWindowPane.OutputString(Environment.NewLine);
      }
    }

    #endregion

    #region KarmaProcessOnExited()
    private void KarmaProcessOnExited(object sender, EventArgs eventArgs)
    {
      ShutdownKarma();
    }
    #endregion


    #region GetProjects()
    public static IList<Project> GetProjects()
    {
      var projects = Application.Solution.Projects;
      var list = new List<Project>();
      var item = projects.GetEnumerator();
      while (item.MoveNext())
      {
        var project = item.Current as Project;
        if (project == null)
        {
          continue;
        }

        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        {
          list.AddRange(GetSolutionFolderProjects(project));
        }
        else
        {
          list.Add(project);
        }
      }

      return list;
    } 
    #endregion

    #region GetSolutionFolderProjects()
    private static IEnumerable<Project> GetSolutionFolderProjects(
      Project solutionFolder)
    {
      var list = new List<Project>();
      for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
      {
        var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
        if (subProject == null)
        {
          continue;
        }
        if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        {
          list.AddRange(GetSolutionFolderProjects(subProject));
        }
        else
        {
          list.Add(subProject);
        }
      }

      return list;
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
          _karmaOutputWindowPane.OutputString("KILL: phantomjs");
          _karmaOutputWindowPane.OutputString(Environment.NewLine);
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
          _karmaOutputWindowPane.OutputString("KILL: phantomjs.exe");
          _karmaOutputWindowPane.OutputString(Environment.NewLine);
          proc.Kill();
        }
      }
      catch
      {
      }

      if (_karmaProcess != null)
      {
        try
        {
          _karmaProcess.Kill();
        }
        catch { }
        _karmaProcess = null;
      }
      if (_webServerProcess != null)
      {
        try
        {
          _webServerProcess.Kill();
        }
        catch { }
        _webServerProcess = null;
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
            Environment.SpecialFolder.ProgramFiles),
          "Google\\Chrome\\Application\\chrome.exe");
      if (File.Exists(chromeFilePath))
      {
        return chromeFilePath;
      }

      chromeFilePath =
        Path.Combine(
          Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFilesX86),
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
                  Environment.SpecialFolder.ProgramFiles),
              "Mozilla Firefox\\firefox.exe");
      if (File.Exists(mozillaFilePath))
      {
        return mozillaFilePath;
      }
      mozillaFilePath =
          Path.Combine(
              Environment.GetFolderPath(
                  Environment.SpecialFolder.ProgramFilesX86),
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

    #region KarmaVsUnit()
    /// <summary>
    /// This function is the callback used to execute a command when the a menu item is clicked.
    /// See the Initialize method to see how the menu item is associated to this function using
    /// the OleMenuCommandService service and the MenuCommand class.
    /// </summary>
    private void KarmaVsUnit(object sender, EventArgs e)
    {
      if (_karmaProcess != null)
      {
        ShutdownKarma();

        _karmaOutputWindowPane.Clear();
        _karmaOutputWindowPane.OutputString("INFO: Karma has shut down!");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);

        return;
      }

      RunKarmaVs("unit");
    } 
    #endregion

    #region KarmaVsE2e
    private void KarmaVsE2e(object sender, EventArgs e)
    {
      if (_karmaProcess != null)
      {
        ShutdownKarma();

        _karmaOutputWindowPane.Clear();
        _karmaOutputWindowPane.OutputString("INFO: Karma has shut down!");
        _karmaOutputWindowPane.OutputString(Environment.NewLine);

        return;
      }

      RunKarmaVs("e2e");
    } 
    #endregion

    #region OutputReceived()
    private void OutputReceived(
          object sender,
          DataReceivedEventArgs dataReceivedEventArgs)
    {
      try
      {
        _karmaOutputWindowPane.Activate();
        _karmaOutputWindowPane.OutputString(
          FixData(dataReceivedEventArgs.Data));
        _karmaOutputWindowPane.OutputString(
          Environment.NewLine);
      }
      // ReSharper disable once EmptyGeneralCatchClause
      catch { }
    }
    #endregion

    #region FixData()
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
    #endregion
  }
}
