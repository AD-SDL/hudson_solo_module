# hudson_solo_module

Contains the `hudson_solo_module` python package, providing an interface and adapter that works alongside Hudson's SOLOSOFT application to automate a Hudson Solo Liquidhandler.

## Installation (Windows)

For either method described below, you'll need to [clone this repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository).

### Using venv
1. Open Command Prompt and navigate to the project directory:
   ```sh
   cd path\to\hudson_solo_module
   ```
2. Create a virtual environment:
   ```sh
   python -m venv .venv
   ```
3. Activate the virtual environment:
   ```sh
   .venv\Scripts\activate
   ```
4. Install dependencies:
   ```sh
   pip install -r requirements.txt
   ```

### Using pdm
1. Install [PDM](https://pdm.fming.dev/latest/#installationhttps://pdm-project.org/latest/#installation):
2. Navigate to the project directory:
   ```sh
   cd path\to\hudson_solo_module
   ```
3. Install dependencies:
   ```sh
   pdm install
   ```

## Starting a Node


To start the MADSci node:

```
python -m hudson_solo_module.solo_node --definition path\to\__.node.yaml
```

An example node definition file is included in `definitions/hudson_solo.node.yaml`
