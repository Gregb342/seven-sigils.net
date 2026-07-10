# Déploiement en production

Architecture : un VPS avec Docker, la stack `deploy/docker-compose.yml`
(MongoDB + API + frontend/nginx derrière Caddy qui gère le HTTPS), et le
workflow GitHub Actions [`deploy.yml`](../.github/workflows/deploy.yml) qui,
à chaque merge dans `main` : joue tous les tests → build les images → les
pousse sur GHCR → se connecte au VPS en SSH → `docker compose pull && up -d`.

## 1. Louer le VPS

- Hetzner CX22 (~4 €/mois) ou OVH VPS (~4 €/mois), **Ubuntu 24.04 LTS**.
- À la création, enregistrer sa **clé SSH publique** (pas de mot de passe root).

## 2. Préparer le serveur (une seule fois)

```bash
# Connecté en root sur le VPS :

# Utilisateur de déploiement (jamais travailler en root)
adduser --disabled-password deploy
mkdir -p /home/deploy/.ssh
cp ~/.ssh/authorized_keys /home/deploy/.ssh/
chown -R deploy:deploy /home/deploy/.ssh
chmod 700 /home/deploy/.ssh && chmod 600 /home/deploy/.ssh/authorized_keys

# Docker
curl -fsSL https://get.docker.com | sh
usermod -aG docker deploy

# Pare-feu : SSH + web, rien d'autre
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp   # HTTP/3
ufw --force enable

# Durcissement SSH : désactiver l'auth par mot de passe
sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart ssh

# Mises à jour de sécurité automatiques
apt-get update && apt-get install -y unattended-upgrades
```

## 3. Installer la stack

```bash
# En tant que deploy :
sudo mkdir -p /opt/seven-sigils && sudo chown deploy:deploy /opt/seven-sigils
cd /opt/seven-sigils
```

Copier depuis le repo (scp ou copier-coller) :
- `deploy/docker-compose.yml` → `/opt/seven-sigils/docker-compose.yml`
- `deploy/Caddyfile` → `/opt/seven-sigils/Caddyfile`
- `deploy/.env.example` → `/opt/seven-sigils/.env`, puis **remplir toutes les
  valeurs** (`JWT_KEY` : `openssl rand -base64 48` ; mot de passe admin ≥ 12
  caractères). `chmod 600 .env`.

Sans domaine pour l'instant : `SITE_ADDRESS=http://IP.DU.SERVEUR` et
`SITE_ORIGIN=http://IP.DU.SERVEUR` (HTTP simple, à basculer dès le domaine acheté).

## 4. Le domaine (dès qu'il est acheté)

Chez le registrar, deux enregistrements DNS **A** pointant vers l'IP du VPS :
`@` et `www`. Puis dans `.env` : `SITE_ADDRESS=seven-sigils.net,
www.seven-sigils.net` et `SITE_ORIGIN=https://seven-sigils.net`, et
`docker compose up -d` — Caddy obtient le certificat Let's Encrypt tout seul.

## 5. Câbler GitHub Actions

Dans le repo → Settings → Secrets and variables → Actions → **New repository
secret** :

| Secret | Valeur |
|---|---|
| `VPS_HOST` | IP du serveur |
| `VPS_USER` | `deploy` |
| `VPS_SSH_KEY` | une clé privée SSH **dédiée au déploiement** (voir ci-dessous) |

Clé dédiée (à générer sur ton poste, pas la clé perso) :
```bash
ssh-keygen -t ed25519 -f deploy_key -N "" -C "github-actions-deploy"
# deploy_key.pub → à ajouter dans /home/deploy/.ssh/authorized_keys sur le VPS
# deploy_key (privée) → contenu du secret VPS_SSH_KEY, puis supprimer le fichier local
```

## 6. Premier déploiement

1. Merger cette branche dans `dev` puis `dev → main` (ou lancer le workflow
   à la main : Actions → deploy → Run workflow).
2. Au premier passage, les images GHCR sont créées **privées** : aller sur
   github.com/Gregb342?tab=packages → chaque package → Package settings →
   Change visibility → **Public** (sinon le VPS ne peut pas les tirer).
   À faire une seule fois ; relancer le workflow ensuite.
3. Vérifier : `https://seven-sigils.net` (ou `http://IP`), `/version` doit
   répondre la version courante.

## Exploitation

```bash
cd /opt/seven-sigils
docker compose ps                 # état
docker compose logs -f api        # logs API
docker compose logs caddy         # certificats/HTTPS

# Sauvegarde MongoDB (à mettre en cron hebdomadaire)
docker compose exec -T mongodb mongodump --archive --db sevensigils > backup-$(date +%F).archive

# Restauration
docker compose exec -T mongodb mongorestore --archive --drop < backup-XXXX.archive
```

Mise en production = merger dans `main`. Rien d'autre.
