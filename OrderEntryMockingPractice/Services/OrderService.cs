using OrderEntryMockingPractice.Models;
using System;
using System.Collections.Generic;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public const string DuplicateSkuError = "Duplicate SKUs in order.";
        public const string NoStockError = "One or more products not in stock.";
        public const string NoCustomerIdError = "Customer ID not supplied.";
        public const string CustomerNotFoundError = "Customer not found.";

        private IProductRepository _productRepo;
        private IOrderFulfillmentService _fulfillmentService;
        private ICustomerRepository _customerService;
        private ITaxRateService _taxService;

        public OrderService(
            IProductRepository productRepo,
            IOrderFulfillmentService fulfillmentService,
            ICustomerRepository customerService,
            ITaxRateService taxService)
        {
            _productRepo = productRepo;
            _fulfillmentService = fulfillmentService;
            _customerService = customerService;
            _taxService = taxService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            // Check duplicate SKU
            if (order.HasNoDuplicateSku() == false)
            {
                throw new ArgumentException(DuplicateSkuError);
            }
            
            // Get customer information
            int customerId = order.CustomerId ?? -1;
            if (customerId == -1) throw new ArgumentException(NoCustomerIdError);
            Customer customer = _customerService.Get(customerId);

            // Get tax rate
            IEnumerable<TaxEntry> tax = _taxService.GetTaxEntries(customer.PostalCode, customer.Country);
            decimal totalTax = 0;
            foreach(TaxEntry entry in tax)
            {
                totalTax += entry.Rate;
            }

            // Check product stock and calculate totals
            decimal netTotal = 0;
            decimal total = 0;
            foreach(OrderItem items in order.OrderItems)
            {
                if(_productRepo.IsInStock(items.Product.Sku) == false)
                {
                    throw new ArgumentException(NoStockError);
                }

                netTotal += items.Product.Price * items.Quantity;
            }
            total = totalTax / 100 * netTotal;

            // All checks pass, get confirmation properties
            OrderConfirmation confirmation = _fulfillmentService.Fulfill(order);

            // Create new summary with required information filled in
            OrderSummary summary = new OrderSummary();

            summary.OrderId = confirmation.OrderId;
            summary.OrderNumber = confirmation.OrderNumber;
            summary.NetTotal = netTotal;
            summary.Total = total;

           
            return summary;
        }
    }
}
