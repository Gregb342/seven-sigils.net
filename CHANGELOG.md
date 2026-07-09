# Changelog

Ce fichier est généré automatiquement par [release-please](https://github.com/googleapis/release-please)
à partir des messages de commits ([Conventional Commits](https://www.conventionalcommits.org/fr/)).
Ne pas l'éditer à la main au-delà de cette introduction.

## [0.3.0](https://github.com/Gregb342/seven-sigils.net/compare/v0.2.0...v0.3.0) (2026-07-09)


### Features

* **quotes:** editable rank titles served with the quotes ([#28](https://github.com/Gregb342/seven-sigils.net/issues/28)) ([4fcae63](https://github.com/Gregb342/seven-sigils.net/commit/4fcae6318c32148832d94d7e548c7c6a3d119c4a))


### Bug Fixes

* **encyclopedia:** full catalog loading and letter-rail filter ([#24](https://github.com/Gregb342/seven-sigils.net/issues/24)) ([810c3a3](https://github.com/Gregb342/seven-sigils.net/commit/810c3a3f4b71d0b1504ecbe98255e140862cd56f))
* **lint:** derive the clamped rounds value at render in StartScreen ([#26](https://github.com/Gregb342/seven-sigils.net/issues/26)) ([2b17cce](https://github.com/Gregb342/seven-sigils.net/commit/2b17ccecdb02940aae88b9480c2d346af9f4596f))
* **quiz:** never propose the same family label twice in the options ([#25](https://github.com/Gregb342/seven-sigils.net/issues/25)) ([83bf851](https://github.com/Gregb342/seven-sigils.net/commit/83bf8518e00fd28027658bb7e7a9e5cbb84a742a))
* **seeder:** honor the attribution field when importing blazonDb.json ([#27](https://github.com/Gregb342/seven-sigils.net/issues/27)) ([dbe42d4](https://github.com/Gregb342/seven-sigils.net/commit/dbe42d4f0f6a73bcf1dd65c6a1af07b4a18d385e))

## [0.2.0](https://github.com/Gregb342/seven-sigils.net/compare/v0.1.0...v0.2.0) (2026-07-09)


### Features

* **competitive:** timed competitive mode with arcade scoring and local rankings ([2c98b2a](https://github.com/Gregb342/seven-sigils.net/commit/2c98b2a198631cc0e58c85d46810b83952414f64))
* **quotes:** end-screen quotes editable from the citadel ([123915c](https://github.com/Gregb342/seven-sigils.net/commit/123915cf440f884c0fa47659bd6ddc801a94ddea))

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
