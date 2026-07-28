using AppDrugsV2.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AppDrugsV2.UnitTests.Domain
{
    [TestFixture]
    public class InventoryTests
    {
        [Test]
        public void Constructor_WithValidArguments_ShouldCreateInventory()
        {

            int drugId = 1;
            int gestorId = 2;
            int quantity = 100;

            var inventory = new Inventory(drugId, gestorId, quantity);

            inventory.DrugId.Should().Be(drugId);
            inventory.GestorFarmaceuticoId.Should().Be(gestorId);
            inventory.Quantity.Should().Be(quantity);
            inventory.IsActive.Should().BeTrue();
            inventory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [TestCase(-1)]
        public void Constructor_WithNegativeQuantity_ShouldThrowArgumentException(int negativeQuantity)
        {
            Action act = () => new Inventory(1, 2, negativeQuantity);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*cantidad*");
        }

        [Test]
        public void AddStock_WithValidQuantity_ShouldIncreaseStock()
        {
            var inventory = new Inventory(1, 2, 50);

            inventory.AddStock(20);

            inventory.Quantity.Should().Be(70);
            inventory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void RemoveStock_WithValidQuantity_ShouldDecreaseStock()
        {
            var inventory = new Inventory(1, 2, 50);

            inventory.RemoveStock(20);

            inventory.Quantity.Should().Be(30);
            inventory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void RemoveStock_WithQuantityGreaterThanAvailable_ShouldThrowInvalidOperationException()
        {
            var inventory = new Inventory(1, 2, 50);

            Action act = () => inventory.RemoveStock(60);


            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No hay suficiente stock disponible.");
        }
    }
}
