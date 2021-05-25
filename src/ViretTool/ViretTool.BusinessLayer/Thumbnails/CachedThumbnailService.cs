//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ViretTool.BusinessLayer.Thumbnails
//{
//    public class CachedThumbnailService<T> : IThumbnailService<T>
//    {
//        public IThumbnailService<T> BaseThumbnailService;

//        private readonly ConcurrentDictionary<(int videoId, int frameId), T> _cache 
//            = new ConcurrentDictionary<(int, int), T>();
//        private readonly LinkedList<(int videoId, int frameId)> _leastRecentlyUsed 
//            = new LinkedList<(int videoId, int frameId)>();
//        private readonly int _cacheSize;

//        public CachedThumbnailService(IThumbnailService<T> thumbnailService, int cacheSize = int.MaxValue)
//        {
//            BaseThumbnailService = thumbnailService;
//            _cacheSize = cacheSize;
//        }

//        public void ClearCache()
//        {
//            _cache.Clear();
//            _leastRecentlyUsed.Clear();
//        }

//        public void TrimCache(int count)
//        {
//            for (int iDropped = _cache.Count; iDropped > count; iDropped--)
//            {
//                _cache.TryRemove(_leastRecentlyUsed.Last.Value, out _);
//                _leastRecentlyUsed.RemoveLast();
//            }
//        }

//        private void CacheThumbnail(int videoId, int frameId, T thumbnail)
//        {
//            _cache.TryAdd((videoId, frameId), thumbnail);
//            TrimCache(_cacheSize);
//        }

//        public T GetThumbnail(int videoId, int frameId)
//        {
//            // try cache
//            if (_cache.TryGetValue((videoId, frameId), out T cachedThumbnail))
//            {
//                return cachedThumbnail;
//            }

//            // else load and then cache
//            T thumbnail = BaseThumbnailService.GetThumbnail(videoId, frameId);
//            CacheThumbnail(videoId, frameId, thumbnail);

//            return thumbnail;
//        }

//        public T[] GetThumbnails(int videoId)
//        {
//            // not cached for now
//            return BaseThumbnailService.GetThumbnails(videoId);
//        }

//        public T[] GetThumbnails(int videoId, int startFrame, int endFrame)
//        {
//            throw new NotImplementedException();
//        }

//        public void Dispose()
//        {
//            BaseThumbnailService.Dispose();
//        }
//    }
//}
