using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Screens
{

    internal class MenuScreen
    {
        // Ubicacion y tamaño de los botones (X, Y, Ancho, Alto)
        private Rectangle _btnPlay = new Rectangle(50, 50, 300, 60);
        private Rectangle _btnTutorial = new Rectangle(50, 130, 300, 60);
        private Rectangle _btnExit = new Rectangle(50, 210, 300, 60);

        private MouseState _prevMouseState;

        public MenuAction Update()
        {
            var mouseState = Mouse.GetState();
            MenuAction action = MenuAction.None;

            // Un solo click, se presiona y antes estaba released
            bool isClick = mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released;

            if (isClick)
            {
                if (_btnPlay.Contains(mouseState.Position)) action = MenuAction.Play;
                else if (_btnTutorial.Contains(mouseState.Position)) action = MenuAction.Tutorial;
                else if (_btnExit.Contains(mouseState.Position)) action = MenuAction.Exit;
            }

            _prevMouseState = mouseState;
            return action;
        }

        /// <summary>
        /// Se crean los botones con su fondo, hover, sombras y texto
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="pixelTexture"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            var mouseState = Mouse.GetState();

            DrawButton(spriteBatch, font, pixelTexture, _btnPlay, "Jugar", mouseState);
            DrawButton(spriteBatch, font, pixelTexture, _btnTutorial, "Tutorial", mouseState);
            DrawButton(spriteBatch, font, pixelTexture, _btnExit, "Salir", mouseState);
        }

        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel,
            Rectangle bounds, string text, MouseState mouse)
        {
            // Efecto de hover, me fijo si la posicion del mouse esta dentro del rectangulo
            bool isHover = bounds.Contains(mouse.Position);

            Color bgColor = isHover ? Color.DarkRed : Color.Black;
            Color textColor = isHover ? Color.Yellow : Color.White;

            // Transparencia para el fondo de los botones
            spriteBatch.Draw(pixel, bounds, bgColor * 0.8f);

            // Centrado del texto dentro del rectangulo
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            // Sombra
            spriteBatch.DrawString(font, text, textPos + new Vector2(2, 2), Color.Black);
            // Texto
            spriteBatch.DrawString(font, text, textPos, textColor);
        }
    }
}