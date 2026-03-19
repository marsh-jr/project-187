using Godot;

namespace Project187
{
	/// Temporary visual flash that shows the melee hit zone, then fades away.
	/// Created entirely in code — no scene file needed.
	public partial class MeleeEffectNode : Node2D
	{
		private float _range;
		private float _angleDeg;
		private float _dirAngle;

		private const float FadeDuration = 0.18f;

		public void Setup(float range, float angleDeg, float directionAngle)
		{
			_range    = range;
			_angleDeg = angleDeg;
			_dirAngle = directionAngle;
		}

		public override void _Ready()
		{
			QueueRedraw();

			var tween = CreateTween();
			tween.TweenProperty(this, "modulate:a", 0f, FadeDuration)
			     .SetEase(Tween.EaseType.In)
			     .SetTrans(Tween.TransitionType.Linear);
			tween.TweenCallback(Callable.From(QueueFree)).SetDelay(FadeDuration);
		}

		public override void _Draw()
		{
			var color = new Color(1f, 0.55f, 0.1f, 0.55f); // orange tint

			if (_angleDeg >= 360f)
			{
				// Full circle
				DrawCircle(Vector2.Zero, _range, color);
			}
			else
			{
				// Arc sector as a polygon
				int   segments  = Mathf.Max(8, (int)(_angleDeg / 10f));
				float halfRad   = Mathf.DegToRad(_angleDeg * 0.5f);
				float step      = Mathf.DegToRad(_angleDeg) / segments;
				float startAngle = _dirAngle - halfRad;

				var points = new Vector2[segments + 2];
				points[0] = Vector2.Zero;
				for (int i = 0; i <= segments; i++)
					points[i + 1] = Vector2.Right.Rotated(startAngle + step * i) * _range;

				DrawPolygon(points, new Color[] { color });
			}
		}
	}
}
