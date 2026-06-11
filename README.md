# Personal Finance Tracker (C#)

A lightweight personal finance tracker built with an ASP.NET Core minimal API and SQLite. This project is intended as a simple starting point to record and inspect transactions locally, with a tiny frontend served from `wwwroot`.

Prerequisites

- .NET 7 SDK

Quick start

```bash
# from the repository root
dotnet restore
dotnet run
```

Open the URL printed by the `dotnet run` command (by default http://localhost:5000) in your browser.

Project layout

- `Personal-Finance-Tracker.csproj` — project file
- `Program.cs` — minimal API, EF Core models and DbContext
- `finance.db` — SQLite database (created automatically)
- `wwwroot/` — static frontend: `index.html`, `app.js`, `styles.css`

Features

- Add transactions with date, amount, category, and optional note
- View recent transactions (latest 100)
- Delete transactions
- Simple summary (balance, income, expenses, totals by category)
- Export transactions as CSV

API Endpoints

- `GET /api/categories` — list category names
- `GET /api/transactions` — list recent transactions (JSON)
- `POST /api/transactions` — add a transaction (JSON body)
- `DELETE /api/transactions/{id}` — delete a transaction by id
- `GET /api/summary` — get balance, income, expenses, and category totals
- `GET /api/transactions/export` — download all transactions as CSV

Example: add a transaction with `curl`:

```bash
curl -X POST http://localhost:5000/api/transactions \
	-H "Content-Type: application/json" \
	-d '{"date":"2026-06-11","amount":-12.50,"category":"Food","note":"Lunch"}'
```

Notes and next steps

- Data is stored in `finance.db` in the project folder. Back it up if needed.
- This is a minimal example — consider adding authentication, user separation, charts, CSV import, or pagination.
