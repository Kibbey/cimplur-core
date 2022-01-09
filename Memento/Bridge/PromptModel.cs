using System.Collections.Generic;

namespace Bridge
{
    public class PromptModel
    {
        public PromptModel() {
            Askers = new List<PersonModelV2>();
        }

        public string Question { get; set; }
        public int PromptId { get; set; }
        public int Order { get; set; }
        public bool Selected { get; set; }
        public IEnumerable<PersonModelV2> Askers { get; set; }
    }
}
