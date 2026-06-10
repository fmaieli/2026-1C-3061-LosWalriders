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
    public static class HallwayGeneratorHelper
    {
        public static List<(VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int PrimitiveCount, Matrix World)> HallwayRooms { get; } = new();
        public static bool IsHallway(char c) => c == 'H' || c == 'V';

        private static WallOpening GetBoundary(int z, int x, HallwayDirection direction, string[] map, Dictionary<(int, int), List<HallwayDirection>> doorRegistry)
        {
            int rows = map.Length;
            int cols = map[0].Length;

            int nz = z, nx = x;
            switch (direction)
            {
                case HallwayDirection.Front: nz++; break;
                case HallwayDirection.Back: nz--; break;
                case HallwayDirection.Left: nx--; break;
                case HallwayDirection.Right: nx++; break;
            }

            if (nz < 0 || nz >= rows || nx < 0 || nx >= cols)
                return WallOpening.Solid();

            char neighbor = map[nz][nx];

            if (IsHallway(neighbor))
                return WallOpening.Empty();

            if (neighbor == ' ' || neighbor == 'Z' || neighbor == 'X')
                return WallOpening.Solid();

            if (doorRegistry.ContainsKey((z, x)) && doorRegistry[(z, x)].Contains(direction))
                return WallOpening.Door(40f, 80f);

            return WallOpening.Solid();
        }

        public static void GenerateHallways(string[] mapLayout, Dictionary<(int, int), List<HallwayDirection>> doorRegistry, GraphicsDevice graphicsDevice,
            ContentManager content,Effect effect, float startWorldX, float startWorldZ,float baseRoomWidth, float baseRoomDepth, float roomHeight, float roomGap, float cellSize,
            List<(VertexBuffer, IndexBuffer, int, Matrix, RoomType)> rooms, List<(Model, Matrix, string)> models, Dictionary<string, Model> modelCache, Random rng, List<BoundingBox> occupiedAreas)
        {
            int rows = mapLayout.Length;
            int cols = mapLayout[0].Length;
            bool[,] visited = new bool[rows, cols];

            // Recorro el mapa y en caso de que ya haya sido procesado (visited) sigo
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (visited[z, x] || !IsHallway(mapLayout[z][x])) continue;

                    char currentCell = mapLayout[z][x];
                    var roomData = LevelGeneratorHelper.GetRoomData(currentCell);

                    var frontBoundary = GetBoundary(z, x, HallwayDirection.Front, mapLayout, doorRegistry);
                    var backBoundary = GetBoundary(z, x, HallwayDirection.Back, mapLayout, doorRegistry);
                    var leftBoundary = GetBoundary(z, x, HallwayDirection.Left, mapLayout, doorRegistry);
                    var rightBoundary = GetBoundary(z, x, HallwayDirection.Right, mapLayout, doorRegistry);

                    int widthCells = 1;
                    int heightCells = 1;

                    // Mezcla los pasillos a partir del mapa
                    // si me encuentro con varias H seguidas compruebo si se encuentra a lo adelante o atras para expandir el pasillo en esa direccion
                    while (x + widthCells < cols && mapLayout[z][x + widthCells] == currentCell && !visited[z, x + widthCells])
                    {
                        var nextFront = GetBoundary(z, x + widthCells, HallwayDirection.Front, mapLayout, doorRegistry);
                        var nextBack = GetBoundary(z, x + widthCells, HallwayDirection.Back, mapLayout, doorRegistry);

                        if (nextFront.Type != frontBoundary.Type || nextBack.Type != backBoundary.Type)
                            break;

                        widthCells++;
                    }

                    // Compruebo con izquierda y derecha
                    if (widthCells == 1)
                    {
                        while (z + heightCells < rows && mapLayout[z + heightCells][x] == currentCell && !visited[z + heightCells, x])
                        {
                            var nextLeft = GetBoundary(z + heightCells, x, HallwayDirection.Left, mapLayout, doorRegistry);
                            var nextRight = GetBoundary(z + heightCells, x, HallwayDirection.Right, mapLayout, doorRegistry);

                            if (nextLeft.Type != leftBoundary.Type || nextRight.Type != rightBoundary.Type)
                                break;

                            heightCells++;
                        }
                    }

                    for (int hz = 0; hz < heightCells; hz++)
                        for (int wx = 0; wx < widthCells; wx++)
                            visited[z + hz, x + wx] = true;

                    float cellStepX = baseRoomWidth * 2f + roomGap;
                    float cellStepZ = baseRoomDepth * 2f + roomGap;

                    // "Pego" las paredes de los pasillos
                    float mergedWidthHalf = baseRoomWidth + roomGap / 2f + (widthCells - 1) * (cellStepX / 2f);
                    float mergedDepthHalf = baseRoomDepth + roomGap / 2f + (heightCells - 1) * (cellStepZ / 2f);

                    float mergedWorldX = startWorldX + (x + (widthCells - 1) / 2f) * cellStepX;
                    float mergedWorldZ = startWorldZ + (z + (heightCells - 1) / 2f) * cellStepZ;

                    // Registro todos los lugares donde hayan pasillos para evitar que se dibujen arboles (logica dentro de LevelGeneratorHelper)
                    occupiedAreas.Add(new BoundingBox(
                        new Vector3(mergedWorldX - mergedWidthHalf, -10f, mergedWorldZ - mergedDepthHalf),
                        new Vector3(mergedWorldX + mergedWidthHalf, 200f, mergedWorldZ + mergedDepthHalf)
                    ));

                    var finalFront = frontBoundary;
                    var finalBack = backBoundary;
                    var finalLeft = GetBoundary(z, x, HallwayDirection.Left, mapLayout, doorRegistry);
                    var finalRight = GetBoundary(z, x + widthCells - 1, HallwayDirection.Right, mapLayout, doorRegistry);

                    if (heightCells > 1)
                    {
                        finalLeft = leftBoundary;
                        finalRight = rightBoundary;
                        finalFront = GetBoundary(z + heightCells - 1, x, HallwayDirection.Front, mapLayout, doorRegistry);
                        finalBack = GetBoundary(z, x, HallwayDirection.Back, mapLayout, doorRegistry);
                    }

                    // Agrego colisiones para las paredes de los pasillos
                    LevelGeneratorHelper.AddWallColliders(new Vector3(mergedWorldX, 0, mergedWorldZ),
                        mergedWidthHalf, mergedDepthHalf, roomHeight, finalFront, finalBack, finalLeft, finalRight
                    );

                    // Creo el pasillo con todo lo calculado utilizando la clase Room
                    var room = new Room();
                    var mesh = room.CreateRoom(
                        mergedWidthHalf, roomHeight, mergedDepthHalf,
                        Color.Black, Color.DarkGray,
                        roomData.Value.FrontBack, roomData.Value.FrontBack,
                        roomData.Value.LeftRight, roomData.Value.LeftRight,
                        finalFront, finalBack, finalLeft, finalRight
                    );

                    var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), mesh.Vertices.Length, BufferUsage.WriteOnly);
                    vertexBuffer.SetData(mesh.Vertices);
                    var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, mesh.Indices.Length, BufferUsage.WriteOnly);
                    indexBuffer.SetData(mesh.Indices);

                    HallwayRooms.Add((vertexBuffer, indexBuffer, mesh.Indices.Length / 3, Matrix.CreateTranslation(mergedWorldX, 0f, mergedWorldZ)));

                    // Dibujo los modelos Miscellaneos dentro de los pasillos para llenarlos con algo
                    IRoomAssets roomTypeInstance = RoomFactory.Create(roomData.Value.Type);
                    if (roomTypeInstance != null)
                    {
                        var placements = ModelPlacementOnRoomHelper.GeneratePlacements(roomTypeInstance, mergedWidthHalf, mergedDepthHalf, cellSize, rng.Next());

                        foreach (var (modelPath, localPos, rotationY) in placements)
                        {
                            if (!modelCache.TryGetValue(modelPath, out var model))
                            {
                                model = content.Load<Model>(TGCGame.ContentFolder3D + modelPath);
                                LevelGeneratorHelper.ApplyCustomEffectToModel(model, effect);
                                modelCache[modelPath] = model;
                            }

                            Matrix modelWorld = Matrix.CreateScale(0.5f) * Matrix.CreateRotationY(rotationY) * Matrix.CreateTranslation(mergedWorldX + localPos.X, localPos.Y, mergedWorldZ + localPos.Z);
                            models.Add((model, modelWorld, modelPath));
                        }
                    }
                }
            }
        }

        public static void DrawHallways(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            // WallTexture, FloorTexture, CeilingTexture, mas un canal extra
            for (int i = 0; i < 4; i++)
                graphicsDevice.SamplerStates[i] = SamplerState.LinearWrap; // Wrap de las texturas

            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);

            // Recorro los pasillos
            foreach (var room in HallwayRooms)
            {
                // Vertices de cada habitacion (pasillo)
                graphicsDevice.SetVertexBuffer(room.VertexBuffer);
                // Indices de cada habitacion
                graphicsDevice.Indices = room.IndexBuffer;

                // Donde se debera de dibujar
                effect.Parameters["World"]?.SetValue(room.World);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    // Aplico la configuracion
                    pass.Apply();
                    // Dibujo
                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        room.PrimitiveCount
                    );
                }
            }
        }
    }
}