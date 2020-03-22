using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderEntryMockingPractice.Models;
using System;
using System.Linq;

namespace OrderEntryMockingPracticeTests
{
    [TestClass]
    public class OrderServiceTests
    {
        private Order order;

        private OrderItem order1;
        private OrderItem order2;
        private OrderItem order3;

        private Product product1;
        private Product product2;
        private Product product3;

        [TestInitialize]
        public void SetupProductsAndOrders()
        {
            order = new Order();

            product1 = new Product
            {
                Sku = "sku1"
            };
            order1 = new OrderItem
            {
                Product = product1
            };

            product2 = new Product
            {
                Sku = "sku2"
            };
            order2 = new OrderItem
            {
                Product = product2
            };

            product3 = new Product
            {
                Sku = "sku1"
            };
            order3 = new OrderItem
            {
                Product = product3
            };
        }

        [TestMethod]
        public void OrderItemsHaveUniqueSku()
        {            
            // Act
            order.OrderItems.Add(order1);
            order.OrderItems.Add(order2);

            bool hasNoDuplicates = order.hasNoDuplicateSku();

            // Assert
            Assert.AreEqual(hasNoDuplicates, true, "Products with different SKUs considered same.");
        }

        [TestMethod]
        public void OrderItemsHaveDuplicateSku()
        {
            // Act
            order.OrderItems.Add(order1);
            order.OrderItems.Add(order3);

            bool hasNoDuplicates = order.hasNoDuplicateSku();

            // Assert
            Assert.AreEqual(hasNoDuplicates, false, "Products with same SKUs considered different.");
        }
    }
}
