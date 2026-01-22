namespace GitAnomalyDetector.Utils
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static Result SuccessResult(string message = "Operation completed successfully.")
        {
            return new Result 
            { 
                Success = true, 
                Message = message
            };
        }

        public static Result FailureResult(string message = "Operation failed.")
        {
            return new Result { Success = false, Message = message };
        }
    }
}
