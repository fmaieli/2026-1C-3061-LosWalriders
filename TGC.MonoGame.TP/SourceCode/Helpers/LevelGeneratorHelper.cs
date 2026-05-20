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

        public static WallOpening DetermineOpening(char currentCell, char neighborCell)
        {
            // TODO: REVISAR LOGICA PARA DETERMINAR SI ES UN PASILLO PARA NO DIBUJAR LAS PAREDES,
            // NO ESTA FUNCIONANDO CORRECTAMENTE
            bool currentIsHallway = (currentCell == 'H' || currentCell == 'V');
            bool neighborIsHallway = (neighborCell == 'H' || neighborCell == 'V');

            if (currentIsHallway && neighborIsHallway)
                return WallOpening.Empty();

            if (currentCell == 'E' && (neighborCell == ' ' || neighborCell == '\0'))
                return WallOpening.Door(40f, 80f);

            if (neighborCell == ' ' || neighborCell == '\0')
                return WallOpening.Solid();

            return WallOpening.Door(40f, 80f);
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
                'Z' => (RoomType.Outdoor, Color.Red, Color.DarkRed),                 // Exit (ahora es Z porque se creo la referencia de Entrance)
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

                    var roomData = GetRoomData(currentCell);
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

                    var frontOpening = DetermineOpening(currentCell, frontNeighbor);
                    var backOpening = DetermineOpening(currentCell, backNeighbor);
                    var leftOpening = DetermineOpening(currentCell, leftNeighbor);
                    var rightOpening = DetermineOpening(currentCell, rightNeighbor);

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
                            if (!modelCache.TryGetValue(modelPath, out var model))
                            {
                                model = content.Load<Model>(TGCGame.ContentFolder3D + modelPath);
                                ApplyCustomEffectToModel(model, effect);
                                modelCache[modelPath] = model;
                            }

                            // Posicion final de los modelos / Se achican a la mitad los modelos porque muchos son realmente grandes
                            var modelWorld = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(roomWorld.Translation + localPos);
                            models.Add((model, modelWorld, modelPath));
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
            var groundResult = GroundBuilderHelper.Create(
                    graphicsDevice,
                    gridWidth * 0.5f + groundMargin,
                    gridDepth * 0.5f + groundMargin,
                    Color.Brown);

            groundVertexBuffer = groundResult.GroundVertexBuffer;
            groundIndexBuffer = groundResult.GroundIndexBuffer;
            groundPrimitiveCount = groundResult.PrimitiveCount;

            // Cantidad de arboles y escala
            int treeCount = 300;
            float treeScale = 0.1f;

            var treeModel = content.Load<Model>("Models/World/PSX_Low_Poly_Tree");
            ApplyCustomEffectToModel(treeModel, effect);

            for (int i = 0; i < treeCount; i++)
            {
                // Valor entre 0 y 1 para generar las distancias de los arboles
                float x = (float)(rng.NextDouble() * (gridWidth + groundMargin * 2) - (gridWidth * 0.5f + groundMargin));
                float z = (float)(rng.NextDouble() * (gridDepth + groundMargin * 2) - (gridDepth * 0.5f + groundMargin));

                // Randomizo la rotacion en Y y con valores aleatorios entre 0 y 1
                float rotY = MathHelper.ToRadians((float)rng.NextDouble() * 360f);

                var world = Matrix.CreateScale(treeScale) * Matrix.CreateRotationY(rotY) *
                    Matrix.CreateTranslation(x, -1f, z); // Mismo nivel que el suelo -> revisar Draw para el suelo

                trees.Add((treeModel, world, "World/PSX_Low_Poly_Tree"));
            }
        }
    }
}