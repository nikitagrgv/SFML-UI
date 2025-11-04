namespace SFMLUI;

internal class Root : Node
{
	private Style _style;

	public Root(Style style)
	{
		_style = style;
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