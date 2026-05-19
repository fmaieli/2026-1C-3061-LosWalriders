using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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

        // Delegamos TODA la generacion del nivel al Helper
        LevelGeneratorHelper.GenerateLevel(
            GraphicsDevice,
            Content,
            _effect,
            _cameraPosition,
            _modelCache,
            _rooms,
            _models,
            _trees,
            out _groundVertexBuffer,
            out _groundIndexBuffer,
            out _groundPrimitiveCount
        );

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