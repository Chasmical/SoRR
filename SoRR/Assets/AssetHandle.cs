using System;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents a handle for an asset, loaded and managed by an <see cref="SoRR.AssetManager"/>.</para>
    /// </summary>
    public sealed class AssetHandle
    {
        /// <summary>
        ///   <para>Gets the asset manager that is responsible for this asset.</para>
        /// </summary>
        public AssetManager AssetManager { get; }
        /// <summary>
        ///   <para>Gets the asset's path, relative to its asset manager.</para>
        /// </summary>
        public string RelativePath { get; }
        /// <summary>
        ///   <para>Gets the asset's version number, which is incremented on every reload.</para>
        /// </summary>
        public int Version { get; private set; }

        private object? value;
        private ValueSparseCollection<Action<AssetHandle>> _listenersCollection = new();

        /// <summary>
        ///   <para>Gets the asset's current value.</para>
        /// </summary>
        public object? Value => value ??= AssetManager.LoadNewAssetOrNull(RelativePath);

        internal AssetHandle(AssetManager assetManager, string relativePath, object? currentValue)
        {
            Debug.Assert(assetManager is not null && relativePath is not null);

            AssetManager = assetManager;
            RelativePath = relativePath;
            value = currentValue;
        }

        /// <summary>
        ///   <para>Adds the specified <paramref name="listener"/> to the changed event listeners.</para>
        /// </summary>
        /// <param name="listener">The event listener to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="listener"/> is <see langword="null"/>.</exception>
        public void AddListener(Action<AssetHandle> listener)
        {
            if (listener is null) throw new ArgumentNullException(nameof(listener));
            _listenersCollection.Add(listener);
        }
        /// <summary>
        ///   <para>Removes the specified <paramref name="listener"/> from the changed event listeners.</para>
        /// </summary>
        /// <param name="listener">The event listener to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="listener"/> is <see langword="null"/>.</exception>
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
