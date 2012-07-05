// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.DotNetWrappers.Windows;
using O2.External.WinFormsUI.O2Environment;
using O2.Interfaces.Views;
using O2.Kernel;
using O2.Views.ASCX.Ascx.MainGUI;
using O2.Views.ASCX.classes.MainGUI;
using O2.Views.ASCX.CoreControls;
using O2.Views.ASCX.O2Findings;
using O2.Views.ASCX;

namespace O2.External.WinFormsUI.Forms
{
    [Serializable]
    public class O2AscxGUI
    {
        static O2AscxGUI()
        {
            new O2MessagesHandler(); // make sure the Messages Handler is setup
        }

        public static AutoResetEvent guiClosed = new AutoResetEvent(false);

        public static bool launch()
        {
            return launch("O2");
        }

        public static bool launch(string parentFormTitle)
        {
            try
            {
                if (isGuiLoaded())
                {
                    DI.log.error("There is already a GUI loaded and only one can be loaded");
                    return false;
                }
                parentFormTitle = ClickOnceDeployment.getFormTitle_forClickOnce(parentFormTitle);
                //new O2DockPanel();

                O2Thread.staThread(() => new O2DockPanel());

                var maxTimeToWaitForGuiCreation = 20000;
                if (O2DockPanel.guiLoaded.WaitOne(maxTimeToWaitForGuiCreation))
                {
                    DI.o2GuiWithDockPanel.invokeOnThread(()=>DI.o2GuiWithDockPanel.Text = parentFormTitle);                    
                    return true;
                }
                if (false == DebugMsg.IsDebuggerAttached())
					//DI.log.reportCriticalErrorToO2Developers(null, null, "from O2AscxGUI: GUI was not available after 20 seconds");
					DI.log.error("from O2AscxGUI: GUI was not available after 20 seconds");
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool launch(string parentFormTitle, int width, int height)
        {
            if (launch(parentFormTitle))
            {
                DI.o2GuiWithDockPanel.invokeOnThread(
                    () =>
                        {
                            DI.o2GuiWithDockPanel.Height = height;
                            DI.o2GuiWithDockPanel.Width = width;
                        });
                
                return true;
            }

            return false;
        }

        public static bool waitForAscxGuiClose(int milisecondsToWait)
        {
            if (false == guiClosed.WaitOne(milisecondsToWait))
            {
                DI.log.error("in waitForAscxGuiClose , Gui didn't close after {0} seconds", milisecondsToWait / 1000);
                return false;
            }
            return true;
        }

        public static void waitForAscxGuiClose()
        {
            guiClosed.WaitOne();
        }

        public static bool close()
        {
            if (isGuiLoaded())
                try
                {
                    if (DI.o2GuiWithDockPanel.okThread(delegate { close(); }))
                    {
                        // before we close this we need to remove all loaded Ascx from the DI.dO2LoadedO2DockContent
                        DI.dO2LoadedO2DockContent.Clear();    

                        // now close the GUI
                        DI.o2GuiWithDockPanel.Close();
                        
                        //DI.o2GuiWithDockPanel.Dispose();

                    }
                }
                catch (Exception ex)
                {
                    DI.log.ex(ex, "in O2AscxGUI.close");
                    return false;
                }
            if (DI.o2GuiWithDockPanel == null)
                return true;
            if (false == DebugMsg.IsDebuggerAttached())
                waitForAscxGuiClose(5000);
            else
                waitForAscxGuiClose();

            DI.log.info("Gui Closed");
            return true;
        }

        public static void closeAscxParent(string ascxControlName)
        {
            if (ascxControlName != null)
            {
                var ascxControlToClose = (ContainerControl)getAscx(ascxControlName);

                if (ascxControlToClose == null)
                    DI.log.error(
                        "in O2AscxGui.closeAscxParent, could not get control: {0}", ascxControlName);
                else
                    O2Forms.closeParentForm(ascxControlToClose);
            }
        }

        public static void setLogViewerDockState(O2DockState o2DockState)
        {
            O2DockUtils.setDockContentState(PublicDI.LogViewerControlName, o2DockState);
        }


        public static void logInfo(string infoMessageToLog)
        {
            DI.log.info(infoMessageToLog);
        }

        public static void logDebug(string debugMessageToLog)
        {
            DI.log.debug(debugMessageToLog);
        }

        public static void logError(string errorMessageToLog)
        {
            DI.log.error(errorMessageToLog);
        }

        /*public static void showMessageBox(string messageBoxText)
        {
            DI.log.showMessageBox(messageBoxText);
        }

        public static DialogResult showMessageBox(string message, string messageBoxTitle,
                                                  MessageBoxButtons messageBoxButtons)
        {
            return DI.log.showMessageBox(message, messageBoxTitle, messageBoxButtons);
        }*/

        public static void openAscx(string ascxControlToLoad, O2DockState dockState, String guiWindowName)
        {
            var type = DI.reflection.getType(ascxControlToLoad);
            if (type == null)
                DI.log.error("in O2AscxGui.openAscx, could not resolve type called: {0}", ascxControlToLoad);
            else
                openAscx(type, dockState, guiWindowName);
        }

        public static Control openAscx(Type ascxControlToLoad)
        {
            string controlName = StringsAndLists.addSpacesOnUpper(ascxControlToLoad.Name.Replace("ascx_", ""));
            return openAscx(ascxControlToLoad, controlName);
        }

        public static Control openAscx(Type ascxControlToLoad, String guiWindowName)
        {
            return openAscx(ascxControlToLoad, O2DockState.Document, guiWindowName);
        }

        /// <summary>
        ///  opens ascx control (Sync mode)
        /// </summary>
        /// <param name="ascxControlToLoad"></param>
        /// <param name="dockState"></param>
        /// <param name="guiWindowName"></param>
        public static Control openAscx(Type ascxControlToLoad, O2DockState dockState, String guiWindowName)
        {
            Control ascxControl = null;
            var sync = new AutoResetEvent(false);
            O2Thread.staThread(() =>
                                   {
                                       ascxControl = O2DockPanel.loadControl(ascxControlToLoad, dockState, guiWindowName);
                                       sync.Set();
                                   });
            sync.WaitOne();

            return ascxControl;
        }
        public static void openAscxASync(string ascxControlToLoad, O2DockState dockState, String guiWindowName)
        {
            var type = DI.reflection.getType(ascxControlToLoad);
            if (type == null)
                DI.log.error("in O2AscxGui.openAscx, could not resolve type called: {0}", ascxControlToLoad);
            else
                openAscxASync(type, dockState, guiWindowName);
        }

        
        public static void openAscxASync(Type ascxControlToLoad, O2DockState dockState, String guiWindowName)
        {
            O2Thread.staThread(() => O2DockPanel.loadControl(ascxControlToLoad, dockState, guiWindowName));
        }

        // was not working (tried to fix the annoying cases where the windows cursor was stuck with the HourGlass shape
        /* public static void setCursor(Cursor cursor)
         {
             if (DI.o2GuiWithDockPanel != null)
                 DI.o2GuiWithDockPanel.Cursor = cursor;
         }*/

        public static void openAscxAsForm(Type ascxControlToLoad)
        {
            openAscxAsForm(ascxControlToLoad, ascxControlToLoad.Name);
        }

        public static void openAscxAsForm(Type ascxControlToLoad, string formName)
        {
            O2DockContent.launchO2DockContentAsStandAloneForm(ascxControlToLoad, formName);
        }

        public static void openAscxAsForm(string ascxControlToLoad, string formName)
        {
            Type typeOfAscxControlToLoad = DI.reflection.getType(ascxControlToLoad);
            if (typeOfAscxControlToLoad == null)
                DI.log.error("in O2Messages.openAscxAsForm could not resolve Type:{0}", ascxControlToLoad);
            else
                O2DockContent.launchO2DockContentAsStandAloneForm(typeOfAscxControlToLoad, formName);
        }

        public static Control getGuiWithDockPanelAsControl()
        {
            return DI.o2GuiWithDockPanel;
        }

        public static Control getAscx(string ascxControlName)
        {
            return O2DockUtils.getAscx(ascxControlName);
        }

        public static bool isAscxLoaded(string ascxControlName)
        {
            return O2DockUtils.getAscx(ascxControlName) != null;
        }

        public static bool isGuiLoaded()
        {
            return DI.o2GuiWithDockPanel != null;
        }

        public static object invokeOnAscxControl(string ascxTargetControl, string methodToExecute)
        {
            return invokeOnAscxControl(ascxTargetControl, methodToExecute, new object[0]);
        }

        public static object invokeOnAscxControl(string ascxTargetControl, string methodToExecute, object[] methodParameters)
        {
            var ascxControl = getAscx(ascxTargetControl);
            if (ascxControl != null)
                return DI.reflection.invoke(ascxControl, methodToExecute, methodParameters);
            return null;
        }

        public static List<String> invokeAndGetStringList(string ascxTargetControl, string methodToExecute)
        {
            return invokeAndGetStringList(ascxTargetControl, methodToExecute, new object[0]);
        }

        public static List<String> invokeAndGetStringList(string ascxTargetControl, string methodToExecute, object[] methodParameters)
        {
            var ascxControl = getAscx(ascxTargetControl);
            if (ascxControl != null)
            {
                var returnData = DI.reflection.invoke(ascxControl, methodToExecute, methodParameters);
                if (returnData != null && returnData is List<String>)
                    return (List<String>) returnData;
            }
            return null;
        }


        public static void clickButton(string ascxControlName, string buttonToClick)
        {
            invokeOnAscxControl(ascxControlName, buttonToClick + "_Click", new object[] { null, null });
        }

        public static void addControlToMenu(Type ascxControlToLoad)
        {            
            string controlName = StringsAndLists.addSpacesOnUpper(ascxControlToLoad.Name.Replace("ascx_", ""));
            addControlToMenu(ascxControlToLoad, controlName);
        }

        public static void addControlToMenu(Type ascxControlToLoad, String guiWindowName)
        {
            addControlToMenu(ascxControlToLoad, O2DockState.Float, guiWindowName);
        }
        
        public static void addControlToMenu(string menuItemName, O2Thread.FuncVoid onMenuItemClick)
        {
            DI.o2GuiWithDockPanel.addToLoadedO2ModulesMenu(menuItemName, onMenuItemClick);
        }

        public static void addControlToMenu(Type ascxControlToLoad, O2DockState dockState, String guiWindowName)
        {
            O2DockPanel.addAscxControlToO2GuiWithDockPanelWithDockState(ascxControlToLoad, O2DockUtils.getDockStateFromO2DockState(dockState), guiWindowName,false);
        }


        public static void workingOnTaskForm_open(string controlName)
        {
            openAscxASync(typeof (ascx_WorkingOnTask), O2DockState.Float, controlName);
        }

        public static void workingOnTaskForm_setText(string controlName, string textValue)
        {
            var ascxControl = getAscx(controlName);
            if (ascxControl!=null && ascxControl is ascx_WorkingOnTask)
            {
                ascxControl.invokeOnThread(
                    () => ((ascx_WorkingOnTask)ascxControl).setWorkingTaskText(textValue));
            }
        }

        public static void workingOnTaskForm_close(string controlName)
        {
            var ascxControl = getAscx(controlName);
            if (ascxControl != null && ascxControl is ascx_WorkingOnTask)
            {
                ascxControl.invokeOnThread(
                    () => ((ascx_WorkingOnTask) ascxControl).close());
            }
        }

        public static void addDefaultControlsToMenu()
        {
            O2AscxGUI.addControlToMenu(typeof(ascx_Directory), O2DockState.Float, "Directory Viewer");
            O2AscxGUI.addControlToMenu(typeof(ascx_FileMappings), O2DockState.Float, "Files Mappings");
            O2AscxGUI.addControlToMenu(typeof(ascx_O2ObjectModel), O2DockState.Float, "O2 Object Model");
            O2AscxGUI.addControlToMenu(typeof(ascx_FindingsViewer), O2DockState.Float, "Findings Viewer");
        }
    }
}
