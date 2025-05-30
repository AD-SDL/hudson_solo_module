# List available commands
default:
  @just --list

# initialize the project
init:
  @which pdm || echo "pdm not found, you'll need to install it: https://github.com/pdm-project/pdm"
  @#pdm config use_uv true
  @pdm install -G:all
  @OSTYPE="" . .venv/bin/activate
  @which pre-commit && pre-commit install && pre-commit autoupdate || true

# Run the pre-commit checks
checks:
  @pre-commit run --all-files || { echo "Checking fixes\n" ; pre-commit run --all-files; }
# Run the pre-commit checks
check: checks
