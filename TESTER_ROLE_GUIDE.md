# 🧪 TESTER ROLE GUIDE - CST2550 Coursework

## Your Role as Quality Assurance (QA) Tester

As the Tester, you are responsible for ensuring the quality and reliability of the software by identifying and fixing defects or issues. This is a **critical role** that directly impacts your team's marks.

---

## 📋 YOUR RESPONSIBILITIES

### 1. **Test Planning & Strategy**
- [ ] Define testing approach and methodology
- [ ] Create comprehensive test plans
- [ ] Identify what needs to be tested
- [ ] Estimate testing effort and timeline

### 2. **Test Case Design**
- [ ] Design test cases for all features
- [ ] Create test data
- [ ] Document expected vs actual results
- [ ] Ensure test coverage of edge cases

### 3. **Test Execution**
- [ ] Run manual tests
- [ ] Execute unit tests (with developers)
- [ ] Perform integration testing
- [ ] Conduct system testing

### 4. **Defect Management**
- [ ] Report bugs clearly with reproduction steps
- [ ] Track defect severity and priority
- [ ] Verify fixes
- [ ] Ensure no regressions

### 5. **Quality Assurance**
- [ ] Verify code quality
- [ ] Check compliance with requirements
- [ ] Validate data integrity
- [ ] Ensure user experience quality

### 6. **Documentation**
- [ ] Document test cases for the report
- [ ] Create test result summaries
- [ ] Provide testing feedback to team
- [ ] Contribute to testing section of report

---

## 🎯 YOUR IMMEDIATE TASKS (PRIORITY ORDER)

### WEEK 1: Test Planning (Due March 31)

#### Task 1.1: Define Testing Approach
Create a document: `Testing_Strategy.md`

```markdown
# Testing Strategy

## Testing Approach
- Unit Testing: NUnit/MSTest (with developers)
- Integration Testing: Manual feature testing
- System Testing: End-to-end workflows
- Data Validation Testing: Database integrity

## Test Types
1. Functional Testing: Does it do what it's supposed to do?
2. Performance Testing: Is it fast enough?
3. Security Testing: Is the data secure?
4. Usability Testing: Is it easy to use?
5. Compatibility Testing: Does it work in different environments?

## Test Environment
- Operating System: Windows/Mac
- Browser: Chrome, Firefox, Safari
- Database: SQLite
- Framework: .NET Core

## Success Criteria
- Zero critical defects
- All test cases pass
- Code compiles and runs
- No regressions between builds
```

#### Task 1.2: Create Test Case Template
Create a file: `Test_Cases_Template.md`

```markdown
# Test Case Template

## Test Case ID: TC-001
- **Title**: [Feature Name] - [Scenario]
- **Pre-conditions**: [Initial state before test]
- **Test Steps**: [Step-by-step actions]
- **Expected Result**: [What should happen]
- **Actual Result**: [What actually happened]
- **Status**: ☐ Pass ☐ Fail
- **Severity**: ☐ Critical ☐ High ☐ Medium ☐ Low
- **Notes**: [Any additional information]
```

#### Task 1.3: List All Test Cases Needed
Create a file: `Test_Cases_Master_List.md`

For Flight Management System, you need tests for:

```
FLIGHT FUNCTIONALITY TESTS:
TC-001: Create Flight - Valid Data
TC-002: Create Flight - Missing Fields
TC-003: Create Flight - Invalid Flight Number
TC-004: Create Flight - Invalid Terminal (>5)
TC-005: Create Flight - Arrival Before Departure
TC-006: Create Flight - Flight Duration >24 hours
TC-007: View Flight Details
TC-008: Edit Flight - Valid Changes
TC-009: Edit Flight - Invalid Data
TC-010: Delete Flight - Confirmation Required
TC-011: Delete Flight - Verify Deleted

SEARCH/FILTER TESTS:
TC-012: Search by Flight Number
TC-013: Search by Airline
TC-014: Search by Destination
TC-015: Search - No Results
TC-016: Filter by Status
TC-017: Filter by Terminal
TC-018: Filter by Date Range

DATA STRUCTURE TESTS:
TC-019: Insert Flight into Data Structure
TC-020: Search Flight by Number
TC-021: Search Flight by Airline
TC-022: Remove Flight from Data Structure
TC-023: Handle Duplicate Insertions
TC-024: Large Dataset Performance

DATABASE TESTS:
TC-025: Create Flight - Verify Database
TC-026: Update Flight - Verify Database
TC-027: Delete Flight - Verify Database
TC-028: Data Integrity Check
TC-029: Concurrent Modifications
TC-030: Database Recovery

VALIDATION TESTS:
TC-031: String Length Validation
TC-032: Required Field Validation
TC-033: Date Format Validation
TC-034: Number Format Validation
TC-035: Regex Pattern Validation

USER INTERFACE TESTS:
TC-036: Navigation Menu Works
TC-037: Form Displays Correctly
TC-038: Error Messages Display Properly
TC-039: Success Messages Display
TC-040: Responsive Design (Mobile/Desktop)
```

---

### WEEK 2: Test Execution & Reporting (Due April 7)

#### Task 2.1: Execute All Test Cases
Go through each test case and record results in `Test_Results.md`

```markdown
# Test Results Summary

| TC ID | Test Name | Status | Defects Found | Severity |
|-------|-----------|--------|---------------|----------|
| TC-001 | Create Flight - Valid | ✅ Pass | 0 | - |
| TC-002 | Create Flight - Missing | ✅ Pass | 0 | - |
| TC-003 | Invalid Flight Number | ✅ Pass | 1 | High |
| ... | ... | ... | ... | ... |

## Defects Found
### Defect ID: DEF-001
- **Title**: Flight number accepts invalid characters
- **Severity**: High
- **Reproduce**: Enter "BA@#$123" as flight number
- **Expected**: Should reject with error message
- **Actual**: Accepts and saves to database
- **Status**: Open → Fixed → Verified

### Defect ID: DEF-002
- **Title**: Terminal field accepts 0 (should be 1-5)
- **Severity**: Medium
- **Reproduce**: Enter 0 in Terminal field
- **Expected**: Should validate 1-5 only
- **Actual**: Accepts 0
- **Status**: Open
```

#### Task 2.2: Test Data Structure
With the developers, create test data for the custom data structure:

```
Test Data Set 1: Empty Structure
- Insert 0 flights
- Search should return empty
- Remove should handle gracefully

Test Data Set 2: Single Flight
- Insert 1 flight (BA123)
- Search should find it
- Remove should work correctly

Test Data Set 3: Multiple Flights (10)
- Insert 10 different flights
- Search each one
- Remove 5 randomly
- Verify integrity

Test Data Set 4: Duplicate Handling
- Try inserting same flight twice
- Should either reject or update
- Verify behavior documented

Test Data Set 5: Large Dataset (100+)
- Insert 100+ flights
- Measure search time
- Measure insert time
- Verify performance acceptable
```

#### Task 2.3: Performance Testing
Document performance metrics:

```markdown
# Performance Test Results

## Insert Operation (100 flights)
- Time: [X ms]
- Average per flight: [Y ms]
- Status: ✅ Acceptable / ❌ Too Slow

## Search Operation (in dataset of 1000)
- Time to find first: [X ms]
- Time for no results: [Y ms]
- Status: ✅ Acceptable / ❌ Too Slow

## Delete Operation
- Time: [X ms]
- Status: ✅ Acceptable / ❌ Too Slow
```

---

### WEEK 3: Report Contribution (Due April 10)

#### Task 3.1: Write Testing Section for Report
Contribute to the PDF report (approximately 1 page):

```markdown
# TESTING SECTION (for report)

## Testing Approach
This section should state (not show code):
- What types of testing were performed
- How test cases were designed
- What tools were used
- Coverage achieved

## Test Cases Table
Create a table showing:
| Test Case ID | Description | Input | Expected Output | Result |
|---|---|---|---|---|
| TC-001 | Create valid flight | BA123, BA, JFK, valid times | Flight created | ✅ Pass |
| TC-002 | Invalid terminal | BA123, BA, JFK, terminal=6 | Error message | ✅ Pass |
| ... | ... | ... | ... | ... |

## Test Results Summary
- Total Test Cases: 40
- Passed: 38
- Failed: 2
- Pass Rate: 95%
- Defects Found: 5
- Critical Defects: 0
```

---

## 🔧 HOW TO TEST YOUR CODE

### 1. **Manual Testing Checklist**

```
BEFORE EACH BUILD:
☐ Can I create a flight with valid data?
☐ Does it reject invalid flight numbers?
☐ Does it validate terminal (1-5)?
☐ Does it check arrival > departure?
☐ Can I edit a flight?
☐ Can I delete a flight?
☐ Can I search for flights?
☐ Does the database save correctly?
☐ Can I load flights from database?

EVERY DAY:
☐ Run the application
☐ Test at least 5 features
☐ Log any issues
☐ Verify previous fixes
☐ Check for regressions
```

### 2. **Data Validation Testing**

```csharp
// Examples of what to test:

// Flight Number Validation
- "BA123" → Should Accept ✅
- "BA@123" → Should Reject ❌
- "" → Should Reject ❌
- "AB" → Should Reject (too short) ❌
- "AB12345678" → Should Reject (too long) ❌

// Terminal Validation  
- "1", "2", "3", "4", "5" → Accept ✅
- "0", "6", "-1" → Reject ❌
- "1.5" → Reject ❌

// DateTime Validation
- 2026-03-24 10:00 to 14:00 → Accept ✅
- 2026-03-24 14:00 to 10:00 → Reject ❌
- 2026-03-24 10:00 to 2026-03-26 10:00 → Reject (>24hrs) ❌
```

### 3. **Integration Testing**

```
Test the full workflow:

1. Start Application
   ☐ Application starts without errors
   ☐ Database connects successfully
   ☐ UI loads properly

2. Create New Flight
   ☐ Form opens
   ☐ Default values set correctly
   ☐ Validation works
   ☐ Flight saved to database
   ☐ Appears in flight list

3. Edit Flight
   ☐ Can select a flight
   ☐ Details load in form
   ☐ Can modify fields
   ☐ Changes save
   ☐ Database updated
   ☐ List view reflects changes

4. Search/Filter
   ☐ Search by flight number works
   ☐ Search returns correct results
   ☐ Filter by status works
   ☐ Filter by terminal works
   ☐ Multiple filters work together

5. Delete Flight
   ☐ Can select flight to delete
   ☐ Confirmation dialog appears
   ☐ Confirmed deletion removes flight
   ☐ Flight gone from list
   ☐ Flight removed from database
```

---

## 📊 TEST DOCUMENTATION YOU'LL CREATE

### Files You'll Create:
1. **Testing_Strategy.md** - Your overall approach
2. **Test_Cases_Master_List.md** - All 40-50 test cases
3. **Test_Results.md** - Results of running all tests
4. **Defect_Log.md** - Bugs found and their status
5. **Performance_Metrics.md** - Performance test results
6. **Test_Evidence.md** - Screenshots/proof of testing

### Format for Report:
Contribute 1-2 pages to the PDF showing:
- Testing methodology (in words, not code)
- Table of test cases (without test code)
- Test results summary
- Defects found and fixed
- Coverage achieved

---

## 🐛 HOW TO REPORT BUGS EFFECTIVELY

When you find a bug, document it clearly:

```markdown
## Bug Report Template

**Bug ID**: DEF-001
**Title**: [Short description]
**Severity**: 🔴 Critical / 🟠 High / 🟡 Medium / 🟢 Low

### Reproduction Steps
1. Open application
2. Click "Add Flight"
3. Enter flight number "BA@123"
4. Click Save

### Expected Behavior
System should display error: "Flight number can only contain letters and numbers"

### Actual Behavior
Flight is saved to database successfully

### Environment
- OS: Windows 10
- Browser: Chrome 120
- Date Found: March 25, 2026

### Status
- Found: March 25, 2026
- Assigned to: [Developer Name]
- Fixed: [Date]
- Verified: [Date]
```

---

## ⏰ YOUR TIMELINE

### March 25-28: Planning Phase
- [ ] Create testing strategy (1 day)
- [ ] Design test cases (2 days)
- [ ] Prepare test data (1 day)

### March 29-31: Initial Testing
- [ ] Test basic functionality (2 days)
- [ ] Report any defects (1 day)

### April 1-5: Full Testing
- [ ] Execute all 40+ test cases
- [ ] Test data structure thoroughly
- [ ] Test database operations
- [ ] Performance testing
- [ ] Report results

### April 6-8: Report & Documentation
- [ ] Write testing section for report
- [ ] Create test evidence
- [ ] Verify all fixes
- [ ] Final regression testing

### April 9-10: Final Review
- [ ] Test everything one more time
- [ ] Verify no regressions
- [ ] Prepare demo content

---

## 💡 TIPS FOR BEING AN EXCELLENT TESTER

### 1. **Think Like a User**
- What would confuse a user?
- What edge cases might break the app?
- What happens with empty data?
- What happens with extremely large data?

### 2. **Be Thorough But Efficient**
- Test systematically, not randomly
- Start with high-priority features
- Document everything
- Don't test the same thing multiple times

### 3. **Communicate Clearly**
- Report bugs immediately
- Provide clear reproduction steps
- Include screenshots when helpful
- Ask questions if unclear

### 4. **Test Early and Often**
- Test as features are developed
- Don't wait until the end
- Test after each major change
- Verify fixes don't break other things

### 5. **Work as a Team**
- Share findings with developers
- Ask clarifying questions
- Help understand requirements
- Celebrate when bugs are fixed

---

## 🎓 WHY YOUR ROLE MATTERS

As Tester, your work impacts **5 marks** in the report section (Unit test code quality and matching testing approach):

- ✅ Well-designed test cases
- ✅ Clear testing strategy
- ✅ Comprehensive coverage
- ✅ Good bug reporting
- ✅ Professional documentation

**Your testing directly determines:**
- Quality of the software
- Confidence in the code
- Credibility of the team
- Marks for the testing section

---

## 📋 CHECKLIST FOR SUCCESS

By April 10, you should have:

- [ ] Testing strategy document
- [ ] 40-50 test cases documented
- [ ] All test cases executed
- [ ] Test results recorded (pass/fail)
- [ ] All defects reported
- [ ] All critical defects verified as fixed
- [ ] Performance metrics documented
- [ ] Testing section written for report
- [ ] Evidence of testing (screenshots, logs)
- [ ] Final regression testing completed

---

## 🚀 HOW TO IMPROVE YOUR WORK RIGHT NOW

### TODAY (March 24):
1. [ ] Read this guide completely
2. [ ] Create Testing_Strategy.md
3. [ ] List all features that need testing
4. [ ] Create test case template

### TOMORROW (March 25):
1. [ ] Design detailed test cases
2. [ ] Prepare test data
3. [ ] Set up test environment
4. [ ] Start documenting approach

### THIS WEEK (March 25-28):
1. [ ] Complete test case design
2. [ ] Begin executing tests
3. [ ] Report any issues found
4. [ ] Track defects

### NEXT WEEK (March 29-April 5):
1. [ ] Execute all remaining tests
2. [ ] Complete test documentation
3. [ ] Verify all fixes
4. [ ] Prepare report section

---

## ❓ QUESTIONS TO GUIDE YOUR TESTING

Ask yourself these questions:

1. **Functionality**: Does each feature work as designed?
2. **Data Validation**: Does it reject invalid input?
3. **Database**: Is data saved and retrieved correctly?
4. **Performance**: Is it fast enough?
5. **Security**: Is data protected?
6. **Usability**: Is it easy to use?
7. **Edge Cases**: What happens with unusual inputs?
8. **Integration**: Do all parts work together?
9. **Regression**: Did the last change break anything?
10. **Documentation**: Is everything documented for the report?

---

## 📞 COMMUNICATION WITH TEAM

Regular updates to give:

- **Daily**: "Tested X features, found Y issues, no blockers"
- **Weekly**: "Completed testing of [module], ready for next phase"
- **Before Deadline**: "Final regression test completed, all critical defects fixed"

---

**You have an important role! Your testing ensures quality and helps the team get excellent marks.** 💪

**Ready to start? Begin with creating your Testing_Strategy.md today!**
