using EnvDTE;
using Microsoft.VisualStudio.TaskStatusCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class TestingSupportService
    {
        private PropertyInfo inProgressCountProp;
        private IVsTaskStatusCenterService statusCenterService;

        private SemaphoreSlim suggestionSemaphore = new SemaphoreSlim(0, 1);
        private AutocompleteSuggestion autocompleteSuggestion;

        public TestingSupportService(IVsTaskStatusCenterService statusCenterService)
        {
            this.statusCenterService = statusCenterService;
            inProgressCountProp = statusCenterService.GetType().GetProperty("InProgressCount");
        }

        public int InProgressBackgroundTasksCount => (int)inProgressCountProp.GetValue(statusCenterService);

        public void SetAutocompleteSuggestion(string suggestionId, bool isCodySuggestion, string suggestionText)
        {
            autocompleteSuggestion = new AutocompleteSuggestion(suggestionId, isCodySuggestion, suggestionText);
            if (suggestionSemaphore.CurrentCount == 0) suggestionSemaphore.Release();
        }

        public async Task<AutocompleteSuggestion> GetAutocompleteSuggestion(int milisecondsTimeout = 8000)
        {
            if (await suggestionSemaphore.WaitAsync(milisecondsTimeout))
                return autocompleteSuggestion;
            else
                return null;
        }


        public class AutocompleteSuggestion
        {
            public AutocompleteSuggestion(string suggestionId, bool isCodySuggestion, string suggestionText)
            {
                SuggestionId = suggestionId;
                IsCodySuggestion = isCodySuggestion;
                SuggestionText = suggestionText;
            }

            public string SuggestionId { get; }
            public bool IsCodySuggestion { get; }
            public string SuggestionText { get; }
        }
    }
}
