# Encompass Work Tracker

A custom-built task and data tracking desktop app developed in C# using WPF (.xaml). Created as an internal solution to reduce workload and ensure compliance with UK data protection standards, this tool replaced the need for duplicate data entry from Airtable into secure local systems.

## 🧠 The Problem It Solves
At Parents 1st UK, personal and sensitive data was initially being logged and managed using Airtable — a U.S.-based platform. Due to differing data protection regulations and GDPR compliance, staff were required to manually duplicate sensitive information into UK-hosted documents, doubling the administrative workload.

This tool was designed as a secure alternative, streamlining the process and keeping all data within a compliant, local system.

## 🛠 Tech Stack
- C# (.NET Framework)
- WPF / XAML (first-time use)
- JSON Serialization
- Asynchronous programming (async/await)
- Modular architecture

## 🚀 Key Features
- 📋 Task tracking with custom data model
- 🔐 Data sanitisation for GDPR safety
- 🔁 Asynchronous task loop
- 🧹 Input cleaning to reduce human error
- 📄 Local data persistence for offline safety
- 📁 Modular and extensible structure
- 🪵 Logging system for traceability

## 👨‍💻 My Role
I was the sole developer and architect of this solution. I identified the issue, proposed a digital fix, and built the tool entirely from scratch using C# and WPF — both designing the system and writing all logic and UI code.

This project also marked my **first experience using XAML and WPF**, expanding my development toolkit beyond Unity and into desktop software design.

## 📁 Project Structure
- `WorkTracker.cs` – Manages the async task loop and core logic
- `Sanitizer.cs` – Handles input validation and safety
- `Logger.cs` – Logs actions and events to a local file
- `Models/` – Contains task models and data structures
- `.xaml` – UI layer using Windows Presentation Foundation

## 📝 Notes
This project highlights my ability to:
- Solve real-world problems through custom software
- Learn and apply new tech rapidly (WPF/XAML)
- Develop with user privacy and compliance in mind
- Design modular and maintainable backend systems

## 🧪 Possible Future Features
- Export data to Excel or CSV
- Multi-user support with access controls
- Integration with local databases (SQLite or LiteDB)
- Configurable templates or workflow rules

- ## 🚧 Status
This project is a functional prototype developed as a proof of concept. While it remains unfinished, it showcases the foundation of a secure, compliant work tracker and demonstrates the use of WPF, async task handling, and modular architecture. Development was paused due to a shift in priorities and internal company decisions.


