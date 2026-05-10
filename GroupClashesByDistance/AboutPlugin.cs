using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;

namespace GroupClashesByDistance
{
    public class AboutPlugin : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            MessageBox.Show(
                Autodesk.Navisworks.Api.Application.Gui.MainWindow,
                "This plugin was developed by T02 for the MCP-2026 project for TUD MSc aBIMM.",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return 0;
        }
    }
}
