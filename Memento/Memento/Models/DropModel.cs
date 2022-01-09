using Domain.Models;
using System;
using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class DropModel
    {
        public DropModel() {
            People = new List<PersonModelV2>();
            Date = DateTime.Now;
            TagIds = new List<long>();
            TimelineIds = new List<int>();
        }

        public string Information { get; set; }
        public DateTime? Date { get; set; }
        public DateTypes DateType { get; set; }
        public List<long> TagIds { get; set; }
        public List<PersonModelV2> People { get; set; }
        public List<int> TimelineIds { get; set; }
        public int PromptId { get; set; }
    }

    public class UpdateDropModel : DropModel {
        public UpdateDropModel() {
            Images = new List<int>();
            Movies = new List<int>();
        }
        public List<int> Images { get; set; }
        public List<int> Movies { get; set; }
    }
}