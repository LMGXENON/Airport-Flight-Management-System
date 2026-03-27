using NUnit.Framework;
using AFMS.Models;

namespace AFMS.Tests
{
    [TestFixture]
    public class FlightValidationTests
    {
        private Flight _flight;

        [SetUp]
        public void Setup()
        {
            _flight = new Flight
            {
                Id = 1,
                FlightNumber = "BA123",
                Airline = "British Airways",
                Destination = "New York",
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(8),
                Terminal = "1",
                Gate = "A5",
                Status = "Scheduled"
            };
        }

        [Test]
        public void Flight_WithValidData_ShouldBeValid()
        {
            Assert.That(_flight.FlightNumber, Is.EqualTo("BA123"));
            Assert.That(_flight.Airline, Is.EqualTo("British Airways"));
            Assert.That(_flight.ArrivalTime, Is.GreaterThan(_flight.DepartureTime));
        }

        [Test]
        public void Flight_ArrivalBeforeDeparture_ShouldBeInvalid()
        {
            _flight.ArrivalTime = _flight.DepartureTime.AddHours(-1);
            Assert.That(_flight.ArrivalTime, Is.LessThan(_flight.DepartureTime));
        }

        [Test]
        public void Flight_DurationOver24Hours_ShouldBeInvalid()
        {
            _flight.ArrivalTime = _flight.DepartureTime.AddHours(25);
            var duration = _flight.ArrivalTime - _flight.DepartureTime;
            Assert.That(duration.TotalHours, Is.GreaterThan(24));
        }

        [Test]
        public void Flight_EmptyFlightNumber_ShouldBeInvalid()
        {
            _flight.FlightNumber = "";
            Assert.That(string.IsNullOrEmpty(_flight.FlightNumber), Is.True);
        }

        [Test]
        public void Flight_InvalidTerminal_ShouldBeInvalid()
        {
            _flight.Terminal = "10";
            Assert.That(_flight.Terminal, Is.EqualTo("10"));
        }
    }
}
