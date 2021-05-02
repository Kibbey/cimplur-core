using System.Collections.Generic;

namespace Domain.Models
{
    public class PromptAskedModel
    {
        public PromptAskedModel () {
            Askeds = new List<Asked>();
        }
        public int PromptId { get; set; }
        public string Question { get; set; }
        public bool Custom { get; set; }
        public IEnumerable<Asked> Askeds { get; set; }
    }
}
