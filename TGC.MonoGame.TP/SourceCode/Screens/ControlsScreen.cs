using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.SourceCode.Screens
{
    public class ControlsScreen
    {
        private Rectangle _btnBack;
        private MouseState _prevMouseState;
        private KeyboardState _prevKeyboardState;

        public bool ShowBackButton { get; set; }

        public void Initialize(int screenWidth, int screenHeight) { }

        public bool Update(int screenWidth, int screenHeight)
        {
            float uiScale = screenHeight / 720f;
            int btnWidth = (int)(300 * uiScale);
            int btnHeight = (int)(60 * uiScale);

            _btnBack = new Rectangle((screenWidth - btnWidth) / 2, screenHeight - (int)(120 * uiScale), btnWidth, btnHeight);

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            bool closeScreen = false;

            if (keyboardState.IsKeyDown(Keys.C) && _prevKeyboardState.IsKeyUp(Keys.C))
            {
                closeScreen = true;
            }

            if (ShowBackButton)
            {
                bool isClick = mouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed;
                if (isClick && _btnBack.Contains(mouseState.Position))
                {
                    closeScreen = true;
                }
            }

            _prevMouseState = mouseState;
            _prevKeyboardState = keyboardState;

            return closeScreen;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, GraphicsDevice graphicsDevice)
        {
            float uiScale = graphicsDevice.Viewport.Height / 720f;
            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;

            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, width, height), Color.Black * 0.95f);

            string titleText = ShowBackButton ? "CONTROLES" : "CONTROLES (Presiona C para volver)";
            Vector2 titleSize = font.MeasureString(titleText) * uiScale;
            Vector2 titlePos = new Vector2((width - titleSize.X) / 2f, 50f * uiScale);

            spriteBatch.DrawString(font, titleText, titlePos + new Vector2(3, 3), Color.DarkGoldenrod, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, titleText, titlePos, Color.Gold, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

            string controlsList =
                "W, A, S, D  -  Moverse\n\n" +
                "E           -  Interactuar (Esconderse, Agarrar fosforos)\n\n" +
                "M           -  Mute / Unmute Musica\n\n" +
                "R           -  Reiniciar Juego\n\n" +
                "ESC         -  Salir del Juego\n\n" +
                "1           -  Encender/Apagar Linterna Nokia\n\n" +
                "2           -  Encender Fosforo\n\n" +
                "C           -  Abrir Controles";

            float controlsScale = uiScale * 0.62f;

            float titleBottom = titlePos.Y + titleSize.Y + (25f * uiScale); // Debajo del titulo
            float bottomLimit = ShowBackButton
                ? _btnBack.Y - (30f * uiScale) // Arriba de boton
                : height - (40f * uiScale);    // No hay boton de 'Volver'

            // Medida del texto
            Vector2 listSize = font.MeasureString(controlsList) * controlsScale;

            // Centrado horizontalmente
            float x = (width - listSize.X) / 2f;

            // Centrado verticalmente dentro de los limites para que nos se pise el texto
            float availableHeight = bottomLimit - titleBottom;
            float y = titleBottom + (availableHeight - listSize.Y) / 2f;

            // Clamp para evitar que se solape
            if (y < titleBottom) y = titleBottom;
            if (y + listSize.Y > bottomLimit) y = bottomLimit - listSize.Y;

            Vector2 listPos = new Vector2(x, y);

            // Dibujo de texto y sombras
            spriteBatch.DrawString(font, controlsList, listPos + new Vector2(2, 2), Color.Black, 0f, Vector2.Zero, controlsScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, controlsList, listPos, Color.White, 0f, Vector2.Zero, controlsScale, SpriteEffects.None, 0f);

            // Dibujo boton 'Volver' solo si viene desde el menu
            if (ShowBackButton)
            {
                DrawButton(spriteBatch, font, pixelTexture, _btnBack, "Volver", Mouse.GetState(), uiScale);
            }
        }

        private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel, Rectangle bounds, string text, MouseState mouse, float uiScale)
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