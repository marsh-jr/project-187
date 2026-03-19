using Godot;
using Godot.Collections;

namespace Project187
{
	/// Draggable card in the module palette. Provides drag data consumed by ChainSlotControl.
	public partial class PaletteCard : PanelContainer
	{
		private string _moduleId;
		private string _category;     // "Core" | "Weapon" | "Processor"
		private string _displayName;
		private string _description;
		private Color  _baseColor;

		public void Setup(string moduleId, string displayName, string category, string description, Color baseColor)
		{
			_moduleId    = moduleId;
			_displayName = displayName;
			_category    = category;
			_description = description;
			_baseColor   = baseColor;

			CustomMinimumSize = new Vector2(130f, 100f);
			MouseFilter = MouseFilterEnum.Stop;

			var style = new StyleBoxFlat();
			style.BgColor = baseColor;
			style.SetBorderWidthAll(2);
			style.BorderColor = new Color(baseColor.R + 0.2f, baseColor.G + 0.2f, baseColor.B + 0.2f, 0.8f);
			style.SetCornerRadiusAll(5);
			AddThemeStyleboxOverride("panel", style);

			var vbox = new VBoxContainer();
			vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			vbox.AddThemeConstantOverride("separation", 3);
			AddChild(vbox);

			// Category badge
			var catLabel = new Label();
			catLabel.Text = category.ToUpper();
			catLabel.AddThemeFontSizeOverride("font_size", 9);
			catLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.6f));
			catLabel.HorizontalAlignment = HorizontalAlignment.Center;
			vbox.AddChild(catLabel);

			// Name
			var nameLabel = new Label();
			nameLabel.Text = displayName;
			nameLabel.AddThemeFontSizeOverride("font_size", 13);
			nameLabel.AddThemeColorOverride("font_color", Colors.White);
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			nameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			nameLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
			vbox.AddChild(nameLabel);

			// Description
			var descLabel = new Label();
			descLabel.Text = description;
			descLabel.AddThemeFontSizeOverride("font_size", 10);
			descLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.75f));
			descLabel.HorizontalAlignment = HorizontalAlignment.Center;
			descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			vbox.AddChild(descLabel);
		}

		public override Variant _GetDragData(Vector2 atPosition)
		{
			// Build a small preview card
			var preview = new PanelContainer();
			preview.CustomMinimumSize = new Vector2(110f, 60f);
			var pStyle = new StyleBoxFlat();
			pStyle.BgColor = _baseColor;
			pStyle.SetCornerRadiusAll(4);
			preview.AddThemeStyleboxOverride("panel", pStyle);
			var lbl = new Label();
			lbl.Text = _displayName;
			lbl.AddThemeFontSizeOverride("font_size", 12);
			lbl.AddThemeColorOverride("font_color", Colors.White);
			lbl.HorizontalAlignment = HorizontalAlignment.Center;
			lbl.VerticalAlignment   = VerticalAlignment.Center;
			lbl.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			preview.AddChild(lbl);
			SetDragPreview(preview);

			return new Dictionary
			{
				["module_id"] = _moduleId,
				["category"]  = _category
			};
		}
	}
}
