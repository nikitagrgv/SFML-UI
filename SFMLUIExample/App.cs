using System.Diagnostics;
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
	private bool _vsync = false;

	private Font? _font;

	private UI? _ui;

	private Text? _debugText;
	private TimeSpan _lastFrameTime;

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
		ContextSettings contextSettings = new(
			depthBits: 24,
			stencilBits: 8,
			antialiasingLevel: 0,
			majorVersion: 4,
			minorVersion: 6,
			attributes: ContextSettings.Attribute.Default | ContextSettings.Attribute.Debug,
			sRgbCapable: false);
		_window = new(mode, "SFMLUI", Styles.Default, contextSettings);
		_window.SetVerticalSyncEnabled(_vsync);

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
			BorderRadius = 10,
			FillColor = new Color(100, 150, 150)
		};
		containerBig.AddChild(container);

		var scroll = new WidgetScrollArea
		{
			Margin = 5,
			FixedWidth = YogaValue.Percent(70),
			FixedHeight = YogaValue.Percent(80),
			Name = "red scroll area",
			BorderRadius = 15,
			FillColor = Color.Red
		};
		container.AddChild(scroll);

		{
			var scroll2 = new WidgetScrollArea
			{
				Margin = 5,
				Padding = 14,
				FlexDirection = YogaFlexDirection.Row,
				FixedWidth = YogaValue.Percent(40),
				FixedHeight = YogaValue.Percent(60),
				Name = "blue scroll area",
				FillColor = Color.Blue
			};
			container.AddChild(scroll2);

			var spam = new Widget
			{
				FixedWidth = 100,
				FixedHeight = 100,
				Margin = 4,
				Name = "spam",
				BorderRadius = 10,
				FillColor = new Color(50, 100, 120)
			};
			scroll2.AddChild(spam);

			var spam2 = new Widget
			{
				Left = -40,
				Top = 30,
				FixedWidth = 50,
				FixedHeight = 50,
				Margin = 4,
				Name = "spam2",
				BorderRadius = 15,
				FillColor = new Color(120, 150, 10)
			};
			scroll2.AddChild(spam2);

			var spam3 = new Widget
			{
				Left = -130,
				Top = 40,
				FixedWidth = 50,
				FixedHeight = 50,
				Margin = 4,
				Name = "spam3",
				BorderRadius = 25,
				FillColor = new Color(10, 200, 100)
			};
			scroll2.AddChild(spam3);
		}

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
			BorderRadius = 10,
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

		var longButton = new WidgetButton
		{
			FixedWidth = YogaValue.Percent(90),
			MinWidth = 140,
			Margin = 10,
			Padding = 40,
			AlignSelf = YogaAlign.Center,
			AlignContent = YogaAlign.Center,
			AlignItems = YogaAlign.Center,
			JustifyContent = YogaJustify.Center,
			FlexDirection = YogaFlexDirection.Column,
			FillColor = new Color(51, 51, 51),
			HoverColor = new Color(69, 69, 69),
			PressColor = new Color(102, 102, 102),
			BorderRadius = 20,
			BorderRadiusBottomRight = 60,
			Name = "long button",
		};
		scroll.AddChild(longButton);

		var longButtonLabel = new WidgetLabel
		{
			MinWidth = 10,
			MinHeight = 10,
			FillColor = Color.Transparent,
			TextColor = Color.White,
			FontSize = 22,
			Font = _font,
			Text = "long button",
			Name = "longButtonLabel",
		};
		longButton.AddChild(longButtonLabel);

		var innerScroll = new WidgetScrollArea
		{
			FixedWidth = YogaValue.Percent(80),
			MinWidth = 250,
			AspectRatio = 2f,
			Margin = 5,
			Name = "inner scroll area",
			BorderRadius = 16,
			FillColor = Color.Green
		};
		scroll.AddChild(innerScroll);

		for (int i = 0; i < 8; ++i)
		{
			var b = new WidgetButton
			{
				Margin = 10,
				Padding = 20,
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
				BorderRadius = 14,
				BorderRadiusBottomRight = 5,
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

		Stopwatch stopwatch = new();
		while (_window.IsOpen)
		{
			stopwatch.Restart();

			_window.DispatchEvents();
			_window.Clear();

			_ui.Update();
			_ui.Draw(_window);
			_window.Display();

			stopwatch.Stop();
			_lastFrameTime = stopwatch.Elapsed;
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

		if (_ui == null)
		{
			return;
		}

		if (e.Code == Keyboard.Key.F1)
		{
			_ui.Style.EnableClipping = !_ui.Style.EnableClipping;
		}

		if (e.Code == Keyboard.Key.F2)
		{
			_ui.Style.EnableVisualizer = !_ui.Style.EnableVisualizer;
		}

		if (e.Code == Keyboard.Key.F3 && _window != null)
		{
			_vsync = !_vsync;
			_window.SetVerticalSyncEnabled(_vsync);
		}

		_ui.OnKeyPressed(e);
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

			double elapsedSec = _lastFrameTime.TotalSeconds;
			double fps = elapsedSec == 0 ? 0 : 1.0 / elapsedSec;
			_debugText.DisplayedString = $"FPS: {fps:F1}\n" +
			                             $"Mouse X: {_debugData.MouseX}\n" +
			                             $"Mouse Y: {_debugData.MouseY}\n" +
			                             $"Hovered: {hoveredName}\n" +
			                             $"Captured: {mouseCapturedName}";
		}
	}
}