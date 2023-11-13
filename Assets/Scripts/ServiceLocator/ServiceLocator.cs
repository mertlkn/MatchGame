using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceLocator
{
    public class ServiceLocator
    {
        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        private static ServiceLocator _instance;

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }

                return _instance;   
            }
        }

        public void RegisterService<T>(T service) where T : IService
        {
            var type = service.GetType();
            if (services.ContainsKey(type))
            {
                return;
            }
            
            services.Add(type, service);
        }
        
        public T GetService<T>() where T : IService
        {
            var type = typeof(T);
            
            if (services.TryGetValue(type, out var service))
            {
                return (T) service;
            }

            return null;
        }
        
        public void UnregisterService<T>(T service) where T : IService
        {
            var type = service.GetType();
            
            if (services.ContainsKey(type))
            {
                services.Remove(type);
            }
        }
    }
}