using Cody.UI.MVVM;
using Newtonsoft.Json.Linq;
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
        private List<string> instructionsHistory;
        private int currentHistoryItem;

        public EditCodeViewModel(IEnumerable<Model> models, string selectedModelId, string instruction, List<string> instructionsHistory)
        {
            var collection = new ObservableCollection<Model>(models);
            var modelsSource = new CollectionViewSource();
            modelsSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SelectedModel.Provider)));
            modelsSource.Source = collection;

            Models = modelsSource;

            SelectedModel = collection.FirstOrDefault(x => x.Id == selectedModelId);

            this.instructionsHistory = instructionsHistory;
            currentHistoryItem = instructionsHistory.Count;
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
                currentHistoryItem = instructionsHistory.Count;
            }
        }

        public bool EditButtonIsEnabled => !string.IsNullOrWhiteSpace(instruction);

        private DelegateCommand historyUpCommand;
        public DelegateCommand HistoryUpCommand => historyUpCommand = historyUpCommand ?? new DelegateCommand(OnHistoryUp);

        private DelegateCommand historyDownCommand;
        public DelegateCommand HistoryDownCommand => historyDownCommand = historyDownCommand ?? new DelegateCommand(OnHistoryDown);

        private void OnHistoryUp()
        {
            if (currentHistoryItem - 1 >= 0)
            {
                currentHistoryItem--;
                SetProperty(ref instruction, instructionsHistory[currentHistoryItem], nameof(Instruction));
                OnNotifyPropertyChanged(nameof(EditButtonIsEnabled));
            }
        }

        private void OnHistoryDown()
        {
            if (currentHistoryItem + 1 <= instructionsHistory.Count)
            {
                currentHistoryItem++;
                var inst = string.Empty;
                if (currentHistoryItem != instructionsHistory.Count) inst = instructionsHistory[currentHistoryItem];

                SetProperty(ref instruction, inst, nameof(Instruction));
                OnNotifyPropertyChanged(nameof(EditButtonIsEnabled));
            }
        }
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
