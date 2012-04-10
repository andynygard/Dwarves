﻿// ----------------------------------------------------------------------------
// <copyright file="DwarvesWorld.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves
{
    using Dwarves.Data;
    using Dwarves.Subsystem;
    using EntitySystem;
    using EntitySystem.Data;
    using FarseerPhysics.Dynamics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// The Dwarves game world.
    /// </summary>
    public class DwarvesWorld
    {
        #region Private Variables

        /// <summary>
        /// The Entity System world.
        /// </summary>
        private EntitySystemWorld entitySystemWorld;

        /// <summary>
        /// The data adapter responsible for loading and saving the game content.
        /// </summary>
        private EntityDataAdapter entityDataAdapter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the DwarvesWorld class.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="content">The content manager.</param>
        public DwarvesWorld(GraphicsDevice device, ContentManager content)
        {
            // Create the entity system world
            this.entitySystemWorld = new EntitySystemWorld();

            // Create the physics world
            var physicsWorld = new World(new Vector2(0.0f, -9.8f));

            // Create the world context
            this.World = new WorldContext(this.entitySystemWorld.EntityManager, physicsWorld, content);

            var updateSystems = this.entitySystemWorld.UpdateSystemManager;
            var drawSystems = this.entitySystemWorld.DrawSystemManager;

            // Create update systems
            updateSystems.AddSystem(new InputSystem(this.World.EntityManager, device));
            updateSystems.AddSystem(new PhysicsSystem(this.World.EntityManager, this.World.Physics));

            // Create draw systems
            drawSystems.AddSystem(new DebugDrawSystem(this.World.EntityManager, this.World.Physics, device, content));

            // Create the data adapter
            DwarvesConfig config = content.Load<DwarvesConfig>("DwarvesConfig");
            this.entityDataAdapter = new DwarvesDataAdapter(config);

#if DEBUG
            ////////////////////////////
            // Development/Debug code //
            ///////////////////////////

            // Load the test level
            var debugWorldLoader = new Dwarves.Debug.DebugWorldLoader();
            debugWorldLoader.LoadTest1(this.World);

            // Save the test level to the database
            this.entityDataAdapter.SaveLevel(this.World.EntityManager, 1);
#endif
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the world.
        /// </summary>
        public WorldContext World { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the world state.
        /// </summary>
        /// <param name="delta">The number of milliseconds since the last step.</param>
        public void Step(int delta)
        {
            // Process the update-related systems
            this.entitySystemWorld.Step(delta);
        }

        /// <summary>
        /// Draw the world state.
        /// </summary>
        /// <param name="delta">The number of milliseconds since the last draw.</param>
        public void Draw(int delta)
        {
            // Process the draw-related systems
            this.entitySystemWorld.Draw(delta);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialise the update systems.
        /// </summary>
        private void InitSystems()
        {
        }

        #endregion
    }
}