"""
REST-based node that interfaces with MADSci and provides a simple Sleep(t) function
"""

import time
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Annotated

from fastapi.datastructures import UploadFile
from madsci.common.types.node_types import RestNodeConfig
from madsci.node_module.helpers import action
from madsci.node_module.rest_node_module import RestNode

from solo_interface.solo_interface import Solo


class SOLONodeConfig(RestNodeConfig):
    """Configuration for Barty the bartender robot."""

    solo_port: int = 11139
    """The TCP port to communicate with the SOLO on"""
    solosoft_path: str = (
        "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe"
    )
    """Path to the SOLOSoft.exe executable"""
    tips_file_path: str = (
        "C:\\ProgramData\\Hudson Robotics\\SoloSoft\\SoloSoft\\TipCounts.csv"
    )
    """Path to TipCounts.csv"""


class SOLONode(RestNode):
    """A node for the SOLO liquid handling robot."""

    solo_interface: Solo = None
    config_model = SOLONodeConfig
    config: SOLONodeConfig

    def startup_handler(self) -> None:
        """Initializes the SOLO client."""
        self.solo_interface = None
        self.solo_interface = Solo(self.config.solo_port, self.config.solosoft_path)

    def shutdown_handler(self) -> None:
        """Handles disconnecting from SOLOSoft on shutdown"""
        if self.solo_interface:
            del self.solo_interface

    def state_handler(self) -> None:
        """Custom state handler logic for the solo node, periodically invoked by node"""
        self.node_state["connected"] = (
            self.solo_interface.connected if self.solo_interface else False
        )

    ### ACTIONS ###
    @action
    def run_protocol(
        self,
        protocol_file: Annotated[UploadFile, "The protocol file (.hso) to run"],
    ) -> None:
        """Runs the provided SoloSoft .hso protocol file."""
        self.solo_interface.open_solo_soft()
        with NamedTemporaryFile(delete_on_close=False) as f:
            f.write(protocol_file.file.read())
            f.close()
            self.solo_interface.client.RunCommand("LOAD " + f.name)
            self.solo_interface.client.RunCommand("RUN " + f.name)

            while self.solo_interface.client.RunCommand("GETSTATUS") != "IDLE":
                time.sleep(1)

            self.solo_interface.solo.close_solo_soft()

    @action
    def refill_tips(
        self,
        position: Annotated[int, "The position on the solo deck to mark refilled"],
    ) -> None:
        """Marks a tipbox at a specific position on the solo deck as refilled."""
        position = int(position)
        with Path(self.config.tips_file_path).open() as tips_file:
            i = 0
            lines = []
            for line in tips_file:
                if i == position - 1:
                    columns = line.split(",")
                    columns[2] = columns[2].replace("0", "1")
                    lines.append(",".join(columns))
                else:
                    lines.append(line)
                i += 1  # noqa
        with Path(self.config.tips_file_path).open(mode="w") as tips_file:
            tips_file.writelines(lines)


if __name__ == "__main__":
    solo_node = SOLONode()
    solo_node.start_node()
