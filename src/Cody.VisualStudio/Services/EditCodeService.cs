using Cody.Core.Infrastructure;
using Cody.UI.ViewModels;
using Cody.UI.Views;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class EditCodeService : IEditCodeService
    {
        private readonly IVsFolderStoreService vsFolderStoreService;
        private List<string> instructionsHistory;

        private const string InstructionsHistoryFile = "Cody.EditCode.History.json";
        private const int MaxHistoryItems = 40;

        public EditCodeService(IVsFolderStoreService vsFolderStoreService)
        {
            this.vsFolderStoreService = vsFolderStoreService;
        }

        public EditCodeResult ShowEditCodeDialog(IEnumerable<EditModel> models, string defaultModelId, string instruction)
        {
            if (instructionsHistory == null)
            {
                var historyFromFile = vsFolderStoreService.LoadData<List<string>>(InstructionsHistoryFile);
                instructionsHistory = historyFromFile ?? new List<string>();
            }

            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var vm = new EditCodeViewModel(models.Select(x => new Model { Id = x.Id, Name = x.Name, Provider = x.Provider }),
                    defaultModelId, instruction, instructionsHistory);
                var window = new EditCodeView();

                window.DataContext = vm;
                if (window.ShowModal() == true)
                {
                    if (!string.IsNullOrWhiteSpace(vm.Instruction) && instructionsHistory.LastOrDefault() != vm.Instruction)
                    {
                        SaveInstructionInHistory(vm.Instruction);
                    }

                    return new EditCodeResult { Instruction = vm.Instruction, ModelId = vm.SelectedModel.Id };
                }

                return null;
            });

            return result;
        }

        private void SaveInstructionInHistory(string instruction)
        {
            instructionsHistory.Add(instruction);
            var itemsToRemove = instructionsHistory.Count - MaxHistoryItems;
            if (itemsToRemove > 0) instructionsHistory.RemoveRange(0, itemsToRemove);

            vsFolderStoreService.SaveData(InstructionsHistoryFile, instructionsHistory);
        }
    }


}
