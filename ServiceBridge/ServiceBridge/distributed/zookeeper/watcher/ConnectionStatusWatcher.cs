using org.apache.zookeeper;
using System;
using System.Threading.Tasks;

namespace ServiceBridge.distributed.zookeeper.watcher
{
    public class ConnectionStatusWatcher : Watcher
    {
        private readonly Action<Event.KeeperState> _status_changed;

        public ConnectionStatusWatcher(Action<Event.KeeperState> _status_changed)
        {
            this._status_changed = _status_changed ?? throw new ArgumentNullException(nameof(_status_changed));
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            var status = watchedEvent.getState();

            this._status_changed.Invoke(status);

            await Task.FromResult(1);
        }
    }
}
