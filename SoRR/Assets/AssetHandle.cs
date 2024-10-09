using System;

namespace SoRR
{
    public sealed class AssetHandle
    {
        public AssetManager AssetManager { get; }
        public string RelativePath { get; }
        public int Version { get; private set; }

        private ValueSparseList<Action<AssetHandle>> listenersList = new();

        private object? value;
        public object? Value => value ??= AssetManager.LoadNewAssetOrNull(RelativePath);

        internal AssetHandle(AssetManager assetManager, string relativePath, object? currentValue)
        {
            AssetManager = assetManager;
            RelativePath = relativePath;
            value = currentValue;
        }

        public void AddListener(Action<AssetHandle> listener)
            => listenersList.Add(listener);
        public void RemoveListener(Action<AssetHandle> listener)
            => listenersList.Remove(listener);

        internal void TriggerReload()
        {
            value = null;
            Version++;

            var listeners = listenersList.GetItems();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.Invoke(this);
        }

    }
}
