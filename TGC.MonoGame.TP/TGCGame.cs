using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Entities.Character;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Geometries;
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

    private readonly Player _player = new();
    private readonly Enemy _enemy = new();

    private VertexBuffer _groundVertexBuffer;
    private IndexBuffer _groundIndexBuffer;
    private int _groundPrimitiveCount;

    private readonly List<(Model Model, Matrix World, string Name)> _trees = new();

    // Post-Processing
    private RenderTarget2D _sceneRenderTarget;
    private FullScreenQuad _fullScreenQuad;
    private Effect _postProcessEffect;
    private Texture2D _overlayTexture;

    // 2D
    private Texture2D _pixelTexture;

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
        IsMouseVisible = false;
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

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        _player.LoadContent(Content, _effect);

        _enemy.LoadContent(Content, _effect);
        // En un principio dibujo el enemigo cerca para comprobar que este funcionando correctamente
        // Revisar donde deberia de spawnear dentro del mapa
        _enemy.Position = _player.Position + new Vector3(0, 0, -250f);

        // Delegamos TODA la generacion del nivel al Helper
        LevelGeneratorHelper.GenerateLevel(
            GraphicsDevice,
            Content,
            _effect,
            _player.Position,
            _modelCache,
            _rooms,
            _models,
            _trees,
            out _groundVertexBuffer,
            out _groundIndexBuffer,
            out _groundPrimitiveCount
        );

        Window.Title = $"TGC.MonoGame.TP - Models: {_models.Count + _trees.Count}";

        #region Post-Processing
        _fullScreenQuad = new FullScreenQuad(GraphicsDevice);

        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
            false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

        _postProcessEffect = Content.Load<Effect>(ContentFolderEffects + "TextureMerge");

        _overlayTexture = Content.Load<Texture2D>(ContentFolderTextures + "blood_splash");
        _postProcessEffect.Parameters["overlayTexture"]?.SetValue(_overlayTexture);
        #endregion

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

        _player.Update(gameTime, _models);
        _enemy.Update(gameTime, _player.Position, _player.IsHidden);
        _view = _player.View;

        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        #region Pass 1 - Post-Processing
        bool applyBloodEffect = _enemy.State == EnemyState.Cooldown;

        if (applyBloodEffect)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        }
        else
        {
            GraphicsDevice.SetRenderTarget(null);
        }
        #endregion

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
        for (int i = 0; i < _models.Count; i++)
        {
            var (model, world, name) = _models[i];

            // Si este es el objeto que el jugador esta mirando de cerca, se le dibuja el borde
            if (_player.InteractableModelIndex == i)
            {
                DrawModelOutline(model, world, name);
            }

            // Dibujo normal
            DrawModelWithCustomEffect(model, world, name);
        }

        // Dibujado de arboles en suelo
        foreach (var (model, world, name) in _trees)
        {
            DrawModelWithCustomEffect(model, world, name);
        }

        // Dibujado de brazos para el jugador
        _player.DrawArms(_view, _projection, GraphicsDevice);

        // Dibujado de enemigo
        _enemy.Draw(_view, _projection);

        #region Pass 2 - Post-Processing
        if (applyBloodEffect)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SetRenderTarget(null);

            _postProcessEffect.Parameters["baseTexture"]?.SetValue(_sceneRenderTarget);
            _postProcessEffect.Parameters["time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            _postProcessEffect.Parameters["intensity"]?.SetValue(_enemy.CooldownIntensity);

            _fullScreenQuad.Draw(_postProcessEffect);
        }
        #endregion

        #region HUD
        if (_player.IsLightActive)
        {
            _spriteBatch.Begin();

            float percentage = _player.CurrentLightDurabilityPercentage;

            int barWidth = 400;
            int barHeight = 20;

            // Centrado horizontalmente y en la parte inferior de la pantalla
            int xPos = (GraphicsDevice.Viewport.Width - barWidth) / 2;
            int yPos = GraphicsDevice.Viewport.Height - 60;

            // Fondo gris oscuro para la barra
            _spriteBatch.Draw(_pixelTexture, new Rectangle(xPos, yPos, barWidth, barHeight), Color.DarkGray);

            // Con el valor del porcentajo voy actualizando el valor lleno de la barra
            int fillWidth = (int)(barWidth * percentage);
            // Blanco para el valor del porcentaje restante
            _spriteBatch.Draw(_pixelTexture, new Rectangle(xPos, yPos, fillWidth, barHeight), Color.White);
            _spriteBatch.End();

            // Restauro DepthStencilState tras usar SpriteBatch
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        #endregion

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

    private void DrawModelOutline(Model model, Matrix world, string name)
    {
        // Modifico Rasterizer para que solo dibuje las caras internas del modelo
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace };
        // Diferencio el escalado del modelo para que se note mas el borde para match box
        float outlineScale = name.Contains("Match_Box") ? 1.20f : 1.03f;

        Matrix outlineWorld = Matrix.CreateScale(outlineScale) * world;

        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var effect = (Effect)part.Effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * outlineWorld);
                effect.Parameters["View"]?.SetValue(_view);
                effect.Parameters["Projection"]?.SetValue(_projection);
                effect.Parameters["UseVertexColor"]?.SetValue(false);
                // Yellow cuando tenga los shaders
                effect.Parameters["DiffuseColor"]?.SetValue(Color.Azure.ToVector3());
            }

            mesh.Draw();
        }

        // Restauro el estado de Rasterizer para dibujar correctamente todo el resto
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
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
}