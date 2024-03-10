using System;

namespace Kira;

[Category("Kira")]
public sealed class LevelGenerator : Component
{
    private NoiseGenerator NoiseGen { get; set; }

    [Property, Range(0, 10)]
    private float Offset { get; set; } = 0.0f;

    [Property] private bool ClampValues { get; set; } = false;
    [Property] private int BlocksRes { get; set; } = 20;
    [Property] private float VoxelSize { get; set; } = 1;

    [Property, Group("Colors")] private Color waterColor { get; set; } = Color.Blue;
    [Property, Group("Colors")] private Color groundColor { get; set; } = Color.Orange;
    [Property, Group("Colors")] private Color grassColor { get; set; } = Color.Green;


    private List<GameObject> SpawnedBlocks { get; set; } = new List<GameObject>();

    protected override void OnStart()
    {
        base.OnStart();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearBlocks();

        NoiseGen = Components.Get<NoiseGenerator>();

        float[,] noiseMap = NoiseGen.CreateNoise(NoiseGen.Scale, BlocksRes, 1);

        for (int y = 0; y < noiseMap.GetLength(0); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(1); x++)
            {
                float val = noiseMap[x, y];

                if (ClampValues) val = MathF.Floor((val + 1) * 2);
                // Log.Info(val);

                Color color = grassColor;

                if (val <= 2)
                {
                    color = waterColor;
                }
                else if (val <= 3)
                {
                    color = groundColor;
                }

                var block = CreateBlock(color);

                float xpos = x * 50 + x * Offset;

                float ypos = y * 50 + y * Offset;
                float zpos = val * 50;

                block.Transform.Position = new Vector3(xpos, ypos, zpos);
            }
        }
    }

    public void ClearBlocks()
    {
        foreach (GameObject block in SpawnedBlocks)
        {
            block.Destroy();
        }

        if (GameObject.Children.Count > 0)
        {
            foreach (GameObject child in GameObject.Children)
            {
                child.Destroy();
            }
        }


        SpawnedBlocks.Clear();
    }

    public void DeleteChunkMesh()
    {
        foreach (GameObject child in GameObject.Children)
        {
            child.Destroy();
        }
    }

    public GameObject CreateChunkMesh()
    {
        Mesh mesh = new Mesh();
        mesh.Material = Material.Load("materials/architecture/tiles/brick_wall_large01a.vmat");
        mesh.CreateVertexBuffer(6, SimpleVertex.Layout, CreateFace());
        ModelBuilder model = new ModelBuilder();
        model.AddMesh(mesh);
        var obj = Scene.CreateObject();
        var modelRend = obj.Components.Create<ModelRenderer>();
        modelRend.Model = model.Create();
        obj.Transform.Scale = Vector3.One * 50;
        obj.SetParent(GameObject);
        return obj;
    }

    private List<SimpleVertex> CreateFace()
    {
        List<SimpleVertex> simpleVertices = new List<SimpleVertex>();
        var tangent = new Vector3(1, 0, 0);
        simpleVertices.Add(new SimpleVertex(Vector3.Zero, Vector3.Backward, tangent, Vector2.Zero));
        simpleVertices.Add(new SimpleVertex(Vector3.Right, Vector3.Backward, tangent, new Vector2(1f, 0f)));
        simpleVertices.Add(new SimpleVertex(Vector3.Up, Vector3.Backward, tangent, new Vector2(0f, 1f)));

        simpleVertices.Add(new SimpleVertex(new Vector3(0f, -1f, 1f), Vector3.Backward, tangent, new Vector2(1f, 1f)));
        simpleVertices.Add(new SimpleVertex(Vector3.Up, Vector3.Backward, tangent, new Vector2(0f, 1f)));
        simpleVertices.Add(new SimpleVertex(Vector3.Right, Vector3.Backward, tangent, new Vector2(1f, 0f)));


        return simpleVertices;
    }

    private GameObject CreateBlock(Color? color = null)
    {
        GameObject block = Scene.CreateObject();
        block.SetParent(GameObject);
        var renderer = block.Components.Create<ModelRenderer>();
        renderer.Model = Model.Cube;

        if (color is not null)
        {
            renderer.Tint = color.Value;
        }

        SpawnedBlocks.Add(block);
        return block;
    }
}