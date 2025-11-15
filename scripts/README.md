# WebTemplate Scripts

Collection of utility scripts for WebTemplate development and maintenance.

## Export-CopilotChats.ps1

**Purpose:** Automatically export GitHub Copilot chat history from VS to markdown files and commit to git.

### Quick Start

```bash
# From repository root
./scripts/Export-CopilotChats.ps1
```

### With Auto-Commit

```bash
./scripts/Export-CopilotChats.ps1 -AutoCommit
```

### What It Does

1. ✅ Runs `Backend/WebTemplate.ChatExporter`
2. ✅ Detects newly exported files
3. ✅ Optionally creates a git commit with timestamp
4. ✅ Tracks export activity in console output

### Output

```
[Info] Starting Copilot chat export...
[Info] Repository: C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate
[Info] Exporter: C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate\Backend\WebTemplate.ChatExporter

[Info] Files before export: 1
[Info] Running ChatExporter...

🚀 GitHub Copilot Chat Exporter
================================
✅ Export completed successfully!
   📁 Location: ...SESSIONS\copilot_exports
   📄 Files created: 1
   💬 Chats exported: 1

[Info] Files after export: 2
[Info] New files created: 1

[Success] New exports:
  ✓ 2025-11-13_12-45-22_SKMemoryTable.md

[Success] Committed to git: chore: export Copilot chat history (1 file(s)) - 2025-11-13 12:45:23
```

### Scheduling

#### Windows Task Scheduler (Daily Export)

1. Open Task Scheduler
2. Create Basic Task
3. Trigger: Daily at 18:00
4. Action: `pwsh.exe -File "C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate\scripts\Export-CopilotChats.ps1" -AutoCommit`

#### cron (Linux/Mac)

```bash
# Daily at 6 PM
0 18 * * * cd /path/to/WebTemplate && pwsh ./scripts/Export-CopilotChats.ps1 -AutoCommit
```

## Other Scripts

(To be added as needed)

---

**Version:** 1.0  
**Created:** 2025-11-13
