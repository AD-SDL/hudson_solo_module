# Hudson SOLO Module

A MADSci powered module for controlling the [Hudson SOLO Automated Liquid Handler](https://hudsonlabautomation.com/products/liquid-handlers/solo-automated-liquid-handler/).

This repository contains a Python interface (`solo_interface.py`) and a MADSci REST Node (`solo_rest_node.py`), which work in conjunction with Hudson's SOLOSoft software.

### Installation

This MADSci SOLO module must be installed on a Windows computer. Hudson's SOLOSoft software must also be installed installed and connected to your SOLO Automated Liquid Handler.

Before using this SOLO REST Node, you will need to clone the module GitHub repository and install the dependencies in a Python virtual environment. Use the code below to complete this step.

General install instructions (using pip):

   1. Clone the repository
      ```sh
      git clone https://github.com/AD-SDL/hudson_solo_module
      ```

   2. Navigate into the hudson_solo_module folder
      ```sh
      cd hudson_solo_module
      ```

   3. Create the virtual environment
      ```sh
      python -m venv .venv
      ```

   4. Activate the virtual environment
      ```sh
      .venv/Scripts/activate
      ```

   5. Install the dependencies

      ```sh
      pip install -e .
      ```

If you wish to install the dependencies using PDM, use the following command:


1. Install [PDM](https://pdm.fming.dev/latest/#installationhttps://pdm-project.org/latest/#installation)
2. Navigate to the project directory
   ```sh
   cd path\to\hudson_solo_module
   ```
3. Install dependencies
   ```sh
   pdm install
   ```

### Creating the SOLOSoft protocol

Before running a protocol on your Huson SOLO with the MADSci REST Node, you must first create and save the SOLOSoft protocol file. This file will have a .hso extension, meaning it is a <u>H</u>udson <u>SO</u>LO protocol.

There are two ways to create this SOLOSoft protocol.

   1. Create and save your protocol within the SOLOSoft application. Copy the file path of your protocol for later use with the MADSci REST Node.

   2. Create your SOLO protocol using our
   [hudson-liquidhandling](https://github.com/AD-SDL/hudson-liquidhandling) package.


### Running the REST Node

The SOLO REST Node can be started with a command in the format below. Ensure that you are in the top level directory of the hudson_solo_module repository when running the commands below.

      python your//path//to//solo_rest_node.py --node_url <(optional str) address for your SOLO MADSci REST Node> --solo_port <(optional int) the TCP port for communicating with your SOLO instrument> --solosoft_path <(optional str) path to the SOLOSoft.exe executable> --tips_file_path <(optional str) path to SOLOSoft's TipCounts.csv file">


--node_url will default to "http://127.0.0.1:2000" \
--solo_port will default to 11139 (the default in the SOLOSoft software) \
--solosoft_path will default to "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe" \
and --tips_file_path will default to         "C:\\ProgramData\\Hudson Robotics\\SoloSoft\\SoloSoft\\TipCounts.csv" (the default for the SOLOSoft software).


Example usage with no optional arguments:


    python solo_rest_node.py


Example usage with all optional arguments:

    python solo_rest_node.py --node_url "http://127.0.0.1:2003"
    --solo_port 11139 --solosoft_path "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe" --tips_file_path "C:\\ProgramData\\Hudson Robotics\\SoloSoft\\SoloSoft\\TipCounts.csv"

### Example Usage in MADSci Workflow YAML file

Below is an example of a MADSci YAML workflow file that interacts with the SOLO REST Node. Replace "PROTOCOL_FILE_PATH.hso" with the path to your .hso SOLOSoft protocol file.

    name: Run SOLO - workflow

    metadata:
      author: RPL
      info: Workflow to run a SOLO protocol
      version: 0.1

    steps:
      - name: Run protocol
         node: solo
         action: run_protocol
         files:
            protocol_file: PROTOCOL_FILE_PATH.hso
