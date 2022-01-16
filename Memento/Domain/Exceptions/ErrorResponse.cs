using System.Collections.Generic;

namespace Memento.Models
{
    public class ErrorResponse
    {
        public ErrorResponse() {
            Data = new List<PropertyError>();
        }
        public string Message { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public IList<PropertyError> Data;
    }

    public class PropertyError {
        public PropertyError() { }

        public PropertyError(string property, string error) {
            Property = property;
            Error = error;
        }

        public string Property { get; set; }
        public string Error { get; set; }
    }
}
