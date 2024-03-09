using System.Globalization;

namespace Kira;

[Category("Kira")]
public sealed class NoiseRenderer : Component, Component.ExecuteInEditor
{
    public bool IsDirty { get; set; } = true;
    private bool hasTexture { get; set; }
    private Texture tx { get; set; }

    [Property, Range(1, 10)]
    public int Resolution { get; set; } = 1;
    [Property, Range(1, 1024)]
    public int SpriteSize { get; set; } = 512;

    public void CreateTexture(float[,] noiseValues)
    {
        if (SpriteSize > 2048) SpriteSize = 2048;
        if (Resolution > 10) Resolution = 10;

        IsDirty = true;
        hasTexture = false;

        // Create a compute shader from a .shader file
        var computeShader = new ComputeShader("code/shaders/map_shader");
        var sp = GameObject.Components.Get<SpriteRenderer>();

        sp.Size = SpriteSize;
        // int noiseScale = SpriteSize * Resolution;

        List<byte> data = new List<byte>();

        for (int x = 0; x < SpriteSize; x++)
        {
            for (int y = 0; y < SpriteSize; y++)
            {
                float lum = noiseValues[x, y] * 255f;
                lum = lum.CeilToInt().Clamp(0, 255);

                byte val = byte.Parse(lum.ToString(CultureInfo.InvariantCulture));

                data.Add(val);
                data.Add(val);
                data.Add(val);
                data.Add(255);
            }
        }


        // Create a texture for the compute shader to use
        var texture = Texture.Create(SpriteSize, SpriteSize)
            .WithUAVBinding() // Needs to have this if we're using it in a compute shader
            .WithFormat(ImageFormat.RGBA8888) // Use whatever you need
            .WithData(data.ToArray())
            .Finish();


        // Attach texture to OutputTexture attribute in shader
        computeShader.Attributes.Set("OutputTexture", texture);

        // Dispatch 
        computeShader.Dispatch(texture.Width, texture.Height, 1);

        tx = texture;
        IsDirty = false;
        hasTexture = true;


        if (!sp.IsValid())
        {
            Log.Info("Could not find sprite renderer");
            return;
        }

        sp.Texture = tx;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        Components.Get<NoiseGenerator>().UpdateNoise();
    }
}