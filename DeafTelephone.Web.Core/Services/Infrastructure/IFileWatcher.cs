namespace DeafTelephone.Web.Core.Services.Infrastructure
{
    using System;

    public interface IFileWatcher
    {
        /// <summary>
        /// Watch for file changes and rise an event if change is occured
        /// </summary>
        /// <param name="fileName">file path + name</param>
        /// <param name="onChanged">event on file change</param>
        void WatchForFile(string fileName, Action onChanged);
    }
}
