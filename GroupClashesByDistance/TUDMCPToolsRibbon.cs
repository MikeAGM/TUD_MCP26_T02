using Autodesk.Navisworks.Api.Plugins;

namespace GroupClashesByDistance
{
    [Plugin("TUDMCPToolsRibbon", "TUD2", DisplayName = "TUD MCP-2026 Tools")]
    [RibbonLayout("TUDMCPTools.xaml")]
    [RibbonTab("ID_TUD_Tab", DisplayName = "TUD MCP-2026")]
    [Command("ID_GROUP_CLASHES",
        DisplayName = "Group Clashes by Distance",
        Icon = @"Resources\GroupClashesByDistancePlugin16x16.png",
        LargeIcon = @"Resources\GroupClashesByDistancePlugin32x32.png",
        ToolTip = "Group clash results by proximity (distance threshold).")]
    [Command("ID_ABOUT",
        DisplayName = "About",
        Icon = @"Resources\ReadmeIcon16x16.png",
        LargeIcon = @"Resources\ReadmeIcon32x32.png",
        ToolTip = "About this plugin.")]
    public class TUDMCPToolsRibbon : CommandHandlerPlugin
    {
        public override int ExecuteCommand(string commandId, params string[] parameters)
        {
            switch (commandId)
            {
                case "ID_GROUP_CLASHES":
                    new GroupClashesByDistancePlugin().Execute();
                    break;
                case "ID_ABOUT":
                    new AboutPlugin().Execute();
                    break;
            }
            return 0;
        }

        public override CommandState CanExecuteCommand(string commandId)
        {
            return new CommandState { IsEnabled = true, IsVisible = true };
        }
    }
}
