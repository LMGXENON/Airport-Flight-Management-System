using NUnit.Framework;
using AFMS.Models;

namespace AFMS.Tests
{
    [TestFixture]
    public class FlightAVLTreeComplexTests
    {
        private FlightAVLTree _avl;

        [SetUp]
        public void Setup()
        {
            _avl = new FlightAVLTree();
        }

        [Test]
        public void AVLTree_BalancingAfterInserts_ShouldMaintainBalance()
        {
            for (int i = 1; i <= 15; i++)
            {
                var flight = new Flight 
                { 
                    FlightNumber = $"FL{i:D3}", 
                    Airline = "TestAirline", 
                    Destination = "TestCity" 
                };
                _avl.Insert(flight);
            }

            Assert.That(_avl.Count, Is.EqualTo(15));
            Assert.That(_avl.Height, Is.LessThanOrEqualTo(4));
        }

        [Test]
        public void AVLTree_DeleteOperations_ShouldMaintainBalance()
        {
            for (int i = 1; i <= 10; i++)
            {
                var flight = new Flight 
                { 
                    FlightNumber = $"FL{i:D3}", 
                    Airline = "TestAirline", 
                    Destination = "TestCity" 
                };
                _avl.Insert(flight);
            }

            _avl.Delete("FL005");
            _avl.Delete("FL010");

            Assert.That(_avl.Count, Is.EqualTo(8));
            Assert.That(_avl.Search("FL005"), Is.Null);
        }

        [Test]
        public void AVLTree_InorderTraversal_ReturnsSortedFlights()
        {
            var flights = new[] { "BA123", "AA456", "UA789", "DL012", "SW345" };
            foreach (var flightNum in flights)
            {
                _avl.Insert(new Flight { FlightNumber = flightNum, Airline = "Test", Destination = "Test" });
            }

            var sorted = _avl.GetAllFlightsSorted();
            Assert.That(sorted[0].FlightNumber, Is.EqualTo("AA456"));
            Assert.That(sorted[4].FlightNumber, Is.EqualTo("UA789"));
        }
    }
}
