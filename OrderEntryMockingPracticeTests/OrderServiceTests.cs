using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
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

        private Mock<IProductRepository> _mockedProductRepo;

        private OrderService _orderService;

        [TestInitialize]
        public void SetupProductsAndOrders()
        {
            // Setup Mocked Interfaces
            _mockedProductRepo = new Mock<IProductRepository>();

            _orderService = new OrderService();

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
        public void NoExceptionIfOrderItemsHaveUniqueSku()
        {            
            // Act
            order.OrderItems.Add(order1);
            order.OrderItems.Add(order2); 

            try
            {
                _orderService.PlaceOrder(order);
            }
            catch(ArgumentException ae)
            {
                // Assert
                Assert.Fail("Expected no exception.");
            }
        }

        [TestMethod]
        public void ExceptionIfOrderItemsHaveDuplicateSku()
        {
            // Act
            order.OrderItems.Add(order1);
            order.OrderItems.Add(order2);
            order.OrderItems.Add(order3);

            try
            {
                _orderService.PlaceOrder(order);
                Assert.Fail("The expected exception was not thrown.");
            }
            catch (ArgumentException ae)
            {
                // Assert
                Assert.AreEqual(ae.Message, OrderService.DuplicateSkuError);
            }
        }

        [TestMethod]
        public void OrderProductsInStock()
        {
            // Arrange
            _mockedProductRepo.Setup(x => x.IsInStock(It.IsAny<string>())).Returns(true);
            var test = _mockedProductRepo.Object;

            // Act
            var isInStock = test.IsInStock(order1.Product.Sku);

            // Assert
            Assert.AreEqual(isInStock, true);
        }
    }
}
