namespace Domain.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException() : base() {
            this.Status = 400;
        }

        public BadRequestException(string message) : base(message) {
            this.Status = 400;
        }
    }
}
