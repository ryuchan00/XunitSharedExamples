#!/usr/bin/env bash
# xUnit.net silently drops test cases whose unique IDs collide, so a regression in this package
# shows up as missing tests rather than failures. Run the tests under both theory discovery
# paths (data enumerated at run time, and pre-enumerated during discovery) and check the exact
# count in each. Update EXPECTED_TESTS when adding tests.
set -euo pipefail

EXPECTED_TESTS=46

for mode in off on; do
  dotnet test --configuration Release --pre-enumerate-theories "$mode" | tee test.log
  # Not anchored at line start: in CI the runner prefixes the line with an ANSI color escape.
  grep -Eq "[[:space:]]total: $EXPECTED_TESTS[[:space:]]*$" test.log ||
    { echo "::error::expected exactly $EXPECTED_TESTS tests with pre-enumeration $mode"; exit 1; }
done
