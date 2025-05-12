"""Interface for controlling the solo device/instrument/robot."""

# * Using .dlls and .NET assemblies
# * pip install pythonnet
# * See docs: https://pythonnet.github.io/pythonnet/python.html
import time

import clr

clr.AddReference(
    r"C:\Program Files (x86)\Hudson Robotics\SoloSoft\Hudson.SoloSoft.Communications.dll"
)
# clr.AddReference("System.IO.Path")
# clr.AddReference("System.Diagnostics")
clr.AddReference("System.Windows.Forms")

from Hudson.SoloSoft.Communications import SoloClient  # noqa
from System.Diagnostics import Process  # noqa
from System.IO import Path  # noqa
from System.Windows.Forms import SendKeys  # noqa


class Solo:
    """Class for controlling the Solo liquidhandler via SOLOSoft"""

    def __init__(
        self,
        port: int = 11139,
        solo_soft_path: str = "C:\\Program Files (x86)\\Hudson Robotics\\SoloSoft\\SOLOSoft.exe",
    ):
        """Create the Solo Interface and connect to the specified device"""
        self.client = SoloClient()
        self.port = port
        self.client.Connect(self.port)
        self.solo_soft_path = solo_soft_path

    def __del__(self):
        """Disconnect from the Solo device"""
        self.client.Disconnect()

    @property
    def connected(self) -> bool:
        """Check whether the solo client is currently connected"""
        return self.client.IsConnected

    def open_solo_soft(self):
        """Attempts to open solo soft"""
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

    def close_solo_soft(self):
        """Close solo soft"""
        processes = Process.GetProcessesByName(
            Path.GetFileNameWithoutExtension(self.solo_soft_path)
        )
        for process in processes:
            process.Kill()
            time.sleep(3)
