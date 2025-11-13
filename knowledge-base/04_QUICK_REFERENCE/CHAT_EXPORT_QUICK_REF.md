# Copilot Chat Export - Quick Reference

## 🚀 Quick Commands

### Manual Export (Right Now)
```bash
cd Backend/WebTemplate.ChatExporter
dotnet run
```

### Automated Export (With Git Commit)
```bash
./scripts/Export-CopilotChats.ps1 -AutoCommit
```

### View Exported Files
```bash
code SESSIONS/copilot_exports/
```

---

## 📋 Key Files

| Path | Purpose |
|------|---------|
| `Backend/WebTemplate.ChatExporter/` | The export tool (C#) |
| `scripts/Export-CopilotChats.ps1` | Automation wrapper (PowerShell) |
| `SESSIONS/copilot_exports/` | Where exports are saved |

---

## ⚙️ Setup Options

### Option A: Nothing (Manual Only)
Just run `dotnet run` whenever you want exports

### Option B: Weekly Reminder
Add calendar event, run script manually each Friday

### Option C: Automatic (Windows Task Scheduler)
1. Open `taskschd.msc`
2. Create Basic Task
3. Trigger: Daily 6 PM
4. Action: 
   ```
   pwsh.exe -NoProfile -WindowStyle Hidden -Command "cd 'C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate'; ./scripts/Export-CopilotChats.ps1 -AutoCommit"
   ```

---

## 🔍 Troubleshooting

| Issue | Solution |
|-------|----------|
| "No databases found" | Close VS first |
| "Access denied" | Run PowerShell as Admin |
| "Script won't run" | Set execution policy to RemoteSigned |
| Empty exports | Normal - data is binary/serialized |

---

## 📊 What Gets Exported

✅ All Copilot chat metadata  
✅ Conversation records  
✅ Memory/embedding data  
✅ Timestamps  
✅ All database tables  

---

## 🎯 Success Indicators

✅ Files appear in `SESSIONS/copilot_exports/`  
✅ File names have timestamps  
✅ Git log shows export commits  
✅ Markdown files are readable with JSON data  

---

## 🔄 Regular Workflow

```
Morning  → Work with Copilot
  ↓
Evening (Auto) → 6 PM export runs
  ↓
Weekly → Review exports, update KB
  ↓
Monthly → Summarize learnings
```

---

**This solves:** Lost chat history  
**Now:** All chats automatically captured and searchable forever
