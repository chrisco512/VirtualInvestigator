// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// -----------------------------------------------------------------------5-----------

#region File Description
//-----------------------------------------------------------------------------
// Item.cs
//
// Copyright(2013) ShrikeSoft
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


#endregion

namespace VirtualInvestigator
{
    class Item : DrawableComponent3D
    {
        #region Fields/Properties
        Texture2D itemTexture;
        public BoundingBox box;

        Matrix rollMatrix = Matrix.Identity;
        Vector3 normal;

        public float angleX;
        public float angleZ;

        public Camera cam;

        public BoundingSphere BoundingSphereTransformed
        {
            get
            {
                BoundingSphere boundingSphere = Model.Meshes[0].BoundingSphere;
                boundingSphere = boundingSphere.Transform(AbsoluteBoneTransforms[0]);
                boundingSphere.Center += Position;
                return boundingSphere;
            }
        }

        protected BoundingBox UpdateBoundingBox(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        #endregion

        #region Initializations
        public Item(Game game, string model)
            : base(game, model)
        {
            preferPerPixelLighting = true;
            //UpdateBox();
        }
        #endregion Initializtions

        #region Loading
        /// <summary>
        /// Load the marble content
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Load the texture of the marble
            //rocketTexture = Game.Content.Load<Texture2D>(@"Textures\Rocket");
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the item
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void Update(GameTime gameTime)
        {
            //this.Position = new Vector3((float)xVal, (float)xVal, -(float)zVal);
            UpdateBox();
            base.Update(gameTime);
        }

        public void UpdateBox()
        {
            this.box = UpdateBoundingBox(this.Model, FinalWorldTransforms);
        }

        /// <summary>
        /// Properly place the item in the game world
        /// </summary>
        protected override void UpdateFinalWorldTransform()
        {
            //This function calculates the final view of the object based off the camera's position and angle
            Vector3 target = cam.position;

            Vector3 dist = target - Position;
            dist.Normalize();

            float theta = (float)Math.Acos(Vector3.Dot(dist, Vector3.Backward));
            Vector3 cross = Vector3.Cross(Vector3.Backward, dist);
            cross.Normalize();

            if (cross.X.Equals(System.Single.NaN))
            {
                int scalar = 1;
                if (Position.Z < target.Z)
                {
                    scalar = -1;
                }
                FinalWorldTransforms = //Matrix.CreateRotationY(scalar * -MathHelper.PiOver2) * 
                                        Matrix.CreateRotationX(MathHelper.PiOver2) *
                                        Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                        /*Matrix.CreateScale(0.10f) * */
                                        Matrix.CreateTranslation(Position);
            }
            else
            {
                Quaternion quaternion = Quaternion.CreateFromAxisAngle(cross, theta);

                // Multiply by two matrices which will place the item in its proper position
                FinalWorldTransforms = Matrix.CreateRotationX(MathHelper.PiOver2) * //Matrix.CreateTranslation(new Vector3(-20,0,0)) *
                            //Matrix.CreateFromQuaternion(quaternion) * 
                            /*Matrix.CreateScale(0.10f) * */
                            Matrix.CreateRotationZ(MathHelper.PiOver2) *
                            Matrix.CreateTranslation(Position);
            }

        }
        #endregion

        #region Render
        /// <summary>
        /// Draw the item
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void Draw(GameTime gameTime)
        {
            var originalSamplerState = GraphicsDevice.SamplerStates[0];

            // Cause the item's textures to linearly clamp            
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            foreach (var mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Set the effect for drawing the item
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = preferPerPixelLighting;

                    /* These must be disabled in order to enable embedded textures */
                    //effect.TextureEnabled = true;
                    //effect.Texture = rocketTexture;

                    // Apply camera settings
                    effect.Projection = cam.Projection; 
                    effect.View = cam.View; 

                    // Apply necessary transformations
                    effect.World = //AbsoluteBoneTransforms[mesh.ParentBone.Index] *
                        FinalWorldTransforms;
                }

                mesh.Draw();
            }

            // Return to the original state
            GraphicsDevice.SamplerStates[0] = originalSamplerState;
        }

        #endregion

        #region Overriding physics calculations
        /// <summary>
        /// Perform collision checks with the maze.
        /// </summary>
        protected override void CalculateCollisions()
        {
        //    Maze.GetCollisionDetails(BoundingSphereTransformed, ref intersectDetails, false);
        //
        //    if (intersectDetails.IntersectWithWalls)
        //    {
        //        foreach (var triangle in intersectDetails.IntersectedWallTriangle)
        //        {
        //            Axis direction = CollideDirection(triangle);
        //            if ((direction & Axis.X) == Axis.X && (direction & Axis.Z) == Axis.Z)
        //            {
        //                Maze.GetCollisionDetails(BoundingSphereTransformed, ref intersectDetails, true);
        //            }
        //        }
        //    }
        }

        /// <summary>
        /// Calculate the marble's acceleration according to the maze's tilt.
        /// </summary>
        protected override void CalculateAcceleration()
        {
        //    if (intersectDetails.IntersectWithGround)
        //    {
        //        // We must take both the maze's tilt and the angle of the floor
        //        // section beneath the marble into account
        //        angleX = 0;
        //        angleZ = 0;
        //        if (intersectDetails.IntersectedGroundTriangle != null)
        //        {
        //            intersectDetails.IntersectedGroundTriangle.Normal(out normal);
        //            angleX = (float)Math.Atan(normal.Y / normal.X);
        //            angleZ = (float)Math.Atan(normal.Y / normal.Z);
        //
        //            if (angleX > 0)
        //            {
        //                angleX = MathHelper.PiOver2 - angleX;
        //            }
        //            else if (angleX < 0)
        //            {
        //                angleX = -(angleX + MathHelper.PiOver2);
        //            }
        //
        //            if (angleZ > 0)
        //            {
        //                angleZ = MathHelper.PiOver2 - angleZ;
        //            }
        //            else if (angleZ < 0)
        //            {
        //                angleZ = -(angleZ + MathHelper.PiOver2);
        //            }
        //        }
        //
        //
        //        // Set the final X, Y and Z axis acceleration for the marble
        //        Acceleration.X = -gravity * (float)Math.Sin(Maze.Rotation.Z - angleX);
        //        Acceleration.Z = gravity * (float)Math.Sin(Maze.Rotation.X - angleZ);
        //        Acceleration.Y = 0;
        //    }
        //    else
        //    {
        //        // If the marble is not touching the floor, it is falling freely
        //        Acceleration.Y = -gravity;
        //    }
        //
        //
        //    if (intersectDetails.IntersectWithWalls)
        //    {
        //        // Change the marble's acceleration due to a collision with a maze wall
        //        UpdateWallCollisionAcceleration(
        //            intersectDetails.IntersectedWallTriangle);
        //    }
        //    if (intersectDetails.IntersectWithFloorSides)
        //    {
        //        // Change the marble's acceleration due to collision with a pit wall
        //        UpdateWallCollisionAcceleration(
        //            intersectDetails.IntersectedFloorSidesTriangle);
        //    }
        }

        /// <summary>
        /// Returns the direction of the collision between the component and a 
        /// triangle.
        /// </summary>
        /// <param name="collideTriangle">The triangle to check.</param>
        /// <returns>The axis at which the collision occurs.</returns>
        //protected Axis CollideDirection(Triangle collideTriangle)
        //{
        //    if (collideTriangle.A.Z == collideTriangle.B.Z && collideTriangle.B.Z == collideTriangle.C.Z)
        //    {
        //        return Axis.Z;
        //    }
        //    else if (collideTriangle.A.X == collideTriangle.B.X && collideTriangle.B.X == collideTriangle.C.X)
        //    {
        //        return Axis.X;
        //    }
        //    else if (collideTriangle.A.Y == collideTriangle.B.Y && collideTriangle.B.Y == collideTriangle.C.Y)
        //    {
        //        return Axis.Y;
        //    }
        //    return Axis.X | Axis.Z;
        //}

        /// <summary>
        /// Update the acceleration when the component collides with walls
        /// </summary>
        /// <param name="wallTriangle">The triangles of the wall that the component 
        /// has collided with.</param>
        //protected void UpdateWallCollisionAcceleration(IEnumerable<Triangle> wallTriangles)
        //{
        //    foreach (var triangle in wallTriangles)
        //    {
        //        Axis direction = CollideDirection(triangle);
        //        // Decrease the acceleration in x-axis of the component
        //        if ((direction & Axis.X) == Axis.X)
        //        {
        //            if (Velocity.X > 0)
        //                Acceleration.X -= wallFriction;
        //            else if (Velocity.X < 0)
        //                Acceleration.X += wallFriction;
        //        }
        //
        //        // Decrease the acceleration in z-axis of the component
        //        if ((direction & Axis.Z) == Axis.Z)
        //        {
        //            if (Velocity.Z > 0)
        //                Acceleration.Z -= wallFriction;
        //            else if (Velocity.Z < 0)
        //                Acceleration.Z += wallFriction;
        //        }
        //    }
        //}

        /// <summary>
        /// Update the velocity when a component collides with walls
        /// </summary>
        /// <param name="wallTriangle">The triangles of the wall that the 
        /// component collided with.</param>
        /// <param name="currentVelocity">The current velocity of the component.</param>
        //protected void UpdateWallCollisionVelocity(IEnumerable<Triangle> wallTriangles, ref Vector3 currentVelocity)
        //{
        //    foreach (var triangle in wallTriangles)
        //    {
        //        Axis direction = CollideDirection(triangle);
        //        // Swap the velocity between x & z if the wall is diagonal
        //        if ((direction & Axis.X) == Axis.X && (direction & Axis.Z) == Axis.Z)
        //        {
        //            float tmp = Velocity.X;
        //            Velocity.X = Velocity.Z;
        //            Velocity.Z = tmp;
        //
        //            tmp = currentVelocity.X;
        //            currentVelocity.X = currentVelocity.Z * 0.3f;
        //            currentVelocity.Z = tmp * 0.3f;
        //        }
        //        // Change the direction of the velocity in the x-axis
        //        else if ((direction & Axis.X) == Axis.X)
        //        {
        //            if ((Position.X > triangle.A.X && Velocity.X < 0) ||
        //                (Position.X < triangle.A.X && Velocity.X > 0))
        //            {
        //                Velocity.X = -Velocity.X * 0.3f;
        //                currentVelocity.X = -currentVelocity.X * 0.3f;
        //            }
        //        }
        //        // Change the direction of the velocity in the z-axis
        //        else if ((direction & Axis.Z) == Axis.Z)
        //        {
        //            if ((Position.Z > triangle.A.Z && Velocity.Z < 0) ||
        //                (Position.Z < triangle.A.Z && Velocity.Z > 0))
        //            {
        //                Velocity.Z = -Velocity.Z * 0.3f;
        //                currentVelocity.Z = -currentVelocity.Z * 0.3f;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Calculate friction between the marble and the maze.
        /// This will manifest as acceleration in a direction opposite to the current
        /// velocity.
        /// </summary>
        protected override void CalculateFriction()
        {
         //   // Calculate the friction when the marble is con the ground
         //   if (intersectDetails.IntersectWithGround)
         //   {
         //       // Calculate the friction in the x-axis
         //       if (Velocity.X > 0)
         //       {
         //           Acceleration.X -= staticGroundFriction * gravity *
         //               (float)Math.Cos(Maze.Rotation.Z - angleX);
         //       }
         //       else if (Velocity.X < 0)
         //       {
         //           Acceleration.X += staticGroundFriction * gravity *
         //               (float)Math.Cos(Maze.Rotation.Z - angleX);
         //       }
         //
         //       // Calculate the friction in z-axis
         //       if (Velocity.Z > 0)
         //       {
         //           Acceleration.Z -= staticGroundFriction * gravity *
         //               (float)Math.Cos(Maze.Rotation.X - angleZ);
         //       }
         //       else if (Velocity.Z < 0)
         //       {
         //           Acceleration.Z += staticGroundFriction * gravity *
         //               (float)Math.Cos(Maze.Rotation.X - angleZ);
         //       }
         //   }
        }

        /// <summary>
        /// Calculate the marble's new velocity and position based on its 
        /// current acceleration.
        /// </summary>
        /// <param name="gameTime">The game time</param>
        protected override void CalculateVelocityAndPosition(GameTime gameTime)
        {
        //    // Calculate the current velocity
        //    float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        //
        //    Vector3 currentVelocity = Velocity;
        //
        //    Velocity = currentVelocity + (Acceleration * elapsed);
        //
        //    // Set a bound on the marble's velocity
        //    Velocity.X = MathHelper.Clamp(Velocity.X, -250, 250);
        //    Velocity.Z = MathHelper.Clamp(Velocity.Z, -250, 250);
        //
        //    // Stop the marble when collide with ground
        //    if (intersectDetails.IntersectWithGround)
        //    {
        //        Velocity.Y = 0;
        //    }
        //
        //    // Update the marble velocity if collide with walls
        //    if (intersectDetails.IntersectWithWalls)
        //    {
        //        UpdateWallCollisionVelocity(
        //            intersectDetails.IntersectedWallTriangle, ref currentVelocity);
        //    }
        //
        //    // Update the marble velocity if collide with floor sides
        //    if (intersectDetails.IntersectWithFloorSides)
        //    {
        //        UpdateWallCollisionVelocity(
        //            intersectDetails.IntersectedFloorSidesTriangle, ref currentVelocity);
        //    }
        //
        //    // If the velocity is low, simply cause the marble to halt
        //    if (-1 < Velocity.X && Velocity.X < 1)
        //    {
        //        Velocity.X = 0;
        //    }
        //    if (-1 < Velocity.Z && Velocity.Z < 1)
        //    {
        //        Velocity.Z = 0;
        //    }
        //
        //    // Update the marble's position
        //    UpdateMovement((Velocity + currentVelocity) / 2, elapsed);
        }

        /// <summary>
        /// Update the marble's position based on momentary velocity.
        /// </summary>
        /// <param name="deltaVelocity">The average velocity between the last two
        /// calls to this method.</param>
        /// <param name="deltaTime">The elapsed time between the last two calls to
        /// this method.</param>
        private void UpdateMovement(Vector3 deltaVelocity, float deltaTime)
        {
        //    // Calculate the change in the marble's position
        //    Vector3 deltaPosition = deltaVelocity * deltaTime;
        //
        //    // Before setting the new position, we must make sure it is legal
        //    BoundingSphere nextPosition = this.BoundingSphereTransformed;
        //    nextPosition.Center += deltaPosition;
        //    IntersectDetails nextIntersectDetails = new IntersectDetails();
        //    Maze.GetCollisionDetails(nextPosition, ref nextIntersectDetails, true);
        //    nextPosition.Radius += 1.0f;
        //
        //    // Move the marble
        //    Position += deltaPosition;
        //
        //    // If the floor not straight then we must reposition the marble vertically
        //    Vector3 forwardVecX = Vector3.Transform(normal,
        //        Matrix.CreateRotationZ(-MathHelper.PiOver2));
        //
        //    Vector3 forwardVecZ = Vector3.Transform(normal,
        //        Matrix.CreateRotationX(-MathHelper.PiOver2));
        //
        //    bool isGroundStraight = true;
        //    if (forwardVecX.X != -1 && forwardVecX.X != 0)
        //    {
        //        Position.Y += deltaPosition.X / forwardVecX.X * forwardVecX.Y;
        //        isGroundStraight = false;
        //    }
        //    if (forwardVecZ.X != -1 && forwardVecZ.X != 0)
        //    {
        //        Position.Y += deltaPosition.Z / forwardVecZ.Z * forwardVecZ.Y;
        //        isGroundStraight = false;
        //    }
        //    // If the marble is already inside the floor, we must reposition it
        //    if (isGroundStraight && nextIntersectDetails.IntersectWithGround)
        //    {
        //        Position.Y = nextIntersectDetails.IntersectedGroundTriangle.A.Y +
        //            BoundingSphereTransformed.Radius;
        //    }
        //
        //    // Finally, we "roll" the marble in accordance to its movement
        //    if (BoundingSphereTransformed.Radius != 0)
        //    {
        //        Rotation.Z = deltaPosition.Z / BoundingSphereTransformed.Radius;
        //        Rotation.X = deltaPosition.X / BoundingSphereTransformed.Radius;
        //    }
        }
        #endregion
    }
}
