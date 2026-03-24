# 📋 WHAT YOU GOTTA DO - COMPLETE BREAKDOWN

---

## 🎯 YOUR ROLE: TESTER (Quality Assurance)

**You are responsible for ensuring the application works correctly before submission.**

---

## 📅 TIMELINE: March 24 - April 10 (17 DAYS)

### Week 1: March 25-28 (THIS WEEK)
- **Goal**: Run first 30 test cases
- **Time**: 2-3 hours/day
- **Work**: Test basic functionality, find bugs
- **Deliverable**: Test results document with bugs found

### Week 2: March 29-April 5 (NEXT WEEK)  
- **Goal**: Run remaining 14 test cases, verify bug fixes
- **Time**: 2-3 hours/day
- **Work**: Complete testing, test data structure, test after fixes
- **Deliverable**: Updated test results, verified fixes

### Week 3: April 6-10 (FINAL WEEK)
- **Goal**: Final regression testing, sign-off
- **Time**: 1-2 hours/day
- **Work**: Re-run all tests, ensure no new bugs, final verification
- **Deliverable**: Final test report, signature of approval

---

## 🚀 WHAT YOU DO RIGHT NOW (March 24-25)

### TODAY - March 24

1. **Read Documentation** (50 minutes)
   - [ ] Read TESTER_ROLE_GUIDE.md (overview of your job)
   - [ ] Read TESTER_IMMEDIATE_ACTION_PLAN.md (weekly schedule)
   - [ ] Read TESTER_QUICK_REFERENCE.md (bookmarkable reference)
   - [ ] Skim SPECIFIC_TEST_CASES.md (know what you'll test)

2. **Set Up Testing Environment** (30 minutes)
   - [ ] Make sure application compiles
   - [ ] Make sure application runs
   - [ ] Make sure database is accessible
   - [ ] Create test flight manually to verify it works
   - [ ] Create file: `My_Test_Results.md` (to track results)

3. **Run First 3 Tests** (1 hour)
   - [ ] TC-001: Create flight with valid data
     - Expected: Flight saved, success message shown
     - Your job: Verify it works
   
   - [ ] TC-007: Create flight with arrival BEFORE departure
     - Expected: REJECTED with error message
     - Your job: Check if it's rejected (likely NOT rejected = BUG!)
   
   - [ ] TC-008: Create 25-hour flight
     - Expected: REJECTED (can't exceed 24 hours)
     - Your job: Check if it's rejected (likely NOT rejected = BUG!)

4. **Report Findings** (30 minutes)
   - [ ] Update My_Test_Results.md with results
   - [ ] Post to group chat:
     ```
     ✅ Started testing
     - TC-001: PASS
     - TC-007: FAIL (no validation for arrival time!)
     - TC-008: FAIL (no validation for flight duration!)
     
     🔴 CRITICAL ISSUES FOUND:
     Your application accepts invalid flight times.
     Need to add validation before continuing.
     ```

### Tomorrow - March 25

1. **Continue Testing** (2 hours)
   - Run TC-002 through TC-012 (Create flight tests)
   - Document each result
   - List any bugs found

2. **Daily Status** (15 minutes)
   - Post to group chat: Summary of what you tested, what passed/failed

---

## 🧪 WHAT TESTING MEANS

**You test by:**

1. **Opening the application**
2. **Trying to do something** (e.g., create a flight)
3. **Checking if it works correctly** (e.g., does it save?)
4. **Documenting the result** (e.g., "PASSED" or "FAILED")
5. **Reporting any issues** (e.g., "Should reject invalid data but doesn't")

**Example Test:**
```
Test: TC-001 (Create flight with valid data)

What I Do:
  1. Click "Add Flight"
  2. Enter: BA123, British Airways, New York, 10:00 to 14:00
  3. Click Save

What Should Happen:
  - Flight saved to database
  - Success message shown
  - Go back to flight list
  
What Actually Happened:
  - ✅ Flight saved
  - ✅ Success message shown
  - ✅ Went back to flight list

Result: ✅ PASS

Next Test: TC-002
```

---

## 🐛 WHAT TO DO WHEN YOU FIND A BUG

**Example: Application accepts invalid flight duration**

```
Step 1: Note the bug
- Test: TC-008
- What: Can create 25-hour flight (should reject)
- Issue: Duration validation missing

Step 2: Try it again to confirm
- Do the exact same steps
- Does it fail again?
- If yes, it's a real bug

Step 3: Report to developers
- Tell your group chat:
  "BUG FOUND: Duration validation not working
   Can create flight longer than 24 hours
   Please add validation to Flight.cs"

Step 4: Wait for fix
- Developer codes the fix
- Developer tells you it's fixed

Step 5: Retest
- Run TC-008 again
- Does it work now?
- If yes: BUG FIXED ✅
- If no: Still broken, report again
```

---

## 📊 YOUR TEST RESULTS FILE

**Create:** `My_Test_Results.md`

**Use this format:**

```markdown
# Testing Results - [Your Name]

Date Started: March 24, 2026
Progress: 3/44 tests completed

## Test Results

| Test # | Description | Status | Severity | Notes |
|--------|-------------|--------|----------|-------|
| TC-001 | Create valid flight | ✅ PASS | - | Works perfectly |
| TC-002 | Missing flight # | ✅ PASS | - | Shows error as expected |
| TC-007 | Arrival < Departure | ❌ FAIL | CRITICAL | No validation - BUG! |
| TC-008 | >24 hour flight | ❌ FAIL | CRITICAL | No validation - BUG! |

## Critical Issues Found

🔴 **Time Validation Missing**
- Tests: TC-007, TC-008
- Impact: Application accepts invalid flights
- Action: Need validation before continuing testing
- Status: Reported to developers on March 24

## Daily Progress

**March 24:**
- Tests run: 3
- Bugs found: 2
- Severity: CRITICAL
- Blockers: Yes - need validation fix

**March 25:**
- Tests run: [you fill this in]
- Bugs found: [you fill this in]
- Severity: [you fill this in]
- Blockers: [yes/no]
```

---

## 📋 THE 44 TESTS YOU'LL RUN

Organized by feature:

### **Feature 1: Create Flight (12 tests)**
- TC-001: Valid data → Should work ✅
- TC-002: Missing flight number → Should error ❌
- TC-003: Missing airline → Should error ❌
- TC-004: Missing destination → Should error ❌
- TC-005: Missing departure time → Should error ❌
- TC-006: Missing arrival time → Should error ❌
- TC-007: **Arrival before departure → Should error ❌** (CRITICAL!)
- TC-008: **Flight >24 hours → Should error ❌** (CRITICAL!)
- TC-009: Special characters in flight # → Check behavior
- TC-010: Very long flight # → Check behavior
- TC-011: Check default values
- TC-012: Empty gate (optional) → Should work ✅

### **Feature 2: View Details (5 tests)**
- TC-013: Valid flight ID → Shows details ✅
- TC-014: No ID provided → Shows list ✅
- TC-015: Invalid ID → Shows 404 ❌
- TC-016: Negative ID → Shows error ❌
- TC-017: String as ID → Shows error ❌

### **Feature 3: Edit Flight (7 tests)**
- TC-018: Change single field → Works ✅
- TC-019: Change multiple fields → Works ✅
- TC-020: Make arrival before departure → Error ❌
- TC-021: Invalid ID → Shows 404 ❌
- TC-022: ID mismatch → Security error ❌
- TC-023: Duration >24 hours → Error ❌
- TC-024: Clear gate field → Works ✅

### **Feature 4: Delete Flight (6 tests)**
- TC-025: Confirmation page shows → Works ✅
- TC-026: Confirmed deletion → Removed ✅
- TC-027: Cancel deletion → Not removed ✅
- TC-028: Invalid ID → Shows 404 ❌
- TC-029: No ID provided → Shows error ❌
- TC-030: Non-existent flight → No error ✅

### **Feature 5: Search & Filter (5 tests)**
- TC-031: Find by flight number
- TC-032: No results found
- TC-033: Partial search
- TC-034: Filter by status
- TC-035: Filter by terminal

### **Feature 6: Database (4 tests)**
- TC-036: Data persists after save
- TC-037: Updates persist
- TC-038: Deleted data gone
- TC-039: Concurrent access handled

### **Feature 7: Data Structure (5 tests)**
- TC-040: Insert operation
- TC-041: Search operation
- TC-042: Delete operation
- TC-043: Handle duplicates
- TC-044: Performance with large dataset

---

## 🎯 CRITICAL TESTS YOU MUST FOCUS ON

These MUST pass:

1. **TC-007**: Arrival before departure must be REJECTED
   - If it's accepted: 🔴 CRITICAL BUG
   
2. **TC-008**: Flight longer than 24 hours must be REJECTED
   - If it's accepted: 🔴 CRITICAL BUG

**These are blocking issues.** Don't continue past TC-008 until these work!

---

## 🔄 YOUR DAILY WORKFLOW

### Morning (30 minutes)
1. Open application
2. Check if any bugs were fixed overnight
3. Plan tests for today

### Daytime (2-3 hours)
1. Run 5-10 test cases
2. Document results in My_Test_Results.md
3. List any new bugs found

### Evening (30 minutes)
1. Post daily status to group chat
2. Summarize tests completed
3. Highlight any blockers
4. Let developers know if they need to fix things

---

## ✅ WEEKLY DELIVERABLES

### End of Week 1 (March 28)
- [ ] Completed TC-001 through TC-030
- [ ] Documented all results
- [ ] Found all major bugs
- [ ] Reported bugs to developers
- [ ] File: My_Test_Results_Week1.md

### End of Week 2 (April 5)
- [ ] Completed TC-031 through TC-044
- [ ] Verified all bug fixes
- [ ] Re-tested failed cases
- [ ] No regressions found
- [ ] File: My_Test_Results_Week2.md

### End of Week 3 (April 10)
- [ ] Final regression test complete
- [ ] All tests passing
- [ ] Sign-off document
- [ ] File: My_Test_Results_Final.md
- [ ] Status: "✅ SYSTEM READY FOR SUBMISSION"

---

## 💼 YOUR RELATIONSHIP WITH DEVELOPERS

**You are NOT enemies. You are teammates.**

When you find a bug:
1. **Tell them clearly** - "Here's what I did, here's what should happen, here's what actually happened"
2. **Don't blame** - "I found an issue we need to fix" (not "You messed up")
3. **Help them fix it** - "Let me know when you've fixed it, I'll test it"
4. **Verify the fix** - Test again to make sure it works

When they fix a bug:
1. **Test it immediately** - Don't wait
2. **Confirm it's fixed** - Tell them "Verified - works now"
3. **Check for new bugs** - Did the fix break something else?

---

## 📞 COMMUNICATION TEMPLATES

### Daily Status
```
✅ MARCH 25 TESTING UPDATE:
- Completed: TC-002 to TC-012 (11 tests)
- Status: 9 PASS, 2 FAIL
- Critical Issues: 2 (time validation)
- Next: Wait for validation fix, then retest
- Blockers: Yes - need time validation before continuing
```

### Bug Report
```
🐛 BUG REPORT - HIGH SEVERITY

Test Case: TC-007
Title: No validation for arrival before departure

Reproduction Steps:
1. Create new flight
2. Set departure: 14:00
3. Set arrival: 10:00 (earlier!)
4. Click Save

Expected Behavior: Rejected with error

Actual Behavior: Saved successfully

Impact: Application accepts invalid data

Assigned To: [Developer Name]
Status: Open
```

### Verification
```
✅ VERIFIED FIX

Bug: TC-007 time validation
Developer: [Name]
Fixed: Yes
Result: Now correctly rejects arrival before departure
Retested: Yes, works perfectly
Status: CLOSED
```

---

## 🎓 WHY YOUR WORK MATTERS

**Marks Breakdown:**

Your testing contributes to:
- **Code Quality**: 15% of marks
- **Testing Section**: 10% of marks  
- **Overall Grade**: 25% depends on code quality

**If code has bugs:**
- Report found: +marks for catching bugs
- Report verified fixed: +marks for quality assurance

**If you skip testing:**
- Bugs not found: -marks for quality
- Submit broken code: -marks for functionality

**Your job = directly impacts final grade.**

---

## 🚨 IF YOU GET STUCK

**Application won't start?**
- Tell all developers immediately
- Can't test if app doesn't run

**Don't know how to test something?**
- Read SPECIFIC_TEST_CASES.md - has detailed instructions
- Ask your Scrum Master (team lead)
- Ask another developer

**Found a bug but not sure if it's a bug?**
- Ask developers: "Is this expected behavior?"
- If they say "no", it's a bug
- Report it

**Test keeps failing but you think it's wrong?**
- Test it multiple times
- Ask another team member to test
- If they get same result, it's real

---

## ✨ TESTING MINDSET

**You are testing like a user would:**
- Don't know the code
- Don't know what "should" happen (first time)
- Just trying to create flights
- Expecting it to work correctly
- Should reject bad data

**You are NOT:**
- Trying to break the code
- Looking for gotchas
- Being mean to developers
- Finding excuses

**You ARE:**
- Being helpful
- Finding issues early
- Improving code quality
- Helping team succeed

---

## 🎯 YOUR SUCCESS CRITERIA

By April 10, you will have delivered:

- [ ] 44 test cases executed
- [ ] All results documented
- [ ] All bugs found and reported
- [ ] All critical bugs verified as fixed
- [ ] No regressions in final testing
- [ ] Signed statement: "System ready for submission"
- [ ] Test results section for PDF report

**If all checked: ✅ YOU'VE DONE YOUR JOB WELL**

---

## 🚀 START TODAY

**Right now:**
1. Open TESTER_QUICK_REFERENCE.md
2. Do the "TODAY" checklist
3. Run TC-001, TC-007, TC-008
4. Document results
5. Report findings

**Tomorrow:**
1. Run TC-002 through TC-012
2. Document results
3. Identify bugs
4. Report to team

**This week:**
1. Complete 30 test cases
2. Document everything
3. Report all bugs
4. Wait for fixes

**You've got this!** 💪

---

**Questions? Re-read this document. It has everything you need.**

**Ready? Let's go test!** 🧪
