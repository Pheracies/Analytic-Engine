# 🚀 Analytical Engine - Full Project & Study Guide

A high-performance C# .NET Minimal API backend paired with a Svelte 5 + TypeScript frontend, designed to process item request workflows in real-time. This project serves as a full-stack learning playground covering RESTful APIs, WebSocket communication (SignalR), local databases (EF Core + SQLite), and modern Svelte 5 state management.

---

## 🏛️ Project Directory Structure

The project uses a mirrored layered pattern on both the backend and frontend to isolate data contracts, networking logic, and UI rendering:

```
Analytic Engine/
├── src/
│   ├── AnalyticEngine.Api/         # C# Web API Server & SignalR Hub (.NET 10.0)
│   │   ├── Api/
│   │   │   ├── Layer1/             # Items validation & CleanString logic
│   │   │   └── Layer2/             # Web APIs, SignalR Hub, DbContext & Server
│   │   └── Main.cs                 # Main entry point
│   ├── AnalyticEngine.Cli/         # CLI runner wrapper
│   └── analytic-engine-ui/         # Svelte 5 + TypeScript + Vite frontend
│       ├── src/
│       │   ├── layer0/             # Types and static maps (types.ts)
│       │   ├── layer1/             # SignalR and REST services (connection.ts)
│       │   └── layer2/             # UI Components (App.svelte, app.css)
└── analytic_engine.db              # SQLite Database (generated automatically)
```

---

## 💻 Tech Stack & Key Concepts

### 1. Networking: HTTP POST vs. SignalR WebSockets
Our dashboard supports sending requests using both protocols. Here is how they differ:

| Feature | HTTP POST (REST) | SignalR (WebSockets) |
| :--- | :--- | :--- |
| **Connection Lifetime** | Closes immediately after response | Stays open indefinitely |
| **Bidirectional?** | No (Client must request first) | **Yes** (Server can push data at any time) |
| **Speed / Latency** | Slower (requires new handshakes) | **Extremely Fast** (sub-millisecond) |
| **Overhead** | High (sends cookies & headers each time) | Very Low (single initial handshake) |
| **Best used for** | Stateless transactions (e.g. login, checkout) | Real-time feeds (e.g. chat, live dashboards) |

---

### 2. C# Backend Concepts
- **CORS (Cross-Origin Resource Sharing)**: Security gatekeeper configured in `WebApi.cs` to explicitly whitelist `http://localhost:5173` with credentials support, allowing the frontend browser tab to talk to the backend port.
- **Inheritance & Overriding (`override` / `virtual`)**:
  - `Hub` (Parent class) contains `virtual` methods like `OnConnectedAsync` which default to doing nothing.
  - `RequestHub` (Child class) inherits from `Hub` and uses the `override` keyword to supply custom logging and welcome broadcasts when clients connect.
  - `base.OnConnectedAsync()` is called inside the override to let Microsoft's underlying registration code execute.
- **EF Core SQLite Value Converters**: SQLite cannot store array collections natively. We use a converter inside `OnModelCreating` to automatically serialize a C# `List<ItemCategories>` list to a JSON string on save, and deserialize it back on read.

---

### 3. Frontend Svelte 5 Runes & Typescript
- **Reactivity (`$state`)**: Declares variables reactive using signals. The UI dynamically repaints only the HTML nodes that read the variable.
- **Computed Values (`$derived` / `$derived.by`)**:
  - `$derived(a * b)`: Spreadsheet-style formula that recalculates automatically.
  - `$derived.by(() => { ... })`: Runs a multi-line code block to calculate a value.
- **Side Effects (`$effect`)**: Automatically triggers a block of code (e.g. logging or saving) when any reactive variable read inside it updates.
- **TypeScript inside Svelte (`<script lang="ts">`)**: Ensures that our Svelte UI elements match the exact data structure types expected by our C# backend, catching typos (like `amout` instead of `amount`) at build time.

---

### 4. Advanced Svelte: Checkbox binding with `bind:group` arrays
Instead of creating separate boolean flags for every category (e.g. `categoryWeapon`, `categoryAccessory`), we can define a single array of selected IDs:
```typescript
let selectedCategories = $state<number[]>([1]); // Holds active category numbers
```
And loop through the options in HTML:
```html
{#each categoryOptions as cat}
  <label>
    <input type="checkbox" value={cat.id} bind:group={selectedCategories} />
    {cat.name}
  </label>
{/each}
```
Svelte will automatically add checked checkbox values into `selectedCategories` and delete unchecked values, generating a clean array of integers that matches C# enums.

---

### 5. Troubleshooting: Port and File Locking
During backend development, builds can fail because of stale background processes keeping files and sockets busy.

#### Find what process is holding port 5000:
```powershell
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Select-Object LocalPort, State, OwningProcess
```

#### Kill the process:
```powershell
# Kill by Process ID (PID)
Stop-Process -Id <PID> -Force

# Or kill by name
Stop-Process -Name "AnalyticEngine.Api" -Force
```

---

## 🚀 How to Run

### Requirements
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18 or higher)](https://nodejs.org)

### Step 1: Run the Backend API
Navigate to the root directory and run:
```bash
dotnet run --project ./src/AnalyticEngine.Api/AnalyticEngine.Api.csproj
```
The server hosts:
- REST API: `http://localhost:5000`
- SignalR WebSocket: `http://localhost:5000/send`

### Step 2: Run the Svelte Frontend
Navigate to the frontend directory and start Vite:
```bash
cd src/analytic-engine-ui
npm install
npm run dev
```
Open [http://localhost:5173](http://localhost:5173) in your browser.
