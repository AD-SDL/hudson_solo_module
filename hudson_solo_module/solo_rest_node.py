"""
A MADSci compatible REST node for interfacing with the Hudson SOLO liquid handling robot.
"""

import time
from pathlib import Path
from typing import Annotated

from madsci.common.types.node_types import RestNodeConfig
from madsci.node_module.helpers import action
from madsci.node_module.rest_node_module import RestNode

from hudson_solo_module.solo_interface import Solo


class SOLONodeConfig(RestNodeConfig):
    """Configuration for Hudson SOLO Liquid Handling Robots."""

    solo_port: int = 11139
    """The TCP port used for communication between SOLOSoft and the SOLO device."""
    solosoft_path: str = (
        "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe"
    )
    """Path to the SOLOSoft.exe executable."""
    tips_file_path: str = (
        "C:\\ProgramData\\Hudson Robotics\\SoloSoft\\SoloSoft\\TipCounts.csv"
    )
    """Path to TipCounts.csv. This file tracks tip usage on the SOLO device."""


class SOLONode(RestNode):
    """A MADSci compatible REST node for the SOLO liquid handling robot."""

    solo_interface: Solo = None
    config_model = SOLONodeConfig
    config: SOLONodeConfig = SOLONodeConfig()

    def startup_handler(self) -> None:
        """Initializes the Solo client."""
        self.solo_interface = None
        self.solo_interface = Solo(self.config.solo_port, self.config.solosoft_path)

    def shutdown_handler(self) -> None:
        """Handles disconnecting from SOLOSoft on shutdown."""
        if self.solo_interface:
            del self.solo_interface

    def state_handler(self) -> None:
        """Custom state handler logic for the SOLO node, periodically invoked by node."""
        self.node_state["connected"] = (
            self.solo_interface.connected if self.solo_interface else False
        )

    @action
    def run_protocol(
        self,
        protocol_file: Annotated[Path, "The protocol file (.hso) to run"],
    ) -> None:
        """Runs the provided SOLOSoft .hso protocol file."""
        self.solo_interface.open_solo_soft()
        self.solo_interface.client.RunCommand("LOAD " + str(protocol_file))
        time.sleep(2)
        self.solo_interface.client.RunCommand("RUN " + str(protocol_file))

        while self.solo_interface.client.RunCommand("GETSTATUS") != "IDLE":
            time.sleep(1)

        self.solo_interface.close_solo_soft()

    @action
    def refill_tips(
        self,
        position: Annotated[int, "The position on the solo deck to mark refilled"],
    ) -> None:
        """Refills a tip box at a specified location in the SOLOSoft software by editing the TipCounts.csv file."""
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
