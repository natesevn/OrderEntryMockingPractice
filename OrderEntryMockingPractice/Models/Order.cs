using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPractice.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderItems = new List<OrderItem>();
        }
        
        public int? CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public bool hasNoDuplicateSku()
        {
            int itemNo = OrderItems.Count();
            int uniqueItems = OrderItems.Select(item => item.Product.Sku).Distinct().Count();

            return (itemNo == uniqueItems);
        }
    }
}
