using Godot;
using WOLF3DSim;

public class Game : Spatial
{
    public static VSwap vswap = new VSwap();

    /// Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DownloadShareware.Main(new string[] { "" });

        vswap.SetPalette(@"Wolf3DSim\Palettes\Wolf3D.pal");

        Godot.Image image = new Image();
        image.CreateFromData(64, 64, false, Image.Format.Rgba8, vswap.PaletteTextureRepeated);
        ImageTexture it = new ImageTexture();
        it.CreateFromImage(image, 0);

        Sprite sprite = new Sprite
        {
            Name = "Sprite1",
            Texture = it,
            Position = new Vector2(200, 200),
            Scale = new Vector2(5, 5)
        };
        AddChild(sprite);

        vswap.Read(@"WOLF3D\VSWAP.WL1");

        Godot.Image imageWall = new Image();
        imageWall.CreateFromData(64, 64, false, Image.Format.Rgba8, vswap.Pages[0]);
        ImageTexture itWall = new ImageTexture();
        itWall.CreateFromImage(imageWall, 0);

        Sprite sprite2 = new Sprite
        {
            Name = "Sprite2",
            Texture = itWall,
            Position = new Vector2(600, 200),
            Scale = new Vector2(5, 5)
        };
        AddChild(sprite2);

        GameMaps maps = new GameMaps().Read(@"WOLF3D\MAPHEAD.WL1", @"WOLF3D\GAMEMAPS.WL1");

        AudioStreamSample audioStreamSample = new AudioStreamSample()
        {
            Data = VSwap.ConcatArrays(
                vswap.Pages[vswap.SoundPage],
                vswap.Pages[vswap.SoundPage + 1]
            ),
            Format = AudioStreamSample.FormatEnum.Format8Bits,
            MixRate = 7000,
            Stereo = false
        };

        AudioStreamPlayer audioStreamPlayer = new AudioStreamPlayer()
        {
            Stream = audioStreamSample,
            VolumeDb = 0.01f
        };

        AddChild(audioStreamPlayer);

        audioStreamPlayer.Play();

        Sprite3D sprite3D = new Sprite3D
        {
            Name = "Sprite3",
            Texture = it,
            Scale = new Vector3(5, 5, 5)
        };

        AddChild(sprite3D);

        //CubeMesh cubeMesh = new CubeMesh()
        //{
        //    Size = new Vector3(1, 1, 1),
        //    Material = new 
        //};

        //MeshInstance meshInsatnce = new MeshInstance()
        //{
        //    Mesh = cubeMesh
        //};

        //AddChild(meshInsatnce);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
