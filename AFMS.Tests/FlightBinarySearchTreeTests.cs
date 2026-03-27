using NUnit.Framework;
using AFMS.Models;

namespace AFMS.Tests
{
    [TestFixture]
    public class FlightBinarySearchTreeTests
    {
        private FlightBinarySearchTree _bst;

        [SetUp]
        public void Setup()
        {
            _bst = new FlightBinarySearchTree();
        }

        [Test]
        public void Insert_SingleFlight_ShouldIncreaseCount()
        {
            var flight = new Flight { FlightNumber = "BA123", Airline = "BA", Destination = "NYC" };
            _bst.Insert(flight);
            Assert.That(_bst.Count, Is.EqualTo(1));
        }

        [Test]
        public void Insert_MultipleFlights_ShouldMaintainOrder()
        {
            var flight1 = new Flight { FlightNumber = "BA123", Airline = "BA", Destination = "NYC" };
            var flight2 = new Flight { FlightNumber = "AA456", Airline = "AA", Destination = "LAX" };
            var flight3 = new Flight { FlightNumber = "UA789", Airline = "UA", Destination = "SFO" };

            _bst.Insert(flight2);
            _bst.Insert(flight1);
            _bst.Insert(flight3);

            var sorted = _bst.GetAllFlightsSorted();
            Assert.That(sorted[0].FlightNumber, Is.EqualTo("AA456"));
            Assert.That(sorted[1].FlightNumber, Is.EqualTo("BA123"));
            Assert.That(sorted[2].FlightNumber, Is.EqualTo("UA789"));
        }

        [Test]
        public void Search_ExistingFlight_ShouldReturnFlight()
        {
            var flight = new Flight { FlightNumber = "BA123", Airline = "BA", Destination = "NYC" };
            _bst.Insert(flight);
            var found = _bst.Search("BA123");
            Assert.That(found, Is.Not.Null);
            Assert.That(found.FlightNumber, Is.EqualTo("BA123"));
        }

        [Test]
        public void Search_NonExistentFlight_ShouldReturnNull()
        {
            var found = _bst.Search("XX999");
            Assert.That(found, Is.Null);
        }

        [Test]
        public void Delete_ExistingFlight_ShouldReturnTrue()
        {
            var flight = new Flight { FlightNumber = "BA123", Airline = "BA", Destination = "NYC" };
            _bst.Insert(flight);
            var deleted = _bst.Delete("BA123");
            Assert.That(deleted, Is.True);
            Assert.That(_bst.Count, Is.Zero);
        }

        [Test]
        public void Delete_NonExistentFlight_ShouldReturnFalse()
        {
            var deleted = _bst.Delete("XX999");
            Assert.That(deleted, Is.False);
        }
    }
}
