using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{
    public class Wall
    {
        public MeshDataWithOpenings CreateFrontWall(float width, float height, float depth, Color color, WallOpening opening)
        {
            return BuildWall(
                origin: new Vector3(-width, 0, depth),
                right: Vector3.UnitX, // Ir hacia la derecha en el X positivo desde el origen
                up: Vector3.UnitY,    // Ir hacia arriba en el Y positivo desde el origen
                wallWidth: width * 2,
                wallHeight: height,
                color: color,
                opening: opening
            );
        }

        public MeshDataWithOpenings CreateBackWall(float width, float height, float depth, Color color, WallOpening opening)
        {
            return BuildWall(
                origin: new Vector3(width, 0, -depth),
                right: -Vector3.UnitX, // Ir hacia la izquierda en el X negativo seria la derecha desde el origen
                up: Vector3.UnitY,
                wallWidth: width * 2,
                wallHeight: height,
                color: color,
                opening: opening
            );
        }

        public MeshDataWithOpenings CreateLeftWall(float width, float height, float depth, Color color, WallOpening opening)
        {
            return BuildWall(
                origin: new Vector3(-width, 0, -depth),
                right: Vector3.UnitZ, // Ir hacia adelante en el eje Z (positivo) seria la derecha de la pared
                up: Vector3.UnitY,
                wallWidth: depth * 2,
                wallHeight: height,
                color: color,
                opening: opening
            );
        }

        public MeshDataWithOpenings CreateRightWall(float width, float height, float depth, Color color, WallOpening opening)
        {
            return BuildWall(
                origin: new Vector3(width, 0, depth),
                right: -Vector3.UnitZ, // Ir hacia atras en el eje Z (negativo) seria la derecha de la pared
                up: Vector3.UnitY,
                wallWidth: depth * 2,
                wallHeight: height,
                color: color,
                opening: opening
            );
        }

        private MeshDataWithOpenings BuildWall(
            Vector3 origin,
            Vector3 right,
            Vector3 up,
            float wallWidth,
            float wallHeight,
            Color color,
            WallOpening opening)
        {
            var vertices = new List<VertexPositionColor>();
            var indices = new List<ushort>();
            var centers = new List<Vector3>();

            void AddQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 upperRight, Vector3 upperLeft) // Creacion de rectangulos (2 triangulos)
            {
                ushort start = (ushort)vertices.Count; // Desde que valor debe de arrancar para crear los triangulos
                vertices.Add(new VertexPositionColor(bottomLeft, color));   // Abajo-Izquierda
                vertices.Add(new VertexPositionColor(bottomRight, color));  // Abajo-Derecha
                vertices.Add(new VertexPositionColor(upperRight, color));   // Arriba-Derecha
                vertices.Add(new VertexPositionColor(upperLeft, color));    // Arriba-Izquierda


                //3(D)------------------ - 2(C)
                //  |                     /   |
                //  |                   /     |
                //  |     Triángulo 2 /       |
                //  |               /         |
                //  |             /           |
                //  |           /             |
                //  |         /   Triángulo 1 |
                //  |       /                 |
                //  |     /                   |
                //  |   /                     |
                //0(A)------------------ - 1(B)
                // Se realiza el calculo de los indices y se va agregando en la lista correspondiente
                indices.Add((ushort)(start + 0));
                indices.Add((ushort)(start + 1));
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 0));
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 3));
            }

            if (opening.Type == WallType.Solid)
            {
                // Se dibujan 2 triangulos para las paredes solidas
                AddQuad(
                    origin,
                    origin + right * wallWidth,
                    origin + right * wallWidth + up * wallHeight,
                    origin + up * wallHeight
                );

                return new MeshDataWithOpenings(vertices.ToArray(), indices.ToArray(), centers);
            }

            // Calcualo del 'agujero' para puerta o ventana
            float holeW = opening.Width;
            float holeH = opening.Height;
           
            // El 'agujero' no puede ser mas grande que la propia pared por lo que se limita que tenga:
            // como minimo valor 1f
            // como maximo el valor del width/height - 1f
            holeW = MathHelper.Clamp(holeW, 1f, wallWidth - 1f); // Ancho de opening
            holeH = MathHelper.Clamp(holeH, 1f, wallHeight - 1f); // Alto de opening

            // Distancia desde donde empieza la pared hasta el borde del 'agujero'
            float holeLeft = (wallWidth - holeW) * 0.5f; 
            float holeBottom = (opening.Type == WallType.Window) // Centrar verticalmente el hueco de la ventana
                ? (wallHeight - holeH) * 0.5f
                : 0f; // Es una puerta y arranca desde abajo el 'agujero'

            float holeRight = holeLeft + holeW; // Borde derecho del 'agujero'
            float holeTop = holeBottom + holeH; // Borde superior del 'agujero'

            // Calculo del centro de 'agujero'
            //             origen pared
            //             + vector hacia la derecha de la pared * (borde izquierdo del agujero + la mitad del ancho del agujero)
            //             + vector hacia arriba de la pared * (borde inferior del agujero + la mitad de la altura del agujero)
            Vector3 holeCenter = origin + right * (holeLeft + holeW * 0.5f) + up * (holeBottom + holeH * 0.5f);
            centers.Add(holeCenter);

            // Se dibujan 8 triangulos para las ventanas y puertas
            // Bottom rectangle
            if (holeBottom > 0f)
            {
                AddQuad(
                    origin,
                    origin + right * wallWidth,                     // Ancho completo
                    origin + right * wallWidth + up * holeBottom,   // Ancho completo + altura hasta el borde inferior del agujero
                    origin + up * holeBottom                        // Desde el origen, altura hasta el borde inferior del agujero
                );
            }

            // Left rectangle
            if (holeLeft > 0f)
            {
                AddQuad(
                    origin + up * holeBottom,                       // Empieza donde termino el Bottom rectangle
                    origin + right * holeLeft + up * holeBottom,    // Ancho hacia la derecha hasta el borde izquierdo del agujero
                    origin + right * holeLeft + up * holeTop,       // Altura hasta el tope del agujero calculado anteriormente (holeTop)
                    origin + up * holeTop                           // Desde el origen, altura hasta el tope del agujero (holeTop)
                );
            }

            // Right rectangle
            if (holeRight < wallWidth)
            {
                AddQuad(
                    origin + right * holeRight + up * holeBottom,   // Empieza desde el borde derecho del agujero y arranca desde donde termino Bottom rectangle
                    origin + right * wallWidth + up * holeBottom,   // Ancho completo, desde donde termino Bottom rectangle
                    origin + right * wallWidth + up * holeTop,      // Ancho completo hasta la altura del borde superior calculado anteriormente (holeTop)
                    origin + right * holeRight + up * holeTop       // Desde el origen + el borde derecho, altura hasta el tope del agujero (holeTop)
                );
            }

            // Top rectangle
            if (holeTop < wallHeight)
            {
                AddQuad(
                    origin + up * holeTop,                          // Origen donde arranca el borde superior del agujero
                    origin + right * wallWidth + up * holeTop,      // Ancho completo, desde el borde superior del agujero
                    origin + right * wallWidth + up * wallHeight,   // Ancho completo y altura completa, desde el origen
                    origin + up * wallHeight                        // Desde el origen, altura completa
                );
            }

            return new MeshDataWithOpenings(vertices.ToArray(), indices.ToArray(), centers);
        }
    }
}