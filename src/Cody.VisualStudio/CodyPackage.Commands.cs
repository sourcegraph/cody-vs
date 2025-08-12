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
        private readonly Dictionary<int, CodyCommand> codyCommands = new Dictionary<int, CodyCommand>();

        private void InitOleMenu()
        {
            AddCommand(CommandIds.CodyToolWindow, null, ShowToolWindow, true, false);
            AddCommand(CommandIds.EditCodeCommandId, null, ShowEditCodeWindow, false, true);
            AddCommand(CommandIds.DocumentCodeCommandId, "cody.command.document-code", InvokeCodyCommand, false, true);
            AddCommand(CommandIds.GenerateUnitTestsCommandId, "cody.command.unit-tests", InvokeCodyCommand, false, true);
            AddCommand(CommandIds.ExplainCodeCommandId, "cody.command.explain-code", InvokeCodyCommand, false, true);
            AddCommand(CommandIds.FindCodeSmellsCommandId, "cody.command.smell-code", InvokeCodyCommand, false, true);
        }

        private void AddCommand(int commandId, string commandName, EventHandler handler, bool enabled, bool isContextMenuItem)
        {
            try
            {
                var command = new CommandID(Guids.CodyPackageCommandSet, commandId);
                var menuCommand = new MenuCommand(handler, command);
                menuCommand.Enabled = enabled;

                codyCommands[commandId] = new CodyCommand
                {
                    MenuCommand = menuCommand,
                    CommandName = commandName,
                    IsContextMenuItem = isContextMenuItem
                };

                OleMenuService.AddCommand(menuCommand);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add command {commandName}", ex);
            }
        }

        public void EnableContextMenu(bool enabled)
        {
            codyCommands
                .Where(x => x.Value.IsContextMenuItem)
                .ToList()
                .ForEach(x => x.Value.MenuCommand.Enabled = enabled);
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

                if (codyCommands.TryGetValue(commandId, out var command))
                {
                    if (commandId == CommandIds.ExplainCodeCommandId ||
                        commandId == CommandIds.FindCodeSmellsCommandId ||
                        commandId == CommandIds.GenerateUnitTestsCommandId)
                    {
                        Logger.Debug($"Showing the chat window for the {command} command");
                        await ShowToolWindowAsync();
                    }
                    else
                    {
                        StatusbarService?.StartProgressAnimation();
                    }

                    if (AgentClient != null)
                    {
                        Logger.Info($"Invoking command: {command}");
                        await AgentService.CommandExecute(new ExecuteCommandParams
                        {
                            Command = command.CommandName
                        });
                    }
                    else Logger.Warn($"AgentClient not jet initialized. Can't invoke command: {command}");
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

        public async void ShowEditCodeWindow(object sender, EventArgs eventArgs)
        {
            var taskId = await AgentService.EditTaskStart();
        }

        public class CodyCommand
        {
            public MenuCommand MenuCommand { get; set; }
            public string CommandName { get; set; }
            public bool IsContextMenuItem { get; set; }
        }
    }
}
