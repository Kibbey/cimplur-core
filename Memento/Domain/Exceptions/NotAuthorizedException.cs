using System;

namespace Domain.Exceptions
{
    public class NotAuthorizedException : BaseException
    {
        public NotAuthorizedException() : base() {
            this.Status = 403;
        }
        public NotAuthorizedException(string message) : base(message) {
            this.Status = 403;
        }
    }
}
