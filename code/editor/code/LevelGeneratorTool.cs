using System.Linq;
using Editor;
using Kira;

namespace Sandbox;

[Title("Level Generator")]
public class LevelGeneratorTool : EditorTool<LevelGenerator>
{
    public override void OnEnabled()
    {
        base.OnEnabled();

        var window = new WidgetWindow(SceneOverlay, "Level Generator");
        window.Icon = "grid_view";
        window.Layout = Layout.Row();
        window.Layout.Spacing = 10;
        window.Width = 300;
        window.Height = 300;
        window.MaximumWidth = 800;
        window.MaximumHeight = 800;
        window.Layout.Margin = 16;

        var createLevelBtn = new Button("Create Level");
        createLevelBtn.Pressed = OnCreateLevel;

        var deleteLevelBtn = new Button("Delete Level");
        deleteLevelBtn.Pressed = OnDeleteLevel;

        window.Layout.Add(createLevelBtn);
        window.Layout.Add(deleteLevelBtn);

        AddOverlay(window, TextFlag.LeftTop, 10);
    }

    private void OnCreateLevel()
    {
        Scene.Components.GetAll<LevelGenerator>().FirstOrDefault().GenerateLevel();
    }

    private void OnDeleteLevel()
    {
        Scene.Components.GetAll<LevelGenerator>().FirstOrDefault().ClearBlocks();
    }
}