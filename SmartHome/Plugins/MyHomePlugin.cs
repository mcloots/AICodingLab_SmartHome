using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SmartHome.Plugins
{
    public class MyHomePlugin
    {
        [KernelFunction("start_my_home")]
        [Description("Execute this function when the user asks to show the map of the house.")]
        public string StartMyHome()
        {
            var running = Process.GetProcessesByName("Lights");

            if (running.Length == 0)
            {
                // Het proces draait nog niet -> starten!
                Process.Start("..\\net9.0-windows\\Lights.exe");
                return "Map of the house started";
            }
            else
            {
                return "Map of the house was already started";
            }
        }
    }
}
