#!/bin/bash

# Flight Details Refactoring - Testing Script
# This script validates the refactored flight details functionality

echo "================================"
echo "Flight Details Refactoring Tests"
echo "================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counter
TESTS_PASSED=0
TESTS_FAILED=0

# Function to run a test
run_test() {
    local test_name=$1
    local test_command=$2
    
    echo -n "Testing: $test_name... "
    
    if eval "$test_command" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ PASS${NC}"
        ((TESTS_PASSED++))
    else
        echo -e "${RED}✗ FAIL${NC}"
        ((TESTS_FAILED++))
    fi
}

# Test 1: Check if FlightDetailsService exists
run_test "FlightDetailsService.cs exists" \
    "[ -f AFMS/Services/FlightDetailsService.cs ]"

# Test 2: Check if FlightDetailsViewModel exists
run_test "FlightDetailsViewModel.cs exists" \
    "[ -f AFMS/Models/FlightDetailsViewModel.cs ]"

# Test 3: Check if refactored JavaScript exists
run_test "flight-details-modal-refactored.js exists" \
    "[ -f AFMS/wwwroot/js/flight-details-modal-refactored.js ]"

# Test 4: Check if FlightDetailsService contains FormatFlightDuration method
run_test "FlightDetailsService has FormatFlightDuration method" \
    "grep -q 'public string FormatFlightDuration' AFMS/Services/FlightDetailsService.cs"

# Test 5: Check if FlightDetailsService contains ValidateFlightDetails method
run_test "FlightDetailsService has ValidateFlightDetails method" \
    "grep -q 'public FlightDetailsValidation ValidateFlightDetails' AFMS/Services/FlightDetailsService.cs"

# Test 6: Check if FlightDetailsViewModel contains DepartureTimeFormatted property
run_test "FlightDetailsViewModel has DepartureTimeFormatted property" \
    "grep -q 'public string DepartureTimeFormatted' AFMS/Models/FlightDetailsViewModel.cs"

# Test 7: Check if FlightDetailsViewModel has FromFlight factory method
run_test "FlightDetailsViewModel has FromFlight factory method" \
    "grep -q 'public static FlightDetailsViewModel FromFlight' AFMS/Models/FlightDetailsViewModel.cs"

# Test 8: Check if FlightController injects FlightDetailsService
run_test "FlightController injects FlightDetailsService" \
    "grep -q 'FlightDetailsService' AFMS/Controllers/FlightController.cs"

# Test 9: Check if Program.cs registers FlightDetailsService
run_test "Program.cs registers FlightDetailsService" \
    "grep -q 'AddScoped<FlightDetailsService>' AFMS/Program.cs"

# Test 10: Check if JavaScript has FlightModalManager class
run_test "JavaScript has FlightModalManager class" \
    "grep -q 'class FlightModalManager' AFMS/wwwroot/js/flight-details-modal-refactored.js"

# Test 11: Check if JavaScript has FlightModalRenderer class
run_test "JavaScript has FlightModalRenderer class" \
    "grep -q 'class FlightModalRenderer' AFMS/wwwroot/js/flight-details-modal-refactored.js"

# Test 12: Check if JavaScript has FlightProgressTracker class
run_test "JavaScript has FlightProgressTracker class" \
    "grep -q 'class FlightProgressTracker' AFMS/wwwroot/js/flight-details-modal-refactored.js"

# Test 13: Check documentation files exist
run_test "FLIGHT_DETAILS_REFACTORING.md exists" \
    "[ -f FLIGHT_DETAILS_REFACTORING.md ]"

run_test "REFACTORING_SUMMARY.md exists" \
    "[ -f REFACTORING_SUMMARY.md ]"

run_test "TESTING_CHECKLIST.md exists" \
    "[ -f TESTING_CHECKLIST.md ]"

echo ""
echo "================================"
echo -e "Results: ${GREEN}$TESTS_PASSED passed${NC}, ${RED}$TESTS_FAILED failed${NC}"
echo "================================"

# Exit with failure if any tests failed
if [ $TESTS_FAILED -gt 0 ]; then
    exit 1
fi

exit 0
