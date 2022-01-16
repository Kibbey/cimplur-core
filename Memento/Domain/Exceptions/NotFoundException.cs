using System;

namespace Domain.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException() : base() {
            this.Status = 404;
        }

        public NotFoundException(string message) : base(message) {
            this.Status = 404;
        }
    }
}
