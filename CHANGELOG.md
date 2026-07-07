# Changelog

Ce fichier est généré automatiquement par [release-please](https://github.com/googleapis/release-please)
à partir des messages de commits ([Conventional Commits](https://www.conventionalcommits.org/fr/)).
Ne pas l'éditer à la main au-delà de cette introduction.

## [0.2.0](https://github.com/Gregb342/seven-sigils.net/compare/v0.1.0...v0.2.0) (2026-07-07)


### Features

* **admin:** add blazon collection export (Mongo -&gt; blazonDb.json shape) ([dd8ec52](https://github.com/Gregb342/seven-sigils.net/commit/dd8ec5289fc09a9efeaecc80dde0dde21e88e1b7))
* **admin:** add blazon collection export (Mongo -&gt; blazonDb.json shape) ([7772b04](https://github.com/Gregb342/seven-sigils.net/commit/7772b04c362652a5159380265e5790da27f42295))
* **admin:** seed admin account from configuration at startup ([52a40ff](https://github.com/Gregb342/seven-sigils.net/commit/52a40ff7b3efb3830ffae48fa2010bd07d0529fc))
* **admin:** seed admin account from configuration at startup ([0a6a5ba](https://github.com/Gregb342/seven-sigils.net/commit/0a6a5ba69a962f456eeb70bafb295a73ecabc1a1))
* **auth:** Iteration 2 - JWT auth, BCrypt, FluentValidation, AuthController, tests ([741a4e6](https://github.com/Gregb342/seven-sigils.net/commit/741a4e62227813ad695f28a7770b714fe518818f))
* **catalog-admin:** Iteration 3 - CatalogController, AdminController, IBlazonRepository extended, tests (43 total) ([0020000](https://github.com/Gregb342/seven-sigils.net/commit/0020000479e0bcb1b3ac8124362e0d16a7a90223))
* **catalog:** make the encyclopedia public (read-only, no auth) ([8792cb9](https://github.com/Gregb342/seven-sigils.net/commit/8792cb90f88e36b1499f548a66f1159824f33b5f))
* **catalog:** make the encyclopedia public (read-only, no auth) ([c720b3c](https://github.com/Gregb342/seven-sigils.net/commit/c720b3cb3c5107060455400f28d2582fc46449b7))
* **citadel:** spartan admin back-office for blazon editing ([4668ee1](https://github.com/Gregb342/seven-sigils.net/commit/4668ee1472714bbbdc71e99f9c2eddf559c96206))
* **citadel:** spartan admin back-office for blazon editing ([2d98703](https://github.com/Gregb342/seven-sigils.net/commit/2d98703d3fdf6b08073b7a71acb153e148b53afa))
* **frontend:** migrate from scaffold to full React app — quiz, auth, encyclopedia ([9fbd0d8](https://github.com/Gregb342/seven-sigils.net/commit/9fbd0d82cdebb2ef7654dd263b99f39580eeb851))
* **gitignore:** add .env to ignore list ([056be2d](https://github.com/Gregb342/seven-sigils.net/commit/056be2d0c7384514ede4e3a4fc53f691082a8892))
* **hardening:** rate limiting, MongoDB health check, docker-compose prod/dev split ([adcbf6e](https://github.com/Gregb342/seven-sigils.net/commit/adcbf6e7f66b91d5569f3252c6b73ac8b270d8f0))
* **highscores:** arcade-style pseudo + local top 10, fully client-side ([7737631](https://github.com/Gregb342/seven-sigils.net/commit/77376310e83460cbdc411e8f0af8b093c3c16b15))
* **highscores:** arcade-style pseudo + local top 10, fully client-side ([e93e966](https://github.com/Gregb342/seven-sigils.net/commit/e93e9660c68d28ecd14b101317bdc799e9b3a77a))
* **infra:** add Docker Compose stack — mongodb, api, frontend ([57837f1](https://github.com/Gregb342/seven-sigils.net/commit/57837f13f534e542bced3e4d911d52260ed6d3e6))
* **release:** automated versioning and changelog via release-please ([0f47138](https://github.com/Gregb342/seven-sigils.net/commit/0f47138e0d8fbea92216790336b30df9a0a35046))
* **release:** automated versioning and changelog via release-please ([8fdfc9b](https://github.com/Gregb342/seven-sigils.net/commit/8fdfc9b3c597c6702bd9cbcc06918f019e916bcb))
* **tests:** add real Testcontainers quiz integration tests + fix 400 on exhausted pool ([d963937](https://github.com/Gregb342/seven-sigils.net/commit/d963937cb4559ae033360f899ef61805cc02cc81))


### Bug Fixes

* **deps:** bump MongoDB.Driver 3.8.0 -&gt; 3.9.0 to fix Snappier/SharpCo… ([c47a98f](https://github.com/Gregb342/seven-sigils.net/commit/c47a98fc1b94c000ca597e101b72319992346d37))
* **deps:** bump MongoDB.Driver 3.8.0 -&gt; 3.9.0 to fix Snappier/SharpCompress CVEs ([4f2459e](https://github.com/Gregb342/seven-sigils.net/commit/4f2459e2264e9513450b153fe129845ad12e51a3))
* **deps:** pin Microsoft.OpenApi 2.7.5 to fix DoS advisory GHSA-v5pm-… ([b84bfdd](https://github.com/Gregb342/seven-sigils.net/commit/b84bfdd1d9203f1ef8b60d8fd9867f2274bb8614))
* **deps:** pin Microsoft.OpenApi 2.7.5 to fix DoS advisory GHSA-v5pm-xwqc-g5wc ([870b2bc](https://github.com/Gregb342/seven-sigils.net/commit/870b2bcbdcb27c195b4c62704f995d7156d7de20))
* **docker:** install curl in API runtime image so healthcheck passes ([5e42199](https://github.com/Gregb342/seven-sigils.net/commit/5e421999b4706a6b604b3a501e14fcc677f474ee))
* **docker:** install curl in API runtime image so healthcheck passes ([e9fd7b2](https://github.com/Gregb342/seven-sigils.net/commit/e9fd7b2ca4fe12d91c5c098468580602841e7eb3))
* **security:** fail-fast on default JWT key, harden HTTPS metadata check ([fd4ca3f](https://github.com/Gregb342/seven-sigils.net/commit/fd4ca3fdf87b56fc33785e3671a4591a725c50b3))
* **security:** fail-fast on default JWT key, harden HTTPS metadata check ([64cff4c](https://github.com/Gregb342/seven-sigils.net/commit/64cff4c734be1063b154c363007d7a17ade838a7))

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
