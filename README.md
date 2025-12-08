# StoryBoardBud

A modern web application for creating storyboards with photos, text, and drag-and-drop editing.

---

## Project Proposal

My term project will be a media site where you edit and upload photos. 
Ideally it will be similar to Pinterest. Users can create image boards and 
write down notes as well to compile for written thoughts to apply to the 
board. Users will have the ability to make their images and storyboards 
public so that others may search for or discover them by looking through 
a discovery tab. 
There would be many use cases for this such as art inspiration, 
developing literature as a writer. Or even large-scale software 
development. 
The UI I envision will be neutral to provide the feeling of a sandbox for 
thought and creativity to flourish. 

## User Stories

As a user I’d like to log in order to access the website 
As a user I’d like to create a storyboard on the site order to work in it 
As a user I’d like to post pictures/notes order to flesh out my ideas 
As a userI’d like to report other users posts order to request action 
against innapropriate posts 
As a administrator I’d like to manage users in order to increase website stability
As a administrator I’d like to remove inappropriate posts in order to keep 
the sight safe for all users.

## AI Disclosure

Google Gemini was used in this project

### How AI Was Utilized

- task approach strategy assistance
- README content and project documentation
- Code organization improvements
- Validation attribute additions
- Error handling enhancements

### Accessibility

- **ARIA labels** on interactive elements (buttons, inputs, regions)
- **Semantic HTML** with proper role attributes (main, navigation, progressbar)
- **Skip navigation link** for keyboard users to bypass navigation menu
- **Alt text** for images throughout the application
- **Keyboard navigation** support with proper tabindex and focus management
- **Live regions** (aria-live) for dynamic content updates and progress indicators
- **Form labels** associated with all input fields
- **Bootstrap accessibility features** including responsive design and focus states

### Code Examples Used

- Microsoft ASP.NET Core Documentation- https://learn.microsoft.com/aspnet/core/ (general framework reference and best practices)
- Bootstrap Documentation - https://getbootstrap.com/ (for responsive UI components)
- Query Validation - https://jqueryvalidation.org/ (client-side form validation library)

- Lots of gemini guidance I couldnt source to any particular website


### Data Model

The application uses the following core entities:

- **ApplicationUser**: Extended Identity user with profile information
- **Board**: User-created storyboards
- **Photo**: Uploaded image files with privacy settings
- **BoardItem**: Photos or text elements positioned on boards
- **FavoritePhoto**: User's saved favorite photos
- **Report**: User reports for content moderation

### Technologies Used

- ASP.NET Core 9.0 MVC
- Entity Framework Core with SQLite
- ASP.NET Identity for authentication and authorization
- Bootstrap 5 for responsive UI
- JavaScript for interactive board editing

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




