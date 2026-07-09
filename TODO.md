# TODO — points connus, reportés volontairement

*(Nettoyé le 2026-07-09 : doublons du quiz, lint react-hooks, attribution à
l'import et titres de rang éditables sont corrigés ; défi du jour, sons
arcade, upload d'images et seed non-bloquant sont volontairement abandonnés
en attendant les retours utilisateurs.)*

## ApplicationUser / rôle Admin — décision actée

Plus de comptes joueurs : le register public a été supprimé, seuls des comptes
Admin existent (seedés côté serveur via ADMIN_EMAIL/ADMIN_PASSWORD).
L'app ne collecte aucune donnée personnelle de joueur (pseudo/scores en
localStorage uniquement). Pas de refresh token nécessaire (JWT 60 min, choix
assumé pour un usage admin).
