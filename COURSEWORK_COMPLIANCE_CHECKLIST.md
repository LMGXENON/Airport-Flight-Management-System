# CST2550 Coursework Compliance Checklist

## 📋 Project Status Analysis

**Current Date**: March 24, 2026  
**Deadline**: Friday, April 10, 2026 @ 5:30 PM  
**Days Remaining**: 17 days

---

## ✅ WHAT YOU HAVE

### Repository & Git
- ✅ GitHub repository created and initialized
- ✅ `.gitignore` configured
- ✅ Regular commits in place
- ✅ Public repository (accessible)

### Source Code
- ✅ C# .NET project structure
- ✅ Controllers (FlightController, HomeController, AccountController)
- ✅ Models (Flight, Flight-related ViewModels)
- ✅ Views (Razor templates)
- ✅ Database context (ApplicationDbContext)
- ✅ Services (AeroDataBoxService, FlightSearchService, FlightSyncService)

### Testing
- ✅ Test reports created (COMPLETE_TEST_REPORT.md)
- ✅ Test results documented

### Technical Implementation
- ✅ Web API/ASP.NET Core implementation
- ✅ Database integration (SQLite)
- ✅ SignalR for real-time updates
- ✅ Background services
- ✅ Data synchronization

---

## ❌ WHAT'S MISSING (CRITICAL)

### 1. **Project Management Documentation** ❌ CRITICAL
   - ❌ No project management evidence
   - ❌ No sprint planning documents
   - ❌ No daily standup meeting minutes
   - ❌ No team role assignments document
   - ❌ No project timeline/Gantt chart
   - ❌ No sprint backlog
   - ❌ No product backlog

### 2. **Custom Data Structure** ❌ CRITICAL
   - ❌ No custom data structure implementation
   - ❌ No justification for data structure choice
   - ❌ **This is a KEY requirement** - you're using standard .NET collections (List<T>, DbSet<T>)
   - ❌ No pseudo-code design
   - ❌ No time-complexity analysis document

### 3. **Unit Tests** ❌ CRITICAL
   - ❌ No unit test project (NUnit or MSTest)
   - ❌ No test code files (.cs test classes)
   - ❌ Test reports exist but no actual test code implementation
   - ❌ No test coverage metrics

### 4. **SQL Database Script** ❌ IMPORTANT
   - ❌ No SQL script file (migrations exist but no .sql file)
   - ❌ No sample data creation script
   - ❌ No database schema documentation

### 5. **Report (PDF)** ❌ CRITICAL
   - ❌ No PDF report submitted
   - ❌ Must include:
     - Introduction
     - Design (with data structure justification)
     - Time-complexity analysis
     - Testing approach
     - Conclusion
     - References (Harvard style)
   - ❌ Must be ≤7 pages (excluding references, cover, contents)
   - ❌ Font size 11-12 point

### 6. **Video Demonstration (MP4)** ❌ CRITICAL
   - ❌ No video submitted
   - ❌ Must be ≤6 minutes (only first 5 minutes marked)
   - ❌ Should demonstrate understanding of implementation
   - ❌ Should explain key functionality, not just show UI

### 7. **Team Role Documentation** ❌ IMPORTANT
   - ❌ No documented role assignments:
     - Team Leader (Scrum Master)
     - Secretary
     - 2x Developers
     - Tester
   - ❌ No justification for role selection

### 8. **Meeting Minutes** ❌ IMPORTANT
   - ❌ No documented team meetings
   - ❌ No sprint planning records
   - ❌ No daily standup notes
   - ❌ No decision logs

### 9. **README.md** ⚠️ INCOMPLETE
   - ✅ File exists
   - ❌ Missing compilation instructions
   - ❌ Missing usage instructions
   - ❌ Missing setup/installation guide
   - ❌ Missing team member information

### 10. **AI Integration** ⚠️ BONUS (Not Required but Extra Marks)
   - ✅ You have DeepSeek AI integration (API calls visible)
   - ❌ But not documented as meeting this requirement
   - ❌ Missing documentation of AI agent features

---

## 🎯 PRIORITY ACTIONS (NEXT 17 DAYS)

### WEEK 1 (By March 31, 2026) - MUST DO
**Priority: CRITICAL**

1. **[ ] Create Unit Test Project**
   - Add NUnit or MSTest project to solution
   - Create test classes for:
     - Flight model validation
     - Search algorithms
     - Custom data structure operations
   - Aim for 20+ test cases
   - Time: 2-3 days

2. **[ ] Implement Custom Data Structure**
   - Design custom data structure (not using List<T> or DbSet<T>)
   - Options:
     - Binary Search Tree (BST)
     - Hash Table
     - Trie (for airline names, flight numbers)
     - Custom AVL Tree
   - Include add, remove, search operations
   - Time: 2-3 days

3. **[ ] Create SQL Database Script**
   - Export/create `.sql` file with:
     - CREATE TABLE statements
     - Sample data INSERT statements
     - Proper schema documentation
   - Time: 1 day

4. **[ ] Document Time-Complexity Analysis**
   - Analyze your custom data structure operations
   - Create document with:
     - Big-O notation
     - Algorithm complexity tables
     - Justification for data structure choice
   - Time: 1-2 days

### WEEK 2 (By April 7, 2026) - MUST DO
**Priority: CRITICAL**

5. **[ ] Create Comprehensive Report (PDF)**
   - 5-7 pages max
   - Sections:
     - Introduction (project description, not task description)
     - Design (data structure + algorithms with pseudocode)
     - Time-complexity analysis
     - Testing approach
     - Conclusion (summary, limitations, reflection)
     - References (Harvard style)
   - Time: 2-3 days

6. **[ ] Create Video Demonstration**
   - Record ≤6 minutes (first 5 mins marked)
   - Show:
     - Program running
     - Key features working
     - Explain implementation approach
     - Demonstrate understanding of code
   - DO NOT: Just show UI or read code line-by-line
   - Time: 1 day (including recording/editing)

7. **[ ] Create Project Management Documentation**
   - Team role assignments with justification
   - Sprint planning documents
   - Backlog items
   - Meeting minutes (create retrospectively if needed)
   - Time: 1-2 days

### FINAL WEEK (By April 10, 2026) - SUBMISSION
**Priority: CRITICAL**

8. **[ ] Update README.md**
   - How to compile and run
   - Usage instructions
   - Team member names and roles
   - Data structure explanation
   - Time: 2 hours

9. **[ ] Final Code Review**
   - Ensure code compiles and runs
   - Verify no hardcoded file paths
   - Check code quality
   - Time: 1 day

10. **[ ] Submission Checklist**
    - All files in repository
    - Repository is public
    - .txt file with repository URL ready
    - All deadlines met
    - Time: 2 hours

---

## 📊 MISSING ITEMS SUMMARY

| Item | Status | Impact | Days to Fix |
|------|--------|--------|------------|
| Custom Data Structure | ❌ MISSING | CRITICAL | 2-3 |
| Unit Tests | ❌ MISSING | CRITICAL | 2-3 |
| PDF Report | ❌ MISSING | CRITICAL | 2-3 |
| Video Demo | ❌ MISSING | CRITICAL | 1-2 |
| SQL Script | ❌ MISSING | IMPORTANT | 1 |
| Time-Complexity Analysis | ❌ MISSING | IMPORTANT | 1-2 |
| Project Management Docs | ❌ MISSING | IMPORTANT | 1-2 |
| Meeting Minutes | ❌ MISSING | IMPORTANT | 1 |
| README (Complete) | ⚠️ INCOMPLETE | MEDIUM | 2 hours |
| Team Roles Document | ❌ MISSING | IMPORTANT | 1 |

---

## 🚨 RISK ASSESSMENT

### CRITICAL ISSUES
1. **No Custom Data Structure**
   - This is a KEY requirement for the coursework
   - Marks will be significantly limited without this
   - Current use of standard collections won't meet criteria

2. **No Unit Tests**
   - Coursework specifically requires NUnit or MSTest
   - Test reports don't substitute for actual test code
   - Will lose 5 marks for code section

3. **No Report**
   - Report is worth significant marks
   - Cannot pass without it
   - Will lose 20+ marks

4. **No Video Demonstration**
   - "No marks will be awarded for code without a demonstration video"
   - This is a hard requirement
   - Could lose ALL code marks without it

### COMPLIANCE ISSUES
- Your current Flight/Airline system doesn't clearly align with coursework brief
- No evidence of comprehensive data structure design
- No pseudo-code documentation
- No time-complexity analysis

---

## ⏰ REALISTIC TIMELINE

### What Can Be Done in 17 Days
- ✅ Custom data structure implementation: YES (2-3 days)
- ✅ Unit tests: YES (2-3 days)
- ✅ PDF report: YES (2-3 days)
- ✅ Video demo: YES (1-2 days)
- ✅ Project management docs: YES (1-2 days)
- ✅ SQL script: YES (1 day)
- ⚠️ Code refactoring: PARTIAL (may need adjustments)

**Total Time Needed**: 10-15 days work
**Days Available**: 17 days
**Status**: ACHIEVABLE but TIGHT ⏱️

---

## 📋 ACTION PLAN

### IMMEDIATE (Next 24 hours)
1. [ ] Meet with team and assign roles
2. [ ] Create project management documentation
3. [ ] Define custom data structure design
4. [ ] Create unit test plan

### THIS WEEK
1. [ ] Implement custom data structure
2. [ ] Create unit tests (NUnit/MSTest)
3. [ ] Create SQL database script
4. [ ] Document time-complexity analysis

### NEXT WEEK
1. [ ] Write PDF report
2. [ ] Record video demonstration
3. [ ] Complete all documentation
4. [ ] Code review and testing

### FINAL WEEK
1. [ ] Final submission preparation
2. [ ] Verify all requirements met
3. [ ] Make repository public
4. [ ] Submit before deadline

---

## 💡 RECOMMENDATIONS

### DO THIS FIRST
1. **Implement custom data structure** (e.g., AVL Tree or Hash Table for flights)
2. **Write unit tests** with NUnit
3. **Create SQL script** with sample data
4. **Document time-complexity** for your algorithms
5. **Write report** with all required sections

### CODE CHANGES NEEDED
- Replace standard List<Flight> with custom data structure
- Implement custom search/sort algorithms
- Update database access to use custom structure
- Add configuration for input file name (don't hardcode)

### DOCUMENTATION NEEDED
- Project management folder with meeting minutes
- Design document with pseudocode
- Time-complexity analysis spreadsheet
- Team roles assignment document
- README with full instructions

---

## 🎯 MARK ALLOCATION IMPACT

**Without Missing Items**: ~40% of marks (severe penalties)
**With All Items**: 100% of marks possible

### Breakdown
- Project Management: 0/30 (MISSING)
- Report Sections: 0/20 (MISSING)
- Code Quality: 5/10 (present but below standard)
- SQL: 0/5 (MISSING)
- Implementation: 10/15 (present but not with custom data structure)
- Requirements: 8/15 (partial match)
- Unit Tests: 0/5 (MISSING)

**Current Score (if submitted now)**: ~23/100 (23%)
**Potential Score (if all completed)**: 85+/100 (85%+)

---

## ✨ NEXT STEP

**I can help you:**
1. Design a custom data structure (BST, AVL, Hash Table, etc.)
2. Create the unit test project and test code
3. Generate the SQL script with sample data
4. Write pseudocode for algorithms
5. Create project management documentation
6. Structure the PDF report

**Which should we start with?**
