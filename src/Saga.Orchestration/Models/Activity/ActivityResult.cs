namespace Saga.Orchestration.Models.Activity
{
    public class ActivityResult<T>
    {
        public bool Valid { get; set; } = true;
        public T Item { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
    }
}
