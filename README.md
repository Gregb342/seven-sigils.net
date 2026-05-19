# Seven Sigils

FR | EN

---

## Français

**Seven Sigils** est un quiz web sur les blasons de l'univers *Game of Thrones*, basé sur le contenu de [La Garde de Nuit](https://lagardedenuit.com).

Refonte fullstack du projet original [`heraldik_of_the_watch`](https://github.com/Gregb342/heraldik_of_the_watch) :  
backend **ASP.NET Core 10** + frontend **React 19 / Vite / TypeScript**.

---

### Fonctionnalités

- Quiz interactif : 4 choix par question
- Deux niveaux de difficulté :
  - **Facile** — pool restreint (~30 familles)
  - **Difficile** — pool étendu (toutes les familles jouables)
- Système d'indices progressifs
- Liens vers les pages de référence La Garde de Nuit
- Attribution et crédits des sources (CC BY-SA 4.0, sauf mention contraire)

---

### Architecture

```
SevenSigils.Frontend      React 19 + Vite + TypeScript   http://localhost:5173
       │  HTTP REST
SevenSigils.Api           ASP.NET Core 10                http://localhost:5189
       │  DI / interfaces
SevenSigils.Application   Services métier
       │
SevenSigils.Infrastructure  MongoDB (MongoDbBlazonRepository, BlazonSeeder, CryptoRandomProvider)
       │
SevenSigils.Domain        Entités pures + interfaces (aucune dépendance externe)
```

**Projets de test**

| Projet | Type | Outils |
|---|---|---|
| `SevenSigils.Tests.Unit` | Tests unitaires | xUnit · FluentAssertions |
| `SevenSigils.Tests.Integration` | Tests d'intégration | xUnit · Testcontainers (MongoDB) · WebApplicationFactory |

---

### Stack technique

| Couche | Technologies |
|---|---|
| Backend | .NET 10 · ASP.NET Core · JWT Bearer · Serilog · FluentValidation · Swagger (dev) · Rate Limiting |
| Base de données | MongoDB 7.0 (via `MongoDB.Driver 3.x`) |
| Frontend | React 19 · Vite 8 · TypeScript |
| Conteneurisation | Docker Compose (mongodb + api + frontend/nginx) |
| Tests | xUnit · FluentAssertions · Testcontainers |

---

### Commandes

#### Docker Compose (recommandé)

```bash
# Démarrer tous les services (mongodb + api + frontend) avec override dev auto-appliqué
docker compose up --build

# En arrière-plan
docker compose up -d --build

# Arrêter
docker compose down
```

L'API est accessible sur `http://localhost:8080`, le frontend sur `http://localhost`.

#### Backend (sans Docker)

```bash
# Restaurer les packages
dotnet restore SevenSigils.slnx

# Compiler
dotnet build SevenSigils.slnx

# Lancer l'API (http://localhost:5189 · Swagger : /swagger)
dotnet run --project SevenSigils.Api

# Tests
dotnet test SevenSigils.slnx
```

#### Frontend (sans Docker)

```bash
cd SevenSigils.Frontend
npm install
npm run dev     # http://localhost:5173
npm run build
npm run lint
```

---

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommandé pour démarrer l'ensemble)

Pour une exécution sans Docker, une instance MongoDB sur `mongodb://localhost:27017` est nécessaire.  
Avec Compass, utilisez l'URI : `mongodb://localhost:27017/`

Au premier démarrage, l'API **seed automatiquement** la collection `blazons` depuis `SevenSigils.Api/data/blazonDb.json` si elle est vide.

---

### Source de données des blasons

`blazonDb.json` (issu de `heraldik_of_the_watch`) :

- `easyModeSlugs` — liste des slugs du mode facile
- `entries` — métadonnées par slug (label, hints, kind, variantOf, includeInHard, housePageUrl…)

Les images des blasons sont dans `SevenSigils.Frontend/public/blazons/`  
et suivent le pattern : `Blason-<slug>-2014-v01-256px.png`

---

### État du projet

| Endpoint | Auth | État |
|---|---|---|
| `POST /api/v1/quiz/question` | Anonyme | ✅ |
| `GET /health` | Anonyme | ✅ |
| `GET /swagger` | Anonyme | ✅ (dev uniquement) |
| `POST /api/v1/auth/register` | Anonyme | ✅ |
| `POST /api/v1/auth/login` | Anonyme | ✅ |
| `GET /api/v1/catalog` | User | ✅ |
| `GET /api/v1/catalog/{slug}` | User | ✅ |
| `POST /api/v1/admin/blazons` | Admin | ✅ |
| `PUT /api/v1/admin/blazons/{slug}` | Admin | ✅ |
| `DELETE /api/v1/admin/blazons/{slug}` | Admin | ✅ |

---

### Crédits

Blasons fournis par [La Garde de Nuit](https://lagardedenuit.com) — licence **CC BY-SA 4.0**, sauf mention contraire.  
Auteur principal des illustrations : **Evrach**.

---

## English

**Seven Sigils** is a web quiz about heraldic sigils from the *Game of Thrones* universe, based on content from [La Garde de Nuit](https://lagardedenuit.com).

Fullstack rewrite of the original [`heraldik_of_the_watch`](https://github.com/Gregb342/heraldik_of_the_watch) project:  
**ASP.NET Core 10** backend + **React 19 / Vite / TypeScript** frontend.

---

### Features

- Interactive quiz with 4 options per question
- Two difficulty levels:
  - **Easy** — restricted pool (~30 houses)
  - **Hard** — extended pool (all playable houses)
- Progressive hint system
- Links to La Garde de Nuit reference pages
- Source attribution and credits (CC BY-SA 4.0 unless otherwise stated)

---

### Quick start

#### Docker Compose (recommended)

```bash
docker compose up --build
```

API: `http://localhost:8080` · Frontend: `http://localhost`  
MongoDB Compass: `mongodb://localhost:27017/`

#### Backend (without Docker)

```bash
dotnet restore SevenSigils.slnx
dotnet run --project SevenSigils.Api
```

#### Frontend (without Docker)

```bash
cd SevenSigils.Frontend && npm install && npm run dev
```

Requires a running MongoDB instance on `mongodb://localhost:27017`.

---

### Credits

Blazon images provided by [La Garde de Nuit](https://lagardedenuit.com) — **CC BY-SA 4.0** license unless otherwise stated.  
Main illustrator: **Evrach**.
