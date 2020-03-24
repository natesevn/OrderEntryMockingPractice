using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPracticeTests
{
    [TestClass]
    public class OrderServiceTests
    {
        // SKU constants
        private const string SkuUniqueOne = "sku1";
        private const string SkuUniqueTwo = "sku2";
        // Signifies product has duplicate SKU 
        private const string SkuDuplicateOne = SkuUniqueOne;
        // Signifies product is out of stock
        private const string SkuOutOfStock = "outStock";

        private const int customerId = 777;
        
        // Mocked interfaces
        private Mock<IProductRepository> _mockedProductRepo;
        private Mock<IOrderFulfillmentService> _mockedFulfillmentService;
        
        // Services
        private OrderService _orderService;

        private Product CreateProduct(string name, string sku)
        {
            return new Product
            {
                Name = name,
                Sku = sku
            };
        }

        private OrderItem CreateOrderItem(Product product, int quantity = 0)
        {
            return new OrderItem
            {
                Product = product,
                Quantity = quantity
            };
        }

        private Order CreateOrder(
            int customerId,
            string skuOne, string skuTwo,
            string productNameOne = "prodOne", string productNameTwo = "prodTwo")
        {
            Product product1 = CreateProduct(productNameOne, skuOne);
            Product product2 = CreateProduct(productNameTwo, skuTwo);

            OrderItem item1 = CreateOrderItem(product1);
            OrderItem item2 = CreateOrderItem(product2);

            Order order = new Order();
            order.OrderItems = new List<OrderItem>
            {
                item1,
                item2 
            };

            return order;        
        }

        [TestInitialize]
        public void SetupProductsAndOrders()
        {
            Random rand = new Random();
            
            // Setup Mocked Interfaces

            _mockedProductRepo = new Mock<IProductRepository>();
            // Mocked product stock check returns true by default
            _mockedProductRepo.Setup(x => x.IsInStock(It.IsAny<string>())).Returns(true);
            // Mocked product stock check returns false if passed SkuOutOfStock
            _mockedProductRepo.Setup(x => x.IsInStock(SkuOutOfStock)).Returns(false);
            
            _mockedFulfillmentService = new Mock<IOrderFulfillmentService>();
            // Mocked fulfillment service copies customer id from passed in order
            _mockedFulfillmentService.Setup(x => x.Fulfill(It.IsAny<Order>()))
                .Returns((Order a) => new OrderConfirmation
                {
                    OrderId = rand.Next(),
                    CustomerId = a.CustomerId ?? rand.Next()
                });
               

            _orderService = new OrderService(_mockedProductRepo.Object, _mockedFulfillmentService.Object);
        }
       
        [TestMethod]
        public void NoExceptionIfOrderItemsHaveUniqueSkuAndInStock()
        {
            Order order = CreateOrder(customerId, SkuUniqueOne, SkuUniqueTwo);
            
            try
            {
                _orderService.PlaceOrder(order);
            }
            catch(ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void ExceptionIfOrderItemsHaveDuplicateSku()
        {
            Order order = CreateOrder(customerId, SkuUniqueOne, SkuDuplicateOne);

            try
            {
                _orderService.PlaceOrder(order);
                Assert.Fail("The expected exception was not thrown.");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ae.Message, OrderService.DuplicateSkuError);
            }
        }

        [TestMethod]
        public void ExceptionIfProductsNotInStock()
        {
            Order order = CreateOrder(customerId, SkuOutOfStock, SkuUniqueOne);

            try
            {
                _orderService.PlaceOrder(order);
                Assert.Fail("The expected exception was not thrown.");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ae.Message, OrderService.NoStockError);
            }
        }

    }
}
