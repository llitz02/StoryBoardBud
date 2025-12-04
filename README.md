# StoryBoardBud

A modern web application for creating collaborative storyboards with photos, text, and interactive drag-and-drop editing. Perfect for filmmakers, designers, product teams, and creative professionals.

## Features

- **Create & Edit Storyboards**: Build unlimited storyboards with drag-and-drop photo and text placement
- **Photo Upload**: Upload high-quality images (up to 10MB) to your personal gallery
- **Text Annotations**: Add text elements to storyboards for notes and ideas
- **Community Discover Page**: Browse and use photos shared by other creators
- **Report System**: Flag inappropriate content for admin review
- **Admin Dashboard**: 
  - User management (lock, unlock, delete)
  - Report moderation and content review
  - User activity overview
- **Responsive UI**: Works on desktop and tablet devices
- **User Authentication**: Secure registration and login with ASP.NET Identity

## Tech Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: SQLite (local development), configurable for production
- **Auth**: ASP.NET Identity with roles (Admin, User)
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, Vanilla JavaScript
- **File Storage**: Local filesystem (ready for cloud integration)

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- A modern web browser

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/StoryBoardBud.git
cd StoryBoardBud
```

2. Navigate to the project:
```bash
cd StoryBoardBud
```

3. Restore packages and build:
```bash
dotnet restore
dotnet build
```

4. Run the application:
```bash
dotnet run
```

5. Open your browser and navigate to: `https://localhost:5294`

### Default Accounts

The application seeds two default accounts on first run:

**Admin Account:**
- Username: `admin`
- Email: `admin@storyboardbud.local`
- Password: `AdminPassword123!`

**Test User Account:**
- Username: `testuser`
- Email: `testuser@storyboardbud.local`
- Password: `TestPassword123!`

## Usage

### Creating a Storyboard

1. Sign in or register
2. Click "My Boards" → "Create New Board"
3. Enter a title and optional description
4. Start editing by:
   - **Uploading photos**: Click "Upload Photo" and select an image file
   - **Adding text**: Type in the text box and click "Add Text"
   - **Using community photos**: Browse the "Browse Public Photos" section and click to add
5. Drag items around the canvas to position them
6. Delete items with the × button
7. Changes are auto-saved

### Discovering Photos

1. Navigate to **Discover** in the top menu
2. Browse public photos from other users
3. Click on a photo to view full details
4. Use the "Report" button if content is inappropriate

### Admin Features

1. Go to **Admin** in the top menu (Admin account only)
2. **Manage Users**: View all users, lock/unlock, or delete accounts
3. **Review Reports**: Filter by status (Pending, Approved, Rejected)
4. **Approve/Reject**: Decide on reported content
   - **Approve**: Hides the photo from the community
   - **Reject**: Keeps the photo visible

## File Structure

```
StoryBoardBud/
├── Controllers/          # API and MVC controllers
│   ├── AdminController.cs
│   ├── BoardsController.cs
│   ├── DiscoverController.cs
│   ├── PhotosController.cs
│   └── ReportsController.cs
├── Data/                # EF Core models and DbContext
│   ├── ApplicationDbContext.cs
│   ├── ApplicationUser.cs
│   ├── Board.cs
│   ├── Photo.cs
│   ├── BoardItem.cs
│   └── Report.cs
├── Services/            # Business logic services
│   ├── IFileStorageService.cs
│   ├── LocalFileStorageService.cs
│   └── DbSeedService.cs
├── Views/               # Razor views
│   ├── Admin/
│   ├── Boards/
│   ├── Discover/
│   └── Shared/
├── wwwroot/
│   ├── uploads/         # Uploaded photos stored here
│   ├── css/
│   ├── js/
│   └── lib/
├── Migrations/          # EF Core migrations
├── Program.cs           # App configuration
└── appsettings.json     # Connection strings
```

## Database Schema

### Users (AspNetUsers)
- Extends IdentityUser
- Fields: FullName, BioDescription, CreatedAt

### Boards
- Id, Title, Description
- OwnerId (FK to ApplicationUser)
- CreatedAt, UpdatedAt

### Photos
- Id, FileName, FilePath, FileSizeBytes
- UploadedById (FK)
- IsPrivate flag
- CreatedAt

### BoardItems
- Id, BoardId (FK), PhotoId (FK)
- TextContent (for text items)
- PositionX, PositionY, Width, Height
- Rotation, ZIndex
- CreatedAt, UpdatedAt

### Reports
- Id, PhotoId (FK), ReportedById (FK)
- Reason, Description
- Status (Pending, Reviewed, Approved, Rejected)
- ReviewedBy (Admin), ReviewedAt, AdminNotes

## API Endpoints

### Boards
- `GET /Boards/MyBoards` - List user's boards
- `POST /Boards/Create` - Create new board
- `GET /Boards/Edit/{id}` - Edit board
- `GET /api/boards/{id}` - Get board JSON

### Photos
- `POST /api/photos/upload` - Upload photo
- `POST /api/photos/add-to-board` - Add photo to board
- `POST /api/photos/add-text` - Add text element
- `POST /api/photos/update-item` - Update position/size/rotation
- `DELETE /api/photos/{id}` - Delete photo
- `DELETE /api/photos/item/{id}` - Delete board item

### Discover
- `GET /Discover` - Discover page (public)
- `GET /discover/api/photos` - Get public photos (paginated)

### Reports
- `POST /api/reports` - Report a photo
- `GET /api/reports` - Get pending reports (Admin only)
- `POST /api/reports/approve/{id}` - Approve report (Admin only)
- `POST /api/reports/reject/{id}` - Reject report (Admin only)

### Admin
- `GET /Admin` - Admin dashboard
- `GET /Admin/Users` - Manage users
- `GET /Admin/UserDetail/{id}` - User details
- `POST /Admin/LockUser/{id}` - Lock user account
- `POST /Admin/DeleteUser/{id}` - Delete user
- `GET /Admin/Reports` - Review reports

## Configuration

### Database

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=storyboardbud.db"
  }
}
```

For SQL Server:
```json
"DefaultConnection": "Server=.;Database=StoryBoardBud;Trusted_Connection=true;"
```

### File Upload

Settings in `Services/LocalFileStorageService.cs`:
- Max file size: 10 MB (configurable)
- Allowed types: .jpg, .jpeg, .png, .gif, .webp
- Storage: `wwwroot/uploads/` (configurable)

## Production Deployment

### Prepare for Production

1. **Update Database Connection**:
   - Update `appsettings.json` with SQL Server or PostgreSQL connection string
   - Or use environment variables: `ConnectionStrings__DefaultConnection`

2. **Configure File Storage**:
   - Replace `LocalFileStorageService` with Azure Blob Storage or AWS S3 implementation
   - Update `Program.cs` service registration

3. **Set Admin Credentials**:
   ```csharp
   // In DbSeedService.cs, update admin password
   var result = await _userManager.CreateAsync(adminUser, "YourSecurePassword!");
   ```

4. **Enable HTTPS** and configure HSTS

5. **Set up backups** for database and uploaded files

### Deployment Steps

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Deploy to your hosting provider (Azure, AWS, etc.)
# Configure environment variables
# Run migrations: dotnet ef database update --project YourProject
```

## Development

### Creating a Migration

```bash
cd StoryBoardBud
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

## Troubleshooting

### Database Issues
- Delete `storyboardbud.db` and restart the app to recreate with seed data
- Check `appsettings.json` connection string

### File Upload Issues
- Ensure `wwwroot/uploads/` directory exists
- Check file permissions on the upload folder
- Verify file size doesn't exceed 10MB limit

### Authentication Issues
- Clear browser cookies and cache
- Ensure Identity tables exist: `dotnet ef database update`

## Roadmap

- [ ] Drag handle for resizing items
- [ ] Image rotation/flip controls
- [ ] Collaborative editing (real-time sync)
- [ ] Export boards as PDF/images
- [ ] Templates and themes
- [ ] Mobile app
- [ ] Advanced search and filtering
- [ ] Photo tagging and organization
- [ ] Private sharing with users/teams

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License - see LICENSE.md for details.

## Support

For issues and questions:
1. Check existing GitHub issues
2. Create a new issue with detailed description
3. Include steps to reproduce bugs

## Contact

- Email: support@storyboardbud.local
- Website: [storyboardbud.com](https://storyboardbud.com)
- Twitter: [@StoryBoardBud](https://twitter.com/storyboardbud)

---

Happy storyboarding! 🎬📸✨
a web app for making storyboards where users can upload pictures and text to flesh out ideas
