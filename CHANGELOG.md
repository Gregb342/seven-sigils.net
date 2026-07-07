# Changelog

Ce fichier est généré automatiquement par [release-please](https://github.com/googleapis/release-please)
à partir des messages de commits ([Conventional Commits](https://www.conventionalcommits.org/fr/)).
Ne pas l'éditer à la main au-delà de cette introduction.

## 0.1.0 (2026-07-07)

Version initiale — refonte fullstack de [`heraldik_of_the_watch`](https://github.com/Gregb342/heraldik_of_the_watch)
(ASP.NET Core 10 + React 19/Vite/TypeScript + MongoDB), reconstituée rétroactivement
depuis l'historique (PRs #1 à #14).

### Features

* **quiz:** API de génération de questions (4 choix, difficultés facile/difficile, indices progressifs) ([#1](https://github.com/Gregb342/seven-sigils.net/pull/1))
* **auth:** authentification JWT, hachage BCrypt, validation FluentValidation ([#2](https://github.com/Gregb342/seven-sigils.net/pull/2))
* **catalog:** catalogue paginé des blasons + endpoints d'administration CRUD ([#2](https://github.com/Gregb342/seven-sigils.net/pull/2))
* **frontend:** application React complète — quiz, encyclopédie, écrans de jeu ([#2](https://github.com/Gregb342/seven-sigils.net/pull/2))
* **infra:** stack Docker Compose (mongodb + api + frontend/nginx), seed automatique depuis blazonDb.json ([#2](https://github.com/Gregb342/seven-sigils.net/pull/2))
* **hardening:** rate limiting, health check MongoDB, split compose dev/prod ([#3](https://github.com/Gregb342/seven-sigils.net/pull/3))
* **catalog:** encyclopédie publique en lecture seule, sans compte ([#9](https://github.com/Gregb342/seven-sigils.net/pull/9))
* **highscores:** pseudo façon arcade + top 10 local par difficulté, 100 % localStorage ([#10](https://github.com/Gregb342/seven-sigils.net/pull/10))
* **admin:** compte admin seedé depuis la configuration, sans valeur par défaut ([#12](https://github.com/Gregb342/seven-sigils.net/pull/12))
* **citadel:** back-office d'édition des blasons (route discrète /#citadel, JWT en mémoire) ([#13](https://github.com/Gregb342/seven-sigils.net/pull/13))
* **admin:** export de la collection au format blazonDb.json (sauvegarde Mongo-canonique) ([#14](https://github.com/Gregb342/seven-sigils.net/pull/14))

### Bug Fixes

* **security:** démarrage refusé hors Development si la clé JWT est absente, placeholder ou trop courte ([#5](https://github.com/Gregb342/seven-sigils.net/pull/5))
* **deps:** MongoDB.Driver 3.8.0 → 3.9.0 — corrige Snappier (GHSA-pggp-6c3x-2xmx) et SharpCompress (GHSA-6c8g-7p36-r338) ([#6](https://github.com/Gregb342/seven-sigils.net/pull/6))
* **deps:** pin Microsoft.OpenApi 2.7.5 — corrige le DoS par récursion GHSA-v5pm-xwqc-g5wc ([#7](https://github.com/Gregb342/seven-sigils.net/pull/7))
* **docker:** installation de curl dans l'image runtime de l'API — le healthcheck échouait systématiquement ([#8](https://github.com/Gregb342/seven-sigils.net/pull/8))

### Code Refactoring

* **auth:** suppression de l'inscription publique et des comptes joueurs — l'auth ne sert plus que le back-office ([#11](https://github.com/Gregb342/seven-sigils.net/pull/11))
