using System;

namespace SoRR
{
    public sealed class AssetHandle
    {
        public AssetManager AssetManager { get; }
        public string RelativePath { get; }
        public int Version { get; private set; }

        private object? value;
        private ValueSparseCollection<Action<AssetHandle>> _listenersCollection = new();

        public object? Value => value ??= AssetManager.LoadNewAssetOrNull(RelativePath);

        internal AssetHandle(AssetManager assetManager, string relativePath, object? currentValue)
        {
            AssetManager = assetManager;
            RelativePath = relativePath;
            value = currentValue;
        }

        public void AddListener(Action<AssetHandle> listener)
        {
            if (listener is null) throw new ArgumentNullException(nameof(listener));
            _listenersCollection.Add(listener);
        }
        public void RemoveListener(Action<AssetHandle> listener)
        {
            if (listener is null) throw new ArgumentNullException(nameof(listener));
            _listenersCollection.Remove(listener);
        }

        internal void TriggerReload()
        {
            value = null;
            Version++;

            var listeners = _listenersCollection.GetItems();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.Invoke(this);
        }

    }
}
