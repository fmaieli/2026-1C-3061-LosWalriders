using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives;
using TGC.MonoGame.TP.SourceCode.Factories;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class LevelGeneratorHelper
    {
        // Lista con todas las colisiones posibles, usada para que el jugador y el enemigo no traspasen paredes
        public static List<BoundingBox> WallColliders { get; } = new List<BoundingBox>();

        // Lista con los centros de las habitaciones para que reaparezca el enemigo
        public static List<Vector3> ValidSpawnPoints { get; } = new List<Vector3>();

        /// <summary>
        /// Reemplaza el material por defecto de los modelos con un material personalizado
        /// </summary>
        public static void ApplyCustomEffectToModel(Model model, Effect effectTemplate)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effectTemplate.Clone();
                }
            }
        }

        // Se mueve la logica de los pasillos a HallwayGeneratorHelper ya se complejizo
        public static WallOpening DetermineRoomOpening(char roomCell, char neighborCell, float offset = 0f)
        {
            if (roomCell == 'E' && (neighborCell == ' ' || neighborCell == '\0'))
                return WallOpening.Door(40f, 80f, offset);

            if (neighborCell == ' ' ||
                neighborCell == '\0' ||
                neighborCell == 'X')
                return WallOpening.Solid();

            return WallOpening.Door(40f, 80f, offset);
        }        

        // Se pintan las paredes de las habitacion con Front-Back y Left-Right
        /// <summary>
        /// A partir de los valores del parametro 'cell' se ve que tipo de habitacion corresponde crear
        /// y cuales seran los colores para las paredes
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static (RoomType Type, Color FrontBack, Color LeftRight)? GetRoomData(char cell)
        {
            return cell switch
            {
                'E' => (RoomType.Entrance, Color.SkyBlue, Color.LightBlue),          // Entrance
                'Z' => (RoomType.Prize, Color.Gold, Color.DarkGoldenrod),            // Exit (ahora es Z porque se creo la referencia de Entrance)
                'H' => (RoomType.Hallway, Color.Yellow, Color.LightGoldenrodYellow),
                'V' => (RoomType.Hallway, Color.DarkGoldenrod, Color.Goldenrod),     // Vents
                'C' => (RoomType.Computer, Color.Gray, Color.DarkGray),
                'O' => (RoomType.Outdoor, Color.DarkGreen, Color.ForestGreen),
                'B' => (RoomType.Bed, Color.DarkOrange, Color.Orange),
                'L' => (RoomType.Living, Color.SaddleBrown, Color.Peru),
                'K' => (RoomType.Kitchen, Color.LightSalmon, Color.PeachPuff),
                'A' => (RoomType.Bath, Color.MediumBlue, Color.CornflowerBlue),      // Baños (se usa A porque la B la usa la habitacion Bed)
                _ => null
            };
        }

        public static void GenerateLevel(GraphicsDevice graphicsDevice, ContentManager content, Effect effect, Vector3 cameraPosition,
            Dictionary<string, Model> modelCache, List<(VertexBuffer, IndexBuffer, int, Matrix)> rooms,
            List<(Model, Matrix, string)> models, List<(Model, Matrix, string)> trees, out VertexBuffer groundVertexBuffer, out IndexBuffer groundIndexBuffer,
            out int groundPrimitiveCount)
        {
            // Elimino las colisiones anteriores por precaucion
            WallColliders.Clear();
            ValidSpawnPoints.Clear();

            #region Carga de modelos
            // Puerta normal
            string normalDoorPath = "Items/PSX_Door";
            if (!modelCache.TryGetValue(normalDoorPath, out var normalDoorModel))
            {
                normalDoorModel = content.Load<Model>(TGCGame.ContentFolder3D + normalDoorPath);
                ApplyCustomEffectToModel(normalDoorModel, effect);
                modelCache[normalDoorPath] = normalDoorModel;
            }

            // Puerta para el PrizeRoom
            string prizeDoorPath = "Items/PSX_Item_Door";
            if (!modelCache.TryGetValue(prizeDoorPath, out var prizeDoorModel))
            {
                prizeDoorModel = content.Load<Model>(TGCGame.ContentFolder3D + prizeDoorPath);
                ApplyCustomEffectToModel(prizeDoorModel, effect);
                modelCache[prizeDoorPath] = prizeDoorModel;
            }

            // Candado bloqueado
            string lockPath = "Items/PSX_Item_Lock_Locked";
            if (!modelCache.TryGetValue(lockPath, out var lockModel))
            {
                lockModel = content.Load<Model>(TGCGame.ContentFolder3D + lockPath);
                ApplyCustomEffectToModel(lockModel, effect);
                modelCache[lockPath] = lockModel;
            }
            #endregion

            // Tomo el modelo en caso de ya estar en modelCache y si existe lo extraigo en doorModel
            if (!modelCache.TryGetValue(normalDoorPath, out var doorModel))
            {
                // Se carga la puerta
                doorModel = content.Load<Model>(TGCGame.ContentFolder3D + normalDoorPath);
                ApplyCustomEffectToModel(doorModel, effect);

                // Guardo el modelo en modelCache
                modelCache[normalDoorPath] = doorModel;
            }

            // Lista de puertas creadas
            var placedDoors = new HashSet<string>();

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

            float startWorldX = cameraPosition.X - (playerGridX * (baseRoomWidth * 2f + roomGap));
            float startWorldZ = cameraPosition.Z - (playerGridZ * (baseRoomDepth * 2f + roomGap));

            var rng = new Random();

            // Control para las celdas ya procesadas
            bool[,] processedCell = new bool[rows, cols];
            var doorRegistry = new Dictionary<(int, int), List<HallwayDirection>>(); // Lista con las coordenadas donde se encuentran las puertas
            var occupiedAreas = new List<BoundingBox>();
            var keySpawnPoints = new List<Vector3>();

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (processedCell[z, x]) continue;  // Si ya fue procesada se saltea

                    char currentCell = mapLayout[z][x]; // Cell a procesar

                    if (currentCell == ' ' || currentCell == 'P' || currentCell == 'X' ||
                        HallwayGeneratorHelper.IsHallway(currentCell))
                    {
                        if (!HallwayGeneratorHelper.IsHallway(currentCell))
                        {
                            processedCell[z, x] = true;
                        }                            
                        continue;
                    }

                    var roomData = GetRoomData(currentCell);
                    if (roomData == null) continue;

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

                    // Guardo el centro como un valor valido de spawn para el enemigo
                    ValidSpawnPoints.Add(new Vector3(mergedWorldX, 0f, mergedWorldZ));

                    // Spawn para las llaves, que no sean del tipo Entrance
                    if (roomData.Value.Type != RoomType.Entrance)
                    {
                        keySpawnPoints.Add(new Vector3(mergedWorldX, 0f, mergedWorldZ));
                    }

                    occupiedAreas.Add(new BoundingBox(
                        new Vector3(mergedWorldX - mergedWidthHalf, -10f, mergedWorldZ - mergedDepthHalf),
                        new Vector3(mergedWorldX + mergedWidthHalf, 200f, mergedWorldZ + mergedDepthHalf)
                    ));

                    // Para determinar las puertas, miramos al vecino que cae justo en el medio de las paredes fusionadas
                    char frontNeighbor = (z + heightCells < rows) ? mapLayout[z + heightCells][x + widthCells / 2] : ' ';
                    char backNeighbor = (z > 0) ? mapLayout[z - 1][x + widthCells / 2] : ' ';
                    char leftNeighbor = (x > 0) ? mapLayout[z + heightCells / 2][x - 1] : ' ';
                    char rightNeighbor = (x + widthCells < cols) ? mapLayout[z + heightCells / 2][x + widthCells] : ' ';

                    // Calculo el centro de la pared donde estara la puerta, lo multiplico por cellStep para poder tener las mismas unidades
                    float doorOffsetX = ((x + widthCells / 2) - (x + (widthCells - 1) / 2f)) * cellStepX;
                    float doorOffsetZ = ((z + heightCells / 2) - (z + (heightCells - 1) / 2f)) * cellStepZ;

                    // Le paso la celda actual, el vecino y el offset de la puerta
                    var frontOpening = DetermineRoomOpening(currentCell, frontNeighbor, doorOffsetX);
                    var backOpening = DetermineRoomOpening(currentCell, backNeighbor, -doorOffsetX);
                    var leftOpening = DetermineRoomOpening(currentCell, leftNeighbor, doorOffsetZ);
                    var rightOpening = DetermineRoomOpening(currentCell, rightNeighbor, -doorOffsetZ);

                    // Se generan primero las habitaciones y luego los pasillos
                    // Una vez se tiene cuales son las puertas de la habitaciones generades se genera luego las puertas en los pasillos
                    // Registro las puertas y pongo el valor contrario para el pasillo ya que seria espejado
                    if (frontOpening.Type == WallType.Door && HallwayGeneratorHelper.IsHallway(frontNeighbor))
                        RegisterDoor(doorRegistry, z + heightCells, x + widthCells / 2, HallwayDirection.Back);

                    // Pared Trasera
                    if (backOpening.Type == WallType.Door && HallwayGeneratorHelper.IsHallway(backNeighbor))
                        RegisterDoor(doorRegistry, z - 1, x + widthCells / 2, HallwayDirection.Front);

                    // Pared Izquierda
                    if (leftOpening.Type == WallType.Door && HallwayGeneratorHelper.IsHallway(leftNeighbor))
                        RegisterDoor(doorRegistry, z + heightCells / 2, x - 1, HallwayDirection.Right);

                    // Pared Derecha
                    if (rightOpening.Type == WallType.Door && HallwayGeneratorHelper.IsHallway(rightNeighbor))
                        RegisterDoor(doorRegistry, z + heightCells / 2, x + widthCells, HallwayDirection.Left);

                    // Con toda la informacion, creo las colisiones para las paredes
                    AddWallColliders(new Vector3(mergedWorldX, 0, mergedWorldZ), mergedWidthHalf, mergedDepthHalf, roomHeight,
                                     frontOpening, backOpening, leftOpening, rightOpening);

                    // Creo la habitacion
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

                    var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), mesh.Vertices.Length, BufferUsage.WriteOnly);
                    vertexBuffer.SetData(mesh.Vertices);

                    var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, mesh.Indices.Length, BufferUsage.WriteOnly);
                    indexBuffer.SetData(mesh.Indices);

                    int primCount = mesh.Indices.Length / 3;
                    var roomWorld = Matrix.CreateTranslation(mergedWorldX, 0f, mergedWorldZ);
                    rooms.Add((vertexBuffer, indexBuffer, primCount, roomWorld));

                    // Poner la puerta dependiendo del tipo de habitacion (se hace para poner la puerta particular para PrizeRoom)
                    string currentDoorPath = normalDoorPath;
                    Model currentDoorModel = normalDoorModel;

                    // Variables para los candados
                    Model currentLockModel = null;
                    string currentLockPath = null;

                    // Se cambia el modelo si es PrizeRoom y asignamos los candados
                    if (roomData.Value.Type == RoomType.Prize)
                    {
                        currentDoorPath = prizeDoorPath;
                        currentDoorModel = prizeDoorModel;
                        currentLockModel = lockModel;
                        currentLockPath = lockPath;
                    }

                    // Pared Frontal
                    PlaceDoorModel(frontOpening, new Vector3(doorOffsetX, 0, mergedDepthHalf), 0f,
                        z + heightCells - 1, x + widthCells / 2, z + heightCells, x + widthCells / 2,
                        placedDoors, mergedWorldX, mergedWorldZ, currentDoorModel, normalDoorPath, models, currentLockModel, currentLockPath);

                    // Pared Trasera
                    PlaceDoorModel(backOpening, new Vector3(doorOffsetX, 0, -mergedDepthHalf), MathHelper.Pi,
                        z, x + widthCells / 2, z - 1, x + widthCells / 2,
                        placedDoors, mergedWorldX, mergedWorldZ, currentDoorModel, normalDoorPath, models, currentLockModel, currentLockPath);

                    // Pared Izquierda
                    PlaceDoorModel(leftOpening, new Vector3(-mergedWidthHalf, 0, doorOffsetZ), -MathHelper.PiOver2,
                        z + heightCells / 2, x, z + heightCells / 2, x - 1,
                        placedDoors, mergedWorldX, mergedWorldZ, currentDoorModel, normalDoorPath, models, currentLockModel, currentLockPath);

                    // Pared Derecha
                    PlaceDoorModel(rightOpening, new Vector3(mergedWidthHalf, 0, doorOffsetZ), MathHelper.PiOver2,
                        z + heightCells / 2, x + widthCells - 1, z + heightCells / 2, x + widthCells,
                        placedDoors, mergedWorldX, mergedWorldZ, currentDoorModel, normalDoorPath, models, currentLockModel, currentLockPath );

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

                        foreach (var (modelPath, localPos, rotationY) in placements)
                        {
                            // Utilizo el diccionario para poder reutilizar elementos que ya se hayan guardado
                            if (!modelCache.TryGetValue(modelPath, out var model))
                            {
                                model = content.Load<Model>(TGCGame.ContentFolder3D + modelPath);
                                ApplyCustomEffectToModel(model, effect);
                                modelCache[modelPath] = model;
                            }

                            // Posicion final de los modelos / Se achican a la mitad los modelos porque muchos son realmente grandes
                            Matrix modelWorld = Matrix.CreateScale(0.5f) * Matrix.CreateRotationY(rotationY) * Matrix.CreateTranslation(roomWorld.Translation + localPos);
                            models.Add((model, modelWorld, modelPath));
                        }
                    }
                }
            }

            #region Generacion de llaves - Random en las habitaciones
            string keyPath = "Items/PSX_Item_Key";
            if (!modelCache.TryGetValue(keyPath, out var keyModel))
            {
                keyModel = content.Load<Model>(TGCGame.ContentFolder3D + keyPath);
                ApplyCustomEffectToModel(keyModel, effect);
                modelCache[keyPath] = keyModel;
            }

            for (int i = 0; i < 3; i++)
            {
                // Se usa la lista de spawn de las llaves
                if (keySpawnPoints.Count > 0)
                {
                    // Habitacion aleatoria
                    int randomIndex = rng.Next(keySpawnPoints.Count);
                    Vector3 spawnPoint = keySpawnPoints[randomIndex];

                    // Se elimina la habitacion para que no puedan aparecer 2 llaves en el mismo lugar
                    keySpawnPoints.RemoveAt(randomIndex);

                    // Posicion de la llave flotando
                    Vector3 keyPos = new Vector3(spawnPoint.X, 35f, spawnPoint.Z);
                    Matrix keyWorld = Matrix.CreateScale(0.5f) * Matrix.CreateRotationY((float)rng.NextDouble() * MathHelper.TwoPi) * Matrix.CreateTranslation(keyPos);

                    models.Add((keyModel, keyWorld, keyPath));
                }
            }
            #endregion

            // Terminada la generacion de las habitaciones genero los pasillos
            HallwayGeneratorHelper.GenerateHallways(
                mapLayout, doorRegistry, graphicsDevice, content, effect,
                startWorldX, startWorldZ, baseRoomWidth, baseRoomDepth, roomHeight, roomGap, cellSize,
                rooms, models, modelCache, rng, occupiedAreas
            );

            // Cols y Rows es el length de mapLayout
            float gridWidth = cols * (baseRoomWidth * 2f + roomGap);
            float gridDepth = rows * (baseRoomDepth * 2f + roomGap);

            // Gran tamaño para el suelo
            float groundMargin = 4000f;
            var groundResult = GroundBuilderHelper.Create(
                    graphicsDevice,
                    gridWidth * 0.5f + groundMargin,
                    gridDepth * 0.5f + groundMargin,
                    Color.Brown);

            groundVertexBuffer = groundResult.GroundVertexBuffer;
            groundIndexBuffer = groundResult.GroundIndexBuffer;
            groundPrimitiveCount = groundResult.PrimitiveCount;

            // Generacion de arboles, con la cantidad a generar y la escala del modelo
            int treeCount = 300;
            float treeScale = 0.1f;

            var treeModel = content.Load<Model>("Models/World/PSX_Low_Poly_Tree");
            ApplyCustomEffectToModel(treeModel, effect);

            int placedTrees = 0;
            int attempts = 0;
            while (placedTrees < treeCount && attempts < treeCount * 10)
            {
                attempts++;
                // Valor entre 0 y 1 para generar las distancias de los arboles
                float x = (float)(rng.NextDouble() * (gridWidth + groundMargin * 2) - (gridWidth * 0.5f + groundMargin));
                float z = (float)(rng.NextDouble() * (gridDepth + groundMargin * 2) - (gridDepth * 0.5f + groundMargin));

                var treeBox = new BoundingBox(new Vector3(x - 5f, -10f, z - 5f), new Vector3(x + 5f, 200f, z + 5f));
                bool collides = false;
                // Verifico con las habitaciones y pasillos ya generados para no poner los arboles dentro
                foreach (var box in occupiedAreas)
                {
                    if (box.Intersects(treeBox)) 
                    { 
                        collides = true; 
                        break; 
                    }
                }

                if (!collides)
                {
                    // Randomizo la rotacion en Y y con valores aleatorios entre 0 y 1
                    float rotY = MathHelper.ToRadians((float)rng.NextDouble() * 360f);
                    var world = Matrix.CreateScale(treeScale) * Matrix.CreateRotationY(rotY) * Matrix.CreateTranslation(x, -1f, z);
                    trees.Add((treeModel, world, "World/PSX_Low_Poly_Tree"));
                    placedTrees++;
                }
            }
        }

        private static void RegisterDoor(Dictionary<(int, int), List<HallwayDirection>> registry, int z, int x, HallwayDirection direction)
        {
            if (!registry.ContainsKey((z, x)))
                registry[(z, x)] = new List<HallwayDirection>();

            if (!registry[(z, x)].Contains(direction))
                registry[(z, x)].Add(direction);
        }

        /// <summary>
        /// Coloca el modelo 3D de una puerta en el mundo. 
        /// Se debe evitar colocar dos puertas en el mismo marco cuando dos habitaciones se conectan.
        /// </summary>
        private static void PlaceDoorModel(
            WallOpening opening,
            Vector3 localPos,
            float rotY,
            int zFirstRoom, int xFirstRoom, int zSecondRoom, int xSecondRoom, // Coordenadas de las dos habitaciones que se conectan
            HashSet<string> placedDoors,            // Registro de puertas ya colocadas para evitar duplicados
            float mergedWorldX,                     // Posicion del centro X de la habitacion en el mundo
            float mergedWorldZ,                     // Posición del centro Z de la habitacion en el mundo
            Model doorModel,                        // Modelo de la puerta
            string doorPath,                        // Path del modelo
            List<(Model, Matrix, string)> models,   // La lista de modelos a renderizar
            Model lockModel = null,                 // Modelo candado
            string lockPath = null)                 // Path candado
        {
            // Si no es una puerta, no se hace nada
            if (opening.Type != WallType.Door) return;

            // Creo un key con las coordenadas de las habitaciones para corroborar el valor y de esta forma no generar un nuevo modelo donde ya exista uno
            string key = $"{
                Math.Min(zFirstRoom, zSecondRoom)}_{Math.Min(xFirstRoom, xSecondRoom)}_{Math.Max(zFirstRoom, zSecondRoom)}_{Math.Max(xFirstRoom, xSecondRoom)
            }";

            // Si la key ya existe entonces, ya se genero el modelo anteriormente, no se hace nada
            if (placedDoors.Contains(key)) return;

            // Si no existia antes, la agrego al registro de puertas para que no se vuelva a crear el modelo
            placedDoors.Add(key);

            Vector3 modelOffset = new Vector3(0f, 0f, 0f);
            Vector3 worldPos = new Vector3(mergedWorldX, 0f, mergedWorldZ) + localPos;

            Matrix doorWorld =
                Matrix.CreateScale(0.4f) *      // Escalo el modelo para que entre en las aberturas de las puertas correctamente
                Matrix.CreateRotationY(rotY) *  // Roto la puerta para que encaje en la pared
                Matrix.CreateTranslation(worldPos + Vector3.Transform(modelOffset, Matrix.CreateRotationY(rotY)));  // Posicion final con el offset rotado

            // Agrero el modelo de la puerta a models para renderizarlo con el resto de modelos
            models.Add((doorModel, doorWorld, doorPath));

            // Candados uno encima del otro
            if (lockModel != null)
            {
                // Donde se encuentra la derecha del modelo de la puerta
                Vector3 rightDir = Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(rotY));

                // Posicion desde el centro de la puerta
                Vector3 lockBasePos = doorWorld.Translation + (rightDir * 45f) + new Vector3(0, 40f, 0);

                for (int i = 0; i < 3; i++)
                {
                    // Cada candado se dibuja 15 unidades mas arriba que el anterior
                    Matrix lockWorld = 
                        Matrix.CreateScale(0.06f) * 
                        Matrix.CreateRotationY(rotY + MathHelper.PiOver2) * 
                        Matrix.CreateTranslation(lockBasePos + new Vector3(0, 30f * i, 0));
                    models.Add((lockModel, lockWorld, lockPath));
                }
            }
        }

        /// <summary>
        /// Construye colisiones las paredes de una habitación o pasillo.
        /// </summary>
        public static void AddWallColliders(Vector3 center, float halfWidth, float halfDepth, float height,
            WallOpening front, WallOpening back, WallOpening left, WallOpening right)
        {
            // Grosor de la pared de colision
            float thick = 2f;

            // Pared Frontal: corre a lo largo del eje X (isAlignedX = true). Offset normal.
            GenerateAABB(center, new Vector3(center.X, 0, center.Z + halfDepth), true, halfWidth, height, front, thick, false);

            // Pared Trasera: corre a lo largo del eje X. 
            // invertOffset = true porque la pared esta en el extremo negativo de Z, espejando la vista.
            GenerateAABB(center, new Vector3(center.X, 0, center.Z - halfDepth), true, halfWidth, height, back, thick, true);

            // Pared Izquierda: corre a lo largo del eje Z (isAlignedX = false). Offset normal.
            GenerateAABB(center, new Vector3(center.X - halfWidth, 0, center.Z), false, halfDepth, height, left, thick, false);

            // Pared Derecha: corre a lo largo del eje Z.
            // invertOffset = true porque esta en el extremo positivo de X, espejando la vista respecto a la izquierda.
            GenerateAABB(center, new Vector3(center.X + halfWidth, 0, center.Z), false, halfDepth, height, right, thick, true);
        }

        /// <summary>
        /// Genera las cajas de colisiones AABB para una pared.
        /// Se divide en partes si tiene una puerta o ventana.
        /// </summary>
        private static void GenerateAABB(Vector3 roomCenter, Vector3 wallCenter, bool isAlignedX, float halfLength, float height, WallOpening opening, float thick, bool invertOffset)
        {
            // Si es Empty no se genera colision
            if (opening.Type == WallType.Empty) return;

            // Si es Solid, creo una caja grande para la colision de la pared
            if (opening.Type == WallType.Solid)
            {
                if (isAlignedX) // Pared horizontal (eje X)
                    WallColliders.Add(
                        new BoundingBox(
                            new Vector3(wallCenter.X - halfLength, 0, wallCenter.Z - thick), 
                            new Vector3(wallCenter.X + halfLength, height, wallCenter.Z + thick)
                        )
                    );
                else            // Pared vertical   (eje Z)
                    WallColliders.Add(
                        new BoundingBox(
                            new Vector3(wallCenter.X - thick, 0, wallCenter.Z - halfLength), 
                            new Vector3(wallCenter.X + thick, height, wallCenter.Z + halfLength)
                        )
                    );
                return; // Terminamos aquí para esta pared.
            }

            // Paredes con puertas
            // El agujero de la puerta no debe de ser mas grande que la pared
            float holeWidth = MathHelper.Clamp(opening.Width, 1f, halfLength * 2f - 1f);
            float holeHeight = MathHelper.Clamp(opening.Height, 1f, height - 1f);

            // Si es la pared de atrás o derecha, invertimos el offset para que la física (invisible) 
            // coincida exactamente con el hueco del modelo 3D (visible).
            float offset = invertOffset ? -opening.Offset : opening.Offset;

            // Con respecto al centro me fijo donde arranca y termina el hueco de la puerta
            float holeMin = offset - holeWidth * 0.5f;
            float holeMax = offset + holeWidth * 0.5f;

            // Colision a la izquierda de la puerta
            // Se crea si el hueco no empieza pegado al borde izquierdo de la pared
            if (holeMin > -halfLength)
            {
                if (isAlignedX)
                {
                    WallColliders.Add(
                        new BoundingBox(new Vector3(
                            wallCenter.X - halfLength, 0, wallCenter.Z - thick), 
                            new Vector3(wallCenter.X + holeMin, height, wallCenter.Z + thick)
                        )
                    );
                }                    
                else
                {
                    WallColliders.Add(
                        new BoundingBox(
                            new Vector3(wallCenter.X - thick, 0, wallCenter.Z - halfLength), 
                            new Vector3(wallCenter.X + thick, height, wallCenter.Z + holeMin)
                        )
                    );
                }                    
            }

            // Colision a la derecha de la puerta
            // Se crea si el hueco no empieza pegado al borde derecho de la pared
            if (holeMax < halfLength)
            {
                if (isAlignedX)
                {
                    WallColliders.Add
                        (new BoundingBox(
                            new Vector3(wallCenter.X + holeMax, 0, wallCenter.Z - thick), 
                            new Vector3(wallCenter.X + halfLength, height, wallCenter.Z + thick)
                        )
                    );
                }
                else
                {
                    WallColliders.Add(
                        new BoundingBox(
                            new Vector3(wallCenter.X - thick, 0, wallCenter.Z + holeMax), 
                            new Vector3(wallCenter.X + thick, height, wallCenter.Z + halfLength)
                        )
                    );
                }
            }

            // No creo una colision para la parte superior de la puerta para que no provoque problemas al pasar por la misma
        }
    }
}