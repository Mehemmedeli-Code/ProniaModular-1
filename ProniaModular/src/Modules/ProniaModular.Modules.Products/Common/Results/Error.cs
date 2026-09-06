namespace ProniaModular.Modules.Products.Common.Results
{
    public enum ErrorType
    {
        Validation,
        NotFound,
        Conflict
    }

    public sealed record Error(string Message, ErrorType Type)
    {
        public static Error Validation(string message) => new(message, ErrorType.Validation);
        public static Error NotFound(string message) => new(message, ErrorType.NotFound);
        public static Error Conflict(string message) => new(message, ErrorType.Conflict);
    }
}
