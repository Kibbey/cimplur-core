using Memento.Models;
using System;
using System.Collections.Generic;

namespace Domain.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException() : base() {
            PropertyErrors = new List<PropertyError>();
        }
        public BaseException(string message) : base(message) {
            PropertyErrors = new List<PropertyError>();
        }
        public IList<PropertyError> PropertyErrors { get; set; }
        public int Status { get; set; }
    }
}
