using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using VSConsole.PowerPack.Core;

namespace DevlinLiles.VSConsole_PowerPack
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("fbb4f1c7-9a8f-4832-bf0d-8741d38ba81b")]
    public class VsConsoleToolWindow : ToolWindowPane, IOleCommandTarget
    {
        private Border _consoleParentPane;
        private List<HostInfo> _hostList;
        private FrameworkElement _pendingFocusPane;
        private IVsTextView _vsTextView;
        private IWpfConsole _wpfConsole;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public VsConsoleToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            
            ToolBar = new CommandID(new Guid(GuidList.guidVSConsole_PowerPackCmdSetString), 4112);
        }

        private IComponentModel ComponentModel
        {
            get { return this.GetService<IComponentModel>(typeof(SComponentModel)); }
        }

        private IWpfConsoleService WpfConsoleService
        {
            get { return ComponentModel.GetService<IWpfConsoleService>(); }
        }

        private VsConsoleWindow ConsolePowerPackWindow
        {
            get { return ComponentModel.GetService<IVsConsoleWindow>() as VsConsoleWindow; }
        }

        private List<HostInfo> HostList
        {
            get
            {
                if (_hostList == null)
                {
                    _hostList = ConsolePowerPackWindow.HostList.ToList();
                    _hostList.Sort(
                        (h1, h2) =>
                            string.Compare(h1.DisplayName, h2.DisplayName, StringComparison.CurrentCultureIgnoreCase));
                }
                return _hostList;
            }
        }

        private string[] HostDisplayNames
        {
            get { return HostList.Select(h => h.DisplayName).ToArray(); }
        }

        private HostInfo ActiveHostInfo
        {
            get { return ConsolePowerPackWindow.ActiveHostInfo; }
        }

        private FrameworkElement PendingFocusPane
        {
            get { return _pendingFocusPane; }
            set
            {
                if (_pendingFocusPane != null)
                    _pendingFocusPane.Loaded -= PendingFocusPane_Loaded;
                _pendingFocusPane = value;
                if (_pendingFocusPane == null)
                    return;
                _pendingFocusPane.Loaded += PendingFocusPane_Loaded;
            }
        }

        private IWpfConsole WpfConsole
        {
            get
            {
                if (_wpfConsole == null)
                {
                    if (ActiveHostInfo != null)
                    {
                        try
                        {
                            _wpfConsole = ActiveHostInfo.WpfConsole;
                        }
                        catch (Exception ex)
                        {
                            _wpfConsole = ActiveHostInfo.WpfConsole;
                            _wpfConsole.Write(ex.ToString());
                        }
                    }
                }
                return _wpfConsole;
            }
        }

        private IVsTextView VsTextView
        {
            get
            {
                if (_vsTextView == null && WpfConsole != null)
                    _vsTextView = (IVsTextView)WpfConsole.VsTextView;
                return _vsTextView;
            }
        }

        private Border ConsoleParentPane
        {
            get
            {
                if (_consoleParentPane == null)
                    _consoleParentPane = new Border();
                return _consoleParentPane;
            }
        }

        public override object Content
        {
            get { return ConsoleParentPane; }
            set { base.Content = value; }
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            int num = -2147221248;
            if (VsTextView != null)
                num = ((IOleCommandTarget)VsTextView).QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            if (num == -2147221248)
            {
                var oleCommandTarget = GetService(typeof(IOleCommandTarget)) as IOleCommandTarget;
                if (oleCommandTarget != null)
                    num = oleCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
            return num;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int num = -2147221248;
            if (VsTextView != null)
                num = ((IOleCommandTarget)VsTextView).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            if (num == -2147221248)
            {
                var oleCommandTarget = GetService(typeof(IOleCommandTarget)) as IOleCommandTarget;
                if (oleCommandTarget != null)
                    num = oleCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            return num;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (menuCommandService == null)
                return;
            var id1 = new CommandID(GuidList.guidVSConsole_PowerPackCmdSet, 528);
            menuCommandService.AddCommand(new OleMenuCommand(HostsList_Exec, id1));
            var id2 = new CommandID(GuidList.guidVSConsole_PowerPackCmdSet, 512);
            menuCommandService.AddCommand(new OleMenuCommand(Hosts_Exec, id2));
            var id3 = new CommandID(GuidList.guidVSConsole_PowerPackCmdSet, 768);
            menuCommandService.AddCommand(new OleMenuCommand(ClearHost_Exec, id3));
        }

        protected override void OnClose()
        {
            try
            {
                Settings.SetDefaultHost(this, ConsolePowerPackWindow.ActiveHost);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            base.OnClose();
        }

        public override void OnToolWindowCreated()
        {
            var vsWindowFrame = (IVsWindowFrame)Frame;
            Guid rguid = VSConstants.GUID_TextEditorFactory;
            vsWindowFrame.SetGuidProperty(-4011, ref rguid);
            ConsolePowerPackWindow.ActiveHostChanged += ConsolePowerPackWindow_ActiveHostChanged;
            ConsolePowerPackWindow_ActiveHostChanged(ConsolePowerPackWindow, null);
            base.OnToolWindowCreated();
        }

        protected override bool PreProcessMessage(ref Message m)
        {
            var vsWindowPane = VsTextView as IVsWindowPane;
            if (vsWindowPane == null)
                return base.PreProcessMessage(ref m);
            var lpmsg = new MSG[1];
            lpmsg[0].hwnd = m.HWnd;
            lpmsg[0].message = (uint)m.Msg;
            lpmsg[0].wParam = m.WParam;
            lpmsg[0].lParam = m.LParam;
            return vsWindowPane.TranslateAccelerator(lpmsg) == 0;
        }

        private void HostsList_Exec(object sender, EventArgs e)
        {
            var menuCmdEventArgs = e as OleMenuCmdEventArgs;
            if (menuCmdEventArgs == null)
                return;
            if (menuCmdEventArgs.InValue != null || menuCmdEventArgs.OutValue == IntPtr.Zero)
                throw new ArgumentException();
            Marshal.GetNativeVariantForObject(HostDisplayNames, menuCmdEventArgs.OutValue);
        }

        private void Hosts_Exec(object sender, EventArgs e)
        {
            var menuCmdEventArgs = e as OleMenuCmdEventArgs;
            if (menuCmdEventArgs == null)
                return;
            if (menuCmdEventArgs.InValue != null && menuCmdEventArgs.InValue is int)
            {
                var index = (int)menuCmdEventArgs.InValue;
                if (index < 0 || index >= HostList.Count)
                    return;
                ConsolePowerPackWindow.ActiveHost = HostList[index].HostName;
            }
            else
            {
                if (!(menuCmdEventArgs.OutValue != IntPtr.Zero))
                    throw new ArgumentException();
                Marshal.GetNativeVariantForObject(ActiveHostInfo != null ? ActiveHostInfo.DisplayName : string.Empty,
                    menuCmdEventArgs.OutValue);
            }
        }

        private void ClearHost_Exec(object sender, EventArgs e)
        {
            if (WpfConsole == null)
                return;
            WpfConsole.Dispatcher.ClearConsole();
        }

        private void ConsolePowerPackWindow_ActiveHostChanged(object sender, EventArgs e)
        {
            _wpfConsole = null;
            _vsTextView = null;
            if (WpfConsole == null)
                return;
            var consolePane = WpfConsole.Content as FrameworkElement;
            ConsoleParentPane.Child = consolePane;
            if (consolePane == null)
                return;
            PendingMoveFocus(consolePane);
        }

        private void PendingMoveFocus(FrameworkElement consolePane)
        {
            if (consolePane.IsLoaded &&
                Microsoft.VisualStudio.PlatformUI.ExtensionMethods.IsConnectedToPresentationSource(consolePane))
            {
                PendingFocusPane = null;
                MoveFocus(consolePane);
            }
            else
                PendingFocusPane = consolePane;
        }

        private void PendingFocusPane_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(PendingFocusPane);
            PendingFocusPane = null;
        }

        private void MoveFocus(FrameworkElement consolePane)
        {
            consolePane.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            if (WpfConsole == null)
                return;
            if (WpfConsole.Content != consolePane)
                return;
            try
            {
                WpfConsole.Dispatcher.Start();
            }
            catch (Exception ex)
            {
                WpfConsole.WriteLine(ex.ToString());
            }
        }
    }
}
