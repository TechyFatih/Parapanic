﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Parapanic
{
    class Camera
    {
        public static Vector2 position;
        static Rectangle view;

        public static void Initialize(int width, int height)
        {
            position = new Vector2();
            view = new Rectangle(0, 0, width, height);
        }

        public static void Update(GameTime gameTime, Ambulance ambulance, World world)
        {
            int newX = (int)ambulance.position.X - view.Width / 2;
            if (newX < 0) newX = 0;
            if (newX > world.Width - view.Width) newX = world.Width - view.Width;
            position.X = newX;
            view.X = newX;
            
            int newY = (int)ambulance.position.Y - view.Height / 2;
            if (newY < 0) newY = 0;
            if (newY > world.Height - view.Height) newY = world.Height - view.Height;
            position.Y = newY;
            view.Y = newY;
        }

        public static void DrawScreen(SpriteBatch spriteBatch, Ambulance ambulance, World world)
        {
            spriteBatch.Begin();

            foreach (Block b in world.grid)
            {
                spriteBatch.Draw(Textures.block, b.position - position, Color.White);
            }
            spriteBatch.Draw(Textures.ambulance, ambulance.position - position, null, Color.White, ambulance.rotation, ambulance.origin, 1, SpriteEffects.None, 1);
            
            spriteBatch.End();
        }
    }
}
