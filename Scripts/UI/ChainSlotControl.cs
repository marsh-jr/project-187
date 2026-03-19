using Godot;
using Godot.Collections;

namespace Project187
{
	/// Drop target for one module position in the chain row.
	/// Shows an empty placeholder or the filled module name.
	public partial class ChainSlotControl : PanelContainer
	{
		public enum SlotType { Core, Weapon, Processor }

		[Signal] public delegate void ModuleDroppedEventHandler(ChainSlotControl slot, string moduleId);
		[Signal] public delegate void ModuleClearedEventHandler(ChainSlotControl slot);

		public SlotType  ExpectedType  { get; private set; }
		public string    FilledModuleId { get; private set; } = null;
		public bool      IsOptional     { get; private set; }

		// Colors for each state
		private static readonly Color EmptyCoreColor   = new(0.08f, 0.15f, 0.35f);
		private static readonly Color FilledCoreColor  = new(0.20f, 0.40f, 0.80f);
		private static readonly Color EmptyWeapColor   = new(0.25f, 0.10f, 0.04f);
		private static readonly Color FilledWeapColor  = new(0.65f, 0.28f, 0.08f);
		private static readonly Color EmptyProcColor   = new(0.08f, 0.22f, 0.12f);
		private static readonly Color FilledProcColor  = new(0.15f, 0.52f, 0.25f);

		private Label  _nameLabel;
		private Label  _hintLabel;
		private Button _removeButton;

		public void Setup(SlotType type, bool optional = false)
		{
			ExpectedType = type;
			IsOptional   = optional;
			CustomMinimumSize = new Vector2(145f, 130f);
			MouseFilter = MouseFilterEnum.Stop;

			// Outer style
			var style = new StyleBoxFlat();
			style.BgColor = EmptyColor(type);
			style.SetBorderWidthAll(2);
			style.BorderColor = BorderColor(type);
			style.SetCornerRadiusAll(6);
			AddThemeStyleboxOverride("panel", style);

			// Inner VBox
			var vbox = new VBoxContainer();
			vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			vbox.AddThemeConstantOverride("separation", 4);
			AddChild(vbox);

			// Type label (top)
			var typeLabel = new Label();
			typeLabel.Text = optional ? $"[{type}?]" : $"[{type}]";
			typeLabel.AddThemeFontSizeOverride("font_size", 11);
			typeLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
			typeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			vbox.AddChild(typeLabel);

			// Name label (center)
			_nameLabel = new Label();
			_nameLabel.Text = optional ? "Drop here\n(optional)" : "Drop here";
			_nameLabel.AddThemeFontSizeOverride("font_size", 13);
			_nameLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			_nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_nameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			_nameLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
			vbox.AddChild(_nameLabel);

			// Hint label (bottom)
			_hintLabel = new Label();
			_hintLabel.Text = "";
			_hintLabel.AddThemeFontSizeOverride("font_size", 10);
			_hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
			_hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_hintLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			vbox.AddChild(_hintLabel);

			// Remove button (hidden when empty)
			_removeButton = new Button();
			_removeButton.Text = "X Remove";
			_removeButton.Visible = false;
			_removeButton.AddThemeFontSizeOverride("font_size", 11);
			_removeButton.Pressed += OnRemovePressed;
			vbox.AddChild(_removeButton);
		}

		public void Fill(string moduleId, string displayName, string description)
		{
			FilledModuleId = moduleId;

			var style = new StyleBoxFlat();
			style.BgColor = FilledColor(ExpectedType);
			style.SetBorderWidthAll(2);
			style.BorderColor = new Color(1f, 1f, 1f, 0.3f);
			style.SetCornerRadiusAll(6);
			AddThemeStyleboxOverride("panel", style);

			_nameLabel.Text = displayName;
			_nameLabel.AddThemeColorOverride("font_color", Colors.White);
			_nameLabel.AddThemeFontSizeOverride("font_size", 14);
			_hintLabel.Text  = description;
			_removeButton.Visible = true;
		}

		public void Clear()
		{
			FilledModuleId = null;

			var style = new StyleBoxFlat();
			style.BgColor = EmptyColor(ExpectedType);
			style.SetBorderWidthAll(2);
			style.BorderColor = BorderColor(ExpectedType);
			style.SetCornerRadiusAll(6);
			AddThemeStyleboxOverride("panel", style);

			_nameLabel.Text = IsOptional ? "Drop here\n(optional)" : "Drop here";
			_nameLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			_nameLabel.AddThemeFontSizeOverride("font_size", 13);
			_hintLabel.Text = "";
			_removeButton.Visible = false;
		}

		private void OnRemovePressed() => EmitSignal(SignalName.ModuleCleared, this);

		// ── Drag-and-drop ──────────────────────────────────────────────────────

		public override bool _CanDropData(Vector2 atPosition, Variant data)
		{
			if (FilledModuleId != null) return false;
			if (data.AsGodotDictionary() is not Dictionary dict) return false;
			return dict["category"].AsString() == ExpectedType.ToString();
		}

		public override void _DropData(Vector2 atPosition, Variant data)
		{
			var dict = data.AsGodotDictionary();
			EmitSignal(SignalName.ModuleDropped, this, dict["module_id"].AsString());
		}

		// ── Helpers ────────────────────────────────────────────────────────────

		private static Color EmptyColor(SlotType t) => t switch
		{
			SlotType.Core      => EmptyCoreColor,
			SlotType.Weapon    => EmptyWeapColor,
			SlotType.Processor => EmptyProcColor,
			_                  => Colors.DarkGray
		};

		private static Color FilledColor(SlotType t) => t switch
		{
			SlotType.Core      => FilledCoreColor,
			SlotType.Weapon    => FilledWeapColor,
			SlotType.Processor => FilledProcColor,
			_                  => Colors.Gray
		};

		private static Color BorderColor(SlotType t) => t switch
		{
			SlotType.Core      => new Color(0.3f, 0.5f, 0.9f, 0.6f),
			SlotType.Weapon    => new Color(0.8f, 0.4f, 0.15f, 0.6f),
			SlotType.Processor => new Color(0.25f, 0.7f, 0.35f, 0.6f),
			_                  => new Color(0.5f, 0.5f, 0.5f, 0.6f)
		};
	}
}
