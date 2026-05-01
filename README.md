# RecipeNest API

REST API for the RecipeNest platform — handles authentication, chef profiles, and recipe management.

Built with **ASP.NET Core 10**, **Entity Framework Core 9**, and **PostgreSQL** (hosted on NeonDB).

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Environment Variables](#environment-variables)
- [API Reference](#api-reference)
- [Database Schema](#database-schema)
- [Authentication](#authentication)
- [Deployment](#deployment)

---

## Tech Stack

| Concern | Library / Version |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 9.0.4 |
| Database driver | Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4 |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer 9.0.4 |
| Password hashing | BCrypt.Net-Next 4.0.3 |
| Container | Docker (multi-stage, `mcr.microsoft.com/dotnet/sdk:10.0`) |

---

## Project Structure

```
RecipeNest.API/
├── Controllers/
│   ├── AuthController.cs       # POST /auth/register, POST /auth/login
│   ├── ChefsController.cs      # GET /chefs, GET /chefs/{id}
│   ├── RecipesController.cs    # Full CRUD /recipes
│   └── ProfileController.cs   # GET/PUT /profile
├── Models/
│   ├── User.cs                 # Auth entity (email + password hash)
│   ├── ChefProfile.cs          # Public chef info (1-to-1 with User)
│   └── Recipe.cs               # Recipe (many-to-1 with ChefProfile)
├── DTOs/
│   ├── AuthDTOs.cs             # RegisterDto, LoginDto, AuthResponseDto
│   ├── ChefDTOs.cs             # ChefListDto, ChefDetailDto, UpdateProfileDto
│   └── RecipeDTOs.cs           # RecipeDto, CreateRecipeDto, UpdateRecipeDto
├── Data/
│   └── AppDbContext.cs         # EF Core context + relationships
├── Migrations/                 # EF Core schema history (auto-applied on startup)
├── Dockerfile                  # Multi-stage Docker build for Render
├── Program.cs                  # DI, JWT, CORS, EF, PORT binding
└── appsettings.json            # Base config (override via env vars in prod)
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run locally

```bash
cd RecipeNest.API
dotnet restore
dotnet run
```

The API starts at **http://localhost:5000**.  
EF Core migrations are applied automatically on startup — tables are created on first run.

---

## Environment Variables

Configuration is read from `appsettings.json` and overridden by environment variables.  
In production (Render), set these as **secret** environment variables:

| Variable | Description | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Full NeonDB / PostgreSQL connection string | `Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true;` |
| `Jwt__Key` | Random secret string, minimum 32 characters | `MyApp_SuperSecret_Key_ChangeThis!` |
| `AllowedOrigins` | Comma-separated list of allowed frontend origins | `https://recipenest.vercel.app` |
| `PORT` | Set automatically by Render — do not set manually | `8080` |

> `appsettings.json` already contains safe defaults for local development.

---

## API Reference

All endpoints are prefixed with `/api`.

### Auth

| Method | Endpoint | Auth | Body | Description |
|---|---|---|---|---|
| `POST` | `/auth/register` | No | `{ name, email, password, bio?, avatarUrl? }` | Create account + chef profile |
| `POST` | `/auth/login` | No | `{ email, password }` | Returns JWT token |

**Login response:**
```json
{
  "token": "<jwt>",
  "chefId": 1,
  "name": "Gordon Ramsay",
  "email": "gordon@example.com",
  "avatarUrl": "/avatars/chef1.png"
}
```

---

### Chefs

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/chefs` | No | List all chefs with recipe count |
| `GET` | `/chefs/{id}` | No | Chef profile + all their recipes |

---

### Recipes

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/recipes` | No | All recipes (newest first) |
| `GET` | `/recipes/{id}` | No | Single recipe detail |
| `GET` | `/recipes/chef/{chefId}` | No | Recipes by a specific chef |
| `POST` | `/recipes` | **Yes** | Create a recipe |
| `PUT` | `/recipes/{id}` | **Yes** | Update own recipe |
| `DELETE` | `/recipes/{id}` | **Yes** | Delete own recipe |

**Recipe body (create / update):**
```json
{
  "title": "Beef Wellington",
  "description": "Classic British dish...",
  "ingredients": "Beef tenderloin, puff pastry...",
  "instructions": "1. Sear the beef...",
  "imageUrl": "https://example.com/image.jpg"
}
```

---

### Profile

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/profile` | **Yes** | Get own chef profile |
| `PUT` | `/profile` | **Yes** | Update name, bio, avatarUrl |

---

## Database Schema

```
User (1) ──────── (1) ChefProfile (1) ──────── (Many) Recipe
```

| Table | Key columns |
|---|---|
| `Users` | `Id`, `Email`, `PasswordHash` |
| `ChefProfiles` | `Id`, `UserId` (FK), `Name`, `Bio`, `AvatarUrl` |
| `Recipes` | `Id`, `ChefId` (FK), `Title`, `Description`, `Ingredients`, `Instructions`, `ImageUrl`, `CreatedAt` |

Cascade delete is configured: removing a `User` deletes their `ChefProfile` and all associated `Recipe` rows.

---

## Authentication

1. Client `POST /api/auth/login` with email + password
2. Server verifies the BCrypt hash and issues a signed JWT (7-day expiry)
3. JWT claims: `userId`, `email`, `chefId`
4. Client stores the token in `localStorage` and sends it as `Authorization: Bearer <token>` on protected requests
5. ASP.NET Core's `[Authorize]` attribute validates the token on every protected endpoint

---

## Deployment

The API is deployed to **Render** using Docker.

1. Push the repo to GitHub
2. Create a **Web Service** on Render, connect the repo, and select the `recipenest-api` service from `render.yaml`
3. Add the secret environment variables listed above in the Render dashboard
4. Deploy — Render builds the Docker image and starts the container

The `Dockerfile` uses a multi-stage build:
- **Build stage** — `mcr.microsoft.com/dotnet/sdk:10.0` compiles and publishes the app
- **Runtime stage** — `mcr.microsoft.com/dotnet/aspnet:10.0` runs the published output (smaller final image)
