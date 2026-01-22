#!/bin/bash
# Pre-push hook for architecture validation
# Install: Copy to .git/hooks/pre-push and chmod +x

set -e

echo "🏗️  Running architecture validation before push..."
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Build the solution
echo "📦 Building solution..."
if ! dotnet build --configuration Release > /dev/null 2>&1; then
    echo -e "${RED}❌ Build failed${NC}"
    echo "   Fix build errors before pushing"
    exit 1
fi
echo -e "${GREEN}✅ Build successful${NC}"
echo ""

# Run architecture tests
echo "🔍 Running architecture tests..."
if ! dotnet test src/tests/ArchitectureTests --no-build --configuration Release --verbosity quiet > /tmp/arch-test-output.txt 2>&1; then
    echo -e "${RED}❌ Architecture tests failed${NC}"
    echo ""
    echo "Test output:"
    cat /tmp/arch-test-output.txt
    echo ""
    echo "Please fix architecture violations before pushing."
    echo "See docs/adr/governance/ADR-0000-architecture-tests.md for guidance."
    exit 1
fi
echo -e "${GREEN}✅ Architecture tests passed${NC}"
echo ""

# Check for analyzer warnings (optional, non-blocking)
echo "🔬 Checking for analyzer warnings..."
WARNINGS=$(dotnet build --no-restore --configuration Release 2>&1 | grep -c "warning ADR" || true)
if [ "$WARNINGS" -gt 0 ]; then
    echo -e "${YELLOW}⚠️  Found $WARNINGS architecture analyzer warning(s)${NC}"
    echo "   Consider reviewing these before pushing (non-blocking)"
else
    echo -e "${GREEN}✅ No analyzer warnings${NC}"
fi
echo ""

echo -e "${GREEN}🎉 All checks passed! Push allowed.${NC}"
exit 0
