using Editor;

namespace Sandbox;

[EditorApp("Plague", "pregnant_woman", "meow")]
public sealed class PlagueApp : Window
{
    public PlagueApp()
    {
        WindowTitle = "Plague";
        MinimumSize = new Vector2(300, 500);
    }
}