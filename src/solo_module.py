"""
REST-based node that interfaces with WEI and provides a simple Sleep(t) function
"""

import time
from pathlib import Path
from tempfile import NamedTemporaryFile

from fastapi.datastructures import UploadFile
from starlette.datastructures import State
from typing_extensions import Annotated
from wei.modules.rest_module import RESTModule
from wei.types.module_types import ModuleState
from wei.types.step_types import StepResponse, StepSucceeded
from wei.utils import extract_version

from solo_interface.solo_interface import Solo

solo_module = RESTModule(
    name="solo",
    version=extract_version(Path(__file__).parent.parent / "pyproject.toml"),
    description="Controls a Hudson SOLO liquidhandler via SOLOSoft",
    model="Hudson SOLO Liquidhandler",
)
solo_module.arg_parser.add_argument(
    "--solo_port", default=11139, type=int, help="The port to communicate with the SOLO"
)
solo_module.arg_parser.add_argument(
    "--solo_soft_path",
    type=str,
    default="C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe",
    help="Path to the SOLOSoft.exe executable",
)
solo_module.arg_parser.add_argument(
    "--tipsFilePath",
    type=str,
    default="C:\\ProgramData\\Hudson Robotics\\SoloSoft\\SoloSoft\\TipCounts.csv",
    help="Path to TipCounts.csv",
)


# ***********#
# *Lifecycle*#
# ***********#
@solo_module.startup()
def custom_startup_handler(state: State):
    """
    Initializes the solo client
    """
    state.solo = None
    state.solo = Solo(state.solo_port, state.solo_soft_path)


@solo_module.shutdown()
def custom_shutdown_handler(state: State):
    """
    Handles disconnecting from SOLOSoft on shutdown
    """
    if state.solo:
        del state.solo


@solo_module.state_handler()
def custom_state_handler(state: State) -> ModuleState:
    """
    Returns the current state of the module
    """

    return ModuleState.model_validate(
        {
            "status": state.status,  # *Required
            "error": state.error,
            # *Custom state fields
            "connected": state.solo.connected if state.solo else None,
        }
    )


###########
# Actions #
###########


@solo_module.action()
def run_protocol(
    state: State,
    protocol_file: Annotated[UploadFile, "The protocol file (.hso) to run"],
) -> StepResponse:
    """Runs the provided SoloSoft .hso protocol file."""
    solo: Solo = state.solo
    solo.open_solo_soft()
    with NamedTemporaryFile(delete_on_close=False) as f:
        f.write(protocol_file.file.read())
        f.close()
        print(f.name)
        status_code = solo.client.RunCommand("LOAD " + f.name)
        print(status_code)
        status_code = solo.client.RunCommand("RUN " + f.name)
        print(status_code)

        while solo.client.RunCommand("GETSTATUS") != "IDLE":
            time.sleep(1)

        solo.close_solo_soft()

    return StepSucceeded()


@solo_module.action()
def refill_tips(
    state: State,
    position: Annotated[int, "The position on the solo deck to mark refilled"],
) -> StepResponse:
    """Marks a tipbox at a specific position on the solo deck as refilled."""
    position = int(position)
    with open(state.tipsFilePath, mode="r") as tips_file:
        i = 0
        lines = []
        for line in tips_file:
            if i == position - 1:
                columns = line.split(",")
                columns[2] = columns[2].replace("0", "1")
                lines.append(",".join(columns))
            else:
                lines.append(line)
            i += 1
    with open(state.tipsFilePath, mode="w") as tips_file:
        tips_file.writelines(lines)
    return StepSucceeded()


if __name__ == "__main__":
    solo_module.start()
