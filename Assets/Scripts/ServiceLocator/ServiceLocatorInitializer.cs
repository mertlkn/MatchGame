using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceLocator
{
    [Serializable]
    public class ServiceInfo
    {
        public IService service;
        public bool destroyOnLoad;
        
    }
    [DefaultExecutionOrder(-1)]
    public class ServiceLocatorInitializer : MonoBehaviour
    {
        [SerializeField] private List<ServiceInfo> services;
        
        private void Awake()
        {
            foreach (var service in services)
            {
                ServiceLocator.Instance.RegisterService(service.service);
            }
        }
        
        private void OnDestroy()
        {
            foreach (var service in services)
            {
                if (service.destroyOnLoad)
                {
                    ServiceLocator.Instance.UnregisterService(service.service);
                }
            }
        }
    }
}