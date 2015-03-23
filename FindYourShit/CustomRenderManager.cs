using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace fys
{
    class CustomRenderManager : IRenderableManager
    {
        private DrawCallData m_calldata;

        public void BeginOverlay(RenderManager.CameraInfo cameraInfo)
        {
        }

        public void BeginRendering(RenderManager.CameraInfo cameraInfo)
        {
        }

        public bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            return false;
        }

        public void EndOverlay(RenderManager.CameraInfo cameraInfo)
        {
        }

        public void EndRendering(RenderManager.CameraInfo cameraInfo)
        {
            // You're free to take this as inspiration, but please do NOT copy this to your own mod and release it on the Steam Workshop.
            // http://steamcommunity.com/sharedfiles/filedetails/?id=411048716 includes it, please link people to that mod instead.

            // I'm sure you might come up with something more creative.
            if (FindIt.renderMode == RenderMode.DEFAULT)
                return;

            // FIXME maybe: not locked
            float scale = FindIt.renderMode == RenderMode.SMALL_ICONS ? 0.75f : 2.5f;

            var currentTarget = WorldInfoPanel.GetCurrentInstanceID();
            object L = GetPrivateVariable<object>(InstanceManager.instance, "m_lock");
            do { }
            while (!Monitor.TryEnter(L, SimulationManager.SYNCHRONIZE_TIMEOUT));

            try
            {
                var dict = GetPrivateVariable<Dictionary<InstanceID, string>>(InstanceManager.instance, "m_names");
                foreach (var instance in dict)
                {
                    // Ignore the current person
                    if (instance.Key.Equals(currentTarget) || instance.Key.Type == InstanceType.District)
                        continue;

                    Vector3 position;
                    Quaternion rotation;
                    Vector3 size;

                    if (!InstanceManager.GetPosition(instance.Key, out position, out rotation, out size))
                        continue;

                    position.y += (float)((double)size.y * 0.850000023841858 + 2.0);

                    // The only reason this is here is because we need access to the camera info, which we would otherwise not have. This is also how BuildingManager does it.
                    // NotificationManager.AddEvent() would be useful, but creates an upwards-floating effect
                    NotificationEvent.RenderInstance(cameraInfo, NotificationEvent.Type.LocationMarker, position, scale, 1f);
                }
            }
            finally
            {
                Monitor.Exit(L);
            }
        }

        public DrawCallData GetDrawCallData()
        {
            return m_calldata;
        }
        private T GetPrivateVariable<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }

        public string GetName()
        {
            return "FindYourShitCustomRenderer";
        }

        public void InitRenderData()
        {
            m_calldata.m_batchedCalls = 0;
            m_calldata.m_defaultCalls = 0;
            m_calldata.m_lodCalls = 0;
            m_calldata.m_overlayCalls = 0;
        }

        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
        }

        public void UndergroundOverlay(RenderManager.CameraInfo cameraInfo)
        {
        }
    }
}
