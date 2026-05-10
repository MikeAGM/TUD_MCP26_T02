# Group Clashes by Distance – Navisworks Plug-in

![Plugin Icon](icon.png)

---

## What it does

When you run a clash detection in Navisworks, you can end up with hundreds of individual clash results that are physically close together — the same pipe hitting the same beam in five different spots, for example. Reviewing them one by one is slow and clutters your reports.

This plugin lets you select a clash test, set a distance in metres, and it automatically bundles nearby clashes into named groups inside Clash Detective. Your report stays organised and easier to coordinate with the rest of the team.

---

## Why we built it

This plugin was developed as part of the **MCP-2026** project by **Team 02**, in partial fulfilment of the **MSc in Applied Building Information Modelling and Management (aBIMM)** at **Technological University Dublin (TU Dublin)**.

The goal was to reduce the time spent manually sorting clash results during coordination reviews and to produce cleaner, more actionable clash reports.

---

## How it works

1. Open your Navisworks file and make sure at least one clash test has been run with results.
2. Go to the **TUD MCP-2026** tab in the Navisworks ribbon.
3. Click **Group Clashes by Distance**.
4. Pick a clash test from the dropdown and enter a distance threshold in metres (default is `3.0 m`).
5. Click **OK**.

The plugin scans all clashes in the selected test and groups together any clashes whose centres are within your chosen distance of each other. The groups appear immediately inside Clash Detective under the selected test, named like:

```
Proximity Clash Group 3.000m 1 (5 clashes)
```

Clashes that have no neighbours within range are left as individual items.

---

## Installation

> Requires **Autodesk Navisworks Manage 2027** and **.NET Framework 4.8**.

1. Download the latest release from the Releases page.
2. Copy zip content into:
   ```
   C:\Program Files\Autodesk\Navisworks Manage 2027\Plugins\
   ```
3. Copy the `GroupClashesByDistance` folder (containing `en-US` and `Resources`) into the same `Plugins` directory.
4. Restart Navisworks Manage 2027.

The **TUD MCP-2026** tab will appear in the ribbon.

---

## Requirements

| | |
|---|---|
| Autodesk Navisworks Manage | 2027 |
| .NET Framework | 4.8 |

---

## Project Context

| | |
|---|---|
| **Programme** | MSc Applied Building Information Modelling and Management (aBIMM) |
| **University** | Technological University Dublin |
| **Module** | MCP-2026 |
| **Team** | Team 02 |
