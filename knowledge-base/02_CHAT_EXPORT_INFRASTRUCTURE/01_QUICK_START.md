# Quick Start: Copilot Chat Export

**Time:** 5 minutes  
**Goal:** Set up automatic chat exports

---

## Option 1: Manual Export (Right Now)

```bash
# From repository root
cd Backend/WebTemplate.ChatExporter
dotnet run
```

✅ Done! Chats exported to `SESSIONS/copilot_exports/`

---

## Option 2: Automated Export (This Week)

### Step 1: Test the automation script

```bash
# From repository root
./scripts/Export-CopilotChats.ps1 -AutoCommit
```

You should see:
```
[Success] Export completed successfully!
```

### Step 2: Schedule in Windows Task Scheduler

1. **Open:** Task Scheduler
   - Press `Win+R`, type `taskschd.msc`, press Enter

2. **Create Task:**
   - Right-click "Task Scheduler Library"
   - Select "Create Basic Task"

3. **Configure:**
   - Name: `Copilot Chat Export`
   - Description: `Daily export of VS Copilot chats`
   - Trigger: `Daily` → `18:00` (6 PM)
   - Action: `Start a program`
     - Program: `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe`
     - Arguments:
       ```
       -NoProfile -WindowStyle Hidden -Command "cd 'C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate'; ./scripts/Export-CopilotChats.ps1 -AutoCommit"
       ```
     - Start in: `C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate`

4. **Finish** → Task will run daily at 6 PM

---

## Option 3: Manual Scheduled Reminder

Add to your calendar:
- **Event:** "Export Copilot chats"
- **Frequency:** Weekly (Friday 5 PM)
- **Action:** Run `./scripts/Export-CopilotChats.ps1 -AutoCommit`

---

## Verify It's Working

### Check exported files
```bash
ls SESSIONS/copilot_exports/
```

You should see markdown files with timestamps:
```
2025-11-13_12-37-31_SKMemoryTable.md
2025-11-13_12-45-22_SKMemoryTable.md
```

### Check git commits
```bash
git log --oneline | head -5
```

You should see entries like:
```
a1b2c3d chore: export Copilot chat history (1 file(s))
```

---

## That's It! 🎉

Your chats are now:
- ✅ Automatically exported
- ✅ Stored as readable markdown
- ✅ Version-controlled in git
- ✅ Never lost again

---

## Troubleshooting

**Q: "Access denied" error**  
A: Run PowerShell as Administrator

**Q: "No databases found"**  
A: Close Visual Studio first (releases database locks)

**Q: Scripts not running?**  
A: Check your execution policy:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Q: Want to see what's being exported?**  
A: Open any `.md` file in `SESSIONS/copilot_exports/`

---

## Next: Use Exported Chats

1. Review exported markdown files
2. Extract important learnings
3. Add to knowledge base
4. Reference in future chats

See `SESSIONS/CHAT_EXPORT_INFRASTRUCTURE.md` for complete details.

---

**Questions?** Check the full documentation:
- `Backend/WebTemplate.ChatExporter/README.md` - Technical details
- `scripts/README.md` - Script documentation
- `SESSIONS/CHAT_EXPORT_INFRASTRUCTURE.md` - Architecture overview
