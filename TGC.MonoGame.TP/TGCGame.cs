using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives;
using TGC.MonoGame.TP.SourceCode.Helpers;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";

    private readonly GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch;

    private Matrix _view;
    private Matrix _world;
    private Matrix _projection;

    private Effect _effect;
    private List<Matrix> _roomWorlds = new();

    private VertexBuffer _roomVertexBuffer;
    private IndexBuffer _roomIndexBuffer;
    private int _roomPrimitiveCount;            // Cantidad total de triangulos, cada 3 debe de ser un triangulo

    private Vector3 _cameraPosition = new Vector3(0, 50, 150);
    private float _playerRotation = 0f;

    private readonly List<(Model Model, Matrix World, string Name)> _models = new();

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

        // Apago el backface culling.
        // Esto se hace por un problema en el diseno del modelo del logo de la materia.
        // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
        var rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        // Seria hasta aca.

        // Configuramos nuestras matrices de la escena.
        _world = Matrix.Identity;
        _view = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
        _projection =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1500);

        base.Initialize();
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        // Creo el cuarto
        var room = new Room();
        var roomMesh = room.CreateRoom(
            width: 150f,
            height: 120f,
            depth: 150f,
            floorColor: Color.Black,
            ceilingColor: Color.Yellow,
            frontWallColor: Color.Red,
            backWallColor: Color.Pink,
            leftWallColor: Color.Green,
            rightWallColor: Color.Blue,
            frontOpening: WallOpening.Door(40f, 80f),
            backOpening: WallOpening.Solid(),
            leftOpening: WallOpening.Window(50f, 30f),
            rightOpening: WallOpening.Window(60f, 30f)
        );

        // Centro para los openings
        List<Vector3> centers = room.OpeningCenters;

        _roomVertexBuffer = new VertexBuffer(
            GraphicsDevice,
            typeof(VertexPositionColor),
            roomMesh.Vertices.Length,
            BufferUsage.WriteOnly
        );
        _roomVertexBuffer.SetData(roomMesh.Vertices);

        _roomIndexBuffer = new IndexBuffer(
            GraphicsDevice,
            IndexElementSize.SixteenBits,
            roomMesh.Indices.Length,
            BufferUsage.WriteOnly
        );
        _roomIndexBuffer.SetData(roomMesh.Indices);

        _roomPrimitiveCount = roomMesh.Indices.Length / 3;

        // Varias habitaciones en distintas posiciones
        _roomWorlds.Add(Matrix.CreateTranslation(0, 0, 0));
        _roomWorlds.Add(Matrix.CreateTranslation(320, 0, 0));
        _roomWorlds.Add(Matrix.CreateTranslation(640, 0, 0));
        _roomWorlds.Add(Matrix.CreateTranslation(960, 0, 0));

        var modelPaths = new[]
        {
            "Player/PSX_Player_Arms",
            "Items/PSX_Door",
            "Items/PSX_Nokia",

            "Level/Bathroom/PSX_Toilet_Paper",
            "Level/Bathroom/PSX_Toilet",

            "Level/Bedroom/PSX_Bed",
            "Level/Bedroom/PSX_Lamp",
            "Level/Bedroom/PSX_Wooden_Closet",
            "Level/Bedroom/PSX_Wooden_Drawers",

            "Level/Computer/PSX_Computer_Chair",
            "Level/Computer/PSX_Dirty_Old_PC",

            "Level/Kitchen/PSX_Empty_Cup",
            "Level/Kitchen/PSX_Microwave",
            "Level/Kitchen/PSX_Plate",
            "Level/Kitchen/PSX_Plate1",
            "Level/Kitchen/PSX_Stockpot",
            "Level/Kitchen/PSX_Wooden_Table1",

            "Level/Living/PSX_Armchair",
            "Level/Living/PSX_Old_TV",
            "Level/Living/PSX_PlayStation1",
            "Level/Living/PSX_TV_Stand",
            "Level/Living/PSX_Wooden_Chair",
            "Level/Living/PSX_Wooden_Chair1",
            "Level/Living/PSX_Wooden_Chair2",
            "Level/Living/PSX_Wooden_Table",

            "Level/Outdoor/Grass",
            "Level/Outdoor/LowPoly_Grass",
            "Level/Outdoor/LowPoly_Tree",
            "Level/Outdoor/PSX_Bush",
            "Level/Outdoor/PSX_Bush2",
            "Level/Outdoor/PSX_Bush3",
            "Level/Outdoor/PSX_Fence_White_Gate_Poles",
            "Level/Outdoor/PSX_Fence_White_Gate",
            "Level/Outdoor/PSX_Fence_White_Left_Closed",
            "Level/Outdoor/PSX_Fence_White_Left_Open",
            "Level/Outdoor/PSX_Fence_White_Right_Closed",
            "Level/Outdoor/PSX_Fence_White_Right_Open",
            "Level/Outdoor/PSX_Fence_White_Right_Pole",

            "Miscellaneous/PSX_Bloody_Cleaver_Knife",
            "Miscellaneous/PSX_Bloody_Fire_Axe",
            "Miscellaneous/PSX_Paper_Stack",
            "Miscellaneous/PSX_Rusty_Barell",
            "Miscellaneous/PSX_Wooden_Barrel"
        };

        float modelSpacing = 150f;
        Vector3 modelsStart = new Vector3(0, 0, -400);

        for (int i = 0; i < modelPaths.Length; i++)
        {
            var model = Content.Load<Model>(ContentFolder3D + modelPaths[i]);
            ApplyCustomEffectToModel(model, _effect);
            var world = Matrix.CreateScale(1f) * Matrix.CreateTranslation(modelsStart + new Vector3(i * modelSpacing, 0, 0));
            _models.Add((model, world, modelPaths[i]));
        }

        base.LoadContent();
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        float moveSpeed = 300f;
        float turnSpeed = 3f;

        if (keyboardState.IsKeyDown(Keys.Left)) _playerRotation += turnSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.Right)) _playerRotation -= turnSpeed * elapsedTime;

        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(_playerRotation));
        Vector3 right = Vector3.Cross(forward, Vector3.Up);

        if (keyboardState.IsKeyDown(Keys.W)) _cameraPosition += forward * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.S)) _cameraPosition -= forward * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.A)) _cameraPosition -= right * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.D)) _cameraPosition += right * moveSpeed * elapsedTime;

        _cameraPosition.Y = 50f;
        UpdateViewMatrix();

        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        if (_effect == null || _roomVertexBuffer == null || _roomIndexBuffer == null)
            return;

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            //FillMode = FillMode.WireFrame
        };

        GraphicsDevice.SetVertexBuffer(_roomVertexBuffer);
        GraphicsDevice.Indices = _roomIndexBuffer;

        _effect.Parameters["View"]?.SetValue(_view);
        _effect.Parameters["Projection"]?.SetValue(_projection);
        _effect.Parameters["UseVertexColor"]?.SetValue(true);
        _effect.Parameters["DiffuseColor"]?.SetValue(Vector3.One);

        foreach (var world in _roomWorlds)
        {
            _effect.Parameters["World"]?.SetValue(world);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    primitiveCount: _roomPrimitiveCount
                );
            }
        }

        foreach (var (model, world, name) in _models)
        {
            DrawModelWithCustomEffect(model, world, name);
        }

        base.Draw(gameTime);
    }

    private void ApplyCustomEffectToModel(Model model, Effect effectTemplate)
    {
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                part.Effect = effectTemplate.Clone();
            }
        }
    }

    private void DrawModelWithCustomEffect(Model model, Matrix world, string modelName)
    {
        Vector3 tint = ModelTintHelper.GetTint(modelName).ToVector3();

        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var effect = (Effect)part.Effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * world);
                effect.Parameters["View"]?.SetValue(_view);
                effect.Parameters["Projection"]?.SetValue(_projection);
                effect.Parameters["UseVertexColor"]?.SetValue(false);
                effect.Parameters["DiffuseColor"]?.SetValue(tint);
            }

            mesh.Draw();
        }
    }

    /// <summary>
    ///     Libero los recursos que se cargaron en el juego.
    /// </summary>
    protected override void UnloadContent()
    {
        // Libero los recursos.
        Content.Unload();
        base.UnloadContent();
    }

    private void UpdateViewMatrix()
    {
        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(_playerRotation));
        _view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + forward, Vector3.Up);
    }
}