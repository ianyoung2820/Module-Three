# Overview

I created a small C# console app called **StudyStreak** to practice real-world C# programming and learn how to structure a simple .NET project. The app lets me start and stop study sessions, add daily goals, mark them as completed, and view summaries for the day or the past week. All progress is saved in a JSON file so the data stays between runs.

My purpose in writing this software was to get more comfortable with object-oriented programming, file input and output, and the general C# workflow. I wanted hands-on experience with loops, conditionals, methods, classes, and basic data storage using JSON.

[Software Demo Video](https://youtu.be/bB29484GgVc)

# Development Environment

I used Visual Studio Code on Windows 11 with the .NET 8 SDK installed. I ran the program through the PowerShell terminal using simple dotnet commands.

The programming language I used was C#. I used the System.Text.Json library for saving and loading data, System.Linq for filtering and calculations, and DateOnly for working with dates.

# Useful Websites

- [C# Documentation (Microsoft)](https://learn.microsoft.com/dotnet/csharp/)
- [System.Text.Json Overview](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json-overview)
- [LINQ Guide](https://learn.microsoft.com/dotnet/csharp/linq/)
- [.NET CLI Reference](https://learn.microsoft.com/dotnet/core/tools/)

# Future Work

- Add a live timer display while a session runs.
- Allow editing and deleting saved goals.
- Export a weekly report to a CSV file.
- Add sound or notifications when a session ends.
- Write a few unit tests for streak tracking and time formatting.
