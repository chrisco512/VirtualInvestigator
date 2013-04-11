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
// ----------------------------------------------------------------------------------

#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace VirtualInvestigator
{
    public class Camera : GameComponent
    {
        #region Fields
        public Vector3 originalPosition;
        public Vector3 position = Vector3.Zero;
        //Vector3 target = Vector3.Zero;
        GraphicsDevice graphicsDevice;

        //public Vector3 ObjectToFollow { get; set; }
        public Matrix Projection { get; set; }
        public Matrix View { get; set; }

        //readonly Vector3 cameraPositionOffset = new Vector3(0, 450 * 1, 100 * 1);
        //readonly Vector3 cameraTargetOffset = new Vector3(0, 0, -50 * 0);
        #endregion

        #region Initializtion
        public Camera(Game game, GraphicsDevice graphics)
            : base(game)
        {
            this.graphicsDevice = graphics;
        }

        /// <summary>
        /// Initialize the camera
        /// </summary>
        public override void Initialize()
        {
            // Create the projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50), 
                graphicsDevice.Viewport.AspectRatio, 1, 10000);

            base.Initialize();
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the camera to follow the object it is set to follow.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Make the camera follow the object
            //position = ObjectToFollow + cameraPositionOffset;

            //target = ObjectToFollow + cameraTargetOffset;

            // Create the view matrix
            View *= Matrix.CreateTranslation(position - originalPosition);

            originalPosition = position;

            base.Update(gameTime);
        }
        #endregion Update

        public Vector3 getPosition()
        {
            return position;
        }
    }
}
