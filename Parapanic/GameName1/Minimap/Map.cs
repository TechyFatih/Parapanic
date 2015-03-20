﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Parapanic.Minimap
{
    class Map
    {
        Texture2D mapTexture;

        //upon world modification, this flag is set.
        //this makes it so we don't have to generate the map
        //every frame;
        public bool DirtyFlag = true;

        public Texture2D GetMapTexture(Parapanic game, World world)
        {
            if (!DirtyFlag)
                return mapTexture;

            if (mapTexture != null)
                mapTexture.Dispose();

            game.GraphicsDevice.Flush();
            RenderTarget2D render = new RenderTarget2D(game.GraphicsDevice, world.Width, world.Height);
            game.GraphicsDevice.SetRenderTarget(render);
            game.GraphicsDevice.Clear(Color.White);

            //NOTE(justin): we need our own spritebatch so that we don't end up drawing other stuff into our map
            SpriteBatch batch = new SpriteBatch(game.GraphicsDevice);
            batch.Begin();

            Texture2D texToDraw = game.Content.Load<Texture2D>("TestPicture"); //just using a test picture for now,
                                                                               //todo: replace test minimap picture with actual
                                                                               //pictures.
            foreach (Block b in world.grid)
            {
                if (b == null)
                    continue;

                Rectangle drawArea = new Rectangle();
                drawArea.X = (int)b.position.X;
                drawArea.Y = (int)b.position.Y;
                drawArea.Width = Block.size;
                drawArea.Height = Block.size;

                if (b.GetType().Equals(typeof(WallBlock)))
                {
                    batch.Draw(texToDraw, drawArea, Color.Green);
                    continue;
                }
                else if (b.GetType().Equals(typeof(FloorBlock)))
                {
                    batch.Draw(texToDraw, drawArea, Color.Blue);
                    continue;
                }
                //todo: other blocks
            }

            batch.End();
            game.GraphicsDevice.Flush();
            game.GraphicsDevice.SetRenderTarget(null);

            mapTexture = (Texture2D)render; //todo: look at speed impact of straight up casting this,
                                            //maybe there's some way to extract the data into a new object
                                            //or does it even matter?
            DirtyFlag = false;
            return mapTexture;
        
        }
    }
}