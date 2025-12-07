# StoryBoardBud

A modern web application for creating storyboards with photos, text, and drag-and-drop editing.

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

1. Clone the repository

2. Navigate to the project

3. Restore packages and build:
```
dotnet restore
dotnet build
```

4. Run the application:
```
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


