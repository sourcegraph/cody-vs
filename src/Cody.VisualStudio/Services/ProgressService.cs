using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Cody.VisualStudio.Services
{
    public class ProgressService : IProgressService
    {
        private IVsTaskStatusCenterService statusCenterService;
        private Dictionary<string, ProgressState> states = new Dictionary<string, ProgressState>();

        public ProgressService(IVsTaskStatusCenterService statusCenterService)
        {
            this.statusCenterService = statusCenterService;
        }

        public void Start(string id, string title, Action actionOnUserCancel)
        {
            if (states.ContainsKey(id))
                throw new InvalidOperationException("Progress with the given id already exists.");

            var handlerOptions = new TaskHandlerOptions
            {
                ActionsAfterCompletion = CompletionActions.None,
                Title = title,
            };

            bool canBeCanceled = actionOnUserCancel != null;
            var progressData = new TaskProgressData
            {
                CanBeCanceled = canBeCanceled,
            };

            var handler = statusCenterService.PreRegister(handlerOptions, progressData);

            var tcs = new TaskCompletionSource<bool>();

            if (canBeCanceled)
            {
                handler.UserCancellation.Register(() =>
                {
                    actionOnUserCancel();
                    tcs.SetCanceled();
                    states.Remove(id);
                });
            }

            handler.RegisterTask(tcs.Task);

            var state = new ProgressState
            {
                TaskHandler = handler,
                TaskCompletionSource = tcs,
                CanBeCanceled = canBeCanceled
            };

            states.Add(id, state);
        }

        public void WriteTrace(object sender, TaskProgressData e)
        {
            Console.WriteLine("Trace !");
        }

        public void ReportProgress(string id, string message, int? increment)
        {
            if (states.TryGetValue(id, out var state))
            {

                if (increment.HasValue) state.PercentComplete = (state.PercentComplete ?? 0) + increment;
                else state.PercentComplete = null;

                //Calling Report() too often is throttled!
                Task.Delay(TimeSpan.FromMilliseconds(400)).ContinueWith(x =>
                {
                    var progressData = new TaskProgressData()
                    {
                        PercentComplete = state.PercentComplete,
                        ProgressText = message,
                        CanBeCanceled = state.CanBeCanceled
                    };

                    state.TaskHandler.Progress.Report(progressData);
                });
            }
        }

        public void End(string id)
        {
            if (states.TryGetValue(id, out var state))
            {
                state.TaskCompletionSource.SetResult(true);
                states.Remove(id);
            }
        }

        private class ProgressState
        {
            public ITaskHandler TaskHandler { get; set; }

            public TaskCompletionSource<bool> TaskCompletionSource { get; set; }

            public int? PercentComplete { get; set; }

            public bool CanBeCanceled { get; set; }
        }
    }
}
