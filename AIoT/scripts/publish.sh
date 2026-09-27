#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="Release"
SELF_CONTAINED="true"
SINGLE_FILE="true"
TRIMMED="false"
NO_RESTORE="false"
RIDS=("win-x64" "linux-x64" "osx-x64")

while [[ $# -gt 0 ]]; do
  case "$1" in
	-c|--configuration)
	  CONFIGURATION="$2"
	  shift 2
	  ;;
	--rid)
	  RIDS=("$2")
	  shift 2
	  ;;
	--rids)
	  IFS=',' read -r -a RIDS <<< "$2"
	  shift 2
	  ;;
	--self-contained)
	  SELF_CONTAINED="$2"
	  shift 2
	  ;;
	--single-file)
	  SINGLE_FILE="$2"
	  shift 2
	  ;;
	--trimmed)
	  TRIMMED="$2"
	  shift 2
	  ;;
	--no-restore)
	  NO_RESTORE="true"
	  shift
	  ;;
	*)
	  echo "Unknown argument: $1"
	  exit 1
	  ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR/../AIoT.csproj"
DIST_ROOT="$SCRIPT_DIR/../dist"

if [[ ! -f "$PROJECT_PATH" ]]; then
  echo "Project not found: $PROJECT_PATH"
  exit 1
fi

mkdir -p "$DIST_ROOT"

echo "== AIoT publish =="
echo "Project: $PROJECT_PATH"
echo "Configuration: $CONFIGURATION"
echo "RIDs: ${RIDS[*]}"

for rid in "${RIDS[@]}"; do
  out_dir="$DIST_ROOT/$rid"
  mkdir -p "$out_dir"

  args=(
	publish "$PROJECT_PATH"
	-c "$CONFIGURATION"
	-r "$rid"
	-o "$out_dir"
	"-p:PublishSingleFile=$SINGLE_FILE"
	"-p:SelfContained=$SELF_CONTAINED"
	"-p:PublishTrimmed=$TRIMMED"
	"-p:AutoBuildProtocolPlugins=true"
  )

  if [[ "$NO_RESTORE" == "true" ]]; then
	args+=(--no-restore)
  fi

  echo
  echo "Publishing $rid -> $out_dir"
  dotnet "${args[@]}"
done

echo

echo "Publish completed. Outputs in: $DIST_ROOT"
echo "Windows executable example: $DIST_ROOT/win-x64/AIoT.exe"
