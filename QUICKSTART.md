# Quick Start Guide

## 🚀 Get Started in 2 Minutes

### Prerequisites
- .NET 9 SDK installed

### Steps

1. **Open Terminal** in the `StoryBoardBud/` folder

2. **Run the app:**
   ```bash
   dotnet run
   ```

3. **Open your browser** to: `https://localhost:5294`

4. **Test Accounts** (already created):
   - **Admin**: `admin` / `AdminPassword123!`
   - **Test User**: `testuser` / `TestPassword123!`

---

## ✨ What to Try First

### As a Regular User:
1. Sign in as `testuser`
2. Go to **My Boards** → **Create New Board**
3. Upload a photo or add text
4. Drag items around the canvas
5. Check out **Discover** to browse community photos
6. Report any inappropriate content

### As an Admin:
1. Sign in as `admin`
2. Go to **Admin** dashboard
3. **Manage Users**: View/lock/delete accounts
4. **Review Reports**: Check and approve/reject flagged content

---

## 📁 Project Structure

```
StoryBoardBud/
├── Controllers/     → API endpoints (Boards, Photos, Reports, Admin)
├── Data/           → Database models
├── Services/       → File upload & seed logic
├── Views/          → UI pages
└── wwwroot/        → Static files + uploaded photos
```

---

## 🔧 Configuration

### Database
Edit `appsettings.json`:
- **SQLite** (default): `"Data Source=storyboardbud.db"`
- **SQL Server**: `"Server=.;Database=StoryBoardBud;Trusted_Connection=true;"`

### File Upload Location
- Uploads go to: `wwwroot/uploads/`
- Max file size: 10MB
- Allowed types: JPG, PNG, GIF, WebP

---

## 📚 Key Features

✅ Create unlimited storyboards  
✅ Drag-and-drop photo placement  
✅ Add text annotations  
✅ Browse community photos  
✅ Report inappropriate content  
✅ Admin user management  
✅ Admin content moderation  

---

## 🆘 Troubleshooting

**Port already in use?**
```bash
dotnet run --urls="https://localhost:5295"
```

**Database issues?**
- Delete `storyboardbud.db` and restart

**File upload not working?**
- Ensure `wwwroot/uploads/` folder exists
- Check file permissions

---

**Enjoy creating! 🎬✨**
