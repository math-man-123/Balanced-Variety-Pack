using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Effects;


public static class TrippyVfx
{
    private const string TrippyShaderPath =
        "res://BalancedVarietyPack/shaders/trippy.gdshader";
    private static readonly Shader TrippyShader =
        GD.Load<Shader>(TrippyShaderPath);
    private static ColorRect? _shaderOverlay;
    
    private static void SetShaderParameters(
        ShaderMaterial shaderMaterial, int highPowerAmount)
    {
        float weight = Math.Clamp(highPowerAmount / 100f, 0f, 1f);

        // fast rise initially, slower rise near 1
        float easedWeight = 1f - Mathf.Pow(1f - weight, 2f);
        float size = Mathf.Lerp(0f, 20f, easedWeight);
        float speed = Mathf.Lerp(0f, 1.2f, easedWeight);
        
        shaderMaterial.SetShaderParameter("chroma_size", size);
        shaderMaterial.SetShaderParameter("chroma_speed",speed);
    }
    
    public static void Show(int highPowerAmount)
    {
        // check if shader overlay already exist -> if so update it
        if (GodotObject.IsInstanceValid(_shaderOverlay))
        {
            if (_shaderOverlay.Material is ShaderMaterial material)
                SetShaderParameters(material, highPowerAmount);
            return;
        }
        
        // make sure run instance exist then grab room and shader
        if (NRun.Instance is null) return;
        Control roomContainer = NRun.Instance.GetNode<Control>("%RoomContainer");
        ShaderMaterial trippyShader = new () { Shader = TrippyShader } ;
        
        // setup shader and create overlay for it
        SetShaderParameters(trippyShader, highPowerAmount);
        _shaderOverlay = new ColorRect
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = trippyShader
        };
        
        // make the overlay fullscreen and add it to room container
        _shaderOverlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        roomContainer.AddChild(_shaderOverlay);
    }

    public static void Hide()
    {
        if (GodotObject.IsInstanceValid(_shaderOverlay))
            _shaderOverlay!.QueueFree();

        _shaderOverlay = null;
    }
}
