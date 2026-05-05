using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public class ModelPlacementOnRoomHelper
    {
        public static readonly List<string> MiscAssets = new()
        {
            "Miscellaneous/PSX_Bloody_Cleaver_Knife",
            "Miscellaneous/PSX_Bloody_Fire_Axe",
            "Miscellaneous/PSX_Paper_Stack",
            "Miscellaneous/PSX_Rusty_Barell",
            "Miscellaneous/PSX_Wooden_Barrel"
        };

        public static List<(string ModelPath, Vector3 Position)> GeneratePlacements(
            IRoomAssets room, float roomWidth, float roomDepth, float cellSize, int seed)
        {
            var results = new List<(string, Vector3)>();
            var rng = new Random(seed); // Genero a partir del parametro

            // Cuantas columnas y celdas existen en la habitacion a partir del ancho y profundidad que se pasaron como parametro
            // Divido por el tamaño de la celda para saber cuantas filas y columnas tendra la grilla
            int cols = Math.Max(1, (int)(roomWidth * 2 / cellSize)); // Se valida que exista por lo menos una habitacion de 1x1
            int rows = Math.Max(1, (int)(roomDepth * 2 / cellSize));
            var grid = new bool[rows, cols];

            Vector3 CellToWorld(int row, int column)
            {
                // Se suman 0.5f para centrar el modelo
                float x = -roomWidth + (column + 0.5f) * cellSize; 
                float z = -roomDepth + (row + 0.5f) * cellSize;
                return new Vector3(x, 0f, z);
            }

            void Place(string modelPath)
            {
                var freeCells = new List<(int row, int col)>();
                // Recorro la grilla y compruebo cuales coordenadas estan libres
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (!grid[r, c])
                            freeCells.Add((r, c));

                if (freeCells.Count == 0) return; // La lista esta vacia por lo que esta totalmente llena la habitacion de modelos

                var pick = freeCells[rng.Next(freeCells.Count)];            // Busco al azar un lugar a partir de la lista de celdas libres
                grid[pick.row, pick.col] = true;                            // Marco el valor como true para que otro modelo no lo pise
                results.Add((modelPath, CellToWorld(pick.row, pick.col)));  // Guardo cual es el modelo y la ubicacion vectorial en el mundo en la lista results
            }

            // Se corre por cada asset que tiene la habitacion
            foreach (var model in room.Assets)
                Place(model);

            // Lo corro una vez mas para meter un asset Miscellaneous
            Place(MiscAssets[rng.Next(MiscAssets.Count)]);

            return results;
        }
    }
}
