using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Screens
{
    public class VictoryScreen
    {
        private Rectangle _btnMainMenu;
        private MouseState _prevMouseState;

        public void Initialize(int screenWidth, int screenHeight)
        {
            // Boton centrado debajo del texto
            _btnMainMenu = new Rectangle((screenWidth - 300) / 2, screenHeight / 2 + 100, 300, 60);
        }

        public MenuAction Update()
        {
            var mouseState = Mouse.GetState();
            MenuAction action = MenuAction.None;

            bool isClick = mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released;

            if (isClick && _btnMainMenu.Contains(mouseState.Position))
            {
                action = MenuAction.MainMenu;
            }

            _prevMouseState = mouseState;
            return action;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, GraphicsDevice graphicsDevice, float timeTaken)
        {
            // Fondo negro
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black * 0.9f);

            // Textos
            string victoryText = "Ganaste, conseguiste el premio!";
            int minutes = (int)timeTaken / 60;
            int seconds = (int)timeTaken % 60;
            string timeText = $"Tiempo: {minutes:D2}:{seconds:D2}";

            Vector2 victorySize = font.MeasureString(victoryText);
            Vector2 timeSize = font.MeasureString(timeText);
            Vector2 centerScreen = new Vector2(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f);

            Vector2 victoryPos = centerScreen - new Vector2(victorySize.X / 2f, victorySize.Y + 15f);
            Vector2 timePos = centerScreen + new Vector2(-timeSize.X / 2f, 15f);

            // Sombras
            spriteBatch.DrawString(font, victoryText, victoryPos + new Vector2(3, 3), Color.DarkGoldenrod);
            spriteBatch.DrawString(font, timeText, timePos + new Vector2(2, 2), Color.Black);

            // Texto principal
            spriteBatch.DrawString(font, victoryText, victoryPos, Color.Gold);
            spriteBatch.DrawString(font, timeText, timePos, Color.White);

            // Boton de reinicio
            DrawButton(spriteBatch, font, pixelTexture, _btnMainMenu, "Volver al Inicio", Mouse.GetState());
        }

        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel, Rectangle bounds, string text, MouseState mouse)
        {
            bool isHover = bounds.Contains(mouse.Position);
            Color bgColor = isHover ? Color.DarkRed : Color.Black;
            Color textColor = isHover ? Color.Yellow : Color.White;

            spriteBatch.Draw(pixel, bounds, bgColor * 0.8f);

            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + (bounds.Height - textSize.Y) / 2);

            spriteBatch.DrawString(font, text, textPos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(font, text, textPos, textColor);
        }
    }
}