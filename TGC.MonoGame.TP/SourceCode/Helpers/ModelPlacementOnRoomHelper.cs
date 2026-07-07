using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public class ModelPlacementOnRoomHelper
    {
        public static List<(string ModelPath, Vector3 Position, float RotationY)> GeneratePlacements(
            IRoomAssets room, float roomWidth, float roomDepth, float cellSize, int seed)
        {
            var advanced = GeneratePlacementsAdvanced(room, roomWidth, roomDepth, cellSize, seed);
            var results = new List<(string, Vector3, float)>();

            foreach (var item in advanced)
                results.Add((item.ModelPath, item.Position, item.Rotation.Y));

            return results;
        }

        // Rotacion en los ejes x,y,z
        public static List<(string ModelPath, Vector3 Position, Vector3 Rotation, float Scale)> GeneratePlacementsAdvanced(
            IRoomAssets room, float roomWidth, float roomDepth, float cellSize, int seed)
        {
            var results = new List<(string, Vector3, Vector3, float)>();
            var rng = new Random(seed);

            // Calculamos el tamaño de la matriz
            int cols = Math.Max(3, (int)(roomWidth * 2 / cellSize));
            int rows = Math.Max(3, (int)(roomDepth * 2 / cellSize));

            // Centro de la habitacion
            int midC = cols / 2;
            int midR = rows / 2;

            // Convierto coordenadas de matriz a mundo
            Vector3 CellToWorld(int c, int r, float offsetY = 0f, Vector3 microOffset = default)
            {
                float x = -roomWidth + (c + 0.5f) * cellSize + microOffset.X;
                float z = -roomDepth + (r + 0.5f) * cellSize + microOffset.Z;
                return new Vector3(x, offsetY, z);
            }

            // Registro el modelo en la lista final
            void PlaceAdvanced(string modelPath, int c, int r, Vector3 rotation, float offsetY = 0f, Vector3 microOffset = default, float scale = 1f)
            {
                c = Math.Clamp(c, 0, cols - 1);
                r = Math.Clamp(r, 0, rows - 1);
                results.Add((modelPath, CellToWorld(c, r, offsetY, microOffset), rotation, scale));
            }

            void Place(string modelPath, int c, int r, float rotY = 0f, float offsetY = 0f, Vector3 microOffset = default, float scale = 1f)
            {
                PlaceAdvanced(modelPath, c, r, new Vector3(0, rotY, 0), offsetY, microOffset, scale);
            }

            // Habitaciones
            switch (room.Type)
            {
                case RoomType.Entrance:
                    Place("Items/PSX_Item_Shotgun", midC, midR, 0f, 50f, new Vector3(0, 0, -50f));
                    break;
                case RoomType.Bed:
                    // Cama perpendicular a la pared trasera
                    Place("Level/Bedroom/PSX_Bed", midC, rows - 1, 0f, 15f, new Vector3(0, 0, -32f));
                    // Closet en la pared contraria a la cama
                    Place("Level/Bedroom/PSX_Wooden_Closet", midC, 0, 0, 0f, new Vector3(0, 0, 15f));

                    // Drawers a la izquierda o derecha (50% de chance)
                    bool drawersOnLeft = rng.Next(2) == 0;
                    if (drawersOnLeft)
                    {
                        Place("Level/Bedroom/PSX_Wooden_Drawers", 0, midR, MathHelper.PiOver2, 0f);
                        Place("Level/Bedroom/PSX_Lamp", cols - 1, midR, -MathHelper.PiOver2, 15f, Vector3.Zero);
                        // Hacha ensangrentada clavada
                        PlaceAdvanced("Miscellaneous/PSX_Bloody_Fire_Axe", 0, midR, new Vector3(0, 0, MathHelper.Pi), 35f, new Vector3(15f, 0, 0));
                    }
                    else
                    {
                        Place("Level/Bedroom/PSX_Wooden_Drawers", cols - 1, midR, -MathHelper.PiOver2, 0f, new Vector3(-15f, 0, 0));
                        Place("Level/Bedroom/PSX_Lamp", cols - 1, midR, -MathHelper.PiOver2, 15f, Vector3.Zero);
                        // Hacha ensangrentada clavada
                        PlaceAdvanced("Miscellaneous/PSX_Bloody_Fire_Axe", cols - 1, midR, new Vector3(MathHelper.PiOver2, -MathHelper.Pi, 0), 35f, new Vector3(-15f, 0, 0));
                    }
                    break;

                case RoomType.Living:
                    // 2 TV Stands en la esquina superior izquierda a medio camino entre la esquina y la puerta
                    int tvCol1 = Math.Max(0, (midC / 2) - 1);
                    int tvCol2 = tvCol1 + 2;

                    Place("Level/Living/PSX_TV_Stand", tvCol1, 0, 0f, 0f, new Vector3(0, 0, 15f));
                    Place("Level/Living/PSX_TV_Stand", tvCol2, 0, 0f, 0f, new Vector3(0, 0, 15f));

                    Place("Level/Living/PSX_Old_TV", tvCol1, 0, 0f, 40f, new Vector3(0, 0, 15f));
                    Place("Level/Living/PSX_Playstation1", tvCol2, 0, 0f, 48f, new Vector3(0, 0, 15f), 0.5f);

                    // 2 Armchairs enfrente
                    Place("Level/Living/PSX_Armchair", tvCol1, 2, MathHelper.Pi, 0f, new Vector3(0, 0, 35f));
                    Place("Level/Living/PSX_Armchair", tvCol2, 2, MathHelper.Pi, 0f, new Vector3(0, 0, 35f));

                    // 2 Mesas a la derecha del centro de la habitación
                    int tableCol1 = midC + 4;
                    int tableCol2 = midC + 7;
                    Place("Level/Living/PSX_Wooden_Table", tableCol1, midR);
                    Place("Level/Living/PSX_Wooden_Table", tableCol2, midR);

                    // Sillas aleatorias alrededor de las mesas
                    for (int i = 0; i < 4; i++)
                    {
                        string chair = rng.Next(2) == 0 ? "Level/Living/PSX_Wooden_Chair" : "Level/Living/PSX_Wooden_Chair1";
                        int targetC = rng.Next(2) == 0 ? tableCol1 : tableCol2;
                        int side = rng.Next(4);
                        float rot = 0f;
                        int cOff = 0, rOff = 0;
                        
                        switch(side) {
                            case 0: rOff = -1; rot = 0f; break; // Arriba
                            case 1: rOff = 1; rot = MathHelper.Pi; break; // Abajo
                            case 2: cOff = -1; rot = MathHelper.PiOver2; break; // Izquierda
                            case 3: cOff = 1; rot = -MathHelper.PiOver2; break; // Derecha
                        }
                        Place(chair, targetC + cOff, midR + rOff, rot, 0f, new Vector3(cOff * 5f, 0, rOff * 5f));
                    }
                    break;

                case RoomType.Kitchen:
                    Place("Level/Kitchen/PSX_Wooden_Table1", midC, midR);

                    // Platos y vasos encima de la mesa - revisar tamaño y altura
                    float tableHeight = 35f;
                    Place("Level/Kitchen/PSX_Plate", midC, midR, 0f, tableHeight, new Vector3(-10f, 0, -10f));
                    Place("Level/Kitchen/PSX_Empty_Cup", midC, midR, 0f, tableHeight, new Vector3(-15f, 0, -5f));

                    Place("Level/Kitchen/PSX_Plate1", midC, midR, 0f, tableHeight, new Vector3(10f, 0, -10f));
                    Place("Level/Kitchen/PSX_Empty_Cup", midC, midR, 0f, tableHeight, new Vector3(15f, 0, -5f));

                    Place("Level/Kitchen/PSX_Plate", midC, midR, 0f, tableHeight, new Vector3(-10f, 0, 10f));
                    Place("Level/Kitchen/PSX_Empty_Cup", midC, midR, 0f, tableHeight, new Vector3(-15f, 0, 5f));

                    Place("Level/Kitchen/PSX_Plate1", midC, midR, 0f, tableHeight, new Vector3(10f, 0, 10f));
                    Place("Level/Kitchen/PSX_Empty_Cup", midC, midR, 0f, tableHeight, new Vector3(15f, 0, 5f));

                    // Cleaver clavado en el centro de la mesa - revisar direccion y altura
                    Place("Miscellaneous/PSX_Bloody_Cleaver_Knife", midC, midR, MathHelper.PiOver4, tableHeight + 2f);
                    break;

                case RoomType.Computer:
                    float computerScale = 0.75f;
                    float chairScale = 0.55f;

                    for (int r = 1; r < rows - 1; r += 3)
                    {
                        // Se dibujan los modelos en 3 columnas, izquierda, centro y derecha
                        int leftColumn = 2;
                        int centerColumn = cols / 2;
                        int rightColumn = cols - 3;

                        int[] tableCols = { leftColumn, centerColumn, rightColumn };

                        // Conjunto de mesa, PC y silla
                        foreach (int c in tableCols)
                        {
                            Place("Level/Living/PSX_Wooden_Table", c, r, 0f, 0f, default);
                            Place("Level/Computer/PSX_Dirty_Old_PC", c, r, 0f, 48f, new Vector3(0, 0, -5f), computerScale);
                            Place("Level/Computer/PSX_Computer_Chair", c, r, MathHelper.Pi, 0f, new Vector3(0, 0, 40f), chairScale);
                        }

                        // Se bloquea aleatoriamente el hueco de la izquierda o el de la derecha entre el conjunto de objetos
                        // Nunca se deben de bloquear ambos espacios para dejar pasar
                        bool blockLeftGap = rng.Next(2) == 0;

                        if (blockLeftGap)
                        {
                            // Colocamos el papel justo en medio del pasillo izquierdo
                            int gapColumn = (leftColumn + centerColumn) / 2;
                            Place("Miscellaneous/PSX_Paper_Stack", gapColumn, r, 0f, 0f, default, 1.5f);
                        }
                        else
                        {
                            // Colocamos el papel justo en medio del pasillo derecho
                            int gapColumn = (centerColumn + rightColumn) / 2;
                            Place("Miscellaneous/PSX_Paper_Stack", gapColumn, r, 0f, 0f, default, 1.5f);
                        }
                    }
                    break;

                case RoomType.Bath:
                    // Toilet en pared contraria a puerta
                    Place("Level/Bathroom/PSX_Toilet", midC, rows - 1, 0f);
                    // Papel en la pared más cercana
                    Place("Level/Bathroom/PSX_Toilet_Paper", midC + 1, rows - 1, -MathHelper.PiOver2, 20f);

                    // Placeholder de Bañera y Sink
                    Place("Miscellaneous/PSX_Wooden_Barrel", 0, rows - 1, 0f);
                    Place("Miscellaneous/PSX_Bloody_Fire_Axe", 0, rows - 1, 0f, 10f, new Vector3(15f, 0, 0)); // Hacha cerca
                    break;

                case RoomType.Outdoor:
                    float postScale = 0.15f;

                    // Arbol tenebroso en el medio
                    Place("Level/Outdoor/PSX_Outdoor_Spooky_Tree", midC, midR, 0f, -5f);
                    // Banco
                    Place("Level/Outdoor/PSX_Outdoor_Bench", midC, midR + 5);

                    // Postes de luz
                    Place("Level/Outdoor/PSX_Lamp_Post", midC, midR + 5, 0f, 0f, new Vector3(100f, 0, 0), postScale);
                    Place("Level/Outdoor/PSX_Lamp_Post", 0, 0, 0f, 0f, default, postScale);
                    Place("Level/Outdoor/PSX_Lamp_Post", cols - 1, rows - 1, 0f, 0f, default, postScale);
                    break;

                case RoomType.Hallway:
                    // En los pasillos, generamos un barril oxidado random de vez en cuando para prender fuego
                    if (rng.Next(100) > 85)
                        Place("Miscellaneous/PSX_Rusty_Barell", rng.Next(cols), rng.Next(rows));
                    break;
                case RoomType.Prize:
                    Place("Items/PSX_Item_Shotgun", midC, midR, 0f, 35f);
                    break;
            }

            // Spawn aleatorio de cajas de fosforos
            if (room.Type != RoomType.Entrance && room.Type != RoomType.Outdoor)
            {
                // 20% de chances
                if (rng.Next(100) < 20)
                {
                    int randomCol = rng.Next(cols);
                    int randomRow = rng.Next(rows);

                    float matchBoxHeight = 0f;

                    Place("Items/PSX_Item_Match_Box", randomCol, randomRow, 0f, matchBoxHeight);
                }
            }

            return results;
        }
    }
}