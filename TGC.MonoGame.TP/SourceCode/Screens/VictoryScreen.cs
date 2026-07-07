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

        public void Initialize(int screenWidth, int screenHeight) { }

        public MenuAction Update(int screenWidth, int screenHeight)
        {
            float uiScale = screenHeight / 720f;
            int btnWidth = (int)(300 * uiScale);
            int btnHeight = (int)(60 * uiScale);

            _btnMainMenu = new Rectangle((screenWidth - btnWidth) / 2, screenHeight / 2 + (int)(100 * uiScale), btnWidth, btnHeight);

            var mouseState = Mouse.GetState();
            MenuAction action = MenuAction.None;

            bool isClick = mouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed;

            if (isClick && _btnMainMenu.Contains(mouseState.Position))
            {
                action = MenuAction.MainMenu;
            }

            _prevMouseState = mouseState;
            return action;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, GraphicsDevice graphicsDevice, float timeTaken)
        {
            float uiScale = graphicsDevice.Viewport.Height / 720f;

            // Fondo negro
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black * 0.9f);

            // Textos
            string victoryText = "Ganaste, conseguiste el premio!";
            int minutes = (int)timeTaken / 60;
            int seconds = (int)timeTaken % 60;
            string timeText = $"Tiempo: {minutes:D2}:{seconds:D2}";

            Vector2 victorySize = font.MeasureString(victoryText) * uiScale;
            Vector2 timeSize = font.MeasureString(timeText) * uiScale;
            Vector2 centerScreen = new Vector2(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f);

            Vector2 victoryPos = centerScreen - new Vector2(victorySize.X / 2f, victorySize.Y + (15f * uiScale));
            Vector2 timePos = centerScreen + new Vector2(-timeSize.X / 2f, 15f * uiScale);

            // Sombras
            spriteBatch.DrawString(font, victoryText, victoryPos + new Vector2(3, 3), Color.DarkGoldenrod, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, timeText, timePos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

            // Texto principal
            spriteBatch.DrawString(font, victoryText, victoryPos, Color.Gold, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, timeText, timePos, Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

            // Boton de reinicio
            DrawButton(spriteBatch, font, pixelTexture, _btnMainMenu, "Volver al Inicio", Mouse.GetState(), uiScale);
        }

        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel, 
            Rectangle bounds, string text, MouseState mouse, float uiScale)
        {
            bool isHover = bounds.Contains(mouse.Position);
            Color bgColor = isHover ? Color.DarkRed : Color.Black;
            Color textColor = isHover ? Color.Yellow : Color.White;

            spriteBatch.Draw(pixel, bounds, bgColor * 0.8f);

            Vector2 textSize = font.MeasureString(text) * uiScale;
            Vector2 textPos = new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + (bounds.Height - textSize.Y) / 2);

            spriteBatch.DrawString(font, text, textPos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, textPos, textColor, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
        }
    }
}