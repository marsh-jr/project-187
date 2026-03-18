using Godot;

namespace Project187
{
    /// Display metadata for an adaptation. The runtime behaviour lives in an IAdaptation implementor.
    /// ImplementingClass must be the fully-qualified C# class name (e.g. "Project187.PiercingAdaptation").
    [GlobalClass]
    public partial class AdaptationData : Resource
    {
        [Export] public string AdaptationName      { get; set; } = "";
        [Export] public AdaptationCategory Category { get; set; } = AdaptationCategory.Universal;
        [Export] public Texture2D Icon              { get; set; }
        [Export] public string Description          { get; set; } = "";
        [Export] public string ImplementingClass    { get; set; } = "";

        /// Creates the runtime IAdaptation instance via reflection.
        public IAdaptation CreateInstance()
        {
            if (string.IsNullOrEmpty(ImplementingClass)) return null;
            var type = System.Type.GetType(ImplementingClass);
            if (type == null)
            {
                GD.PushWarning($"AdaptationData: could not find type '{ImplementingClass}'");
                return null;
            }
            return System.Activator.CreateInstance(type) as IAdaptation;
        }
    }
}
