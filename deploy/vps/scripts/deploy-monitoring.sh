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

if ! command -v curl >/dev/null 2>&1; then
  ${SUDO} apt-get update
  ${SUDO} apt-get install -y curl
fi

dump_monitoring_logs() {
  ${SUDO} docker ps -a || true
  ${SUDO} docker logs --tail 200 funnyactivities_loki || true
  ${SUDO} docker logs --tail 200 funnyactivities_promtail || true
  ${SUDO} docker logs --tail 200 funnyactivities_prometheus || true
  ${SUDO} docker logs --tail 200 funnyactivities_grafana || true
}

wait_for_url() {
  local name="$1"
  local url="$2"
  local attempts="${3:-30}"

  for ((i=1; i<=attempts; i++)); do
    if curl -fsS "${url}" >/dev/null 2>&1; then
      echo "${name} is ready at ${url}"
      return 0
    fi
    sleep 2
  done

  echo "${name} failed health check at ${url}" >&2
  return 1
}

DOCKER_COMPOSE_CMD=()
USING_LEGACY_DOCKER_COMPOSE="false"
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
  USING_LEGACY_DOCKER_COMPOSE="true"
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
      USING_LEGACY_DOCKER_COMPOSE="true"
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
marker_start = "# BEGIN funnyactivities grafana"
marker_end = "# END funnyactivities grafana"

marked_block_pattern = re.compile(
    rf"^[ \t]*{re.escape(marker_start)}.*?^[ \t]*{re.escape(marker_end)}\n?",
    flags=re.MULTILINE | re.DOTALL,
)
legacy_block_pattern = re.compile(
    r"^[ \t]*location /grafana/api/live/ \{\n.*?^[ \t]*\}\n\s*^[ \t]*location /grafana/ \{\n.*?^[ \t]*\}\n?",
    flags=re.MULTILINE | re.DOTALL,
)

if marker_start in content and marker_end in content:
    updated = marked_block_pattern.sub(snippet, content, count=1)
elif "location /grafana/" in content:
    updated, replacements = legacy_block_pattern.subn(snippet, content, count=1)
    if replacements == 0:
        raise SystemExit("Could not replace existing Grafana nginx block")
else:
    match = re.search(r"^\s*location /\s*\{\s*$", content, flags=re.MULTILINE)
    if match is None:
        raise SystemExit("Could not locate insertion point for Grafana nginx snippet")
    updated = content[:match.start()] + snippet + content[match.start():]

if updated != content:
    site_config.write_text(updated, encoding="utf-8")
PY

${SUDO} nginx -t
${SUDO} systemctl reload nginx
"${DOCKER_COMPOSE_CMD[@]}" --env-file "${ENV_FILE}" -f "${ROOT_DIR}/docker-compose.monitoring.yml" pull
if [[ "${USING_LEGACY_DOCKER_COMPOSE}" == "true" ]]; then
  "${DOCKER_COMPOSE_CMD[@]}" --env-file "${ENV_FILE}" -f "${ROOT_DIR}/docker-compose.monitoring.yml" down --remove-orphans || true
fi
"${DOCKER_COMPOSE_CMD[@]}" --env-file "${ENV_FILE}" -f "${ROOT_DIR}/docker-compose.monitoring.yml" up -d --remove-orphans

if ! wait_for_url "Loki" "http://127.0.0.1:3100/ready"; then
  dump_monitoring_logs
  exit 1
fi

if ! wait_for_url "Prometheus" "http://127.0.0.1:9090/-/ready"; then
  dump_monitoring_logs
  exit 1
fi

if ! wait_for_url "Grafana" "http://127.0.0.1:3000/api/health"; then
  dump_monitoring_logs
  exit 1
fi

echo "Monitoring stack deployed. Grafana should be reachable at https://makethen.com/grafana/"
