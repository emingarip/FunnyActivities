#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENV_FILE="/etc/funnyactivities/monitoring.env"
APP_LOG_DIR="/var/www/funnyactivities/publish/logs"

if [[ "$(id -u)" -eq 0 ]]; then
  SUDO=""
else
  SUDO="sudo"
fi

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command not found: $1" >&2
    exit 1
  fi
}

require_command docker
require_command nginx
require_command python3
require_command openssl
require_command apt-get

DOCKER_COMPOSE_CMD=()
if ${SUDO} docker compose version >/dev/null 2>&1; then
  if [[ -n "${SUDO}" ]]; then
    DOCKER_COMPOSE_CMD=(sudo docker compose)
  else
    DOCKER_COMPOSE_CMD=(docker compose)
  fi
elif command -v docker-compose >/dev/null 2>&1; then
  if [[ -n "${SUDO}" ]]; then
    DOCKER_COMPOSE_CMD=(sudo docker-compose)
  else
    DOCKER_COMPOSE_CMD=(docker-compose)
  fi
else
  ${SUDO} apt-get update
  if ${SUDO} apt-get install -y docker-compose-plugin; then
    if ${SUDO} docker compose version >/dev/null 2>&1; then
      if [[ -n "${SUDO}" ]]; then
        DOCKER_COMPOSE_CMD=(sudo docker compose)
      else
        DOCKER_COMPOSE_CMD=(docker compose)
      fi
    fi
  fi

  if [[ ${#DOCKER_COMPOSE_CMD[@]} -eq 0 ]]; then
    ${SUDO} apt-get install -y docker-compose
    if command -v docker-compose >/dev/null 2>&1; then
      if [[ -n "${SUDO}" ]]; then
        DOCKER_COMPOSE_CMD=(sudo docker-compose)
      else
        DOCKER_COMPOSE_CMD=(docker-compose)
      fi
    fi
  fi

  if [[ ${#DOCKER_COMPOSE_CMD[@]} -eq 0 ]]; then
    echo "Neither docker compose nor docker-compose is available on the VPS." >&2
    exit 1
  fi
fi

${SUDO} mkdir -p "${APP_LOG_DIR}"
${SUDO} mkdir -p /etc/funnyactivities

if [[ ! -f "${ENV_FILE}" ]]; then
  GENERATED_PASSWORD="$(openssl rand -base64 24 | tr -d '\n')"
  cat <<EOF | ${SUDO} tee "${ENV_FILE}" >/dev/null
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=${GENERATED_PASSWORD}
GRAFANA_ROOT_URL=https://makethen.com/grafana/
API_LOG_DIR=${APP_LOG_DIR}
EOF
  ${SUDO} chmod 600 "${ENV_FILE}"
  echo "${GENERATED_PASSWORD}" | ${SUDO} tee /etc/funnyactivities/grafana.initial-password >/dev/null
  ${SUDO} chmod 600 /etc/funnyactivities/grafana.initial-password
  echo "Created ${ENV_FILE}. Initial Grafana password saved to /etc/funnyactivities/grafana.initial-password"
fi

SITE_CONFIG="$(${SUDO} sh -c 'grep -Rsl "server_name .*makethen\\.com" /etc/nginx/sites-enabled /etc/nginx/conf.d 2>/dev/null | head -n 1 || true')"
if [[ -z "${SITE_CONFIG}" ]]; then
  echo "Unable to find the active nginx site config for makethen.com" >&2
  exit 1
fi

SNIPPET_FILE="${ROOT_DIR}/nginx/grafana-location.conf"

${SUDO} env SITE_CONFIG="${SITE_CONFIG}" SNIPPET_FILE="${SNIPPET_FILE}" python3 - <<'PY'
import os
import pathlib
import re
import sys

site_config = pathlib.Path(os.environ["SITE_CONFIG"])
snippet_file = pathlib.Path(os.environ["SNIPPET_FILE"])
content = site_config.read_text(encoding="utf-8")
snippet = snippet_file.read_text(encoding="utf-8").rstrip() + "\n\n"

if "location /grafana/" in content:
    sys.exit(0)

match = re.search(r"^\s*location /\s*\{\s*$", content, flags=re.MULTILINE)
if match is None:
    raise SystemExit("Could not locate insertion point for Grafana nginx snippet")

site_config.write_text(content[:match.start()] + snippet + content[match.start():], encoding="utf-8")
PY

${SUDO} nginx -t
${SUDO} systemctl reload nginx
"${DOCKER_COMPOSE_CMD[@]}" --env-file "${ENV_FILE}" -f "${ROOT_DIR}/docker-compose.monitoring.yml" pull
"${DOCKER_COMPOSE_CMD[@]}" --env-file "${ENV_FILE}" -f "${ROOT_DIR}/docker-compose.monitoring.yml" up -d --remove-orphans

echo "Monitoring stack deployed. Grafana should be reachable at https://makethen.com/grafana/"
