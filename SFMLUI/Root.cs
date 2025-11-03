namespace SFMLUI;

internal class Root : Node
{
	private UI _ui;

	public bool EnableClipping => _ui.EnableClipping;
	public bool EnableVisualizer => _ui.EnableVisualizer;

	public Root(UI ui)
	{
		_ui = ui;
		Root = this;
		Name = "_ROOT_";
	}

	public void CalculateLayout()
	{
		OuterYoga.CalculateLayout();
	}

	public override bool AcceptsMouse(float x, float y)
	{
		return false;
	}
}