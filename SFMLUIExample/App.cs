using System.Reflection;
using Facebook.Yoga;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SFMLUI;

namespace SFML_UI;

public class App
{
	private RenderWindow? _window;
	private Font? _font;

	private UI? _ui;

	private Text? _debugText;

	struct DebugData
	{
		public int MouseX;
		public int MouseY;
	}

	private DebugData _debugData;

	public void Run()
	{
		UI.InitializeGL();

		VideoMode mode = new(800, 600);
		_window = new(mode, "SFMLUI");

		_window.Closed += (_, _) => { OnClose(); };
		_window.Resized += (_, e) => { OnResize(e); };
		_window.KeyPressed += (_, e) => { OnKeyPressed(e); };
		_window.MouseMoved += (_, e) => { OnMouseMoved(e); };
		_window.MouseButtonPressed += (_, e) => { OnMousePressed(e); };
		_window.MouseButtonReleased += (_, e) => { OnMouseReleased(e); };
		_window.MouseWheelScrolled += (_, e) => { OnMouseScrolled(e); };

		using var fontStream = Assembly.GetExecutingAssembly()
			.GetManifestResourceStream("SFML_UI.res.JetBrainsMono-Regular.ttf");

		_font = new Font(fontStream);
		_debugText = new Text("Cursor Pos: ", _font, 22);

		_ui = new UI((Vector2f)_window.Size);
		_ui.DrawEnd += () =>
		{
			UpdateDebugText();
			_window.Draw(_debugText);
		};

		var root = _ui.Root;

		var containerBig = new Widget
		{
			Wrap = YogaWrap.Wrap,
			Padding = 10,
			FlexGrow = 1.0f,
			Name = "containerBig",
			FillColor = new Color(50, 100, 50)
		};
		root.AddChild(containerBig);

		var container = new Widget
		{
			Wrap = YogaWrap.Wrap,
			Padding = 90,
			Margin = 50,
			FlexGrow = 1.0f,
			Name = "container",
			FillColor = new Color(100, 150, 150)
		};
		containerBig.AddChild(container);

		var scroll = new WidgetScrollArea
		{
			Margin = 5,
			FixedWidth = YogaValue.Percent(70),
			FixedHeight = YogaValue.Percent(80),
			Name = "red scroll area",
			FillColor = Color.Red
		};
		container.AddChild(scroll);

		var scroll2 = new WidgetScrollArea
		{
			Margin = 5,
			FixedWidth = YogaValue.Percent(40),
			FixedHeight = YogaValue.Percent(60),
			Name = "blue scroll area",
			FillColor = Color.Blue
		};
		container.AddChild(scroll2);

		var scroll2Content = new Widget
		{
			Padding = 14,
			FlexDirection = YogaFlexDirection.Row,
			PositionType = YogaPositionType.Absolute,
			Left = 0,
			Top = 0,
			Name = "blue scroll area content",
			FillColor = new Color(0, 0, 100)
		};
		scroll2.AddChild(scroll2Content);

		var spam = new Widget
		{
			FixedWidth = 100,
			FixedHeight = 100,
			Margin = 4,
			Name = "spam",
			FillColor = new Color(50, 100, 120)
		};
		scroll2Content.AddChild(spam);

		var spam2 = new Widget
		{
			FixedWidth = 100,
			FixedHeight = 100,
			Margin = 4,
			Name = "spam2",
			FillColor = new Color(50, 100, 120)
		};
		scroll2Content.AddChild(spam2);

		var button = new WidgetButton
		{
			MinWidth = 140,
			Margin = 10,
			Padding = 15,
			PaddingTop = 10,
			PaddingBottom = 10,
			AlignSelf = YogaAlign.Center,
			AlignContent = YogaAlign.Center,
			AlignItems = YogaAlign.Center,
			JustifyContent = YogaJustify.Center,
			FlexDirection = YogaFlexDirection.Column,
			FillColor = new Color(51, 51, 51),
			HoverColor = new Color(69, 69, 69),
			PressColor = new Color(102, 102, 102),
			Name = "big button",
		};
		scroll.AddChild(button);

		var buttonLabel = new WidgetLabel
		{
			MinWidth = 10,
			MinHeight = 10,
			FillColor = Color.Transparent,
			TextColor = Color.White,
			FontSize = 22,
			Font = _font,
			Text = "button",
			Name = "buttonLabel",
		};
		button.AddChild(buttonLabel);

		var innerScroll = new WidgetScrollArea
		{
			FixedWidth = YogaValue.Percent(80),
			MinWidth = 250,
			AspectRatio = 2f,
			Margin = 5,
			Name = "inner scroll area",
			FillColor = Color.Green
		};
		scroll.AddChild(innerScroll);

		for (int i = 0; i < 8; ++i)
		{
			var b = new WidgetButton
			{
				Margin = 10,
				Padding = 15,
				PaddingTop = 10,
				PaddingBottom = 10,
				AlignSelf = YogaAlign.Center,
				AlignContent = YogaAlign.Center,
				AlignItems = YogaAlign.Center,
				JustifyContent = YogaJustify.Center,
				FlexDirection = YogaFlexDirection.Column,
				FillColor = new Color(51, 51, 81),
				HoverColor = new Color(69, 69, 99),
				PressColor = new Color(102, 102, 132),
				Name = $"button_{i}",
			};
			innerScroll.AddChild(b);

			var bl = new WidgetLabel
			{
				MinWidth = 10,
				MinHeight = 10,
				FillColor = Color.Transparent,
				TextColor = Color.White,
				FontSize = 17,
				Font = _font,
				Text = $"butt {i}",
				Name = $"buttonLabel {i}",
			};
			b.AddChild(bl);
		}

		for (int i = 0; i < 10; ++i)
		{
			var b = new WidgetButton
			{
				Margin = 10,
				Padding = 15,
				PaddingTop = 10,
				PaddingBottom = 10,
				AlignSelf = YogaAlign.Center,
				AlignContent = YogaAlign.Center,
				AlignItems = YogaAlign.Center,
				JustifyContent = YogaJustify.Center,
				FlexDirection = YogaFlexDirection.Column,
				FillColor = new Color(51, 51, 81),
				HoverColor = new Color(69, 69, 99),
				PressColor = new Color(102, 102, 132),
				Name = $"button_{i}",
			};
			scroll.AddChild(b);

			var bl = new WidgetLabel
			{
				MinWidth = 10,
				MinHeight = 10,
				FillColor = Color.Transparent,
				TextColor = Color.White,
				FontSize = 17,
				Font = _font,
				Text = $"butt {i}",
				Name = $"buttonLabel {i}",
			};
			b.AddChild(bl);
		}

		var box3 = new Widget
		{
			FixedWidth = 70,
			FixedHeight = 70,
			Margin = 10,
			Name = "box3 blue",
			FillColor = Color.Blue
		};
		scroll.AddChild(box3);

		button.Clicked += () => { Console.WriteLine("Clicked!"); };

		// _ui.Root

		while (_window.IsOpen)
		{
			_window.DispatchEvents();
			_window.Clear();

			_ui.Draw(_window);
			_window.Display();
		}
	}

	private void OnClose()
	{
		_window?.Close();
	}

	private void OnResize(SizeEventArgs e)
	{
		if (_ui != null)
		{
			_ui.Size = new Vector2f(e.Width, e.Height);
		}
	}

	private void OnKeyPressed(KeyEventArgs e)
	{
		if (e.Code == Keyboard.Key.Escape)
		{
			_window?.Close();
		}

		if (e.Code == Keyboard.Key.F1)
		{
			Node.EnableClipping = !Node.EnableClipping;
		}

		_ui?.OnKeyPressed(e);
	}

	private void OnMouseMoved(MouseMoveEventArgs e)
	{
		_debugData.MouseX = e.X;
		_debugData.MouseY = e.Y;
		UpdateDebugText();

		_ui?.OnMouseMoved(e);
	}

	private void OnMousePressed(MouseButtonEventArgs e)
	{
		_ui?.OnMousePressed(e);
	}

	private void OnMouseReleased(MouseButtonEventArgs e)
	{
		_ui?.OnMouseReleased(e);
	}

	private void OnMouseScrolled(MouseWheelScrollEventArgs e)
	{
		_ui?.OnMouseScrolled(e);
	}

	private void UpdateDebugText()
	{
		if (_debugText != null)
		{
			Node? mouseCaptured = _ui?.MouseCapturedNode;
			string? mouseCapturedName = mouseCaptured?.Name;

			Node? hovered = _ui?.HoveredNode;
			string? hoveredName = hovered?.Name;

			_debugText.DisplayedString = $"Mouse X: {_debugData.MouseX}\n" +
			                             $"Mouse Y: {_debugData.MouseY}\n" +
			                             $"Hovered: {hoveredName}\n" +
			                             $"Captured: {mouseCapturedName}";
		}
	}
}