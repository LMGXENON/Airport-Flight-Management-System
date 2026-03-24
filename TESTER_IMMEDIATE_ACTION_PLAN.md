# 🚀 YOUR IMMEDIATE ACTION PLAN - CST2550 COURSEWORK

**📅 Date: March 24, 2026**  
**⏰ Time Until Deadline: 17 DAYS (April 10)**  
**✅ Status: You have clear documentation. NOW BUILD.**

---

## 🎯 YOUR TESTER ROLE - WHAT YOU DO RIGHT NOW

You are the **Quality Assurance Tester**. Your job is to ensure everything works before submission.

### YOUR CHECKLIST (TODAY - March 24)

- [ ] **1. Read SPECIFIC_TEST_CASES.md** (Takes 30 mins)
  - This file has ALL the tests you need to run
  - 44 specific test cases for your flight system
  
- [ ] **2. Start Testing TODAY** (Pick 6 tests)
  - TC-001: Create valid flight
  - TC-002: Missing flight number  
  - TC-007: Arrival before departure (TIME VALIDATION - CRITICAL!)
  - TC-013: View flight details
  - TC-025: Delete confirmation
  - TC-026: Actual deletion
  
- [ ] **3. Document Your Results** (Create a file)
  - Create: `My_Test_Results.md`
  - Record: Pass/Fail for each test
  - Example:
    ```
    ✅ TC-001: PASS - Flight created successfully
    ✅ TC-002: PASS - Validation error shown
    ❌ TC-007: FAIL - No validation for arrival before departure!
    ⚠️ Need to report this bug to developers
    ```

---

## 🔍 CRITICAL VALIDATION ISSUES - CHECK THESE FIRST

Your code needs to validate flight times. **Check if these work:**

### Test 1: Can you create a flight where Arrival is BEFORE Departure?
```
Flight Number: BA123
Airline: British Airways
Destination: NYC
Departure: 2026-03-25 14:00 (2 PM)
Arrival: 2026-03-25 10:00 (10 AM) ← EARLIER!
Expected: ❌ REJECTED with error
Actual: ???
```

**If this PASSES (accepts bad data)**: 🔴 CRITICAL BUG to report

### Test 2: Can you create a 25-hour flight?
```
Departure: 2026-03-25 10:00
Arrival: 2026-03-26 14:00 (25 hours later)
Expected: ❌ REJECTED with error "Cannot exceed 24 hours"
Actual: ???
```

**If this PASSES (accepts 25 hour flight)**: 🔴 CRITICAL BUG to report

---

## 📋 YOUR WEEKLY TESTING SCHEDULE

### Week 1: March 25-28 (This Week)
**Goal: Test all basic functionality**

**Mon-Tue (March 25-26):**
- [ ] Run TC-001 to TC-012 (Create flight tests)
- [ ] Time: ~2 hours
- [ ] Document all results

**Wed-Thu (March 27-28):**
- [ ] Run TC-013 to TC-030 (View, Edit, Delete, Search)
- [ ] Time: ~2 hours
- [ ] Create summary of bugs found

### Week 2: March 29-April 5 (Next Week)
**Goal: Verify fixes and test data structure**

**Mon-Wed (March 31-April 2):**
- [ ] Re-test all failed tests from Week 1
- [ ] Verify developers fixed bugs

**Thu-Fri (April 3-5):**
- [ ] Test custom data structure (TC-040 to TC-044)
- [ ] Performance testing

### Week 3: April 6-10 (Final Week)
**Goal: Final verification before deadline**

**Mon-Tue (April 7-8):**
- [ ] Full regression test (all tests again)
- [ ] Ensure no new bugs introduced

**Wed-Thu (April 9-10):**
- [ ] Help with video demo preparation
- [ ] Final checks before submission

---

## 📊 YOUR TEST RESULT TRACKING

Create this file: `My_Test_Results.md`

```markdown
# My Testing Results - [Your Name]

## Current Status
- Date Started: March 24
- Tests Completed: 0/44
- Tests Passed: 0
- Tests Failed: 0
- Bugs Found: 0

## Test Results by Feature

### Create Flight Tests (TC-001 to TC-012)
| Test ID | Description | Status | Date | Notes |
|---------|-------------|--------|------|-------|
| TC-001 | Valid flight | [ ] | | |
| TC-002 | Missing flight # | [ ] | | |
| TC-003 | Missing airline | [ ] | | |
| TC-007 | Arrival < Departure | [ ] | | CRITICAL |
| TC-008 | Duration > 24 hrs | [ ] | | CRITICAL |

### Bugs Found
- **BUG-001**: [Description]
- **BUG-002**: [Description]

## Critical Issues ⚠️
- Time validation not working (TC-007, TC-008)

## Sign-off
Tested by: [Your Name]
Date: March 25, 2026
```

---

## 🐛 HOW TO REPORT BUGS TO DEVELOPERS

When you find a bug, tell developers IMMEDIATELY:

```
❌ BUG REPORT - CRITICAL

Test: TC-007 (Arrival before Departure)
What I Did:
  - Created flight with departure 14:00, arrival 10:00
  - Clicked Save

What Should Happen:
  - System rejects with error message
  - Shows: "Arrival time must be after departure time"
  - Flight is NOT saved

What Actually Happened:
  - Flight was saved to database!
  - No error message shown
  - BAD DATA in system

Who to Tell:
  - Tell the developer who codes Flight validation
  - Send this message to your group chat
  - Flag as BLOCKING ISSUE
```

---

## 💻 YOUR TEAM ROLES SUMMARY

**5 People, Clear Jobs:**

1. **Scrum Master** (Abdullahi?) - Keeps team organized
   - What to do: Schedule daily standups, track progress, manage timeline
   
2. **Secretary** - Documents everything
   - What to do: Record team meetings, track decisions, prepare report
   
3. **Developer 1** - Write the custom data structure
   - What to do: Design and code AVL Tree or Binary Search Tree
   
4. **Developer 2** - Write unit tests
   - What to do: Create NUnit/MSTest project with 20+ tests
   
5. **YOU - Tester** - Verify everything works
   - What to do: Run tests, find bugs, verify fixes (THIS IS YOUR JOB!)

---

## 🎯 THE BIG PICTURE - WHAT YOUR TEAM NEEDS TO DELIVER

By April 10, you need:

### ✅ CODE (Already have most of this)
- [x] Working flight management application
- [x] Database (SQLite)
- [ ] **Custom data structure** (AVL Tree, BST, or Hash Table) - NOT DONE YET
- [ ] Unit test project (NUnit/MSTest) - NOT DONE YET

### ✅ DOCUMENTATION (Partially done)
- [x] Code (compiles and runs)
- [x] README.md (exists but incomplete)
- [ ] **PDF Report** (5-7 pages) - NOT DONE
- [ ] **SQL database script** (.sql file) - NOT DONE
- [ ] **Project management docs** (sprint planning, meeting notes) - NOT DONE

### ✅ VIDEO & PRESENTATION
- [ ] **MP4 video** (≤6 minutes) - NOT DONE
- [ ] Shows code working AND explains implementation

### ✅ YOUR TESTING
- [ ] **Test cases documented** (44 test cases in SPECIFIC_TEST_CASES.md) ✅
- [ ] **Test results recorded** (YOUR JOB - start today)
- [ ] **Defects logged** (YOUR JOB)
- [ ] **All critical bugs verified as FIXED**

---

## 📅 PRIORITY ORDER (WHAT TO FOCUS ON)

### 🔴 MUST DO THIS WEEK (March 25-28)

1. **Start Testing** (You - start TODAY)
   - Run the 44 test cases
   - Find bugs early
   - Report to developers
   - Estimated: 2-3 days of work

2. **Implement Custom Data Structure** (Developers - must start NOW)
   - Design (pseudocode)
   - Implement in C#
   - Integrate with application
   - Estimated: 2-3 days per developer

3. **Create Unit Test Project** (Developers - must start NOW)
   - Add NUnit/MSTest project to solution
   - Write 20+ test cases
   - Verify all tests pass
   - Estimated: 2-3 days per developer

### 🟠 MUST DO NEXT WEEK (March 29-April 5)

4. **Fix All Bugs You Found** (Developers + You verify)
   - You report bugs
   - Developers fix them
   - You verify fixes work
   - No regressions

5. **Write PDF Report** (Secretary + all team)
   - Introduction & Overview
   - Design & Architecture
   - Time-Complexity Analysis
   - Testing Methodology
   - Conclusion & References
   - Estimated: 2-3 days

6. **Create SQL Script** (Developer)
   - Export database schema
   - Add sample data
   - Estimated: 1 day

7. **Project Management Docs** (Secretary + all)
   - Team roles (already done - see your TESTER_ROLE_GUIDE.md!)
   - Sprint planning
   - Meeting minutes
   - Estimated: 1-2 days

### 🟡 MUST DO FINAL WEEK (April 6-10)

8. **Record Video Demo** (All team - 5-10 mins to record)
   - Show app running
   - Explain code
   - Demonstrate understanding
   - Estimated: 1-2 days

9. **Final Testing & Regression** (You)
   - Re-run all tests one more time
   - Verify no new bugs
   - Sign-off that system ready
   - Estimated: 1 day

10. **Final Review & Submission** (Everyone)
    - Check all files present
    - Review report
    - Submit before April 10 11:59 PM

---

## 🚦 YOUR TESTING STATUS TRACKER

Keep this updated daily:

```markdown
# March 24 Status: Not Started
- Tests Run: 0/44
- Bugs Found: 0
- Status: Ready to begin

# March 25 Status: [You fill this in]
- Tests Run: 6/44
- Bugs Found: 2
- Status: Testing basic functionality

# March 26 Status:
- Tests Run: 12/44
- Bugs Found: 3
- Status: Testing validation

# ... continue each day
```

---

## ✅ YOUR RESPONSIBILITIES AS TESTER

You need to:

1. **Run the tests** in SPECIFIC_TEST_CASES.md
2. **Document results** in MY_Test_Results.md
3. **Report bugs** immediately to group chat
4. **Verify fixes** when developers fix bugs
5. **Ensure no regressions** (that fixes didn't break something else)
6. **Sign off** that system is ready for submission

---

## 🎯 WHAT YOU DO TODAY (March 24)

### RIGHT NOW:

1. **Read these files** (30 minutes):
   - TESTER_ROLE_GUIDE.md (the overview)
   - SPECIFIC_TEST_CASES.md (the detailed tests)

2. **Set up your testing** (30 minutes):
   - Create: `My_Test_Results.md`
   - Create test environment (running application)
   - Get database ready

3. **Run first 6 tests** (1-2 hours):
   - TC-001: Create valid flight ✅
   - TC-002: Missing flight number
   - TC-007: Arrival before departure (⚠️ CRITICAL!)
   - TC-013: View details
   - TC-025: Delete confirmation
   - TC-026: Delete confirmed

4. **Report findings** (30 minutes):
   - Update My_Test_Results.md
   - Post to group chat: "Started testing, here are initial findings..."
   - List any bugs found

---

## 📞 COMMUNICATION

**Daily status to give your team:**

```
✅ TODAY'S TESTING SUMMARY:
- Completed: TC-001 to TC-006 (6 tests)
- Status: All PASSED
- Bugs Found: 0
- Blockers: None
- Next: Continue with TC-007 (time validation)
```

**Weekly summary to give team:**

```
📊 WEEK 1 TESTING REPORT:
- Tests Completed: 25/44
- Passed: 22
- Failed: 3
- Critical Bugs: 2 (time validation)
- Status: On Track
- Next Week: Continue testing + verify bug fixes
```

---

## 🎓 WHY THIS MATTERS

Your testing is worth **marks** because:

1. **Code Quality** - Your testing proves code works
2. **Bug Discovery** - You catch issues before submission
3. **Report Content** - Your test results go in the PDF report
4. **Team Confidence** - Team knows system is ready

**Teams with good testing scores better on coursework.**

---

## 🚀 LET'S GO!

### Your immediate mission:
**Start testing TODAY. Run 6 quick tests. Report findings. Move to the next set.**

### Your timeline:
- **This week (March 25-28)**: Test 30+ cases
- **Next week (March 29-April 5)**: Verify fixes + test data structure
- **Final week (April 6-10)**: Regression test + sign-off

### Your goal:
**By April 10, deliver a fully tested, working flight management system.**

---

## ❓ IF YOU HAVE QUESTIONS

**Before you test, clarify:**
1. Is there a running instance of the application?
2. Do you have database access?
3. Who is your contact for reporting bugs?
4. When should you report bugs? (ASAP or daily?)

**If the code is missing time validation:**
- This is a bug that MUST be fixed
- Tell developers immediately
- Do NOT proceed past TC-008 until this is fixed
- This is CRITICAL for the application to work correctly

---

## 📋 FINAL CHECKLIST

Before you start testing:
- [ ] Application compiles without errors
- [ ] Application runs and loads
- [ ] Database is created and accessible
- [ ] You can create a test flight manually
- [ ] You have SPECIFIC_TEST_CASES.md file
- [ ] You have created My_Test_Results.md file
- [ ] You have a way to document pass/fail
- [ ] You know who to tell if you find bugs

**Once all checked, START TESTING!** 🧪

---

**Your role is critical. Your testing ensures submission quality. GO! 💪**
