"""Python interface for Hudson SOLO liquid handling robots."""

# * Using .dlls and .NET assemblies
# * pip install pythonnet
# * See docs: https://pythonnet.github.io/pythonnet/python.html

import time

import clr

clr.AddReference(
    r"C:\Program Files (x86)\Hudson Robotics\SoloSoft\Hudson.SoloSoft.Communications.dll"
)
clr.AddReference("System.Windows.Forms")

from Hudson.SoloSoft.Communications import SoloClient  # type: ignore # noqa
from System.Diagnostics import Process  # type: ignore # noqa
from System.IO import Path  # type: ignore # noqa
from System.Windows.Forms import SendKeys  # type: ignore # noqa


class Solo:
    """Interface for Hudson SOLO liquid handling robots."""

    def __init__(
        self,
        port: int = 11139,
        solo_soft_path: str = "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe",
    ) -> None:
        """Create Solo interface and connect to the SOLO device."""
        self.client = SoloClient()
        self.port = port
        self.client.Connect(self.port)
        self.solo_soft_path = solo_soft_path

    def __del__(self) -> None:
        """Disconnect from the SOLO device on deletion."""
        self.client.Disconnect()

    @property
    def connected(self) -> bool:
        """Checks whether the SOLO client is currently connected."""
        return self.client.IsConnected

    def open_solo_soft(self) -> None:
        """Attempts to open SOLOSoft application."""
        processes = Process.GetProcessesByName(
            Path.GetFileNameWithoutExtension(self.solo_soft_path)
        )
        if len(processes) > 0:
            self.close_solo_soft()
        if len(processes) == 0:
            Process.Start(self.solo_soft_path)
            time.sleep(10)
            SendKeys.SendWait("{ENTER}")
            time.sleep(2)
            SendKeys.SendWait("{ENTER}")
            self.client.RunCommand("CLOSEALLFILES")

    def close_solo_soft(self) -> None:
        """Close SOLOSoft application."""
        processes = Process.GetProcessesByName(
            Path.GetFileNameWithoutExtension(self.solo_soft_path)
        )
        for process in processes:
            process.Kill()
            time.sleep(3)
