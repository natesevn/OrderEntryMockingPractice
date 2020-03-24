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

        private const int CustomerId = 1;
        private const string PostalCode = "123";
        private const string Country = "USA";

        private const int FulfillmentOrderId = 2;
        private const string FulfillmentConfirmationNumber = "abc";

        private TaxEntry UsTax = new TaxEntry
        {
            Description = "federal",
            Rate = 15
        };
        
        // Mocked interfaces
        private Mock<IProductRepository> _mockedProductRepo;
        private Mock<IOrderFulfillmentService> _mockedFulfillmentService;
        private Mock<ICustomerRepository> _mockedCustomerRepo;
        private Mock<ITaxRateService> _mockedTaxService;
        
        // Services
        private OrderService _orderService;

        private Product CreateProduct(string sku, decimal price)
        {
            return new Product
            {
                Sku = sku,
                Price = price
            };
        }

        private OrderItem CreateOrderItem(Product product, decimal quantity)
        {
            return new OrderItem
            {
                Product = product,
                Quantity = quantity
            };
        }

        /*
         * Create full order given customer id and 2 products 
         * Price and quantity optional for testing purposes
         */
        private Order CreateOrder(
            int customerId,
            string skuOne, string skuTwo,
            decimal priceOne = 0, decimal priceTwo = 0,
            decimal quantityOne = 0, decimal quantityTwo = 0)
        {
            Product product1 = CreateProduct(skuOne, priceOne);
            Product product2 = CreateProduct(skuTwo, priceTwo);

            OrderItem item1 = CreateOrderItem(product1, quantityOne);
            OrderItem item2 = CreateOrderItem(product2, quantityTwo);

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
                    OrderNumber = FulfillmentConfirmationNumber,
                    OrderId = FulfillmentOrderId, 
                    CustomerId = a.CustomerId ?? CustomerId
                });

            _mockedCustomerRepo = new Mock<ICustomerRepository>();
            _mockedCustomerRepo.Setup(x => x.Get(It.Is<int>(id => id == CustomerId)))
                .Returns((int id) => new Customer
                {
                    CustomerId = id,
                    PostalCode = PostalCode,
                    Country = Country
                });

            _mockedTaxService = new Mock<ITaxRateService>();
            _mockedTaxService.Setup(x => x.GetTaxEntries(It.Is<string>(pc => pc == PostalCode), It.Is<string>(country => country == Country)))
                .Returns(new List<TaxEntry>
                {
                    UsTax
                });

            _orderService = new OrderService(_mockedProductRepo.Object, _mockedFulfillmentService.Object);
        }
       
        [TestMethod]
        public void ValidOrderPassedToFulfillmentService()
        {
            Order order = CreateOrder(CustomerId, SkuUniqueOne, SkuUniqueTwo);
            
            try
            {
                _orderService.PlaceOrder(order);
                _mockedFulfillmentService.Verify(x => x.Fulfill(order), Times.Once());
            }
            catch(ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void OrderSummaryHasFulfillmentConfirmationNumber()
        {
            Order order = CreateOrder(CustomerId, SkuUniqueOne, SkuUniqueTwo);

            try
            {
                OrderSummary summary = _orderService.PlaceOrder(order);
                Assert.AreEqual(FulfillmentConfirmationNumber, summary.OrderNumber, "Order number in summary differs from number generated by fulfillment service.");

            }
            catch (ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void OrderSummaryHasFulfillmentOrderID()
        {
            Order order = CreateOrder(CustomerId, SkuUniqueOne, SkuUniqueTwo);

            try
            {
                OrderSummary summary = _orderService.PlaceOrder(order);
                Assert.AreEqual(FulfillmentOrderId, summary.OrderId, "Order ID in summary differs from ID generated by fulfillment service.");
            }
            catch (ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void CorrectNetTotalCalculation()
        {
            Order order = CreateOrder(CustomerId,
                SkuUniqueOne, SkuUniqueTwo,
                10, 20, 2, 1);

            decimal expectedNetTotal = (10 * 2) + (20 * 1);

            try
            {
                OrderSummary summary = _orderService.PlaceOrder(order);
                Assert.AreEqual(expectedNetTotal, summary.NetTotal, "Net total not calculated properly");
            }
            catch (ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void CorrectOrderTotalCalculation()
        {
            Order order = CreateOrder(CustomerId,
                SkuUniqueOne, SkuUniqueTwo,
                10, 20, 2, 1);

            decimal expectedNetTotal = (10 * 2) + (20 * 1);
            decimal expectedOrderTotal = UsTax.Rate / 100 * expectedNetTotal;

            try
            {
                OrderSummary summary = _orderService.PlaceOrder(order);
                Assert.AreEqual(expectedOrderTotal, summary.Total, "Order total not calculated properly");
            }
            catch (ArgumentException ae)
            {
                Assert.Fail("Expected no exception but received: " + ae.Message);
            }
        }

        [TestMethod]
        public void ExceptionIfOrderItemsHaveDuplicateSku()
        {
            Order order = CreateOrder(CustomerId, SkuUniqueOne, SkuDuplicateOne);

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
            Order order = CreateOrder(CustomerId, SkuOutOfStock, SkuUniqueOne);

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
