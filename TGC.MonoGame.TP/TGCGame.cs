using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Factories;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

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
    private readonly List<(VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int PrimitiveCount, Matrix World)> _rooms = new();
    private readonly Dictionary<string, Model> _modelCache = new();

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

        // Creo una matriz de 3x3 con habitaciones
        // Dimensiones elegidas para todas las habitaciones
        float roomWidth = 150f;
        float roomHeight = 120f;
        float roomDepth = 150f;

        float roomGap = 1f;   // Distancia entre las habitaciones para que no haya bleeding
        float cellSize = 30f; // Tamaño de cada celda definido arbitrariamente para todas las habitaciones

        // Tamaño de las aberturas
        var door = WallOpening.Door(40f, 80f);
        var window = WallOpening.Window(50f, 30f);
        var solid = WallOpening.Solid();

        var grid = new[]
        {
            // Row Z = 0
            new[]
            {
                (RoomType.Bed,     front: door, back: solid, left: window, right: door),
                (RoomType.Kitchen, front: door, back: window, left: door,   right: door),
                (RoomType.Bed,     front: door, back: solid, left: door,   right: window),
            },
            // Row Z = 1
            new[]
            {
                (RoomType.Living,  front: door, back: door,  left: window, right: door),
                (RoomType.Hallway, front: door, back: door,  left: door,   right: door),
                (RoomType.Bath,    front: door, back: door,  left: door,   right: window),
            },
            // Row Z = 2
            new[]
            {
                (RoomType.Computer, front: solid, back: door, left: window, right: door),
                (RoomType.Outdoor,  front: window, back: door, left: door, right: door),
                (RoomType.Bed,      front: solid, back: door, left: door, right: window),
            }
        };

        var rng = new Random(); // Semilla random para que los elementos se muestren de forma aleatoria cada vez que se genera

        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                var roomDefinition = grid[z][x]; // Tupla de 5 elementos

                var room = new Room();
                var mesh = room.CreateRoom(
                    width: roomWidth,
                    height: roomHeight,
                    depth: roomDepth,
                    floorColor: Color.Black,
                    ceilingColor: Color.Yellow,
                    frontWallColor: Color.Red,
                    backWallColor: Color.Pink,
                    leftWallColor: Color.Green,
                    rightWallColor: Color.Blue,
                    frontOpening: roomDefinition.front,
                    backOpening: roomDefinition.back,
                    leftOpening: roomDefinition.left,
                    rightOpening: roomDefinition.right
                );

                var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), mesh.Vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(mesh.Vertices);

                var indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, mesh.Indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(mesh.Indices);

                int primCount = mesh.Indices.Length / 3;

                float worldX = x * (roomWidth * 2f + roomGap);
                float worldZ = z * (roomDepth * 2f + roomGap);
                var roomWorld = Matrix.CreateTranslation(worldX, 0f, worldZ); // Muevo la habitacion al lugar donde debera de quedar

                _rooms.Add((vertexBuffer, indexBuffer, primCount, roomWorld)); // Agrego la informacion de cada habitacion en la variable global _rooms

                // Renderizado de modelos por habitacion
                IRoomAssets roomTypeInstance = RoomFactory.Create(roomDefinition.Item1);
                if (roomTypeInstance != null)
                {
                    var placements = ModelPlacementOnRoomHelper.GeneratePlacements(
                        roomTypeInstance,
                        roomWidth,
                        roomDepth,
                        cellSize,
                        seed: rng.Next()
                    );

                    foreach (var (modelPath, localPos) in placements)
                    {
                        // Utilizo el diccionario para poder reutilizar elementos que ya se hayan guardado
                        if (!_modelCache.TryGetValue(modelPath, out var model))
                        {
                            model = Content.Load<Model>(ContentFolder3D + modelPath);
                            ApplyCustomEffectToModel(model, _effect);
                            _modelCache[modelPath] = model;
                        }

                        // Posicion final de los modelos / Se achican a la mitad los modelos porque muchos son realmente grandes
                        var modelWorld = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(roomWorld.Translation + localPos);
                        _models.Add((model, modelWorld, modelPath));
                    }
                }
            }
        }

        Window.Title = $"TGC.MonoGame.TP - Models: {_models.Count}";

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

        // Ya no se fija en VertexBuffer y IndexBuffer sino en la cantidad de habitaciones que existan
        if (_effect == null || _rooms.Count == 0)
            return;

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            //FillMode = FillMode.WireFrame
        };

        _effect.Parameters["View"]?.SetValue(_view);
        _effect.Parameters["Projection"]?.SetValue(_projection);
        _effect.Parameters["UseVertexColor"]?.SetValue(true);
        _effect.Parameters["DiffuseColor"]?.SetValue(Vector3.One);

        foreach (var room in _rooms)
        {
            GraphicsDevice.SetVertexBuffer(room.VertexBuffer);
            GraphicsDevice.Indices = room.IndexBuffer;
            _effect.Parameters["World"]?.SetValue(room.World);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    room.PrimitiveCount // Utilizo las primitivas guardadas LoadContent para dibujar las habitaciones
                );
            }
        }

        foreach (var (model, world, name) in _models)
        {
            DrawModelWithCustomEffect(model, world, name);
        }

        base.Draw(gameTime);
    }

    /// <summary>
    /// Reemplao el material por defecto de los modelos con un material personalizado
    /// </summary>
    /// <param name="model"></param>
    /// <param name="effectTemplate"></param>
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