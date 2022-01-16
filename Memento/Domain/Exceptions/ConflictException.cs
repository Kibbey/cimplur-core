using System;

namespace Domain.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException() : base() {
            this.Status = 409;
        }

        public ConflictException(string message) : base(message) {
            this.Status = 409;
        }
    }
}
