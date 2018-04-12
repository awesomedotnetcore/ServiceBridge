using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBridge.core
{
    public delegate void RefAction<T>(ref T arg);

    public class Lazy_<T> : IDisposable
    {
        private readonly object _create_lock = new object();
        private readonly object _dispose_lock = new object();
        private readonly Func<T> _creator;

        private RefAction<T> _when_created;
        private RefAction<T> _when_disposed;
        private T _value;
        private bool _created = false;

        public bool IsValueCreated => this._created;

        public bool HasValue => this._created;

        public Lazy_(Func<T> _creator)
        {
            this._creator = _creator ?? throw new ArgumentNullException(nameof(_creator));
            this._created = false;
        }

        public Lazy_<T> WhenCreated(RefAction<T> _when_created)
        {
            this._when_created = _when_created ?? throw new ArgumentNullException(nameof(_when_created));
            return this;
        }

        public Lazy_<T> WhenDispose(RefAction<T> _when_disposed)
        {
            this._when_disposed = _when_disposed ?? throw new ArgumentNullException(nameof(_when_disposed));
            return this;
        }

        public T Value
        {
            get
            {
                if (!this._created)
                {
                    lock (this._create_lock)
                    {
                        if (!this._created)
                        {
                            this._value = this._creator.Invoke();
                            this._created = true;
                            //创建时调用
                            this._when_created?.Invoke(ref this._value);
                        }
                    }
                }

                return this._value;
            }
        }

        public void Dispose()
        {
            if (this._created)
            {
                lock (this._dispose_lock)
                {
                    if (this._created)
                    {
                        //销毁时调用
                        this._when_disposed?.Invoke(ref this._value);
                        //初始化
                        this._value = default(T);
                        this._created = false;
                    }
                }
            }
        }
    }
}
