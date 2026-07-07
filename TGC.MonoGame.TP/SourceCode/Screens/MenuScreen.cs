using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Screens
{
    internal class MenuScreen
    {
        private Rectangle _btnPlay;
        private Rectangle _btnTutorial;
        private Rectangle _btnExit;

        private MouseState _prevMouseState;

        public MenuAction Update(int screenWidth, int screenHeight)
        {
            float uiScale = screenHeight / 720f;
            int btnWidth = (int)(300 * uiScale);
            int btnHeight = (int)(60 * uiScale);
            int startX = (int)(50 * uiScale);

            // Calculo de tamaño de botones segun viewport
            _btnPlay = new Rectangle(startX, (int)(50 * uiScale), btnWidth, btnHeight);
            _btnTutorial = new Rectangle(startX, (int)(130 * uiScale), btnWidth, btnHeight);
            _btnExit = new Rectangle(startX, (int)(210 * uiScale), btnWidth, btnHeight);

            var mouseState = Mouse.GetState();
            MenuAction action = MenuAction.None;

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
        /// Se crean los botones con su fondo, hover, sombras y texto.
        /// Su tamaño depende de screenWidth y screenHeight.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="pixelTexture"></param>
        /// /// <param name="screenWidth"></param>
        /// /// <param name="screenHeight"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, int screenWidth, int screenHeight)
        {
            var mouseState = Mouse.GetState();
            float uiScale = screenHeight / 720f;

            DrawButton(spriteBatch, font, pixelTexture, _btnPlay, "Jugar", mouseState, uiScale);
            DrawButton(spriteBatch, font, pixelTexture, _btnTutorial, "Controles", mouseState, uiScale);
            DrawButton(spriteBatch, font, pixelTexture, _btnExit, "Salir", mouseState, uiScale);
        }

        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel, 
            Rectangle bounds, string text, MouseState mouse, float uiScale)
        {
            // Efecto de hover, me fijo si la posicion del mouse esta dentro del rectangulo
            bool isHover = bounds.Contains(mouse.Position);

            Color bgColor = isHover ? Color.DarkRed : Color.Black;
            Color textColor = isHover ? Color.Yellow : Color.White;

            // Transparencia para el fondo de los botones
            spriteBatch.Draw(pixel, bounds, bgColor * 0.8f);

            // Centrado del texto dentro del rectangulo
            Vector2 textSize = font.MeasureString(text) * uiScale;
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2, 
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            // Sombra
            spriteBatch.DrawString(font, text, textPos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
            // Texto
            spriteBatch.DrawString(font, text, textPos, textColor, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
        }
    }
}