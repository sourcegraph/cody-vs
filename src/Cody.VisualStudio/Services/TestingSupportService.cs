using EnvDTE;
using Microsoft.VisualStudio.TaskStatusCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class TestingSupportService
    {
        private PropertyInfo inProgressCountProp;
        private IVsTaskStatusCenterService statusCenterService;


        public TestingSupportService(IVsTaskStatusCenterService statusCenterService)
        {
            this.statusCenterService = statusCenterService;
            inProgressCountProp = statusCenterService.GetType().GetProperty("InProgressCount");
        }

        public int InProgressBackgroundTasksCount => (int)inProgressCountProp.GetValue(statusCenterService);

        public string LastDisplayedAutocompleteSuggestion { get; set; }

    }
}
