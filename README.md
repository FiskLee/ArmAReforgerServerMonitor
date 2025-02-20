- **Backend** (ArmaLogBackend) on **Linux**:

  - Advanced **licensing** with usage constraints & remote kill (so you can void a license key).
  - **Offline geolocation** with MaxMind (`GeoLite2-Country.mmdb`).
  - **YAML-based** player data (`players.yaml`).
  - **EF Core** for performance data (`PerformanceDbContext`, `PerformanceRecord`, stored in `performance.sqlite`).
  - **Disk I/O** checks (Linux uses `iostat`, Windows portion is commented out).
  - **Log monitoring** (reads `console.log`) extracting FPS, CPU usage, memory usage, etc.
  - **Daily usage** stats, **uptime** tracking, advanced logging, crash logs, systemd service.
  - Verbose debugging logs and easy troubleshooting.

- **Frontend** (ArmaLogFrontend) on **Windows**:

  - WPF app with multiple 

    bar charts

    :

    1. **FPS** (0–60)
    2. **FrameTime** (0–?), dynamically scaled
    3. **Player Count** (0–128)
    4. **CPU usage** (0–100)
    5. **Individual CPU cores usage** (0–100)

  - **Active players** label, **raw performance data** text block, **performance summary** text, **lock scroll** for logs, **“no logs yet”** label, **stats updated** label, **time range** selection, **pause** performance, **use GB** checkbox for memory usage, etc.

  - **Login** fields (username, password, server URL) with **Connect** / **Disconnect**.

  - **Players** tab (shows players from the YAML DB).

  - **Raw Data** tab (show/pause raw data).

  - **Logs** tab with “Fetch backend logs,” “Show frontend logs,” lock scroll, “no logs yet,” etc.

  - Title: **“Arma Reforger Server Monitor Frontend (Made By ArmAGaming.World)”**.

  - All lines commented to help new developers.

Additionally, we **optimize** resource usage,  add verbose logs, and include the ability to “void a license key” by calling a remote kill endpoint. 

Below is a step-by-step guide:

------

# Table of Contents

1. Backend (Linux)
   1. [Installation Steps](#backend-installation)
   2. [Systemd Setup](#backend-systemd)
   3. [Full Code for ArmaLogBackend](#backend-code)
2. Frontend (Windows)
   1. [Installation Steps](#frontend-installation)
   2. [Full Code for ArmaLogFrontend](#frontend-code)
3. [Summary of Changes & Removed Features](#summary-of-changes)
4. [Detailed Summary of Each App](#detailed-summary)

------

<a name="backend"></a>

## 1) Backend (Linux)

<a name="backend-installation"></a>

### 1.1) Installation Steps

1. **Install .NET 8** on your Linux system (Debian/Ubuntu example):

   ```
   bashCopysudo apt-get update
   sudo apt-get install dotnet-sdk-8.0
   ```

   Or see [Microsoft docs](https://learn.microsoft.com/dotnet/core/install/linux).

2. **Clone** the backend:

   ```
   bashCopygit clone https://github.com/YourUser/ArmaLogBackend.git
   cd ArmaLogBackend
   ```

3. **Place** these files in the **root** of `ArmaLogBackend`:

   - `players.yaml` (YAML for players)
   - `GeoLite2-Country.mmdb` (MaxMind geolocation DB)
   - `licenseUsage.json` (for licensing usage store)
   - `crashlogs/` folder (optional, for crash logs)

4. **Build & Run**:

   ```
   bashCopydotnet build
   dotnet run
   ```

   By default, it listens on `http://0.0.0.0:5000`.
   Test by visiting `http://<server-ip>:5000` => you see “ArmaLogBackend is running.”

<a name="backend-systemd"></a>

### 1.2) Systemd Setup

Create a file: `/etc/systemd/system/armalogbackend.service`:

```
iniCopy[Unit]
Description=ArmaLog Backend
After=network.target

[Service]
ExecStart=/usr/bin/dotnet /root/ArmaLogBackend/ArmaLogBackend.dll
Restart=always
User=root
WorkingDirectory=/root/ArmaLogBackend

[Install]
WantedBy=multi-user.target
```

Then:

```
bashCopysudo systemctl enable armalogbackend.service
sudo systemctl start armalogbackend.service
sudo systemctl status armalogbackend.service
```



### 1.4) **Data Files**:

- **`players.yaml`** in root
- **`GeoLite2-Country.mmdb`** in root
- **`licenseUsage.json`** in root (if using licensing)
- **`crashlogs/`** folder for crash logs

That’s it for the **backend**.

------

<a name="frontend"></a>

## 2) Frontend (Windows)

<a name="frontend-installation"></a>

### 2.1) Installation Steps

1. **Install .NET 6** or **.NET 7** on Windows from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

2. Clone

    the frontend:

   ```
   bashCopygit clone https://github.com/YourUser/ArmaLogFrontend.git
   cd ArmaLogFrontend
   ```

3. Build & Run

   :

   ```
   bashCopydotnet build
   dotnet run
   ```

4. In the **UI**: fill out **username**, **password**, **server URL** (like `http://<server-ip>:5000`), then **Connect**. Switch to the **Performance** tab, see bar charts, etc.



## 3) Summary of Changes & Removed Features

1. **Changes**:
   - **Licensing** with usage constraints + remote kill is optional (commented out).
   - **DiskIO** uses Linux `iostat`, Windows code is commented out.
   - **Multiple bar charts** in the frontend: FPS, FrameTime, PlayerCount, CPU usage, CPU cores usage.
   - **Raw data** tab, **logs** tab with lock scroll, “no logs yet,” “stats updated,” etc.
   - **Performance summary** text with CPU, memory, disk, net usage.
   - **Players** tab with “ShowPlayers” from the YAML DB.
   - **Time range** selection, “pause” performance button, “use GB” memory toggle.
   - **Active players** label on top.
2. **Removed or disabled**:
   - RCON references are commented or excluded.
   - Windows `PerformanceCounter` is commented out.
   - Any leftover inline middleware that caused “Delegate 'RequestDelegate' does not take 0 arguments” is removed.

------

<a name="detailed-summary"></a>

## 4) Detailed Summary of Each App

1. **Backend**:
   - **ArmaLogBackend** runs on Linux.
   - Optionally checks a license key, calls remote kill endpoint. If invalid, it exits.
   - Loads geolocation DB, YAML players, EF Core DB for performance.
   - Monitors `console.log` for FPS, CPU usage, memory usage, storing them in `PerformanceDbContext`.
   - Exposes endpoints for performance, daily usage, players, raw data, logs, uptime.
   - Crash logs are written to `crashlogs/` if an unhandled exception occurs.
   - Systemd script provided to auto-start on boot.
2. **Frontend**:
   - **ArmaLogFrontend** is a WPF .NET 6 app on Windows.
   - Has a **MainWindow** with multiple bar charts, raw data tab, logs tab, players tab, “active players,” “no logs yet,” etc.
   - Polls the backend every 2 seconds for performance data (unless paused).
   - Displays memory usage in MB or GB, user toggles with a checkbox.
   - Has login fields, connect/disconnect to a hypothetical `/api/login` or `/api/disconnect`.
   - **FrontendLogger** writes errors to `FrontendErrors.log` and crashes to `FrontendCrashes.log`.

They communicate via HTTP endpoints. The user can “void” a license key by making the remote kill server return `true` for that license ID.



