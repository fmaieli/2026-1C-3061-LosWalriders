using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Entities.Character;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Geometries;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Helpers.Managers;
using TGC.MonoGame.TP.SourceCode.Screens;

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

    private Effect _effect;         // BasicShader (habitaciones y suelo)
    private Effect _modelsEffect;   // ModelsTexturesShader (modelos)

    private KeyboardState _previousKeyboardState;

    private readonly List<(
        VertexBuffer VertexBuffer,
        IndexBuffer IndexBuffer, 
        int PrimitiveCount, 
        Matrix World, 
        RoomType Type)> _rooms = new();
    private readonly Dictionary<string, Model> _modelCache = new();
    private readonly List<(Model Model, Matrix World, string Name)> _models = new();

    private readonly Player _player = new();
    private readonly Enemy _enemy = new();

    private VertexBuffer _groundVertexBuffer;
    private IndexBuffer _groundIndexBuffer;
    private int _groundPrimitiveCount;

    private readonly List<(Model Model, Matrix World, string Name)> _trees = new();
    private Texture2D _woodFloorTexture;
    private Texture2D _concreteWallTexture;

    private float _gameTimer = 300f; // 5 Minutos (en segundos)
    private bool _isGameOver = false;
    private float _timeTaken = 0f;

    // FPS
    private int _frameCount = 0;
    private float _timeSinceLastUpdate = 0f;
    private int _currentFps = 0;

    // Menu - Victory - GameOver
    private GameState _gameState = GameState.Menu;
    private bool _showingControls = false;
    private ControlsScreen _controlsScreen = new ControlsScreen();
    private MenuScreen _menuScreen = new MenuScreen();
    private VictoryScreen _victoryScreen = new VictoryScreen();
    private GameOverScreen _gameOverScreen = new GameOverScreen();

    // Animacion de camera
    private Vector3 _menuCameraPosition;
    private Vector3 _menuCameraTarget;
    private float _transitionProgress = 0f;

    // Post-Processing
    private RenderTarget2D _sceneRenderTarget;
    private FullScreenQuad _fullScreenQuad;
    private Effect _postProcessEffect;
    private Texture2D _overlayTexture;
    private float _grainIntensity = 0f;

    private Effect _distortionEffect;
    private RenderTarget2D _distortionRenderTarget;
    private float _darknessTimer = 0f; // Sube de 0 a 3 segundos
    private float _distortionFactor = 0f;

    // 2D - Skybox
    private Texture2D _pixelTexture;
    private SpriteFont _spriteFont;
    private SkyBox _skyBox;

    // Musica
    private Song _menuMusic;
    private Song _gameMusic;

    // Efectos de sonido
    private SoundEffect _carDoorOpen;
    private SoundEffect _carDoorClose;
    private bool _hasPlayedDoorClose = false;
    private SoundEffect _terrorScream;
    private float _screamTimer = 60f;

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

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
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 5000);

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

        // SpriteFont
        _spriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "HUD");

        // Skybox
        var skyBoxModel = Content.Load<Model>(ContentFolder3D + "skybox/cube");
        var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/skybox_night");

        var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
        _skyBox = new SkyBox(skyBoxModel, skyBoxTexture, skyBoxEffect, 3000f);

        // Efecto distorcion
        _distortionEffect = Content.Load<Effect>(ContentFolderEffects + "ScreenDistortion");

        _distortionRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.Viewport.Width, 
            GraphicsDevice.Viewport.Height,
            false, 
            SurfaceFormat.Color, 
            DepthFormat.None, 
            0, 
            RenderTargetUsage.DiscardContents
        );

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        _modelsEffect = Content.Load<Effect>(ContentFolderEffects + "ModelsTexturesShader");

        RoomTextureManager.LoadTextures(Content);

        _player.LoadContent(Content, _modelsEffect);
        _enemy.LoadContent(Content, _modelsEffect);

        // En un principio dibujo el enemigo cerca para comprobar que este funcionando correctamente
        // Revisar donde deberia de spawnear dentro del mapa
        _enemy.Position = _player.Position + new Vector3(0, 0, -500f);

        // Busco donde esta el jugador y lo apunto al enemigo hacia él
        Vector3 directionToPlayer = _player.Position - _enemy.Position;
        directionToPlayer.Normalize();
        _enemy.Forward = directionToPlayer;
        
        LoadLevel();

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

        // Vista inicial del menu
        _menuCameraPosition = _player.Position + new Vector3(0, 15f, 250f);
        _menuCameraTarget = _player.Position + new Vector3(0, 20f, 0f);

        // Musica del juego
        _menuMusic = Content.Load<Song>(ContentFolderMusic + "menu_terror_music");
        _gameMusic = Content.Load<Song>(ContentFolderMusic + "ambience");
        // Efectos de sonido
        _carDoorOpen = Content.Load<SoundEffect>(ContentFolderSounds + "car_door_open");
        _carDoorClose = Content.Load<SoundEffect>(ContentFolderSounds + "car_door_close");
        _terrorScream = Content.Load<SoundEffect>(ContentFolderSounds + "terror_scream");

        // Se configura para que este en loop constante
        MediaPlayer.IsRepeating = true;
        // Arranca con la musica del menu apenas se ejecuta
        MediaPlayer.Play(_menuMusic);

        // Pantallas Victory - GameOver
        _victoryScreen.Initialize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _gameOverScreen.Initialize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        base.LoadContent();   
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        #region conteo FPS
        _timeSinceLastUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Paso 1 segundo
        if (_timeSinceLastUpdate >= 1f)
        {
            _currentFps = _frameCount; // Guarda la cantidad de frames
            _frameCount = 0;           // Reinicio el contador
            _timeSinceLastUpdate -= 1f;// Resto el segundo que ya se proceso
        }
        #endregion

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Me tiene harto la musica
        if (keyboardState.IsKeyDown(Keys.M) && _previousKeyboardState.IsKeyUp(Keys.M))
        {
            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
        }

        // Reinicio con la R
        if (keyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
        {
            ResetGame();
        }

        if (keyboardState.IsKeyDown(Keys.C) && _previousKeyboardState.IsKeyUp(Keys.C))
        {
            if (_gameState == GameState.Playing || _gameState == GameState.Transitioning)
            {
                _showingControls = !_showingControls;
                _controlsScreen.ShowBackButton = false;
            }
        }

        // Muestra la pantalla de controles
        if (_showingControls)
        {
            IsMouseVisible = true;
            bool closeRequested = _controlsScreen.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            if (closeRequested)
            {
                _showingControls = false;
                if (_gameState == GameState.Playing || _gameState == GameState.Transitioning)
                    IsMouseVisible = false;
            }

            _previousKeyboardState = keyboardState;
            return;
        }

        switch (_gameState)
        {
            case GameState.Menu:
                // Habilito el mouse para el menu
                IsMouseVisible = true;

                var action = _menuScreen.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                if (action == MenuAction.Play)
                {
                    _gameState = GameState.Transitioning;
                    MediaPlayer.Stop();

                    // Sonido de puerta de auto abriendose
                    _carDoorOpen.Play();
                    _hasPlayedDoorClose = false;
                }
                else if (action == MenuAction.Tutorial)
                {
                    _showingControls = true;
                    _controlsScreen.ShowBackButton = true;
                }
                else if (action == MenuAction.Exit)
                {
                    Exit();
                }

                // Camara estatica mirando a la puerta de entrada
                _view = Matrix.CreateLookAt(_menuCameraPosition, _menuCameraTarget, Vector3.Up);
                break;

            case GameState.Transitioning:
                // Deshabilito el mouse nuevamente
                IsMouseVisible = false;
                // Dura 2 segundos la transicion
                _transitionProgress += (float)gameTime.ElapsedGameTime.TotalSeconds / 2.0f;

                // 70% de la transicion
                if (_transitionProgress >= 0.7f && !_hasPlayedDoorClose)
                {
                    _carDoorClose.Play();
                    _hasPlayedDoorClose = true; // Para que no vuelva a sonar nuevamente
                }

                // 100% de la transicion
                if (_transitionProgress >= 1f)
                {
                    _transitionProgress = 1f;
                    _gameState = GameState.Playing;
                    MediaPlayer.Play(_gameMusic);
                }

                // Posicion de la camara del jugador y hacia donde esta mirando
                Matrix playerRotation = Matrix.CreateFromYawPitchRoll(_player.Rotation, 0f, 0f);
                Vector3 playerTarget = _player.Position + Vector3.Transform(Vector3.Forward, playerRotation);

                // MathHelper.SmoothStep hace que el vuelo arranque suave, acelere y frene suavemente al llegar
                Vector3 currentPos = Vector3.Lerp(_menuCameraPosition, _player.Position, MathHelper.SmoothStep(0, 1, _transitionProgress));
                Vector3 currentTarget = Vector3.Lerp(_menuCameraTarget, playerTarget, MathHelper.SmoothStep(0, 1, _transitionProgress));

                _view = Matrix.CreateLookAt(currentPos, currentTarget, Vector3.Up);
                break;

            case GameState.Playing:
                if (!_isGameOver)
                {
                    _gameTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (_player.HasWon)
                {
                    // Restamos los 5 minutos (300) menos el tiempo sobrante para saber cuánto tardó
                    _timeTaken = 300f - _gameTimer;
                    _gameState = GameState.Victory;
                    MediaPlayer.Stop(); // Paramos la música de tensión
                    // Opcional: _victorySound.Play(); si tuvieras un sonido
                }

                // Calculo que cad un minuto se escuche un grito
                _screamTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_screamTimer <= 0f)
                {
                    // Reinicio el temporizador de grito
                    _screamTimer = 60f;
                    
                    SoundEffectInstance screamInstance = _terrorScream.CreateInstance();
                    // Disminuyo el volumen para que parezca mas lejano
                    screamInstance.Volume = 0.2f;
                    // Paneo del sonido de forma aleatoria para que parezca que viene de algun lugar del nivel
                    // -1.0f (totalmente a la Izquierda) a 1.0f (totalmente a la Derecha)
                    System.Random rnd = new System.Random();
                    screamInstance.Pan = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    screamInstance.Play();
                }

                #region Girar modelos flotando en el aire
                for (int i = 0; i < _models.Count; i++)
                {
                    if (_models[i].Name.Contains("PSX_Item_Shotgun") || _models[i].Name.Contains("PSX_Item_Key"))
                    {
                        var modelTuple = _models[i];

                        Vector3 position = modelTuple.World.Translation;
                        modelTuple.World.Translation = Vector3.Zero;
                        modelTuple.World *= Matrix.CreateRotationY(2f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                        modelTuple.World.Translation = position;
                        _models[i] = modelTuple;
                    }
                }
                #endregion

                _player.Update(gameTime, _models, GraphicsDevice.Viewport);
                _enemy.Update(gameTime, _player.Position, _player.IsHidden);

                // Distancia de granulado filmico con respecto al enemigo
                float distanceToEnemy = Vector3.Distance(_player.Position, _enemy.Position);
                float grainRadius = 400f; // Misma distancia que radio de vision del enemigo

                // Si no nos atrapo y no estamos escondidos en el ropero
                if (_enemy.State != EnemyState.Cooldown && !_player.IsHidden && distanceToEnemy < grainRadius)
                {
                    // La intensidad aumenta de 0 a los 400, 1.0 si esta pegado a nosotros
                    _grainIntensity = MathHelper.Clamp(1.0f - (distanceToEnemy / grainRadius), 0f, 1f);
                }
                else
                {
                    _grainIntensity = 0f;
                }

                #region Distorcion en pantalla por oscuridad
                if (!_player.IsLightActive)
                {
                    // El blur va apareciendo progresivamente durante 3 segundos
                    _darknessTimer = MathHelper.Clamp(_darknessTimer + (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, 3f);
                }
                else
                {
                    // El jugador logra encender una luz, la vista se despeja el doble de rapido
                    _darknessTimer = MathHelper.Clamp(_darknessTimer - (float)gameTime.ElapsedGameTime.TotalSeconds * 2f, 0f, 3f);
                }

                // Factor de 0f a 1f para que el shader lo entienda
                _distortionFactor = _darknessTimer / 3.0f;
                #endregion

                _view = _player.View;
                break;

            case GameState.GameOver:
                IsMouseVisible = true;
                if (_gameOverScreen.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) == MenuAction.MainMenu)
                {
                    ResetGame();
                }
                break;

            case GameState.Victory:
                IsMouseVisible = true;
                if (_victoryScreen.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) == MenuAction.MainMenu)
                {
                    ResetGame();
                }
                break;
        }

        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        _frameCount++;

        #region Pass 1 - Post-Processing
        bool applyBloodEffect = _enemy.State == EnemyState.Cooldown;
        bool applyGrainEffect = _grainIntensity > 0f;
        bool applyDistortionEffect = _distortionFactor > 0f;

        // Se activa el render para cualquiera de los efectos posibles
        bool usePostProcessing = applyBloodEffect || applyGrainEffect || applyDistortionEffect;

        if (usePostProcessing)
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

        #region Skybox
        // Configuración para el Skybox
        var originalRasterizerState = GraphicsDevice.RasterizerState;
        var rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;

        // Dibujado de skybox
        // Uso el shader de Skybox
        Matrix viewSkyBox = _view;
        viewSkyBox.Translation = Vector3.Zero; // Esto fija el skybox al centro de la camara
        _skyBox.Draw(viewSkyBox, _projection, Vector3.Zero);

        // Restauro el estado de Rasterizer
        GraphicsDevice.RasterizerState = originalRasterizerState;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        #endregion

        // Ya no se fija en VertexBuffer y IndexBuffer sino en la cantidad de habitaciones que existan
        if (_effect == null || _rooms.Count == 0)
            return;

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        // Dibujado para las paredes y habitaciones
        GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            //FillMode = FillMode.WireFrame
        };

        _effect.Parameters["View"]?.SetValue(_view);
        _effect.Parameters["Projection"]?.SetValue(_projection);
        _effect.Parameters["UseVertexColor"]?.SetValue(1.0f);
        _effect.Parameters["DiffuseColor"]?.SetValue(Vector3.One);

        _effect.Parameters["Tiling"]?.SetValue(new Vector2(3.0f, 3.0f));

        LightManager.ApplyLightingToShader(_effect);

        // Tiling en BasicShader para que pueda dibujar correctamente las texturas
        GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

        if (RoomTextureManager.RoomTextures.TryGetValue(RoomType.Outdoor, out var outdoorTex))
        {
            _effect.Parameters["FloorTexture"]?.SetValue(outdoorTex.Floor);
            _effect.Parameters["WallTexture"]?.SetValue(outdoorTex.Wall);
        }

        // Suelo
        if (_groundVertexBuffer != null && _groundIndexBuffer != null)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(_groundVertexBuffer);
            GraphicsDevice.Indices = _groundIndexBuffer;
            // Por bleeding entre habitaciones y suelo
            _effect.Parameters["World"]?.SetValue(Matrix.CreateTranslation(0f, -1f, -3500f));

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
        foreach (var (vertexBuffer, indexBuffer, primitiveCount, worldMatrix, roomType) in _rooms)
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            _effect.Parameters["World"]?.SetValue(worldMatrix);

            if (RoomTextureManager.RoomTextures.TryGetValue(roomType, out var textures))
            {
                _effect.Parameters["WallTexture"]?.SetValue(textures.Wall);
                _effect.Parameters["FloorTexture"]?.SetValue(textures.Floor);
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 
                    0, 
                    0, 
                    primitiveCount
                );
            }
        }

        // Saco el RasterizerState para que dibuje todas las caras de los modelos
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        LightManager.ApplyLightingToShader(_modelsEffect);

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
        if (usePostProcessing)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SetRenderTarget(applyDistortionEffect ? _distortionRenderTarget : null);

            _postProcessEffect.Parameters["baseTexture"]?.SetValue(_sceneRenderTarget);
            _postProcessEffect.Parameters["time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            _postProcessEffect.Parameters["bloodIntensity"]?.SetValue(applyBloodEffect ? _enemy.CooldownIntensity : 0f);
            _postProcessEffect.Parameters["grainIntensity"]?.SetValue(_grainIntensity);

            _fullScreenQuad.Draw(_postProcessEffect);

            if (applyDistortionEffect)
            {
                GraphicsDevice.SetRenderTarget(null); 

                _distortionEffect.Parameters["ScreenTexture"]?.SetValue(_distortionRenderTarget);
                _distortionEffect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
                _distortionEffect.Parameters["BlurFactor"]?.SetValue(_distortionFactor);
                _distortionEffect.Parameters["ScreenResolution"]?.SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

                _fullScreenQuad.Draw(_distortionEffect);

                // Limpio ScreenTexture
                _distortionEffect.Parameters["ScreenTexture"]?.SetValue((Texture2D)null);
                GraphicsDevice.Textures[0] = null;
            }
        }
        #endregion

        #region HUD

        _spriteBatch.Begin();

        float uiScale = GraphicsDevice.Viewport.Height / 720f;

        if (_showingControls)
        {
            _controlsScreen.Draw(_spriteBatch, _spriteFont, _pixelTexture, GraphicsDevice);
        }
        else
        {
            if (_gameState == GameState.Menu)
            {
                // Dibujado de botones del menu
                _menuScreen.Draw(_spriteBatch, _spriteFont, _pixelTexture, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            }
            else if (_gameState == GameState.GameOver)
            {
                _gameOverScreen.Draw(_spriteBatch, _spriteFont, _pixelTexture, GraphicsDevice);
            }
            else if (_gameState == GameState.Victory)
            {
                _victoryScreen.Draw(_spriteBatch, _spriteFont, _pixelTexture, GraphicsDevice, _timeTaken);
            }
            else if (_gameState == GameState.Playing)
            {
                #region Timer
                int minutes = (int)_gameTimer / 60;
                int seconds = (int)_gameTimer % 60;
                string timeText = $"{minutes:D2}:{seconds:D2}";

                // Cuanto mide el texto para poder centrarlo en la pantalla
                Vector2 textSize = _spriteFont.MeasureString(timeText) * uiScale;
                Vector2 textPosition = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2f, 20f * uiScale);

                // Sombra en texto para que se note un poco mas
                _spriteBatch.DrawString(_spriteFont, timeText, textPosition + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                // Dibujamos el texto blanco real, si queda 30 segundos o menos cambio el valor a rojo
                _spriteBatch.DrawString(_spriteFont, timeText, textPosition, _gameTimer <= 30f ? Color.Red : Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                #endregion

                #region Texto de Objetivo

                string objectiveText = string.Empty;
                float textScale = 0.55f;
                // Evaluamos si el jugador ya tiene las 3 llaves para cambiar el texto
                if (_player.CollectedKeys < 3)
                {
                    textScale = 0.55f;
                    objectiveText = "Busca y recolecta todas las llaves en la mansión";
                }
                else
                {
                    textScale = 0.45f;
                    objectiveText = "Busca tu premio en la habitación del fondo de la mansión";
                }

                // Ubicado arriba a la izquierda
                float finalObjScale = textScale * uiScale;
                Vector2 objectivePosition = new Vector2(20f * uiScale, 20f * uiScale);

                // Sombra del texto
                _spriteBatch.DrawString(_spriteFont, objectiveText, objectivePosition + new Vector2(2, 2),
                                        Color.Black, 0f, Vector2.Zero, finalObjScale, SpriteEffects.None, 0f);

                // Texto objetivo
                _spriteBatch.DrawString(_spriteFont, objectiveText, objectivePosition,
                                        Color.White, 0f, Vector2.Zero, finalObjScale, SpriteEffects.None, 0f);
                #endregion

                #region Barra de durabilidad de luces
                if (_player.IsLightActive)
                {
                    float percentage = _player.CurrentLightDurabilityPercentage;

                    int barWidth  = (int)(400 * uiScale);
                    int barHeight = (int) (20 * uiScale);

                    // Centrado horizontalmente y en la parte inferior de la pantalla
                    int xPos = (GraphicsDevice.Viewport.Width - barWidth) / 2;
                    int yPos = GraphicsDevice.Viewport.Height - (int)(60 * uiScale); ;

                    // Fondo gris oscuro para la barra
                    _spriteBatch.Draw(_pixelTexture, new Rectangle(xPos, yPos, barWidth, barHeight), Color.DarkGray);

                    // Con el valor del porcentajo voy actualizando el valor lleno de la barra
                    int fillWidth = (int)(barWidth * percentage);
                    // Blanco para el valor del porcentaje restante
                    _spriteBatch.Draw(_pixelTexture, new Rectangle(xPos, yPos, fillWidth, barHeight), Color.White);
                }
                #endregion

                #region Llaves Recolectadas
                // Texto arriba a la derecha HUD
                string keysText = "Llaves: ";

                Vector2 keysPosition = new Vector2(GraphicsDevice.Viewport.Width - (250f * uiScale), 25f * uiScale);

                _spriteBatch.DrawString(_spriteFont, keysText, keysPosition + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(_spriteFont, keysText, keysPosition, Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                #endregion

                #region FPS
                string fpsText = $"FPS: {_currentFps}";
                Vector2 fpsPosition = new Vector2(20f * uiScale, GraphicsDevice.Viewport.Height - (50f * uiScale));

                // Sombra de text
                _spriteBatch.DrawString(_spriteFont, fpsText, fpsPosition + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

                // Dibujado del texto con validacion para que se ponga en rojo si baja de 30 FPS
                _spriteBatch.DrawString(_spriteFont, fpsText, fpsPosition, _currentFps < 30 ? Color.Red : Color.Yellow, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                #endregion
            }
        }

        _spriteBatch.End();

        // Restauro DepthStencilState tras usar SpriteBatch
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        #region Dibujo los modelos de las llaves
        if (_gameState == GameState.Playing)
        {
            // Limpio la profundidad asi se dibujan las llaves por arriba del resto de modelos
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // Creamos una cámara ortográfica. (1 unidad de espacio = 1 píxel en pantalla)
            Matrix uiProjection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0.1f, 100f);
            Matrix uiView = Matrix.CreateLookAt(new Vector3(0, 0, 10f), Vector3.Zero, Vector3.Up);

            if (_modelCache.TryGetValue("Items/PSX_Item_Key", out var keyModel))
            {
                for (int i = 0; i < 3; i++)
                {
                    // Posicion de dibujo de las llaves
                    float xPos = GraphicsDevice.Viewport.Width - (100f * uiScale) + (45f * uiScale * i);
                    float yPos = 60f * uiScale;  // Altura a la que se dibujan las llaves

                    Matrix keyWorld = Matrix.CreateScale(1f * uiScale) *                                                  // Tamaño del HUD
                                      Matrix.CreateRotationZ(MathHelper.PiOver2) *                              // Rotado en Z para que el modelo se note mas
                                      Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 2f) * // Se giran sobre Y para darle dinamismo al HUD
                                      Matrix.CreateTranslation(xPos, yPos, 0f);

                    // Por defecto tienen el color negro hasta que se recolecta alguna llave pintandolas de dorado
                    Vector3 keyColor = i < _player.CollectedKeys ? Color.Gold.ToVector3() : Color.Black.ToVector3();

                    // Dibujado de las llaves
                    foreach (var mesh in keyModel.Meshes)
                    {
                        foreach (var part in mesh.MeshParts)
                        {
                            var fx = (Effect)part.Effect;
                            fx.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * keyWorld);
                            fx.Parameters["View"]?.SetValue(uiView);
                            fx.Parameters["Projection"]?.SetValue(uiProjection);
                            fx.Parameters["UseVertexColor"]?.SetValue(0.0f);

                            // Las llaves ignoran la linterna
                            fx.Parameters["IsLightActive"]?.SetValue(0.0f);

                            // Forzamos el color silueta negra o dorado y multiplicando x10 para el HUD
                            fx.Parameters["DiffuseColor"]?.SetValue(keyColor * 10f);
                        }
                        mesh.Draw();
                    }
                }
            }
        }
        #endregion

        #endregion

        base.Draw(gameTime);
    }

    private void DrawModelWithCustomEffect(Model model, Matrix world, string modelName)
    {
        Vector3 tint = ModelTintHelper.GetTint(modelName).ToVector3();

        // Modelos que no tengan color definido,
        // sino dibuja por defecto en negro el modelo del auto
        if (tint == Vector3.Zero) tint = Vector3.One;

        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var effect = (Effect)part.Effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * world);
                effect.Parameters["View"]?.SetValue(_view);
                effect.Parameters["Projection"]?.SetValue(_projection);
                effect.Parameters["UseVertexColor"]?.SetValue(1.0f);
                effect.Parameters["DiffuseColor"]?.SetValue(tint);

                LightManager.ApplyLightingToShader(effect);
            }

            mesh.Draw();
        }
    }

    private void DrawModelOutline(Model model, Matrix world, string name)
    {
        // Modifico Rasterizer para que solo dibuje las caras internas del modelo
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace };
        // Diferencio el escalado del modelo para que se note mas el borde para match box y lock
        float outlineScale = name.Contains("Match_Box") || name.Contains("Lock") ? 1.20f : 1.03f;

        Matrix outlineWorld = Matrix.CreateScale(outlineScale) * world;

        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var effect = (Effect)part.Effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * outlineWorld);
                effect.Parameters["View"]?.SetValue(_view);
                effect.Parameters["Projection"]?.SetValue(_projection);
                effect.Parameters["UseVertexColor"]?.SetValue(0.0f);
                // Yellow cuando tenga los shaders
                effect.Parameters["DiffuseColor"]?.SetValue(Color.Azure.ToVector3());
            }

            mesh.Draw();
        }

        // Restauro el estado de Rasterizer para dibujar correctamente todo el resto
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
    }

    private void LoadLevel()
    {
        // Limpiamos todo antes de volver a llenar
        _models.Clear();
        _rooms.Clear();
        _trees.Clear();
        _modelCache.Clear(); // Es importante limpiar el cache para forzar la recarga

        // Delegamos TODA la generacion del nivel al Helper
        LevelGeneratorHelper.GenerateLevel(
            GraphicsDevice,
            Content,
            _modelsEffect,
            _player.Position,
            _modelCache,
            _rooms,
            _models,
            _trees,
            out _groundVertexBuffer,
            out _groundIndexBuffer,
            out _groundPrimitiveCount
        );
    }

    private void ResetGame()
    {
        // Reiniciamos variables de juego
        _gameTimer = 300f;
        _isGameOver = false;
        _transitionProgress = 0f;
        _player.ResetStats();

        // Llamamos a la carga del nivel
        LoadLevel();

        _gameState = GameState.Menu;
        MediaPlayer.Play(_menuMusic);
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