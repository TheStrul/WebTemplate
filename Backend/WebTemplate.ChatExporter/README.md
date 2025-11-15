# ChatExporter Tool Documentation

## Overview

The **WebTemplate.ChatExporter** is a .NET 9 console application that exports GitHub Copilot chat history from Visual Studio's local storage into readable markdown files.

## Problem It Solves

When you work in Visual Studio with GitHub Copilot Chat:
- ❌ Chat history is stored in binary SQLite databases in AppData
- ❌ Switching from Solution View → Folder View can lose UI references to chats
- ❌ No built-in export mechanism
- ❌ Chat data isn't version-controlled or backed up

**Solution:** ChatExporter reads the SQLite database and exports everything to readable markdown files in your `SESSIONS/copilot_exports/` folder.

## How It Works

### Architecture

```
┌─────────────────────────────────────────┐
│  VS AppData Copilot Databases           │
│  (Binary SQLite files)                  │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  ChatExporter.exe                       │
│  - Scans AppData locations              │
│  - Opens SQLite connections             │
│  - Extracts all table data              │
│  - Converts to JSON/Markdown            │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  SESSIONS/copilot_exports/              │
│  📄 2025-11-13_12-37-31_*.md            │
│  (Readable markdown with timestamps)    │
└─────────────────────────────────────────┘
```

### Key Components

| Component | Responsibility |
|-----------|-----------------|
| `CopilotChatExporter` | Main export orchestrator |
| `FindCopilotDatabases()` | Scans VS AppData for database files |
| `ExportFromDatabaseAsync()` | Opens SQLite connection and extracts data |
| `FindSolutionRoot()` | Locates WebTemplate repo root for output path |

## Usage

### Run Manually

```bash
cd Backend/WebTemplate.ChatExporter
dotnet run
```

Output:
```
🚀 GitHub Copilot Chat Exporter
================================

📍 Scanning: C:\Users\avist\AppData\Local\Microsoft\VisualStudio

Found 1 Copilot database(s):
  📂 C:\Users\avist\AppData\Local\Microsoft\VisualStudio\17.0_f7357e6e\copilot\copilot_sqlite_memory.db

✅ Export completed successfully!
   📁 Location: C:\Users\avist\source\repos\GitHubLocal\Customers\WebTemplate\SESSIONS\copilot_exports
   📄 Files created: 1
   💬 Chats exported: 1
```

### Output Structure

```
SESSIONS/copilot_exports/
├── 2025-11-13_12-37-31_SKMemoryTable.md
├── 2025-11-13_12-38-45_ChatHistory.md
└── 2025-11-13_12-39-12_ConversationMetadata.md
```

Each file contains:
- **Metadata:** Export timestamp, source database, table name
- **Raw Data:** JSON-formatted records from the database

### Example Exported Markdown

```markdown
# Copilot Chat Export

| Property | Value |
|----------|-------|
| Exported | 2025-11-13 12:37:31 |
| Source | copilot_sqlite_memory.db |
| Table | SKMemoryTable |
| Records | 1 |

## Raw Data

\`\`\`json
{
  "collection": "suggestedActionEmbedding",
  "key": null,
  "metadata": null,
  "embedding": null,
  "timestamp": null
}
\`\`\`
```

## Database Locations

ChatExporter automatically scans these locations:

### Main Copilot Folder
```
C:\Users\{User}\AppData\Local\Microsoft\VisualStudio\Copilot\
```

### VS Instance-Specific Folders
```
C:\Users\{User}\AppData\Local\Microsoft\VisualStudio\17.0_*\copilot\
```

Where `17.0_*` is your VS 2022 installation identifier.

## Supported Data

The tool extracts:
- ✅ Chat session metadata
- ✅ Conversation records
- ✅ Memory/embedding data
- ✅ All other SQLite table data

## Limitations & Known Issues

### Current
- Exports **all** table data (raw, not filtered)
- JSON format may include null values
- No deduplication across multiple exports

### Future Enhancements
- [ ] Parse chat conversation structure (turn-by-turn format)
- [ ] Filter/deduplicate multiple exports
- [ ] Generate a summary index
- [ ] Support incremental exports (only new data)
- [ ] Create a web viewer for exported chats
- [ ] Auto-scheduled exports (daily/weekly)

## Integration with Knowledge Base

Exported chats should be:
1. **Reviewed** for important learnings
2. **Summarized** into SESSIONS/ documentation
3. **Committed** to git for version control
4. **Referenced** in knowledge base updates

### Suggested Workflow

```bash
# 1. Export current chat history
cd Backend/WebTemplate.ChatExporter
dotnet run

# 2. Review exported files
code ../../SESSIONS/copilot_exports/

# 3. Extract key learnings into session notes
# Edit SESSIONS/SESSION_20251113_FOUNDATION.md

# 4. Commit to git
git add SESSIONS/
git commit -m "Export Copilot chat history - session 13/11/2025"
```

## Troubleshooting

### No databases found
- Ensure VS is closed (releases SQLite locks)
- Check that the AppData paths exist

### "Access denied" errors
- Run PowerShell/Terminal as Administrator
- Ensure VS Copilot has been used (creates the database)

### Empty tables
- Copilot memory is stored as binary/serialized data
- Some columns may contain encoded data

## Technical Details

### Dependencies
- **System.Data.SQLite** - SQLite database access
- **Dapper** - Micro ORM for querying

### Configuration
- **Output Path:** Auto-detected from solution root
- **File Format:** Markdown with embedded JSON
- **Naming Convention:** `YYYY-MM-DD_HH-mm-ss_TableName.md`

### Database Schema
The actual Copilot database schema is not publicly documented. ChatExporter is reverse-engineered to:
1. Detect all available tables
2. Query all records
3. Export everything to readable format

## Future Integration

### Scenario: Automated Session Export

```csharp
// Could be scheduled in CI/CD or as VS extension
var exporter = new CopilotChatExporter();
var result = await exporter.ExportAllChatsAsync();

if (result.Success && result.FilesCreated > 0)
{
    // Trigger git commit
    // Notify Strul of new exports
    // Update knowledge base index
}
```

---

**Version:** 1.0  
**Created:** 2025-11-13  
**Status:** Working, minimal functionality  
**Next:** Enhance parsing, add deduplication, create UI
