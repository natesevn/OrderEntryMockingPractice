using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public const string DuplicateSkuError = "Duplicate SKUs in order.";
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
                throw new System.ArgumentException(DuplicateSkuError);
            }


            

            return null;
        }
    }
}
