using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class EditTaskNotificationHandlers
    {
        private readonly ILog logger;
        private readonly IEditCodeService editCodeService;
        private readonly IDocumentService documentService;
        private readonly IStatusbarService statusbarService;

        public EditTaskNotificationHandlers(ILog logger, IEditCodeService editCodeService,
            IDocumentService documentService, IStatusbarService statusbarService)
        {
            this.logger = logger;
            this.editCodeService = editCodeService;
            this.documentService = documentService;
            this.statusbarService = statusbarService;
        }

        [AgentCallback("textEditor/selection")]
        public void Selection(string uri, Range selection)
        {
            // no needed yet
            //var path = uri.ToWindowsPath();
            //documentService.SelectInDocument(path, selection);
        }

        [AgentCallback("editTask/getUserInput", deserializeToSingleObject: true)]
        public UserEditPromptResult GetUserInput(UserEditPromptRequest request)
        {
            try
            {
                var models = request.AvailableModels
                    .Where(x => x.IsModelAvailable)
                    .Select(x => new EditModel { Id = x.Model.Id, Name = x.Model.Title, Provider = x.Model.Provider });

                var result = editCodeService.ShowEditCodeDialog(models, request.SelectedModelId, request.Instruction);

                if (result != null)
                {
                    statusbarService.StartProgressAnimation();
                    return new UserEditPromptResult { Instruction = result.Instruction, SelectedModelId = result.ModelId };
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.Error("User input failed.", ex);
            }

            return null;
        }
    }
}
