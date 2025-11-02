namespace SFMLUI;

internal class Root : Node
{
	public Root()
	{
		Name = "_ROOT_";
	}

	public void CalculateLayout()
	{
		Yoga.CalculateLayout();
	}

	public override bool AcceptsMouse(float x, float y)
	{
		return false;
	}
}