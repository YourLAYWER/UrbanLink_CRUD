# UrbanLink API - Municipal Transit Backend

UrbanLink is a robust, time-aware transit routing API built to service the Nelson Mandela Bay Municipality (Gqeberha). It provides real-time route calculation, fare estimation, and digital ticketing for multi-modal municipal transit.

## 🚀 Key Features

*   **Time-Aware Routing Engine:** Features a custom implementation of Dijkstra's algorithm that accounts for bus schedules, transfer penalties, and overnight wait times.
*   **Dynamic Graph Initialization:** Automatically calculates node offsets and transit timelines on startup without requiring heavy GTFS data mapping.
*   **Secure Authentication:** JWT-based user authentication and role-based access control (e.g., automated Student/Pensioner fare discounts).
*   **Entity Framework Core:** Fully normalized SQL Server database architecture managing Stops, Routes, Segments, and Journey Histories.

## 🛠️ Tech Stack

*   **Framework:** .NET 10 / ASP.NET Core Web API
*   **Language:** C#
*   **ORM:** Entity Framework Core
*   **Database:** SQL Server
*   **Authentication:** JSON Web Tokens (JWT)

## ⚙️ Getting Started

### Prerequisites
*   .NET 10 SDK
*   SQL Server (LocalDB or standard instance)
*   Visual Studio 2022 or VS Code

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/YourLAWYER/UrbanLink-API.git](https://github.com/yourusername/UrbanLink-API.git)
   cd UrbanLink-API
