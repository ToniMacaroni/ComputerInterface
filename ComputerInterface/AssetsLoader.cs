using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComputerInterface
{
    internal class AssetsLoader : IDisposable
    {
        public bool IsLoaded { get; private set; }

        private AssetBundle _loadedBundle;
        private Task _loadingTask;

        public async Task<T> GetAsset<T>(string name) where T : Object
        {
            if (!IsLoaded)
            {
                if (_loadingTask == null)
                {
                    _loadingTask = LoadBundleAsyncInternal();
                }

                await _loadingTask;
            }

            var completionSource = new TaskCompletionSource<T>();

            var assetBundleRequest = _loadedBundle.LoadAssetAsync<T>(name);
            assetBundleRequest.completed += _ =>
            {
                if (assetBundleRequest.asset == null)
                {
                    Debug.LogError($"Asset {name} not found");
                    completionSource.SetResult(null);
                    return;
                }

                completionSource.SetResult((T)assetBundleRequest.asset);
            };

            return await completionSource.Task;
        }

        private async Task LoadBundleAsyncInternal()
        {
            var completionSource = new TaskCompletionSource<AssetBundle>();

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ComputerInterface.Resources.assets");
            if (stream == null)
            {
                Debug.LogError("Couldn't load embedded assets");
                return;
            }

            var assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(stream);
            assetBundleCreateRequest.completed += _ =>
            {
                completionSource.SetResult(assetBundleCreateRequest.assetBundle);
            };

            _loadedBundle = await completionSource.Task;

            IsLoaded = true;
        }

        public void Dispose()
        {
            _loadedBundle.Unload(true);
        }
    }
}