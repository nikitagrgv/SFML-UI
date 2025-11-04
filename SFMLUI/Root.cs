namespace SFMLUI;

internal class Root : Node
{
	public Root(Style style)
	{
		Style = style;
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