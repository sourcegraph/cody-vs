using Cody.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.UI.ViewModels
{
    public class EditCodeViewModel : NotifyPropertyChangedBase
    {
        public EditCodeViewModel(IEnumerable<Model> models, string selectedModelId, string instruction)
        {
            Models = new ObservableCollection<Model>(models);
            SelectedModel = Models.FirstOrDefault(x => x.Id == selectedModelId);
            Instruction = instruction;
        }

        public ObservableCollection<Model> Models { get; set; }

        private Model selectedModel;
        public Model SelectedModel
        {
            get => selectedModel;
            set => SetProperty(ref selectedModel, value);
        }

        private string instruction;
        public string Instruction
        {
            get => instruction;
            set
            {
                SetProperty(ref instruction, value);
                OnNotifyPropertyChanged(nameof(EditButtonIsEnabled));
            }
        }

        public bool EditButtonIsEnabled => !string.IsNullOrWhiteSpace(instruction);
    }

    public class Model
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public interface IEditCodeWindow
    {
        void CloseWindow();
    }
}
