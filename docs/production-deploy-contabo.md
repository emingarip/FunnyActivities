# FunnyActivities Production Deploy (Contabo VPS)

Bu dokuman `docker-compose.prod.yml` ve `deploy/Caddyfile` kullanarak tek VPS uzerinde production kurulumunu anlatir.

## 1) Sunucu hazirligi

1. Ubuntu VPS'e SSH ile baglanin.
2. Firewall acin:
   - `sudo ufw allow OpenSSH`
   - `sudo ufw allow 80/tcp`
   - `sudo ufw allow 443/tcp`
   - `sudo ufw enable`
3. Docker ve Docker Compose plugin kurun.

## 2) Proje ve env dosyasi

1. Repo'yu sunucuya alin.
2. Ornek env dosyasini kopyalayin:
   - `cp .env.production.example .env.production`
3. `.env.production` icinde tum degerleri gercek production degerleriyle doldurun.

## 3) DNS

Asagidaki iki A kaydini VPS public IP'sine yonlendirin:

- `APP_DOMAIN` (ornek: `api.example.com`)
- `FILES_DOMAIN` (ornek: `files.example.com`)

## 4) Production stack'i kaldirma

Asagidaki komutla sistemi ayaga kaldirin:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.production up -d --build
```

Kontrol:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.production ps
docker compose -f docker-compose.prod.yml --env-file .env.production logs -f webapi
```

## 5) Guncelleme

```bash
git pull
docker compose -f docker-compose.prod.yml --env-file .env.production up -d --build
```

## 6) Yedekleme (minimum)

- Postgres: gunluk dump (`pg_dump`) + harici depolama.
- MinIO: `minio_data` volume snapshot/rsync ile gunluk yedek.
- Restore testi aylik en az 1 kez.

## Notlar

- Bu stack production icin gereksiz monitoring container'larini (prometheus/grafana/jaeger/alertmanager) dahil etmez.
- Canliya cikmadan once güvenlik PR'larinin merge edilmesi gerekir:
  - auth policy
  - CORS allowlist
  - secret management
  - forwarded headers + rate limiting
