#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${SCRIPT_DIR}/../publish/quizapp-iis"

echo "===================================================="
echo " Publishing QuizApp for IIS Deployment (.NET 10)    "
echo "===================================================="

echo "Cleaning previous output..."
rm -rf "${OUTPUT_DIR}"

echo "Building and publishing Release configuration..."
dotnet publish "${SCRIPT_DIR}/QuizApp.csproj" -c Release -o "${OUTPUT_DIR}"

echo ""
echo "✅ Publish complete! Files generated in: ${OUTPUT_DIR}"
ls -la "${OUTPUT_DIR}"
