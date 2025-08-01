using Cody.Core.Agent.Protocol;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio
{
    public partial class CodyPackage
    {
        private readonly Dictionary<int, string> codyCommandMap = new Dictionary<int, string>()
        {
            [CommandIds.DocumentCodeCommandId] = "cody.command.document-code",
            [CommandIds.GenerateUnitTestsCommandId] = "cody.command.unit-test",
            [CommandIds.ExplainCodeCommandId] = "cody.command.explain-code",
            [CommandIds.FindCodeSmellsCommandId] = "cody.command.smell-code",
        };

        private async Task InitOleMenu()
        {
            try
            {
                if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService oleMenuService)
                {
                    var commandId = new CommandID(Guids.CodyPackageCommandSet, CommandIds.CodyToolWindow);
                    oleMenuService.AddCommand(new MenuCommand(ShowToolWindow, commandId));


                    var documentCodeCmdId = new CommandID(Guids.CodyPackageCommandSet, CommandIds.DocumentCodeCommandId);
                    oleMenuService.AddCommand(new MenuCommand(InvokeCodyCommand, documentCodeCmdId));

                    var generateUnitTestsCmdId = new CommandID(Guids.CodyPackageCommandSet, CommandIds.GenerateUnitTestsCommandId);
                    oleMenuService.AddCommand(new MenuCommand(InvokeCodyCommand, generateUnitTestsCmdId));

                    var explainCodeCmdId = new CommandID(Guids.CodyPackageCommandSet, CommandIds.ExplainCodeCommandId);
                    oleMenuService.AddCommand(new MenuCommand(InvokeCodyCommand, explainCodeCmdId));

                    var findCodeSmellsCmdId = new CommandID(Guids.CodyPackageCommandSet, CommandIds.FindCodeSmellsCommandId);
                    oleMenuService.AddCommand(new MenuCommand(InvokeCodyCommand, findCodeSmellsCmdId));
                }
                else
                {
                    throw new NotSupportedException($"Cannot get {nameof(OleMenuCommandService)}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error("Cannot initialize menu items", ex);
            }
        }

        public async void InvokeCodyCommand(object sender, EventArgs eventArgs)
        {
            try
            {
                var menuCommand = sender as MenuCommand;
                if (menuCommand == null)
                {
                    Logger.Error("Cannot get MenuCommand instance");
                    return;
                }

                var commandId = menuCommand.CommandID.ID;

                if (codyCommandMap.TryGetValue(commandId, out string command))
                {
                    if (commandId == CommandIds.ExplainCodeCommandId ||
                       commandId == CommandIds.FindCodeSmellsCommandId)
                    {
                        Logger.Debug($"Showing the chat window for the {command} command");
                        await ShowToolWindowAsync();
                    }

                    Logger.Info($"Invoking command: {command}");
                    await AgentService.CommandExecute(new ExecuteCommandParams
                    {
                        Command = command
                    });
                }
                else Logger.Error($"Cant find command for id: {commandId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to invoke command", ex);
            }

        }

        public async void ShowToolWindow(object sender, EventArgs eventArgs) => await ShowToolWindowAsync();

        public async Task ShowToolWindowAsync()
        {
            try
            {
                Logger.Debug("Showing Tool Window ...");
                var window = await ShowToolWindowAsync(typeof(CodyToolWindow), 0, true, DisposalToken);
                if (window?.Frame is IVsWindowFrame windowFrame)
                {
                    bool isVisible = windowFrame.IsVisible() == 0;
                    bool isOnScreen = windowFrame.IsOnScreen(out int screenTmp) == 0 && screenTmp == 1;

                    Logger.Debug($"IsVisible:{isVisible} IsOnScreen:{isOnScreen}");

                    if (!isVisible || !isOnScreen)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                        Logger.Debug("Shown.");

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot toggle Tool Window.", ex);
            }
        }
    }
}
