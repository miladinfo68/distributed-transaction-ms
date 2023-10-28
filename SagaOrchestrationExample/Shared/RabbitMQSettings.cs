namespace Shared
{
    public static class RabbitMQSettings
    {
        public const string Order_Orchestrator_Queue = "order-orchestrator-queue";

        public const string Check_Order_Items_Stock_Queue = "check-order-items-stock-queue";
        public const string Payment_Queue = "payment-queue";
        public const string Payment_Fail_Queue = "payment-fail-queue";
        public const string Order_Complete_Queue = "order-complete-queue";
        public const string Order_Fail_Queue = "order-fail-queue";
        public const string Rollback_Order_Items_Stock_Queue = "rollback_order_items_stock_queue";
    }
}
