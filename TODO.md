# TODO — points connus, reportés volontairement

## Quiz : doublons possibles dans les options

`QuizQuestionService.CreateQuestionAsync` filtre les distracteurs uniquement contre
le label de la cible, pas entre eux. Si deux blasons du pool partagent le même
`FamilyLabel` (variantes d'une même maison, cf. `variantOf` dans `blazonDb.json`),
une question peut proposer deux fois la même réponse.

**Fix envisagé** : `.DistinctBy(x => x.FamilyLabel, StringComparer.OrdinalIgnoreCase)`
avant le `Take(OptionsCount - 1)` dans `SevenSigils.Application/Services/QuizQuestionService.cs`.
Ajouter d'abord un test unitaire reproduisant le cas (variantes partageant un label).

## Seed bloquant au démarrage

`Program.cs` exécute `BlazonSeeder.SeedAsync()` avant `app.Run()` : si MongoDB est
lent à répondre, l'API ne démarre pas du tout, et le healthcheck Docker
(`retries: 3`) peut provoquer une boucle de redémarrage.

**Fix envisagé** : déplacer le seeding dans un `IHostedService` (tâche de fond
après démarrage) avec retries — l'API répond pendant que le seed s'exécute.

## ApplicationUser / rôle Admin

Le modèle `ApplicationUser` (email + rôles) vient d'un choix Copilot historique,
principalement pour créer un utilisateur admin. À réévaluer : est-ce que le
register/login public est encore pertinent, ou seul un compte admin suffit ?
Si l'app reste sans données personnelles, pas de refresh token nécessaire
(JWT 60 min, choix assumé).
