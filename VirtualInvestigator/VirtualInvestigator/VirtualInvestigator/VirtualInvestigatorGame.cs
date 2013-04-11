using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices.Sensors;
using Microsoft.Devices;

namespace VirtualInvestigator
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class VirtualInvestigatorGame : Microsoft.Xna.Framework.Game
    {

        Vector3[] positions = new Vector3[] { new Vector3(-30, 0, -15), new Vector3(-30, 0, 45) };
        string[] models = new string[]{ "Army_boots", "blackhat" };

        //when creating a new item:
        //


        Motion motion;
        Matrix attitude;

        Camera camera;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        int bulletCntDwn = 90;

        String yaw = "", pitch = "", roll = "", strCamPos = "";

        String screenData;

        //Accelerometer accelerometer;
        //Compass compass;

        //Basic effect object
        BasicEffect basicEffect;

        // vertex data with position and color
        VertexPositionColor[] pointList;

        // vertex buffer to hold the vertex data
        VertexBuffer vertexBuffer;
        //Vector3 position;

        //Models
        Item[] items;
        List<Bullet> bullets;

        // the left and right hit region on the screen for rotating the axes
        Rectangle recLeft;
        Rectangle recRight;
        Rectangle recUp;
        Rectangle recDown;

        double prevX = 0, prevY = 0, prevZ = 0;
        double prevHeading = 0, heading = 0;

        Vector3 angle;

        Vector2 gestureDelta;

        float rotation = 45;

        public VirtualInvestigatorGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
       

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default forr Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {

          double myX =  e.X ;

          if (Math.Abs(myX - prevX) > 0.035)
            {
                camera.View *= Matrix.CreateRotationX((float)(Math.PI * (myX - prevX)));

                prevX = myX;
            }
         /* else if (myX - prevX < 0.5)
          {
              camera.View *= Matrix.CreateRotationX((float)(Math.PI * (prevX - myX)));

              prevX = myX;
          }*/
         
            
            //if (Math.Abs(e.Y - prevY) > 0.04)
            //{
            //    camera.View *= Matrix.CreateRotationY((float)(Math.PI * (e.Y - prevY)));
            //
            //    prevY = e.Y;
            //}
            
        }

        public void CompassReadingChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            heading = e.SensorReading.TrueHeading;

            if (Math.Abs(heading - prevHeading) > 8 )
            {
                camera.View *= Matrix.CreateRotationY((float)(Math.PI * (heading - prevHeading) / 180 ));

                prevHeading = heading;
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.Identity;

            pointList = new VertexPositionColor[6];
            pointList[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
            pointList[1] = new VertexPositionColor(new Vector3(50, 0, 0), Color.Red);
            pointList[2] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            pointList[3] = new VertexPositionColor(new Vector3(0, 50, 0), Color.White);
            pointList[4] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue);
            pointList[5] = new VertexPositionColor(new Vector3(0, 0, 50), Color.Blue);

            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, 6, BufferUsage.None);
            vertexBuffer.SetData<VertexPositionColor>(pointList);

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            


            //accelerometer = new Accelerometer();
            //accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(AccelerometerReadingChanged);
            //
            //compass = new Compass();
            //compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(400);
            //compass.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<CompassReading>>(CompassReadingChanged);
            //
            //try
            //{
            //    compass.Start();
            //    accelerometer.Start();
            //}
            //catch (Exception e)
            //{
            //    Exit();
            //}

            if (!Motion.IsSupported)
            {
                //MessageBox.Show("the Motion API is not supported on this device.");
                return;
            }

            // If the Motion object is null, initialize it and add a CurrentValueChanged
            // event handler.
            if (motion == null)
            {
                motion = new Motion();
                motion.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);
                motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged);

            }

            // Try to start the Motion API.
            try
            {
                motion.Start();
            }
            catch (Exception)
            {
                //MessageBox.Show("unable to start the Motion API.");
                Exit();
            }

            InitializeCamera();
            InitializeRocket();

            angle = new Vector3();

            // Enable the FreeDrag gesture 
            TouchPanel.EnabledGestures = GestureType.FreeDrag; 
            
            // Define the camera position and the target position 
            camera.position = new Vector3(0, 0, 0);
            Vector3 target = new Vector3(0, 0, -10);
            Matrix original = camera.View;
            camera.View = Matrix.CreateLookAt(camera.position, target, Vector3.Backward);
            // Create the camera View matrix and Projection matrix 
            if (camera.View.Forward.X.Equals(System.Single.NaN) || camera.View.Forward.Y.Equals(System.Single.NaN))
            {
                camera.View = Matrix.CreateLookAt(camera.position, target, Vector3.Up);
            }
            else
            {
                camera.View = Matrix.CreateLookAt(camera.position, target, Vector3.Backward);
            }
            camera.Projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000.0f); 
            
            // Define the four hit regions on touchscreen 
            recUp = new Rectangle(GraphicsDevice.Viewport.Width / 4, 0, GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2); 
            recDown = new Rectangle(GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2); 
            recRight = new Rectangle( 3 * GraphicsDevice.Viewport.Width / 4, 0, GraphicsDevice.Viewport.Width / 4,GraphicsDevice.Viewport.Height); 
            recLeft = new Rectangle(0, 0, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        public void InitializeCamera()
        {
            camera = new Camera(this, GraphicsDevice)
                {
                    originalPosition = new Vector3(0, 0, 0)
                };
            camera.Initialize();
        }

        public void InitializeRocket()
        {
            items = new Item[2];
            
            items[0] = new Item(this, models[0])
            {
                Position = positions[0],
                cam = camera
            };
            items[0].Initialize();
            items[1] = new Item(this, models[1])
            {
                Position = positions[1],
                cam = camera
            };
            items[1].Initialize();
            //
            //rockets[1] = new Rocket(this)
            //{
            //    Position = new Vector3(-100, 0, 0),
            //    cam = camera
            //};
            //rockets[1].Initialize();
            //
            //rockets[2] = new Rocket(this)
            //{
            //    Position = new Vector3(100, 0, 0),
            //    cam = camera
            //};
            //rockets[2].Initialize();
            //
            //rockets[3] = new Rocket(this)
            //{
            //    Position = new Vector3(0, 100, 0),
            //    cam = camera
            //};
            //rockets[3].Initialize();
            //
            //rockets[4] = new Rocket(this)
            //{
            //    Position = new Vector3(0, -100, 0),
            //    cam = camera
            //};
            //rockets[4].Initialize();

            bullets = new List<Bullet>();
        }

        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {
            // This event arrives on a background thread. Use BeginInvoke
            // to call a method on the UI thread.
            //Dispatcher.BeginInvoke(() => CurrentValueChanged(e.SensorReading));
            CurrentValueChanged(e.SensorReading);
        }



        private void CurrentValueChanged(MotionReading reading)
        {

            if (screenData == null)
            {
                screenData = "Yaw : " +reading.Attitude.Yaw.ToString() +"\nPitch : " +reading.Attitude.Pitch.ToString() +"\nRoll : " +reading.Attitude.Roll.ToString();
            }


            // Get the RotationMatrix from the MotionReading.
            // Rotate it 90 degrees around the X axis to put it in xna coordinate system.

            if (reading.Gravity.Z < 0)
                attitude = Matrix.CreateFromYawPitchRoll(-reading.Attitude.Pitch, reading.Attitude.Roll, -reading.Attitude.Yaw); // reading.Attitude.RotationMatrix; //Matrix.CreateRotationZ(MathHelper.PiOver2) * reading.Attitude.RotationMatrix;
            else
            {
                float difference = -MathHelper.Pi - reading.Attitude.Roll;
                float difference2 = MathHelper.Pi - reading.Attitude.Pitch;
                float difference3 = MathHelper.Pi - reading.Attitude.Yaw;
                attitude = Matrix.CreateFromYawPitchRoll(difference2, difference, difference3); // reading.Attitude.RotationMatrix; //Matrix.CreateRotationZ(MathHelper.PiOver2) * reading.Attitude.RotationMatrix;
            }


            camera.View = attitude;

            yaw = reading.Attitude.Yaw.ToString();
            pitch = reading.Attitude.Pitch.ToString();
            roll = reading.Attitude.Roll.ToString();

            // Loop through the points in the list
            //for (int i = 0; i < points.Count; i++)
            //{
            //    // Create a World matrix for the point.
            //    Matrix world = Matrix.CreateWorld(points[i], new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            //
            //    // Use Viewport.Project to project the point from 3D space into screen coordinates.
            //    Vector3 projected = viewport.Project(Vector3.Zero, projection, view, world * attitude);
            //
            //
            //    if (projected.Z > 1 || projected.Z < 0)
            //    {
            //        // If the point is outside of this range, it is behind the camera.
            //        // So hide the TextBlock for this point.
            //        textBlocks[i].Visibility = Visibility.Collapsed;
            //    }
            //    else
            //    {
            //        // Otherwise, show the TextBlock
            //        textBlocks[i].Visibility = Visibility.Visible;
            //
            //        // Create a TranslateTransform to position the TextBlock.
            //        // Offset by half of the TextBlock's RenderSize to center it on the point.
            //        TranslateTransform tt = new TranslateTransform();
            //        tt.X = projected.X - (textBlocks[i].RenderSize.Width / 2);
            //        tt.Y = projected.Y - (textBlocks[i].RenderSize.Height / 2);
            //        textBlocks[i].RenderTransform = tt;
            //    }
            //}
        }




        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //rocket = Content.Load<Model>("missile_2");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            camera.Update(gameTime);

            //bulletCntDwn -= 1;
            //
            //if (bulletCntDwn == 0)
            //{
            //    bulletCntDwn = 90;
            //
            //    bullets.Add(new Bullet(this, camera){ Position = new Vector3(0, 0, -3) });
            //    bullets.Last().Initialize();
            //}

            //move bullets
            foreach (Bullet bullet in bullets)
            {
                bullet.Update(gameTime);
            }

            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
           //     this.Exit();




            foreach(Item rocket in items) 
            {
                rocket.Update(gameTime);
            }

            strCamPos = camera.position.ToString();
            
            // TODO: Add your update logic here
           /* TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0 && touches[0].State == TouchLocationState.Pressed)
            {
                Point point = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);

                if (recUp.Contains(point))
                {
                    //camera.View *= Matrix.CreateRotationX(MathHelper.ToRadians(-5)); //camera.position += new Vector3(0, 0, 5);
                    
                }
                else if(recDown.Contains(point))
                {
                    //camera.View *= Matrix.CreateRotationX(MathHelper.ToRadians(5));  //camera.position += new Vector3(0, 0, -5);
                }
                else if (recLeft.Contains(point))
                {
                    //camera.View *= Matrix.CreateRotationY(MathHelper.ToRadians(-5));
                }
                else if (recRight.Contains(point))
                {
                    //camera.View *= Matrix.CreateRotationY(MathHelper.ToRadians(5));
                }

            }

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gestures = TouchPanel.ReadGesture();
                switch (gestures.GestureType) 
                { 
                    // If the GestureType is FreeDrag 
                    case GestureType.FreeDrag: 
                        // Read the Delta.Y to angle.X, Delta.X to angle.Y 
                        // Because the rotation value around axis Y 
                        // depends on the Delta changing on axis X 
                        angle.X = gestures.Delta.Y * 0.001f; 
                        angle.Y = gestures.Delta.X * 0.001f; 
                        gestureDelta = gestures.Delta; 
                        // Identify the view and rotate it 
                        camera.View *= Matrix.Identity;
                        camera.View *= Matrix.CreateRotationX(angle.X);
                        camera.View *= Matrix.CreateRotationY(angle.Y); 
                    
                        // Reset the angle to next coming gesture. 
                        angle.X = 0; 
                        angle.Y = 0; 
                    break; 
                }

            }*/

            base.Update(gameTime);
        }

        public void DrawModel(Model model) 
        {
            Matrix[] transforms = new Matrix[model.Bones.Count]; 
            model.CopyAbsoluteBoneTransformsTo(transforms); 
            
            // Draw the model. A model can have multiple meshes. 
            foreach (ModelMesh mesh in model.Meshes) 
            {
                foreach (BasicEffect effect in mesh.Effects) 
                {
                    effect.EnableDefaultLighting(); 
                    effect.World = transforms[mesh.ParentBone.Index]; 
                    effect.View = camera.View; 
                    effect.Projection = camera.Projection; 
                } 
                mesh.Draw(); 
            } 
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default; 
            GraphicsDevice.BlendState = BlendState.Opaque;

            basicEffect.World = Matrix.CreateRotationY(MathHelper.ToRadians(rotation)) *
                                Matrix.CreateRotationX(MathHelper.ToRadians(50));
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            basicEffect.VertexColorEnabled = true;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(vertexBuffer, 0);
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, pointList, 0, 3);
            }

            foreach (Bullet bullet in bullets)
            {
                bullet.Draw(gameTime);
            }

            foreach (Item rocket in items)
            {
                rocket.Draw(gameTime);
            }

            string message = string.Format("Current Data \n Yaw: {0} \n Pitch: {1} \n Roll: {2} \n CamPos: {3}", yaw, pitch, roll, strCamPos);

            //spriteBatch.Begin();
            //spriteBatch.DrawString(font, message, new Vector2(50, 50), Color.White);
            //spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
