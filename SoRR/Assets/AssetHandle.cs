using System;

namespace SoRR
{
    public sealed class AssetHandle
    {
        public AssetManager AssetManager { get; }
        public string RelativePath { get; }
        public int Version { get; private set; }

        private ValueSparseList<Action<AssetHandle>> listenersList = new();

        public event Action<AssetHandle> OnRefreshed
        {
            add => AddListener(value);
            remove => RemoveListener(value);
        }

        private object? value;
        public object? Value => value ??= AssetManager.LoadNewAssetInternal(RelativePath);

        public AssetHandle(AssetManager assetManager, string relativePath, object? currentValue)
        {
            AssetManager = assetManager;
            RelativePath = relativePath;
            value = currentValue;
        }

        public void AddListener(Action<AssetHandle> listener)
            => listenersList.Add(listener);
        public void RemoveListener(Action<AssetHandle> listener)
            => listenersList.Remove(listener);

        public void TriggerRefresh()
        {
            value = null;
            Version++;

            var listeners = listenersList.GetItems();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.Invoke(this);
        }

    }
}
