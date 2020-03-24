using OrderEntryMockingPractice.Models;
using System;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public const string DuplicateSkuError = "Duplicate SKUs in order.";
        public const string NoStockError = "One or more products not in stock.";

        private IProductRepository _productRepo;
        private IOrderFulfillmentService _fulfillmentService;

        public OrderService(
            IProductRepository productRepo,
            IOrderFulfillmentService fulfillmentService)
        {
            _productRepo = productRepo;
            _fulfillmentService = fulfillmentService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            // Check order properties
            if (order.HasNoDuplicateSku() == false)
            {
                throw new ArgumentException(DuplicateSkuError);
            }

            foreach(OrderItem items in order.OrderItems)
            {
                if(_productRepo.IsInStock(items.Product.Sku) == false)
                {
                    throw new ArgumentException(NoStockError);
                }
            }

            // All checks pass, get confirmation properties
            OrderConfirmation confirmation = _fulfillmentService.Fulfill(order);
            
            // Create new summary with required information filled in
            OrderSummary summary = new OrderSummary();
            summary.OrderId = confirmation.OrderId;

            return summary;
        }
    }
}
