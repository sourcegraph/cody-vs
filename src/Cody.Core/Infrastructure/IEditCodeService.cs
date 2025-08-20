using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IEditCodeService
    {
        EditCodeResult ShowEditCodeDialog(IEnumerable<EditModel> models, string defaultModelId, string instruction);
    }

    public class EditModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
    }

    public class EditCodeResult
    {
        public string ModelId { get; set; }
        public string Instruction { get; set; }
    }
}
