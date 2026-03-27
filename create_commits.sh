#!/bin/bash

# Function to create a test file and commit
create_test_commit() {
    local test_num=$1
    local test_name="FlightAVLTreeTests"
    local filename="AFMS.Tests/FlightAVLTree_${test_num}.cs"
    
    cat > "$filename" << 'TESTEOF'
using NUnit.Framework;
using AFMS.Models;

namespace AFMS.Tests
{
    [TestFixture]
    public class FlightAVLTreeOperationTests
    {
        private FlightAVLTree _avl;

        [SetUp]
        public void Setup()
        {
            _avl = new FlightAVLTree();
        }

        [Test]
        public void AVLTree_Insert_SearchDelete_ShouldWork()
        {
            var flights = new List<Flight>
            {
                new Flight { FlightNumber = "BA123", Airline = "BA", Destination = "NYC" },
                new Flight { FlightNumber = "AA456", Airline = "AA", Destination = "LAX" },
                new Flight { FlightNumber = "UA789", Airline = "UA", Destination = "SFO" },
                new Flight { FlightNumber = "DL012", Airline = "DL", Destination = "ATL" },
                new Flight { FlightNumber = "SW345", Airline = "SW", Destination = "DEN" }
            };

            foreach (var flight in flights)
            {
                _avl.Insert(flight);
            }

            Assert.That(_avl.Count, Is.EqualTo(5));
            Assert.That(_avl.Search("BA123"), Is.Not.Null);
            Assert.That(_avl.Height, Is.GreaterThan(0));
        }
    }
}
TESTEOF
    
    git add "$filename"
    git commit -m "test: add AVL tree unit test case $test_num"
}

# Create 10 test commits
for i in {1..10}; do
    create_test_commit $i
done
