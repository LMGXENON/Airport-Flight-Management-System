# 🎯 SPECIFIC TEST CASES FOR AFMS PROJECT

## Flight Model Validation Tests

Based on your `Flight.cs` model, here are the specific validation rules to test:

### Required Fields
The following fields MUST have values:
- FlightNumber (cannot be null/empty)
- Airline (cannot be null/empty)
- Destination (cannot be null/empty)
- DepartureTime (required)
- ArrivalTime (required)

### Optional Fields
- Gate (nullable string)
- Terminal (defaults to "1")
- Status (defaults to "Scheduled")

---

## 🧪 DETAILED TEST CASES BY FEATURE

### **FEATURE 1: CREATE FLIGHT (Add Flight)**

#### TC-001: Create Flight with Valid Data
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA123"
  2. Enter Airline: "British Airways"
  3. Enter Destination: "New York"
  4. Set Departure Time: 2026-03-25 10:00
  5. Set Arrival Time: 2026-03-25 14:00
  6. Enter Terminal: "2"
  7. Enter Gate: "A5"
  8. Click Save

Expected Result: 
  - Flight saved to database
  - "Flight added successfully!" message shown
  - Redirected to Flight Index/Home page
  - Flight appears in flight list

Status: [ ] Pass [ ] Fail
Notes: This is the happy path - most common scenario
```

#### TC-002: Create Flight - Missing Flight Number
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Leave Flight Number EMPTY
  2. Enter Airline: "British Airways"
  3. Enter Destination: "New York"
  4. Set valid times
  5. Click Save

Expected Result:
  - Form NOT submitted
  - Error message appears: "Flight number is required"
  - User stays on Add Flight page

Status: [ ] Pass [ ] Fail
Severity: HIGH
Notes: Required field validation
```

#### TC-003: Create Flight - Missing Airline
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA123"
  2. Leave Airline EMPTY
  3. Enter Destination: "New York"
  4. Set valid times
  5. Click Save

Expected Result:
  - Error message: "Airline is required"
  - Form not submitted
  - User stays on Add Flight page

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-004: Create Flight - Missing Destination
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA123"
  2. Enter Airline: "British Airways"
  3. Leave Destination EMPTY
  4. Set valid times
  5. Click Save

Expected Result:
  - Error message: "Destination is required"
  - Form not submitted

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-005: Create Flight - No Departure Time
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter all required text fields (BA123, BA, NYC)
  2. Leave Departure Time EMPTY
  3. Set Arrival Time: 2026-03-25 14:00
  4. Click Save

Expected Result:
  - Error message: "Departure time is required"
  - Form not submitted

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-006: Create Flight - No Arrival Time
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter all required text fields
  2. Set Departure Time: 2026-03-25 10:00
  3. Leave Arrival Time EMPTY
  4. Click Save

Expected Result:
  - Error message: "Arrival time is required"
  - Form not submitted

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-007: Create Flight - Arrival Before Departure
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA123"
  2. Enter Airline: "British Airways"
  3. Enter Destination: "New York"
  4. Set Departure Time: 2026-03-25 14:00 (LATER)
  5. Set Arrival Time: 2026-03-25 10:00 (EARLIER)
  6. Click Save

Expected Result:
  - Form rejected OR validation error shown
  - Message indicates: "Arrival time must be after departure time"
  - Flight not saved
  
Status: [ ] Pass [ ] Fail
Severity: CRITICAL
Notes: **CHECK YOUR CODE** - Does Flight.cs have IValidatableObject validation?
       If not, add it to Validate() method
```

#### TC-008: Create Flight - Flight Duration > 24 Hours
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA123"
  2. Enter Airline: "British Airways"
  3. Enter Destination: "Sydney"
  4. Set Departure Time: 2026-03-25 10:00
  5. Set Arrival Time: 2026-03-26 14:00 (25 hours later)
  6. Click Save

Expected Result:
  - Form rejected
  - Error message: "Flight duration cannot exceed 24 hours"
  - Flight not saved

Status: [ ] Pass [ ] Fail
Severity: CRITICAL
Notes: **CHECK YOUR CODE** - Need validation for >24 hour flights
       Add to Flight.cs Validate() method if missing
```

#### TC-009: Create Flight - Special Characters in Flight Number
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "BA@#$123"
  2. Enter Airline: "British Airways"
  3. Enter Destination: "New York"
  4. Set valid times
  5. Click Save

Expected Result:
  - EITHER form rejects with validation error
  - OR flight saves (check what YOUR requirements are)
  
Status: [ ] Pass [ ] Fail
Notes: Test what behavior is expected for special characters
```

#### TC-010: Create Flight - Very Long Flight Number
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Enter Flight Number: "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789"
  2. Fill other required fields
  3. Click Save

Expected Result:
  - Either rejects with "too long" message
  - OR saves successfully
  
Status: [ ] Pass [ ] Fail
Notes: Depends on your StringLength attribute on FlightNumber
```

#### TC-011: Create Flight - Default Values
```
Pre-conditions: On Add Flight page (just loaded)
Test Steps:
  1. Check form defaults WITHOUT entering anything
  2. Look at Pre-populated values:
     - Departure Time: Should be DateTime.Now
     - Arrival Time: Should be DateTime.Now.AddHours(2)
     - Terminal: Should be "1"
  
Expected Result:
  - Departure Time field shows current date/time
  - Arrival Time field shows 2 hours later
  - Terminal field shows "1"

Status: [ ] Pass [ ] Fail
Notes: From Add() action: defaults are set in code
```

#### TC-012: Create Flight - Empty Gate (Optional Field)
```
Pre-conditions: On Add Flight page
Test Steps:
  1. Fill all required fields
  2. Leave Gate EMPTY (optional field)
  3. Click Save

Expected Result:
  - Flight saved successfully
  - No error for empty Gate

Status: [ ] Pass [ ] Fail
Notes: Gate is nullable, should allow empty value
```

---

### **FEATURE 2: VIEW FLIGHT DETAILS**

#### TC-013: View Flight Details - Valid Flight ID
```
Pre-conditions: 
  - Flight with ID 5 exists in database
  - On Flight Index page

Test Steps:
  1. Click on a flight (or navigate to /Flight/Details/5)
  2. Wait for Details page to load

Expected Result:
  - Flight details display correctly
  - All fields show correct values
  - Page title shows "Flight Details"

Status: [ ] Pass [ ] Fail
```

#### TC-014: View Flight Details - No ID Provided
```
Pre-conditions: Navigate to /Flight/Details (no ID)
Test Steps:
  1. Go to URL: /Flight/Details

Expected Result:
  - Per code: Should show list of all flights
  - Index view displays
  
Status: [ ] Pass [ ] Fail
Notes: Code has special handling: "if (id == null) return View("Index", flights);"
```

#### TC-015: View Flight Details - Invalid Flight ID
```
Pre-conditions: Flight with ID 999 does NOT exist
Test Steps:
  1. Navigate to /Flight/Details/999

Expected Result:
  - 404 Not Found page displayed
  - Message: "The resource you requested could not be found"

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-016: View Flight Details - Negative ID
```
Pre-conditions: 
Test Steps:
  1. Navigate to /Flight/Details/-5

Expected Result:
  - 404 Not Found OR error handling

Status: [ ] Pass [ ] Fail
Notes: Edge case testing
```

#### TC-017: View Flight Details - String as ID
```
Pre-conditions:
Test Steps:
  1. Navigate to /Flight/Details/abc

Expected Result:
  - Error page OR 400 Bad Request
  - URL parameter validation fails

Status: [ ] Pass [ ] Fail
Notes: Input validation
```

---

### **FEATURE 3: EDIT FLIGHT**

#### TC-018: Edit Flight - Change Single Field
```
Pre-conditions: 
  - Flight BA123 exists with ID 1
  - On Flight Edit page for flight 1

Test Steps:
  1. Change Gate from "A5" to "B7"
  2. Leave all other fields unchanged
  3. Click Save

Expected Result:
  - Flight updated in database
  - Message: "Flight updated successfully!"
  - Gate now shows "B7"
  - Redirected to Flight Index

Status: [ ] Pass [ ] Fail
```

#### TC-019: Edit Flight - Change Multiple Fields
```
Pre-conditions: Flight exists with ID 1
Test Steps:
  1. Change Flight Number: "BA123" → "BA999"
  2. Change Destination: "NYC" → "LAX"
  3. Change Terminal: "1" → "3"
  4. Click Save

Expected Result:
  - All changes saved
  - Success message shown
  - Flight list shows updated values

Status: [ ] Pass [ ] Fail
```

#### TC-020: Edit Flight - Make Departure After Arrival
```
Pre-conditions: Flight with ID 1 exists
Test Steps:
  1. On Edit page for flight 1
  2. Change Departure Time to: 2026-03-25 14:00
  3. Change Arrival Time to: 2026-03-25 10:00
  4. Click Save

Expected Result:
  - Form rejected
  - Error message shown
  - Flight NOT updated

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-021: Edit Flight - Invalid ID in URL
```
Pre-conditions: 
Test Steps:
  1. Navigate to /Flight/Edit/999 (non-existent ID)

Expected Result:
  - 404 Not Found displayed

Status: [ ] Pass [ ] Fail
```

#### TC-022: Edit Flight - ID Mismatch
```
Pre-conditions: Flight data sent with ID 5, but URL is /Flight/Edit/10
Test Steps:
  1. Navigate to /Flight/Edit/10
  2. Somehow submit data where flight.Id != 10
  3. Click Save

Expected Result:
  - 404 Not Found (from: if (id != flight.Id))
  
Status: [ ] Pass [ ] Fail
Notes: Security feature - prevents ID tampering
```

#### TC-023: Edit Flight - Make Duration >24 Hours
```
Pre-conditions: Flight with ID 1 exists
Test Steps:
  1. Edit flight 1
  2. Set Departure: 2026-03-25 10:00
  3. Set Arrival: 2026-03-26 14:00 (25 hours)
  4. Click Save

Expected Result:
  - Validation error: "Cannot exceed 24 hours"
  - Flight not updated

Status: [ ] Pass [ ] Fail
Severity: CRITICAL
```

#### TC-024: Edit Flight - Clear Optional Gate Field
```
Pre-conditions: Flight has Gate = "A5"
Test Steps:
  1. Edit the flight
  2. Clear the Gate field
  3. Click Save

Expected Result:
  - Flight updated with Gate = null/empty
  - No error

Status: [ ] Pass [ ] Fail
```

---

### **FEATURE 4: DELETE FLIGHT**

#### TC-025: Delete Flight - Confirmation Page Shows
```
Pre-conditions: Flight with ID 1 exists
Test Steps:
  1. Click Delete button on Flight Index
  2. Wait for Delete confirmation page

Expected Result:
  - Delete confirmation page loads
  - Shows flight details to confirm deletion
  - "Delete" and "Cancel" buttons visible

Status: [ ] Pass [ ] Fail
```

#### TC-026: Delete Flight - Confirmed Deletion
```
Pre-conditions: On Delete confirmation page for flight ID 1
Test Steps:
  1. Click "Delete" button to confirm

Expected Result:
  - Flight removed from database
  - Message: "Flight deleted successfully!"
  - Redirected to Flight Index
  - Flight no longer in list

Status: [ ] Pass [ ] Fail
Severity: HIGH
```

#### TC-027: Delete Flight - Cancel Deletion
```
Pre-conditions: On Delete confirmation page
Test Steps:
  1. Click "Cancel" button

Expected Result:
  - Deletion cancelled
  - Redirected to Flight Index
  - Flight still exists in database
  - Flight still appears in list

Status: [ ] Pass [ ] Fail
```

#### TC-028: Delete Flight - Invalid ID
```
Pre-conditions:
Test Steps:
  1. Navigate to /Flight/Delete/999 (non-existent)

Expected Result:
  - 404 Not Found page

Status: [ ] Pass [ ] Fail
```

#### TC-029: Delete Flight - No ID Provided
```
Pre-conditions:
Test Steps:
  1. Navigate to /Flight/Delete (no ID)

Expected Result:
  - 404 Not Found (from: if (id == null))

Status: [ ] Pass [ ] Fail
```

#### TC-030: Delete Non-Existent Flight
```
Pre-conditions: Flight with ID 999 does not exist
Test Steps:
  1. Navigate directly to /Flight/Delete/999
  2. (It would redirect, but if somehow you submit)

Expected Result:
  - No error
  - Flight simply doesn't get deleted
  - Message: "Flight deleted successfully!" (or no error)
  
Status: [ ] Pass [ ] Fail
Notes: Code checks "if (flight != null)" before deleting
```

---

### **FEATURE 5: SEARCH & FILTER (From HomeController)**

These tests require you to check the **HomeController.cs** for search/filter implementation.

#### TC-031: Search - Find Flight by Flight Number
```
Pre-conditions:
  - Flight "BA123" exists in database
  - On search page

Test Steps:
  1. Enter search: "BA123"
  2. Click Search

Expected Result:
  - Only flight BA123 displayed
  - No other flights shown

Status: [ ] Pass [ ] Fail
```

#### TC-032: Search - No Results
```
Pre-conditions: On search page
Test Steps:
  1. Search for: "XYZ999" (doesn't exist)
  2. Click Search

Expected Result:
  - Message: "No flights found"
  - Empty results list

Status: [ ] Pass [ ] Fail
```

#### TC-033: Search - Partial Flight Number
```
Pre-conditions: Flights BA123, BA456, BA789 exist
Test Steps:
  1. Search for: "BA" (partial)

Expected Result:
  - All BA flights returned (BA123, BA456, BA789)
  - Other airline flights not shown

Status: [ ] Pass [ ] Fail
Notes: Depends on search implementation (exact vs. contains)
```

#### TC-034: Filter - By Status
```
Pre-conditions: 
  - Flights with Status: "Scheduled", "Delayed", "Cancelled"
  - On filter page

Test Steps:
  1. Select Status: "Delayed"
  2. Click Filter

Expected Result:
  - Only "Delayed" flights shown
  - Other statuses filtered out

Status: [ ] Pass [ ] Fail
```

#### TC-035: Filter - By Terminal
```
Pre-conditions:
  - Flights in Terminal 1, 2, 3
  - On filter page

Test Steps:
  1. Select Terminal: "2"
  2. Click Filter

Expected Result:
  - Only Terminal 2 flights shown

Status: [ ] Pass [ ] Fail
```

---

### **FEATURE 6: DATABASE INTEGRITY**

#### TC-036: Database - Flight Persists After Save
```
Pre-conditions: Database is clean
Test Steps:
  1. Add Flight: BA123, BA, NYC, valid times
  2. Refresh page
  3. Query database directly

Expected Result:
  - Flight BA123 still exists
  - Data not lost after refresh

Status: [ ] Pass [ ] Fail
Severity: CRITICAL
```

#### TC-037: Database - Updated Values Persist
```
Pre-conditions: Flight BA123 exists with Gate="A5"
Test Steps:
  1. Edit flight: Change Gate to "B7"
  2. Refresh page
  3. Check flight in database

Expected Result:
  - Gate shows "B7" in database
  - Change persisted

Status: [ ] Pass [ ] Fail
Severity: CRITICAL
```

#### TC-038: Database - Deleted Flight Gone
```
Pre-conditions: Flight BA123 exists
Test Steps:
  1. Delete flight BA123
  2. Query database
  3. Search for BA123

Expected Result:
  - Flight NOT in database
  - Search returns zero results

Status: [ ] Pass [ ] Fail
Severity: CRITICAL
```

#### TC-039: Database - Concurrent Access
```
Pre-conditions: Multiple users could access simultaneously
Test Steps:
  1. User A: Start editing Flight BA123
  2. User B: Start editing Flight BA123
  3. User A: Save changes
  4. User B: Save changes

Expected Result:
  - Code handles DbUpdateConcurrencyException
  - Shows error OR last write wins
  - No data corruption

Status: [ ] Pass [ ] Fail
Notes: Your Edit action has try-catch for this!
```

---

### **FEATURE 7: DATA STRUCTURE (CUSTOM - WHEN IMPLEMENTED)**

When your team implements the custom data structure (AVL Tree, BST, or Hash Table), test:

#### TC-040: Data Structure - Insert Flight
```
Pre-conditions: Data structure is empty
Test Steps:
  1. Insert Flight object: {BA123, BA, NYC}
  2. Verify insertion

Expected Result:
  - Flight added to structure
  - Structure size = 1

Status: [ ] Pass [ ] Fail
```

#### TC-041: Data Structure - Search by Key
```
Pre-conditions: Data structure has 10 flights
Test Steps:
  1. Search for "BA123"
  2. Measure search time

Expected Result:
  - Flight found correctly
  - Search time acceptable
  
Status: [ ] Pass [ ] Fail
```

#### TC-042: Data Structure - Delete Flight
```
Pre-conditions: Data structure has flight BA123
Test Steps:
  1. Delete BA123
  2. Try searching for BA123

Expected Result:
  - Flight no longer found
  - Structure rebalanced (if AVL/BST)

Status: [ ] Pass [ ] Fail
```

#### TC-043: Data Structure - Handle Duplicates
```
Pre-conditions: Data structure has BA123
Test Steps:
  1. Try inserting BA123 again

Expected Result:
  - Either rejects duplicate
  - Or updates existing entry
  - No data corruption

Status: [ ] Pass [ ] Fail
```

#### TC-044: Data Structure - Large Dataset (1000+ flights)
```
Pre-conditions:
Test Steps:
  1. Insert 1000 random flights
  2. Search for specific flight
  3. Measure performance
  4. Delete 500 flights
  5. Verify structure integrity

Expected Result:
  - All operations complete successfully
  - Search time acceptable
  - No memory leaks
  
Status: [ ] Pass [ ] Fail
```

---

## 📊 TEST EXECUTION SUMMARY TEMPLATE

Create a file: `Test_Results_Summary.md`

```markdown
# Test Results - [DATE]

## Test Execution Summary

| Category | Total | Passed | Failed | Pass Rate |
|----------|-------|--------|--------|-----------|
| Create Flight | 12 | 10 | 2 | 83% |
| View Details | 5 | 5 | 0 | 100% |
| Edit Flight | 7 | 6 | 1 | 86% |
| Delete Flight | 6 | 6 | 0 | 100% |
| Search/Filter | 5 | 4 | 1 | 80% |
| Database | 4 | 4 | 0 | 100% |
| Data Structure | 5 | 0 | 0 | 0% |
| **TOTALS** | **44** | **35** | **4** | **80%** |

## Defects Found

### DEF-001 - CRITICAL
- **Title**: Duration validation not working
- **Test**: TC-008
- **Reproduction**: Create flight 25 hours long
- **Expected**: Rejected
- **Actual**: Saved successfully
- **Status**: Open → [Assigned to Developer Name] → Fixed → Verified

### DEF-002 - HIGH
- **Title**: Arrival before departure accepted
- **Test**: TC-007
- **Status**: Open

## Test Execution Timeline
- Started: March 25, 2026
- In Progress: March 25-31
- Completed: April 5, 2026

## Next Steps
- [ ] Fix all critical defects
- [ ] Retest failed test cases
- [ ] Verify no regressions
- [ ] Final regression test on April 9
```

---

## 🎯 PRIORITIZED TESTING ORDER

### Phase 1: Core Functionality (March 25-28)
1. TC-001 (Happy path)
2. TC-002 to TC-006 (Required field validation)
3. TC-013 (View details)
4. TC-025, TC-026 (Delete)

### Phase 2: Validation & Edge Cases (March 29-31)
1. TC-007, TC-008 (Time validation - CRITICAL)
2. TC-020, TC-023 (Edit validation)
3. TC-014 to TC-017 (View edge cases)

### Phase 3: Database & Integration (April 1-4)
1. TC-036 to TC-038 (Database persistence)
2. TC-031 to TC-035 (Search/filter)
3. TC-039 (Concurrent access)

### Phase 4: Data Structure (April 5-7)
1. TC-040 to TC-044 (Custom data structure)

### Phase 5: Final Verification (April 8-10)
1. Retest all failed tests
2. Final regression test
3. Performance testing

---

## 💡 IMPORTANT NOTES FOR YOUR TESTING

### ⚠️ CRITICAL ISSUES TO CHECK NOW:

1. **Time Validation** (TC-007, TC-008):
   - Check if `Flight.cs` validates Arrival > Departure
   - Check if duration > 24 hours is rejected
   - **Your code may be missing this validation!**

2. **Validation Method**:
   - Look for `IValidatableObject` implementation in Flight.cs
   - Check `Validate(ValidationContext context)` method
   - If missing, this is a BUG to report to developers

3. **Database Storage**:
   - Ensure EF Core is saving to SQLite correctly
   - Test by querying database directly after creates/updates

4. **Error Handling**:
   - Are error messages user-friendly?
   - Do validation messages match field names?

---

## 📝 HOW TO FILL OUT TEST CASE RESULTS

For each test case:

```markdown
#### TC-001: Create Flight with Valid Data
**Status**: ✅ PASS / ❌ FAIL / ⏭️ SKIP

**Test Date**: March 25, 2026
**Tested By**: [Your Name]

**Actual Result**: Flight BA123 saved, "Flight added successfully!" shown, redirected to Index

**Observations**: 
- Form had clean UI
- Default times worked correctly
- Success message appeared immediately

**Issues Found**: None

**Screenshot**: [Attach if needed]
```

---

## 🚀 START HERE

**Tomorrow (March 25)**, start with these tests in order:

1. **TC-001** - Basic create flow
2. **TC-002** - Missing flight number
3. **TC-003** - Missing airline
4. **TC-013** - View details
5. **TC-025** - Delete confirmation
6. **TC-026** - Delete confirmed

Run these 6 tests and document results. This will give you confidence in the basic flows!

Then proceed to validation tests (TC-007, TC-008) which are critical.

---

**Questions to ask as you test:**
1. Does the feature work as described?
2. Are error messages clear?
3. Is data saved to database?
4. Can I reproduce any errors?
5. Do validations prevent bad data?

**Good luck!** 🎯
