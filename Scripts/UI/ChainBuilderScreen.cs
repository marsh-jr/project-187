using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Project187
{
	/// Full-screen module chain builder.
	/// Shows before each round. Player drags modules from the palette into the chain row,
	/// then clicks "START ROUND" to emit ChainConfirmed with the assembled generator data.
	public partial class ChainBuilderScreen : CanvasLayer
	{
		[Signal] public delegate void ChainConfirmedEventHandler(Array<EnergyGeneratorData> generators);

		// ── Module Registry ────────────────────────────────────────────────────

		public enum ModuleCategory { Core, Weapon, Processor }

		public record ModuleDef(
			string          Id,
			string          DisplayName,
			ModuleCategory  Category,
			string          Description,
			Color           CardColor,
			string          ImplementingClass);

		private static readonly ModuleDef[] AllModules =
		{
			// Cores
			new("TurboCore",    "Turbo Core",    ModuleCategory.Core,      "0.7-0.9s / 70-85%",   new Color(0.20f,0.40f,0.80f), "Project187.TurboCore"),
			new("MilitaryCore", "Military Core", ModuleCategory.Core,      "1.1-1.3s / 105-120%", new Color(0.15f,0.30f,0.65f), "Project187.MilitaryCore"),
			new("HeavyCore",    "Heavy Core",    ModuleCategory.Core,      "1.8-2.1s / 210-230%", new Color(0.10f,0.20f,0.50f), "Project187.HeavyCore"),
			// Processors
			new("AggressiveCP", "Aggressive Proc.", ModuleCategory.Processor, "On-Hit / 30-40%",    new Color(0.15f,0.52f,0.25f), "Project187.AggressiveCombatProcessor"),
			new("PreciseCP",    "Precise Proc.",    ModuleCategory.Processor, "On-Crit / 100-115%", new Color(0.10f,0.40f,0.20f), "Project187.PreciseCombatProcessor"),
			// Projectile weapons
			new("MachineGun", "Machine Gun", ModuleCategory.Weapon, "P 12dmg 5shots p1", new Color(0.65f,0.28f,0.08f), "Project187.MachineGun"),
			new("Shotgun",    "Shotgun",     ModuleCategory.Weapon, "P 16dmg 6shots 45deg", new Color(0.60f,0.22f,0.06f), "Project187.Shotgun"),
			new("Flak",       "Flak",        ModuleCategory.Weapon, "P 13dmg 8ring p100", new Color(0.55f,0.18f,0.05f), "Project187.Flak"),
			new("Revolver",   "Revolver",    ModuleCategory.Weapon, "P 25dmg 40%crit p3", new Color(0.70f,0.32f,0.10f), "Project187.Revolver"),
			// Melee weapons
			new("Slam",   "Slam",   ModuleCategory.Weapon, "M 25dmg 360deg",    new Color(0.55f,0.15f,0.45f), "Project187.Slam"),
			new("Sword",  "Sword",  ModuleCategory.Weapon, "M 20dmg 75deg",     new Color(0.50f,0.12f,0.40f), "Project187.Sword"),
			new("Skewer", "Skewer", ModuleCategory.Weapon, "M 20dmg 20deg long", new Color(0.45f,0.10f,0.35f), "Project187.Skewer"),
		};

		private static ModuleDef Lookup(string id)
		{
			foreach (var m in AllModules)
				if (m.Id == id) return m;
			return null;
		}

		// ── Persistent chain (survives scene reloads) ──────────────────────────

		private static readonly List<string> _persistedChain = new();

		// ── UI state ──────────────────────────────────────────────────────────

		private readonly List<string> _chain = new();   // module IDs in order
		private HBoxContainer _chainRow;
		private Button        _startButton;
		private readonly List<ChainSlotControl> _slots = new();

		// ── Build UI in _Ready ─────────────────────────────────────────────────

		public override void _Ready()
		{
			Layer = 5;
			Hide();

			// Root panel
			var root = new Panel();
			root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			var rootStyle = new StyleBoxFlat();
			rootStyle.BgColor = new Color(0.08f, 0.08f, 0.11f);
			root.AddThemeStyleboxOverride("panel", rootStyle);
			AddChild(root);

			// Title
			var title = new Label();
			title.Text = "MODULE CONFIGURATION";
			title.AddThemeFontSizeOverride("font_size", 28);
			title.AddThemeColorOverride("font_color", Colors.White);
			title.HorizontalAlignment = HorizontalAlignment.Center;
			title.AnchorLeft = 0; title.AnchorRight = 1;
			title.AnchorTop = 0;  title.AnchorBottom = 0;
			title.OffsetTop = 20; title.OffsetBottom = 60;
			root.AddChild(title);

			// Chain scroll area (center, leaves room for palette at bottom)
			var chainScroll = new ScrollContainer();
			chainScroll.AnchorLeft = 0;  chainScroll.AnchorRight = 1;
			chainScroll.AnchorTop = 0;   chainScroll.AnchorBottom = 0;
			chainScroll.OffsetTop = 75;  chainScroll.OffsetBottom = -220;
			chainScroll.OffsetLeft = 20; chainScroll.OffsetRight = -20;
			chainScroll.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
			root.AddChild(chainScroll);

			_chainRow = new HBoxContainer();
			_chainRow.AddThemeConstantOverride("separation", 6);
			_chainRow.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			chainScroll.AddChild(_chainRow);

			// Start button
			_startButton = new Button();
			_startButton.Text = "START ROUND";
			_startButton.AddThemeFontSizeOverride("font_size", 20);
			_startButton.AnchorLeft = 0.5f; _startButton.AnchorRight = 0.5f;
			_startButton.AnchorTop  = 1f;   _startButton.AnchorBottom = 1f;
			_startButton.OffsetLeft = -120; _startButton.OffsetRight  = 120;
			_startButton.OffsetTop  = -205; _startButton.OffsetBottom = -165;
			_startButton.Disabled = true;
			_startButton.Pressed += OnStartPressed;
			root.AddChild(_startButton);

			// Palette panel (bottom)
			var palette = new Panel();
			palette.AnchorLeft = 0;  palette.AnchorRight  = 1;
			palette.AnchorTop  = 1;  palette.AnchorBottom = 1;
			palette.OffsetTop  = -155; palette.OffsetBottom = 0;
			var palStyle = new StyleBoxFlat();
			palStyle.BgColor = new Color(0.05f, 0.05f, 0.07f);
			palette.AddThemeStyleboxOverride("panel", palStyle);
			root.AddChild(palette);

			var palLabel = new Label();
			palLabel.Text = "AVAILABLE MODULES";
			palLabel.AddThemeFontSizeOverride("font_size", 11);
			palLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
			palLabel.AnchorLeft = 0; palLabel.AnchorRight = 1;
			palLabel.AnchorTop = 0;  palLabel.AnchorBottom = 0;
			palLabel.OffsetTop = 6;  palLabel.OffsetBottom = 24;
			palLabel.OffsetLeft = 10;
			palette.AddChild(palLabel);

			var palScroll = new ScrollContainer();
			palScroll.AnchorLeft = 0;  palScroll.AnchorRight  = 1;
			palScroll.AnchorTop  = 0;  palScroll.AnchorBottom = 1;
			palScroll.OffsetTop  = 26; palScroll.OffsetBottom = -6;
			palScroll.OffsetLeft = 6;  palScroll.OffsetRight  = -6;
			palScroll.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
			palette.AddChild(palScroll);

			var palRow = new HBoxContainer();
			palRow.AddThemeConstantOverride("separation", 8);
			palRow.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			palScroll.AddChild(palRow);

			foreach (var def in AllModules)
			{
				var card = new PaletteCard();
				card.Setup(def.Id, def.DisplayName, def.Category.ToString(), def.Description, def.CardColor);
				palRow.AddChild(card);
			}

			// Build initial slot row (just the core slot)
			AppendSlot(ChainSlotControl.SlotType.Core, optional: false);
		}

		// ── Show / hide ────────────────────────────────────────────────────────

		public void ShowForRound()
		{
			_chain.Clear();
			foreach (var id in _persistedChain)
				_chain.Add(id);
			RebuildSlotRow();
			Show();
		}

		// ── Slot management ────────────────────────────────────────────────────

		private void RebuildSlotRow()
		{
			// Clear existing slots
			foreach (var child in _chainRow.GetChildren())
				child.QueueFree();
			_slots.Clear();

			// Always start with the core slot
			AppendSlot(ChainSlotControl.SlotType.Core, optional: false);

			// Replay the persisted chain
			for (int i = 0; i < _chain.Count; i++)
			{
				if (i >= _slots.Count) break;
				var def = Lookup(_chain[i]);
				if (def == null) continue;
				_slots[i].Fill(_chain[i], def.DisplayName, def.Description);
				AppendNextSlot(def.Category);
			}

			UpdateStartButton();
		}

		private void AppendNextSlot(ModuleCategory filledCategory)
		{
			switch (filledCategory)
			{
				case ModuleCategory.Core:
					AppendSlot(ChainSlotControl.SlotType.Weapon, optional: false);
					break;
				case ModuleCategory.Weapon:
					AppendSlot(ChainSlotControl.SlotType.Processor, optional: true);
					break;
				case ModuleCategory.Processor:
					AppendSlot(ChainSlotControl.SlotType.Weapon, optional: false);
					break;
			}
		}

		private void AppendSlot(ChainSlotControl.SlotType type, bool optional)
		{
			// Arrow separator (not before the very first slot)
			if (_slots.Count > 0)
			{
				var arrow = new Label();
				arrow.Text = "->";
				arrow.AddThemeFontSizeOverride("font_size", 24);
				arrow.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
				arrow.VerticalAlignment = VerticalAlignment.Center;
				_chainRow.AddChild(arrow);
			}

			var slot = new ChainSlotControl();
			slot.Setup(type, optional);
			slot.ModuleDropped += OnModuleDropped;
			slot.ModuleCleared += OnModuleCleared;
			_chainRow.AddChild(slot);
			_slots.Add(slot);
		}

		private void OnModuleDropped(ChainSlotControl slot, string moduleId)
		{
			int slotIndex = _slots.IndexOf(slot);
			if (slotIndex < 0) return;

			var def = Lookup(moduleId);
			if (def == null) return;

			// Update chain list
			while (_chain.Count <= slotIndex)
				_chain.Add(null);
			_chain[slotIndex] = moduleId;

			// Fill the slot visually
			slot.Fill(moduleId, def.DisplayName, def.Description);

			// Append the next slot (dynamic growth)
			AppendNextSlot(def.Category);

			UpdateStartButton();
		}

		private void OnModuleCleared(ChainSlotControl slot)
		{
			int slotIndex = _slots.IndexOf(slot);
			if (slotIndex < 0) return;

			// Truncate chain
			if (slotIndex < _chain.Count)
				_chain.RemoveRange(slotIndex, _chain.Count - slotIndex);

			// Remove all slots and arrows after this one
			int totalChildren = _chainRow.GetChildCount();
			// Each slot after index slotIndex has an arrow + slot pair: 2 children each
			// Slot[0] is child 0, Arrow+Slot[1] are children 1+2, Arrow+Slot[2] are 3+4, etc.
			int firstChildToRemove = slotIndex == 0 ? 1 : slotIndex * 2 - 1;
			// Actually: child indices: slot0=0, arrow1=1, slot1=2, arrow2=3, slot2=4, ...
			// So slot[i] is at child index i*2, arrow before slot[i] (i>0) is at i*2-1
			// Children after slot[slotIndex]: starting at slotIndex*2 + 1
			int removeFromChildIdx = slotIndex * 2 + 1;
			for (int c = totalChildren - 1; c >= removeFromChildIdx; c--)
			{
				var child = _chainRow.GetChild(c);
				child.QueueFree();
			}
			// Remove from _slots list
			if (slotIndex + 1 < _slots.Count)
				_slots.RemoveRange(slotIndex + 1, _slots.Count - slotIndex - 1);

			// Clear this slot
			slot.Clear();

			UpdateStartButton();
		}

		private void UpdateStartButton()
		{
			// Need at least a Core (index 0) and a Weapon (index 1)
			bool valid = _chain.Count >= 2
				&& _chain[0] != null
				&& _chain[1] != null;
			_startButton.Disabled = !valid;
		}

		// ── Start Round ────────────────────────────────────────────────────────

		private void OnStartPressed()
		{
			// Save chain for next time
			_persistedChain.Clear();
			foreach (var id in _chain)
				if (id != null) _persistedChain.Add(id);

			var generators = BuildGenerators();
			Hide();
			EmitSignal(SignalName.ChainConfirmed, generators);
		}

		// ── Chain -> Generator Data ────────────────────────────────────────────

		private Array<EnergyGeneratorData> BuildGenerators()
		{
			// Filter out any null entries (optional processor slots that were left empty)
			var chain = new List<string>();
			foreach (var id in _chain)
				if (!string.IsNullOrEmpty(id)) chain.Add(id);

			if (chain.Count == 0) return new Array<EnergyGeneratorData>();

			var projScene = GD.Load<PackedScene>("res://Scenes/Attacks/Projectile.tscn");

			// Walk right-to-left, building nested structure
			// chain = [Core, Weapon, Proc?, Weapon, Proc?, Weapon...]
			// index 0 = Core, rest alternate weapon/processor

			// Start from the rightmost weapon
			AttackData prevAttack = CreateWeaponData(chain[chain.Count - 1], projScene);

			// Process pairs from right to left (skip index 0 = core)
			for (int i = chain.Count - 2; i >= 1; i--)
			{
				var def = Lookup(chain[i]);
				if (def == null) continue;

				if (def.Category == ModuleCategory.Processor)
				{
					// proc -> prevAttack; weapon[i-1] gets proc as chain slot
					var proc = CreateProcessorData(chain[i]);
					proc.Attack = prevAttack;

					i--; // consume weapon at i-1
					if (i < 1) break;
					var weap = CreateWeaponData(chain[i], projScene);
					weap.ChainSlots = new Array<EnergyGeneratorData> { proc };
					prevAttack = weap;
				}
				else // Weapon without a processor (shouldn't normally happen, but handle it)
				{
					var weap = CreateWeaponData(chain[i], projScene);
					weap.ChainSlots = new Array<EnergyGeneratorData>();
					prevAttack = weap;
				}
			}

			var core = CreateCoreData(chain[0]);
			core.Attack = prevAttack;
			return new Array<EnergyGeneratorData> { core };
		}

		// ── Factory Methods ────────────────────────────────────────────────────

		private static EnergyGeneratorData CreateCoreData(string id)
		{
			EnergyGeneratorData data = id switch
			{
				"TurboCore"    => new TurboCoreData(),
				"MilitaryCore" => new MilitaryCoreData(),
				"HeavyCore"    => new HeavyCoreData(),
				_              => new TurboCoreData()
			};
			data.ImplementingClass = Lookup(id)?.ImplementingClass ?? "";
			return data;
		}

		private static EnergyGeneratorData CreateProcessorData(string id)
		{
			EnergyGeneratorData data = id switch
			{
				"AggressiveCP" => new AggressiveCombatProcessorData(),
				"PreciseCP"    => new PreciseCombatProcessorData(),
				_              => new AggressiveCombatProcessorData()
			};
			data.ImplementingClass = Lookup(id)?.ImplementingClass ?? "";
			return data;
		}

		private static AttackData CreateWeaponData(string id, PackedScene projScene)
		{
			var data = new AttackData { CritMultiplier = 2.0f, MaxAdaptationSlots = 2 };
			data.ImplementingClass = Lookup(id)?.ImplementingClass ?? "";
			data.AttackId   = id;
			data.AttackName = Lookup(id)?.DisplayName ?? id;

			switch (id)
			{
				case "MachineGun":
					data.Type = AttackType.Projectile; data.BaseDamage = 12f; data.CritChance = 0.06f;
					data.ProjectileCount = 5; data.PierceCount = 1; data.SpreadAngleDegrees = 15f;
					data.ProjectileSpeed = 500f; data.ProjectileDuration = 1.0f;
					data.SpreadMode = ProjectileSpreadMode.FanAim;
					data.ProjectileScene = projScene;
					break;
				case "Shotgun":
					data.Type = AttackType.Projectile; data.BaseDamage = 16f; data.CritChance = 0.06f;
					data.ProjectileCount = 6; data.PierceCount = 0; data.SpreadAngleDegrees = 45f;
					data.ProjectileSpeed = 300f; data.ProjectileDuration = 1.0f;
					data.SpreadMode = ProjectileSpreadMode.FanAim;
					data.ProjectileScene = projScene;
					break;
				case "Flak":
					data.Type = AttackType.Projectile; data.BaseDamage = 13f; data.CritChance = 0.06f;
					data.ProjectileCount = 8; data.PierceCount = 100; data.SpreadAngleDegrees = 0f;
					data.ProjectileSpeed = 150f; data.ProjectileDuration = 0.6f;
					data.SpreadMode = ProjectileSpreadMode.Radial;
					data.ProjectileScene = projScene;
					break;
				case "Revolver":
					data.Type = AttackType.Projectile; data.BaseDamage = 25f; data.CritChance = 0.40f;
					data.ProjectileCount = 1; data.PierceCount = 3; data.SpreadAngleDegrees = 0f;
					data.ProjectileSpeed = 700f; data.ProjectileDuration = 1.0f;
					data.SpreadMode = ProjectileSpreadMode.FanAim;
					data.ProjectileScene = projScene;
					break;
				case "Slam":
					data.Type = AttackType.Melee; data.BaseDamage = 25f; data.CritChance = 0.06f;
					data.MeleeRange = 100f; data.AreaAngle = 360f;
					break;
				case "Sword":
					data.Type = AttackType.Melee; data.BaseDamage = 20f; data.CritChance = 0.06f;
					data.MeleeRange = 150f; data.AreaAngle = 75f;
					break;
				case "Skewer":
					data.Type = AttackType.Melee; data.BaseDamage = 20f; data.CritChance = 0.06f;
					data.MeleeRange = 250f; data.AreaAngle = 20f;
					break;
			}

			data.ChainSlots = new Array<EnergyGeneratorData>();
			return data;
		}
	}
}
