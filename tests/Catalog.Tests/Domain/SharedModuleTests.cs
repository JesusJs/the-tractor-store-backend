using System;
using System.Collections.Generic;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;
using Xunit;

namespace TractorEcommerce.Modules.Catalog.Tests.Domain
{
    public class SharedModuleTests
    {
        [Fact]
        public void CheckoutRequestedEvent_Properties_ShouldBeAssignable()
        {
            var cartId = "cart-1";
            var customerId = "customer-1";
            var items = new List<CartItemDto>
            {
                new CartItemDto("SKU-1", 2, 100)
            };
            var total = 200m;

            var e = new CheckoutRequestedEvent(cartId, customerId, items, total);

            Assert.Equal(cartId, e.CartId);
            Assert.Equal(customerId, e.CustomerId);
            Assert.Equal(items, e.Items);
            Assert.Equal(total, e.TotalAmount);
        }

        [Fact]
        public void OrderPlacedEvent_Properties_ShouldBeAssignable()
        {
            var orderId = Guid.NewGuid();
            var customerId = "customer-1";
            var items = new List<CartItemDto>
            {
                new CartItemDto("SKU-1", 2, 100)
            };
            var placedAt = DateTime.UtcNow;

            var e = new OrderPlacedEvent(orderId, customerId, items, placedAt);

            Assert.Equal(orderId, e.OrderId);
            Assert.Equal(customerId, e.CustomerId);
            Assert.Equal(items, e.Items);
            Assert.Equal(placedAt, e.PlacedAt);
        }

        [Fact]
        public void DomainConflictException_Constructor_SetsMessage()
        {
            var msg = "Conflict occurred";
            var ex = new DomainConflictException(msg);

            Assert.Equal(msg, ex.Message);
        }

        [Fact]
        public void DomainValidationException_Constructor_SetsMessageAndDetails()
        {
            var msg = "Validation failed";
            var details = new { Error = "Invalid format" };
            var ex = new DomainValidationException(msg, details);

            Assert.Equal(msg, ex.Message);
            Assert.Same(details, ex.Details);
        }
    }
}
