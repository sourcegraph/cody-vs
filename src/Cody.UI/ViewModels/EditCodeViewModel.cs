using Cody.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Cody.UI.ViewModels
{
    public class EditCodeViewModel : NotifyPropertyChangedBase
    {
        public EditCodeViewModel(IEnumerable<Model> models, string selectedModelId, string instruction)
        {
            var collection = new ObservableCollection<Model>(models);
            var modelsSource = new CollectionViewSource();
            modelsSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SelectedModel.Provider)));
            modelsSource.Source = collection;

            Models = modelsSource;

            SelectedModel = collection.FirstOrDefault(x => x.Id == selectedModelId);
            Instruction = instruction;
        }

        public CollectionViewSource Models { get; set; }

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
        public string Provider { get; set; }
    }

    public interface IEditCodeWindow
    {
        void CloseWindow();
    }
}
