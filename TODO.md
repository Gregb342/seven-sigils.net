# TODO — points connus, reportés volontairement

## Mode compétitif — suites prévues

1. **Défi du jour** (validé sur le principe) : même série de questions pour tous,
   seedée par la date — nécessite une génération déterministe côté backend
   (IRandomProvider seedable + endpoint dédié).
2. Sons arcade Web Audio (écartés de la v1, envisageables plus tard).
3. Titres de rang (Mestre de la Citadelle…) : encore en dur dans le frontend
   (`resources/quotes.ts`) — seules les citations sont éditables via la citadel.
   Rendre les titres éditables si le besoin se présente.

## Back-office : upload des images de blasons

Les PNG vivent dans `SevenSigils.Frontend/public/blazons/` (buildés dans
l'image nginx). Créer un blason via la citadel ne suffit donc pas à afficher
son image : il faut copier le PNG à la main et rebuilder. Chantier à part :
stockage, validation de fichiers, redimensionnement.

## Export : l'attribution n'est pas réimportée par le seeder

`GET /api/v1/admin/blazons/export` inclut l'attribution de chaque blason
(fidélité de sauvegarde), mais `BlazonSeeder` l'ignore à l'import et
hardcode Evrach / CC BY-SA 4.0. Si des attributions sont éditées via la
citadel, améliorer le seeder pour honorer le champ si présent.

## Encyclopédie : bug d'affichage de l'index alphabétique

La barre de droite de l'encyclopédie (navigation par lettre de maison) a un
souci d'affichage. À investiguer côté `EncyclopediaScreen.tsx` / CSS.

## Lint : 2 erreurs react-hooks préexistantes (set-state-in-effect)

`npm run lint` échoue sur `EncyclopediaScreen.tsx:43` et `StartScreen.tsx:29` :
setState synchrone dans un useEffect (règle react-hooks 7). Corriger en
clampant/dérivant la valeur au rendu ou dans le handler plutôt qu'en effet.

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

## ApplicationUser / rôle Admin — décision actée

Plus de comptes joueurs : le register public a été supprimé, seuls des comptes
Admin existent (seedés côté serveur — à implémenter avec le back-office).
L'app ne collecte aucune donnée personnelle de joueur (pseudo/scores en
localStorage uniquement). Pas de refresh token nécessaire (JWT 60 min, choix
assumé pour un usage admin).
