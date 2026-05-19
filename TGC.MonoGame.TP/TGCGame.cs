using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    private readonly List<(Model Model, Matrix World, string Name)> _models = new();

    // Variables de camara Player
    private Vector3 _cameraPosition = new Vector3(0, 50, 150);
    private float _playerRotation = 0f;

    // Variables de camara Free (para debuguear y ver rapidamente el nivel generado)
    private float _cameraPitch = 0f;
    private bool _freeCameraMode = false;
    private KeyboardState _previousKeyboardState;

    private VertexBuffer _groundVertexBuffer;
    private IndexBuffer _groundIndexBuffer;
    private int _groundPrimitiveCount;

    private readonly List<(Model Model, Matrix World, string Name)> _trees = new();

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
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 2500);

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

        string[] mapLayout = new string[]
        {
            // 15 horizontal
            "       Z       ", // 00 -> Exit Level
            "  HHHHHH       ", // 01
            "  H OOOOOOO    ", // 02
            "  H OOOOOOO   X", // 03
            " CC OOOOOOOHHHH", // 04
            " CC OOOOOOOHBBH", // 05
            "BCC OOOOOOOHBBH", // 06
            "BCC OOOOOOOHH H", // 07
            "HCCHOOOOOOOHBBH", // 08
            "HCCHOOOOOOO BBH", // 09
            "HCCH   H   HHHH", // 10
            "HCCH   H   HBBH", // 11
            "HCCHHHHHHHHHBBH", // 12
            "H  V  BH      H", // 13
            "H  V  BH      H", // 14
            "HHHHHHHHHHHVVVH", // 15
            "HBB H  H  HKK H", // 16
            "HBB HLLLLLHKK H", // 17
            "HH HHLLLLLHKK H", // 18
            "HBB HLLLLLHKK H", // 19
            "HBB HLLLLLHKK H", // 20
            "H V HLLLLLHKK H", // 21
            " BB H  H  HKKBH", // 22 
            " BB H EEE HKKBH", // 23 (Entrance Level 3x3)
            " HHHHHEEEHHHHHH", // 24 
            "      EPE      ", // 25 (Player en el centro de Entrance)
            "               "  // 26
        };

        int rows = mapLayout.Length;
        int cols = mapLayout[0].Length;

        float baseRoomWidth = 150f;
        float baseRoomDepth = 150f;
        float roomHeight = 120f;        
        float roomGap = 0.5f;         // Distancia entre las habitaciones para que no haya bleeding
        float cellSize = 30f;

        // Se busca al Player dentro del "mapa" para poder mutar su valor a E
        int playerGridX = 0, playerGridZ = 0;
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (mapLayout[z][x] == 'P')
                {
                    playerGridX = x;
                    playerGridZ = z;

                    // Se convierte a P por E para armar el bloque continuo de la habitacion Entrance
                    char[] rowChars = mapLayout[z].ToCharArray();
                    rowChars[x] = 'E';
                    mapLayout[z] = new string(rowChars);
                    break;
                }
            }
        }

        float startWorldX = _cameraPosition.X - (playerGridX * (baseRoomWidth * 2f + roomGap));
        float startWorldZ = _cameraPosition.Z - (playerGridZ * (baseRoomDepth * 2f + roomGap));

        var rng = new Random();

        // Control para las celdas ya procesadas
        bool[,] processedCell = new bool[rows, cols];

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (processedCell[z, x]) continue;  // Si ya fue procesada se saltea

                char currentCell = mapLayout[z][x]; // Cell a procesar

                // Ignorar celdas de Player, Exit, Enemy o vacias
                if (currentCell == ' ' || currentCell == 'P' || currentCell == 'Z' || currentCell == 'X')
                {
                    processedCell[z, x] = true;
                    continue;
                }

                var roomData = LevelGeneratorHelper.GetRoomData(currentCell);
                if (roomData == null)
                {
                    processedCell[z, x] = true;
                    continue;
                }
               
                // Ver que tan ancha es la habitación hacia la derecha
                int widthCells = 1;
                while (x + widthCells < cols && mapLayout[z][x + widthCells] == currentCell && !processedCell[z, x + widthCells])
                {
                    widthCells++;
                }

                // Ver que tan profunda es hacia abajo (manteniendo el mismo ancho)
                int heightCells = 1;
                bool canExpand = true;
                while (z + heightCells < rows)
                {
                    for (int i = 0; i < widthCells; i++)
                    {
                        if (mapLayout[z + heightCells][x + i] != currentCell || processedCell[z + heightCells, x + i])
                        {
                            canExpand = false;
                            break;
                        }
                    }
                    if (!canExpand) break;
                    heightCells++;
                }

                // Marcar todas las celdas del bloque como visitadas
                for (int hz = 0; hz < heightCells; hz++)
                    for (int wx = 0; wx < widthCells; wx++)
                        processedCell[z + hz, x + wx] = true;

                float cellStepX = baseRoomWidth * 2f + roomGap;
                float cellStepZ = baseRoomDepth * 2f + roomGap;

                // Nuevos anchos/profundidades
                float mergedWidthHalf = baseRoomWidth + (widthCells - 1) * (cellStepX / 2f);
                float mergedDepthHalf = baseRoomDepth + (heightCells - 1) * (cellStepZ / 2f);

                // Centro exacto en el mundo 3D
                float mergedWorldX = startWorldX + (x + (widthCells - 1) / 2f) * cellStepX;
                float mergedWorldZ = startWorldZ + (z + (heightCells - 1) / 2f) * cellStepZ;

                // Para determinar las puertas, miramos al vecino que cae justo en el medio de las paredes fusionadas
                char frontNeighbor = (z + heightCells < rows) ? mapLayout[z + heightCells][x + widthCells / 2] : ' ';
                char backNeighbor = (z > 0) ? mapLayout[z - 1][x + widthCells / 2] : ' ';
                char leftNeighbor = (x > 0) ? mapLayout[z + heightCells / 2][x - 1] : ' ';
                char rightNeighbor = (x + widthCells < cols) ? mapLayout[z + heightCells / 2][x + widthCells] : ' ';

                var frontOpening = LevelGeneratorHelper.DetermineOpening(currentCell, frontNeighbor);
                var backOpening = LevelGeneratorHelper.DetermineOpening(currentCell, backNeighbor);
                var leftOpening = LevelGeneratorHelper.DetermineOpening(currentCell, leftNeighbor);
                var rightOpening = LevelGeneratorHelper.DetermineOpening(currentCell, rightNeighbor);

                var room = new Room();
                var mesh = room.CreateRoom(
                    width: mergedWidthHalf, 
                    height: roomHeight, 
                    depth: mergedDepthHalf, 
                    floorColor: Color.Black,
                    ceilingColor: Color.DarkGray,
                    frontWallColor: roomData.Value.FrontBack,
                    backWallColor: roomData.Value.FrontBack,
                    leftWallColor: roomData.Value.LeftRight,
                    rightWallColor: roomData.Value.LeftRight,
                    frontOpening: frontOpening, 
                    backOpening: backOpening,
                    leftOpening: leftOpening, 
                    rightOpening: rightOpening
                );

                var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), mesh.Vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(mesh.Vertices);

                var indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, mesh.Indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(mesh.Indices);

                int primCount = mesh.Indices.Length / 3;
                var roomWorld = Matrix.CreateTranslation(mergedWorldX, 0f, mergedWorldZ);
                _rooms.Add((vertexBuffer, indexBuffer, primCount, roomWorld));

                // Renderizado de modelos por habitacion
                IRoomAssets roomTypeInstance = RoomFactory.Create(roomData.Value.Type);
                if (roomTypeInstance != null)
                {
                    var placements = ModelPlacementOnRoomHelper.GeneratePlacements(
                        roomTypeInstance, 
                        mergedWidthHalf, 
                        mergedDepthHalf,
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

        // Crear suelo
        // TODO: REVISAR PARA COMPLETAR EL LARGO DE TODAS LAS HABITACIONES - GRILLA DE 15x27
        float gridWidth = 3 * (baseRoomWidth * 2f + roomGap); // Ancho de la 3 habitaciones
        float gridDepth = 3 * (baseRoomDepth * 2f + roomGap); // Profundidad de las 3 habitaciones

        // Gran tamaño para el suelo alrededor
        float groundMargin = 4000f;
        (_groundVertexBuffer, _groundIndexBuffer, _groundPrimitiveCount) =
            GroundBuilderHelper.Create(
                GraphicsDevice,
                gridWidth * 0.5f + groundMargin,
                gridDepth * 0.5f + groundMargin,
                Color.Brown);

        // Cantidad de arboles y escala
        int treeCount = 300;
        float treeScale = 0.1f;

        var treeModel = Content.Load<Model>("Models/World/PSX_Low_Poly_Tree");
        ApplyCustomEffectToModel(treeModel, _effect);

        for (int i = 0; i < treeCount; i++)
        {
            // Valor entre 0 y 1 para generar las distancias de los arboles
            float x = (float)(rng.NextDouble() * (gridWidth + groundMargin * 2) - (gridWidth * 0.5f + groundMargin));
            float z = (float)(rng.NextDouble() * (gridDepth + groundMargin * 2) - (gridDepth * 0.5f + groundMargin));

            // Randomizo la rotacion en Y y con valores aleatorios entre 0 y 1
            float rotY = MathHelper.ToRadians((float)rng.NextDouble() * 360f);

            var world = Matrix.CreateScale(treeScale) * Matrix.CreateRotationY(rotY) *
                Matrix.CreateTranslation(x, -1f, z); // Mismo nivel que el suelo -> revisar Draw para el suelo

            _trees.Add((treeModel, world, "World/PSX_Low_Poly_Tree"));
        }

        Window.Title = $"TGC.MonoGame.TP - Models: {_models.Count + _trees.Count}";

        base.LoadContent();
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Valores para saber si presione Ctrl y Shift (Izquierdos)
        bool isCtrlDown = keyboardState.IsKeyDown(Keys.LeftControl);
        bool isShiftDown = keyboardState.IsKeyDown(Keys.LeftShift);

        if (isCtrlDown && isShiftDown && keyboardState.IsKeyDown(Keys.F) && _previousKeyboardState.IsKeyUp(Keys.F))
        {
            _freeCameraMode = !_freeCameraMode; // Activo/Desactivo FreeCamera
        }

        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // En FreeCamera la velocidad es el doble
        float moveSpeed = _freeCameraMode ? 600f : 300f;
        float turnSpeed = 3f;

        // Rotacion de la camara para modo normal
        if (keyboardState.IsKeyDown(Keys.Left)) _playerRotation += turnSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.Right)) _playerRotation -= turnSpeed * elapsedTime;

        // Rotacion de la camara arriba y abajo
        if (_freeCameraMode)
        {
            if (keyboardState.IsKeyDown(Keys.Up)) _cameraPitch += turnSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.Down)) _cameraPitch -= turnSpeed * elapsedTime;

            // Limitamos el pitch para no dar una vuelta completa
            _cameraPitch = MathHelper.Clamp(_cameraPitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);
        }
        else
        {
            // Se endereza la vista a 0
            _cameraPitch = 0f;
        }

        // Movimiento vertical
        if (_freeCameraMode)
        {
            // Subir y bajar en el eje Y
            if (keyboardState.IsKeyDown(Keys.E)) _cameraPosition += Vector3.Up * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.Q)) _cameraPosition -= Vector3.Up * moveSpeed * elapsedTime;
        }
        else
        {
            // En modo normal, el jugador se queda al ras del suelo
            _cameraPosition.Y = 50f;
        }

        Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(_playerRotation, _cameraPitch, 0f);
        Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);
        Vector3 right = Vector3.Transform(Vector3.Right, cameraRotation);

        if (keyboardState.IsKeyDown(Keys.W)) _cameraPosition += forward * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.S)) _cameraPosition -= forward * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.A)) _cameraPosition -= right * moveSpeed * elapsedTime;
        if (keyboardState.IsKeyDown(Keys.D)) _cameraPosition += right * moveSpeed * elapsedTime;       

        UpdateViewMatrix();

        // Guardado del estado del teclado para proximo frame - toggle de teclas
        _previousKeyboardState = keyboardState;

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

        // Suelo
        if (_groundVertexBuffer != null && _groundIndexBuffer != null)
        {
            GraphicsDevice.SetVertexBuffer(_groundVertexBuffer);
            GraphicsDevice.Indices = _groundIndexBuffer;
            // Por bleeding entre habitaciones y suelo
            _effect.Parameters["World"]?.SetValue(Matrix.CreateTranslation(0f, -1f, 0f));

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _groundPrimitiveCount
                );
            }
        }

        // Habitaciones
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

        // Dibujado de modelos en habitaciones
        foreach (var (model, world, name) in _models)
        {
            DrawModelWithCustomEffect(model, world, name);
        }

        // Dibujado de arboles en suelo
        foreach (var (model, world, name) in _trees)
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
        Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(_playerRotation, _cameraPitch, 0f);
        Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);

        _view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + forward, Vector3.Up);
    }
}