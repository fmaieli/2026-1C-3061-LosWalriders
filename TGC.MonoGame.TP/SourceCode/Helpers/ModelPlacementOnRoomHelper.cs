using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public class ModelPlacementOnRoomHelper
    {
        // Devuelve Path, Local Position y Rotation Y
        public static List<(string ModelPath, Vector3 Position, float RotationY)> GeneratePlacements(
            IRoomAssets room, float roomWidth, float roomDepth, float cellSize, int seed)
        {
            var results = new List<(string, Vector3, float)>();
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
            void Place(string modelPath, int c, int r, float rotY = 0f, float offsetY = 0f, Vector3 microOffset = default)
            {
                // Para no salir de la lista
                c = Math.Clamp(c, 0, cols - 1);
                r = Math.Clamp(r, 0, rows - 1);
                results.Add((modelPath, CellToWorld(c, r, offsetY, microOffset), rotY));
            }

            // Habitaciones
            switch (room.Type)
            {
                case RoomType.Entrance:
                    Place("Items/PSX_Item_Shotgun", midC, midR, 0f, 50f, new Vector3(0, 0, -50f));
                    break;
                case RoomType.Bed:
                    // Cama perpendicular a la pared trasera
                    Place("Level/Bedroom/PSX_Bed", midC, rows - 1, 0f);
                    // Closet en la pared contraria a la cama
                    Place("Level/Bedroom/PSX_Wooden_Closet", midC, 0, MathHelper.Pi);

                    // Drawers a la izquierda o derecha (50% de chance)
                    bool drawersOnLeft = rng.Next(2) == 0;
                    if (drawersOnLeft)
                    {
                        Place("Level/Bedroom/PSX_Wooden_Drawers", 0, midR, MathHelper.PiOver2);
                        Place("Level/Bedroom/PSX_Lamp", cols - 1, midR, -MathHelper.PiOver2);
                        // Hacha ensangrentada clavada en los drawers (Offset Y alto para que quede arriba)
                        // Ver de rotarla para que se vea mejor
                        Place("Miscellaneous/PSX_Bloody_Fire_Axe", 0, midR, 0f, 35f);
                    }
                    else
                    {
                        Place("Level/Bedroom/PSX_Wooden_Drawers", cols - 1, midR, -MathHelper.PiOver2);
                        Place("Level/Bedroom/PSX_Lamp", 0, midR, MathHelper.PiOver2);
                        Place("Miscellaneous/PSX_Bloody_Fire_Axe", cols - 1, midR, 0f, 35f);
                    }
                    break;

                case RoomType.Living:
                    // Mesa central
                    Place("Level/Living/PSX_Wooden_Table", midC, midR);

                    // Sillas intercaladas a los costados de la mesa
                    Place("Level/Living/PSX_Wooden_Chair", midC - 1, midR, MathHelper.PiOver2);
                    Place("Level/Living/PSX_Wooden_Chair1", midC + 1, midR, -MathHelper.PiOver2);

                    // TV Stand va en la fila opuesta
                    Place("Level/Living/PSX_TV_Stand", midC, rows - 1, 0f);
                    Place("Level/Living/PSX_Old_TV", midC, rows - 1, 0f, 30f);                               // Arriba del TV Stand
                    Place("Level/Living/PSX_Playstation1", midC, rows - 1, 0f, 30f, new Vector3(15f, 0, 0)); // Al lado de la TV

                    // Armchairs a un cuerpo de distancia, apuntando a la TV
                    Place("Level/Living/PSX_Armchair", midC - 1, midR + 1, MathHelper.Pi);
                    Place("Level/Living/PSX_Armchair", midC + 1, midR + 1, MathHelper.Pi);
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
                    for (int r = 1; r < rows - 1; r += 3)
                    {
                        // Se dibujan los modelos en 3 columnas, izquierda, centro y derecha
                        int leftColumn = 1;
                        int centerColumn = cols / 2;
                        int rightColumn = cols - 2;

                        int[] tableCols = { leftColumn, centerColumn, rightColumn };

                        // Conjunto de mesa, PC y silla
                        foreach (int c in tableCols)
                        {
                            Place("Level/Living/PSX_Wooden_Table", c, r);
                            Place("Level/Computer/PSX_Dirty_Old_PC", c, r, 0f, 35f);
                            Place("Level/Computer/PSX_Computer_Chair", c, r, MathHelper.Pi, 0f, new Vector3(0, 0, 10f));
                        }

                        // Se bloquea aleatoriamente el hueco de la izquierda o el de la derecha entre el conjunto de objetos
                        // Nunca se deben de bloquear ambos espacios para dejar pasar
                        bool blockLeftGap = rng.Next(2) == 0;

                        if (blockLeftGap)
                        {
                            // Colocamos el papel justo en medio del pasillo izquierdo
                            int gapColumn = (leftColumn + centerColumn) / 2;
                            Place("Miscellaneous/PSX_Paper_Stack", gapColumn, r);
                        }
                        else
                        {
                            // Colocamos el papel justo en medio del pasillo derecho
                            int gapColumn = (centerColumn + rightColumn) / 2;
                            Place("Miscellaneous/PSX_Paper_Stack", gapColumn, r);
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
                    // Laberinto
                    //for (int r = 0; r < rows; r++)
                    //{
                    //    for (int c = 0; c < cols; c++)
                    //    {
                    //        // Dejamos el centro libre
                    //        if (Math.Abs(c - midC) <= 1 && Math.Abs(r - midR) <= 1) continue;

                    //        if ((r + c) % 2 == 0 && rng.Next(100) > 30) // 70% chance en celdas pares
                    //        {
                    //            string bush = rng.Next(3) switch { 0 => "Level/Outdoor/PSX_Bush", 1 => "Level/Outdoor/PSX_Bush2", _ => "Level/Outdoor/PSX_Bush3" };
                    //            Place(bush, c, r);
                    //        }
                    //    }
                    //}

                    // Árbol tenebroso en el medio
                    Place("Level/Outdoor/LowPoly_Tree", midC, midR);
                    // Coleccionable en el centro
                    Place("Miscellaneous/PSX_Paper_Stack", midC, midR + 1);

                    // Barriles oxidados en esquinas
                    Place("Miscellaneous/PSX_Rusty_Barell", 0, 0);
                    Place("Miscellaneous/PSX_Wooden_Barrel", cols - 1, rows - 1);
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