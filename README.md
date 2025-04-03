
# Encompass Work Tracker

A modular, multi-window desktop application built in C# using WPF (.xaml) to securely track client cases and contact attempts. Designed as a GDPR-compliant internal tool for charities and care teams, it replaces the need for third-party platforms like Airtable by keeping sensitive data local and structured. This was developed entirely in my own time to solve a real-world workflow bottleneck — and was planned with future expansion, multiple user roles, and administrative permissions in mind.

---

## 💡 Why I Built This

At Parents 1st UK, staff were manually duplicating sensitive client information from Airtable into UK-hosted documents due to GDPR concerns. This not only wasted time, but introduced a risk of error and inconsistency.

Rather than continuing with this inefficient workflow, I designed and built a prototype of a custom work tracker that could:
- Eliminate redundant data entry
- Keep all sensitive data on UK-hosted systems
- Support future expansion to include case management, task assignment, and multi-user roles

---

## 🧠 Key Features

### ✅ Core Systems
- **WPF/XAML UI with multiple windows** for distinct workflows
- **Authentication system** with login/register support
- **Modular architecture** separating UI, models, and services
- **GDPR-aware data handling** with archiving reasons and limited exposure

### 📁 Case Management
- Create, view, edit, and archive client cases
- Store structured data including name, status, notes, and timestamps
- Use of `CaseModel` and `CaseStatusService` to cleanly handle state

### 📞 Contact Attempt Logging
- Log contact events with date, type (phone, email, etc.), and notes
- Assign attempts to specific cases for history tracking
- Uses `ContactAttempt` and `ContactAttemptService` for separation of logic and UI

### 🔐 User Authentication
- Register and login system with local credential validation
- Prepares groundwork for multi-user access control
- Designed with team hierarchy in mind

### 🧱 Administrative Vision (Future-Oriented)
- Assign cases to users based on role
- Implement permission levels (staff, coordinator, manager)
- Generate activity reports and logs
- Integrate with local databases (SQLite) or secure cloud storage

---

## 🛠 Tech Stack

- **Language**: C#
- **Framework**: WPF (.xaml)
- **Architecture**: MV-like structure (manual implementation)
- **Data Handling**: In-memory data model (for prototype)
- **Tooling**: Visual Studio, .NET Framework

---

## 👨‍💻 My Role

I was the sole developer and architect. From initial concept to implementation, every part of this system was designed, built, and tested by me — outside of my official work hours.

This was also my **first time using WPF/XAML**, and I designed the UI architecture and bindings while learning the framework in real time. The app is built with modularity, extensibility, and security in mind.

---

## 🚧 Project Status

This is a **functional prototype** and proof of concept. I made the decision to pause development due to internal organisational changes and a loss of faith in the direction of the company. Regardless, it stands as a clear representation of my ability to:

- Solve real-world problems through custom software
- Learn and apply new technologies rapidly
- Design scalable systems with user roles and compliance in mind

---

## 📁 Project Structure

```
├── App.xaml / App.xaml.cs              # App startup and shared resources
├── MainWindow.xaml / .cs              # Navigation/dashboard window
├── LoginWindow.xaml / .cs             # User login
├── RegisterWindow.xaml / .cs          # User registration
├── CaseModel.cs                       # Data model for cases
├── CaseStatusService.cs              # Business logic for case states
├── NewCaseWindow.xaml / .cs           # Create new case
├── CasesWindow.xaml / .cs             # View/edit existing cases
├── CaseDetailsWindow.xaml / .cs       # Inspect full case details
├── ArchiveReasonWindow.xaml / .cs     # Archive with reason
├── ContactAttempt.cs                 # Contact record model
├── ContactAttemptService.cs          # Handles contact data logic
├── ContactAttemptWindow.xaml / .cs    # UI for adding contact attempts
├── UpdateManager.cs                  # Planned real-time UI sync
```

---

## 🧪 Potential Future Features
- CSV/Excel export for offline reporting
- SQLite database integration
- Role-based dashboards
- Notification system for overdue cases
- Print-friendly reports for meetings or audits

---

## 🙌 Final Thoughts

Even though this project didn’t get fully deployed, it’s a reflection of how I work:  
→ **Identify the problem**  
→ **Design for real users**  
→ **Build scalable systems**  
→ **Stay adaptable and solution-focused**

If you’re looking for someone who not only codes — but **thinks ahead**, **designs for people**, and learns on the fly — this project is one of the best examples of how I work.
