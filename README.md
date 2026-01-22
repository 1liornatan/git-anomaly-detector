# Git Anomaly Detector

Small .NET service that analyzes GitHub events and detects suspicious activity (unusual push times, suspicious team names, unusual repository access, etc.).

## Prerequisites

- .NET SDK 10.0 or newer installed. Verify with:

```powershell
dotnet --version
```

## Build and run

From the repository root run the project directly:

```powershell
dotnet run --project GitAnomalyDetector
```

Or change into the project folder and run:

```powershell
cd GitAnomalyDetector
dotnet run
```

Or publish and run exe:

```powershell
dotnet publish
```

### Default web URLs

By default the app listens on the standard ASP.NET Core ports when run locally:

- http://localhost:3000

Configuration is read from `appsettings.json` in the `GitAnomalyDetector` project. Adjust logging or other settings there as needed.

Note: the default web URL used by the app is configured inside `GitAnomalyDetector/appsettings.json` — change that value to update the app's default listening address.

## Example Anomalies

When the detector runs it emits anomaly messages similar to the examples below.

- Anomaly Detected: Type=UnusualPushTime, Description=Push to repository 'AnomalyOrganizationTask/TestRepo123' occurred at an unusual time: 14:53 UTC., Severity=Medium
- Anomaly Detected: Type=HackerPrefixTeam, Description=Team 'hackerasdoaskdoksao' created with suspicious 'hacker' prefix., Severity=Critical
- Anomaly Detected: Type=UnusualRepositoryAccess, Description=Push to repository 'AnomalyOrganizationTask/test11111' occurred at an unusual time: 23:05 UTC., Severity=High

These lines are representative of the detector's output format and severity classifications.

## Where to look in the code

- Detector implementations: GitAnomalyDetector/AnomalyDetection/Detectors
- Event handling: GitAnomalyDetector/Program.cs and GitAnomalyDetector/Services
