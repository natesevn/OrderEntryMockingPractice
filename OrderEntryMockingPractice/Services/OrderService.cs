using OrderEntryMockingPractice.Models;
using System;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public const string DuplicateSkuError = "Duplicate SKUs in order.";
        public const string NoStockError = "One or more products not in stock.";

        private IProductRepository _productRepo;

        public OrderService(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            // Validate order properties
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

            

            return null;
        }
    }
}
