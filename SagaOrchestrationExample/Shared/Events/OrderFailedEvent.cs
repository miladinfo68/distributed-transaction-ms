namespace Shared.Events
{
    public class OrderFailedEvent
    {
        public int OrderId { get; set; }
        public string Message { get; set; }
    }
}
