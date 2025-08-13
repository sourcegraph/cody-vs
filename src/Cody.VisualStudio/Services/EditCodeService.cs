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
        public EditCodeResult ShowEditCodeDialog(IEnumerable<EditModel> models, string defaultModelId, string instruction)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var vm = new EditCodeViewModel(models.Select(x => new Model { Id = x.Id, Name = x.Name, Provider = x.Provider }),
                    defaultModelId, instruction);
                var window = new EditCodeView();

                window.DataContext = vm;
                if (window.ShowModal() == true)
                {
                    return new EditCodeResult { Instruction = vm.Instruction, ModelId = vm.SelectedModel.Id };
                }

                return null;
            });

            return result;
        }
    }


}
