# SQL Server Connection Troubleshooting Guide

**Problem:** Database creation fails with connection error  
**Error Code:** Error 40 (Named Pipes Provider)

## 🔍 What This Error Means

```
SQL Error: A network-related or instance-specific error occurred while 
establishing a connection to SQL Server. The server was not found or was 
not accessible.
```

This means the application **cannot reach** the SQL Server instance specified in the connection string.

---

## ✅ How to Fix This

### **Step 1: Verify SQL Server is Running**

**Windows 11/10:**
1. Press `Windows + R`
2. Type `services.msc` and press Enter
3. Look for **SQL Server (SQLEXPRESS)** or your instance name
4. It should have status **"Running"**

**If not running:**
- Right-click the service → **Start**
- Wait 10-15 seconds for it to start

### **Step 2: Verify Connection String is Correct**

In WebTemplate.Setup, the connection string should look like:

```
Server=(localdb)\mssqllocaldb;Database=YourDatabaseName;Trusted_Connection=true;
```

**Common variations:**
- `Server=(localdb)\mssqllocaldb;` - LocalDB instance (default)
- `Server=localhost\SQLEXPRESS;` - SQL Server Express
- `Server=.\SQLEXPRESS;` - Current machine SQL Server Express
- `Server=YOUR_SERVER_NAME;` - Remote SQL Server

**To find your instance name:**

Open Command Prompt and run:
```cmd
sqlcmd -L
```

This lists available SQL Server instances.

### **Step 3: Test Connection Before Generation**

In the Database tab of WebTemplate.Setup:

1. Enter your connection string
2. Check **"Test connection"**
3. Click **"Generate Project"**

The app will test the connection **before** attempting to create the database.

### **Step 4: Verify SQL Server Accepts Connections**

**For LocalDB:**
```cmd
sqllocaldb info
sqllocaldb start mssqllocaldb
```

**For SQL Server Express:**
1. Open SQL Server Management Studio (SSMS)
2. Try to connect to your instance
3. If it connects, SQL Server is working

### **Step 5: Check Network Configuration (If Using Remote Server)**

If connecting to a remote SQL Server:

1. Verify the server name/IP is reachable: `ping SERVER_NAME`
2. Ensure port 1433 is open
3. Verify SQL Server is configured for remote connections:
   - Open **SQL Server Configuration Manager**
   - Select your instance
   - Ensure **TCP/IP** is enabled

---

## 🛠️ Alternative Solutions

### **Option A: Let WebTemplate Create the Database**

If you have SQL Server running but the database doesn't exist:

1. Open WebTemplate.Setup
2. Go to **Database** tab
3. Enable **"Create database if not exists"** checkbox ✅
4. Click **"Generate Project"**

The app will automatically create the database before initialization.

### **Option B: Create Database Manually First**

If you prefer to create the database manually:

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Right-click **Databases** → **New Database**
4. Enter database name: `YourProjectNameDb`
5. Click **OK**
6. Disable **"Create database if not exists"** in WebTemplate.Setup
7. Generate your project

### **Option C: Use Different SQL Server Instance**

1. Ensure the SQL Server instance is running
2. Update connection string with correct server name
3. Retry generation

---

## 📋 Common Connection Strings

### **LocalDB (Recommended for Development)**
```
Server=(localdb)\mssqllocaldb;Database=MyAppDb;Trusted_Connection=true;
```

### **SQL Server Express**
```
Server=.\SQLEXPRESS;Database=MyAppDb;Trusted_Connection=true;
```

### **Remote SQL Server with SQL Authentication**
```
Server=myserver.com;Database=MyAppDb;User Id=sa;Password=YourPassword;
```

### **Azure SQL Database**
```
Server=yourserver.database.windows.net;Database=MyAppDb;User Id=admin;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

---

## 🔐 Security Note: Connection Strings

⚠️ **Never hardcode passwords in connection strings!**

**Better approach:**
1. Use **Trusted_Connection=true** when possible (Windows Authentication)
2. Store sensitive values in **User Secrets** (development)
3. Use environment variables (production)
4. Use Azure Key Vault (Azure deployments)

---

## ✨ What Improved Error Messages You'll See Now

### **Scenario 1: SQL Server Not Running**
```
Cannot connect to SQL Server '.\SQLEXPRESS'.

Please verify:
• SQL Server is running
• Server name is correct: .\SQLEXPRESS
• Network connectivity is available
• SQL Server is configured to accept remote connections

Original error: A network-related or instance-specific error occurred...
```

### **Scenario 2: Permission Denied**
```
Permission denied: Current user doesn't have permissions to create databases.

Please verify:
• Login credentials have dbcreator role
• User has appropriate permissions on the server

Original error: The CREATE DATABASE permission was denied...
```

### **Scenario 3: Database Already Exists**
```
Database 'MyAppDb' already exists on server '.\SQLEXPRESS'
```

---

## 📞 Still Having Issues?

1. **Check SQL Server logs:**
   - Event Viewer → Windows Logs → Application
   - Look for SQL Server errors

2. **Verify with SQL Server Management Studio:**
   - Try connecting with same credentials
   - If SSMS can't connect, the issue is with SQL Server setup, not WebTemplate

3. **Test connectivity:**
   ```cmd
   sqlcmd -S (localdb)\mssqllocaldb -E
   ```
   (If this works, SQL Server is accessible)

4. **Review Database Tab Settings:**
   - Ensure connection string is correct
   - Test connection before generating
   - Enable "Create database if not exists" if needed

---

## ✅ Recommended Setup

For best results with WebTemplate.Setup:

1. ✅ Install SQL Server Express or LocalDB
2. ✅ Verify it's running (`services.msc` or SQL Server Configuration Manager)
3. ✅ Test connection in SSMS before using WebTemplate
4. ✅ Use **Trusted_Connection=true** (Windows Authentication)
5. ✅ Enable **"Create database if not exists"** in WebTemplate.Setup
6. ✅ Click **"Test connection"** before generating

This ensures the best experience! 🚀

---

**Last Updated:** 2024  
**Applies To:** WebTemplate.Setup with C# Template Engine
