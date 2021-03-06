﻿using Microsoft.Practices.Unity;

namespace DataCloner.Universal.Registries
{
    /// <summary>
    /// Base class for registering implementations to the Unity container.
    /// </summary>
    public class RegistryBase
    {
        /// <summary>
        /// The Unity container.
        /// </summary>
        protected IUnityContainer Container { get; set; }
        
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="container">The Unity container.</param>
        protected RegistryBase(IUnityContainer container)
        {
            Container = container;
        }
    }
}
