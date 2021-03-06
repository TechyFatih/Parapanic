﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Parapanic
{
    class Ambulance : Car
    {
        Parapanic game;

        public bool hasPatient;
        bool intersected;
        const double MAX_TURN_RATE = 0.12;

        bool drifting = false;

        const float driftCoeff = .98f;
        const float driftRate = .005f;

        float currentDrift = driftCoeff;

        public float drawDirection;

        public int patientTimer = 0;
        public int maxTime = 2400;
        const int maxTimeMultiplier = 40;
        const int maxTimeDivider = 100;

        public bool toMenu;
        public bool lost = false;
        public bool won = false;

        const int MaxPatients = 2;
        public int patientsSaved = 0;

        public Ambulance(int x, int y, float direction, double topSpeed, double acceleration, double friction, Parapanic game)
            : base(x, y, 0, direction, topSpeed, acceleration, friction) 
        { 
            hasPatient = false; 
            drawDirection = direction;
            this.game = game;
        }

        public override void Update(World world)
        {
            if(hasPatient)
            {
                patientTimer++;

                if(patientTimer >= maxTime)
                {
                    lost = true;
                }
            }
            else
            {
                patientTimer = 0;
            }

            intersected = false;
            frictionEnabled = true;
            MouseState mouse = Mouse.GetState();
            //Left Click - Acceleration
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (speed + acceleration < topSpeed)
                    speed += acceleration;
                else
                    speed = topSpeed;

                if (speed > 0)
                    frictionEnabled = false;
            }

            //Right Click - Brake/Reverse
            if (mouse.RightButton == ButtonState.Pressed)
            {
                if (speed - acceleration > 0)
                    speed -= acceleration;
                else
                {
                    if (speed - acceleration > -topSpeed / 2)
                        speed -= acceleration / 2;
                    else
                        speed = -topSpeed / 2;
                }

                if (speed < 0)
                    frictionEnabled = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                drifting = true;

                //driftV = speedV * ((1 - driftCoeff)) / 2;

                if (currentDrift - driftRate > .05 * driftCoeff)
                {
                    currentDrift -= driftRate;
                }
                else
                {
                    currentDrift = 0;
                }
                
                speed *= currentDrift;
            }
            else
            {
                //driftV = new Vector2(0, 0);
                if(drifting)
                {
                    direction = drawDirection;
                    drifting = false;
                    currentDrift = driftCoeff;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                toMenu = true;
            }

            base.Update(world);

            //Console.WriteLine(intersected);
            
            //Mouse Direction - Turning
            double turnrate = (Math.Abs(speed) > 1) ? ((MAX_TURN_RATE / topSpeed) * Math.Abs(speed)) : 0; //Don't turn when not moving

            double mouseDirection = Utilities.NormAngle(Math.Atan2(mouse.Y - position.Y + Camera.position.Y,
                                                                   mouse.X - position.X + Camera.position.X));

            if (Math.Abs(mouseDirection - direction) > turnrate)
            {
                double refDir = direction;
                if (direction >= Math.PI)
                {
                    refDir -= Math.PI;
                    mouseDirection = Utilities.NormAngle(mouseDirection - Math.PI);
                }
                if (mouseDirection > refDir && mouseDirection < refDir + Math.PI)
                {
                    direction = (float)Utilities.NormAngle(direction + turnrate);
                    drawDirection = drifting?((float)Utilities.NormAngle(drawDirection + 1.3*turnrate)):direction;
                }
                else
                { 
                    direction = (float)Utilities.NormAngle(direction - turnrate);
                    drawDirection = drifting ? ((float)Utilities.NormAngle(drawDirection - 1.3 * turnrate)) : direction;
                }
            }

        }

        protected override void OnCollision(World world, int xCoord, int yCoord)
        {
            Block block = world.grid[xCoord, yCoord];

            if (block is WallBlock)
                intersected = true;
            else if (block is PatientBlock &&
                     !hasPatient)
            {
                hasPatient = true;
                int xPos = (int)world.grid[xCoord, yCoord].position.X;
                int yPos = (int)world.grid[xCoord, yCoord].position.Y;
                world.pointsOfInterest.Remove(((PatientBlock)block).POIHandle);
                world.grid[xCoord, yCoord] = new RoadBlock(xPos, yPos);
                Minimap.Map.DirtyFlag = true;
                patientTimer = 0;
                maxTime = Math.Max((int)(world.hospitalPosition - position).Length() * maxTimeMultiplier / maxTimeDivider, 15 * 60);
                //maxTime = 0;
            }
            else if (block is HospitalBlock &&
                     hasPatient)
            {
                hasPatient = false;
                int xPos = (int)world.grid[xCoord, yCoord].position.X;
                int yPos = (int)world.grid[xCoord, yCoord].position.Y;
                game.Score += (maxTime - patientTimer) * game.ScoreMultiplier;

                if (++patientsSaved == MaxPatients)
                {
                    if (game.ScoreMultiplier >= game.numLevels)
                    {
                        won = true;
                    }
                    else
                    {
                        game.gameState = Parapanic.State.PickALevel;
                        game.Level = new PickALevel(game);
                        game.ScoreMultiplier++;
                    }
                }

                //world.pointsOfInterest.Remove(((HospitalBlock)block).POIHandle);
                //world.grid[xCoord, yCoord] = new RoadBlock(xPos, yPos);
                Minimap.Map.DirtyFlag = true;
            }


            base.OnCollision(world, xCoord, yCoord);
        }
    }
}
