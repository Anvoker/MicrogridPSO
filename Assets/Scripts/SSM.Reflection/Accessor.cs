using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using UnityEngine;

namespace SSM
{
    public class Accessor<T>
    {
        public Func<T> getter;
        public Action<T> setter;

        public Accessor(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public T Get()
        {
            return getter.Invoke();
        }

        public void Set(T arg1)
        {
            setter.Invoke(arg1);
        }
    }

    public class Accessor<T, K>
    {
        public Func<T, K> getter;
        public Action<T, K> setter;

        public Accessor(Func<T, K> getter, Action<T, K> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public K Get(T arg1)
        {
            return getter.Invoke(arg1);
        }

        public void Set(T arg1, K arg2)
        {
            setter.Invoke(arg1, arg2);
        }
    }

    public class Accessor<T1, T2, K>
    {
        public Func<T1, T2, K> getter;
        public Action<T1, T2, K> setter;

        public Accessor(Func<T1, T2, K> getter, Action<T1, T2, K> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public K Get(T1 arg1, T2 arg2)
        {
            return getter.Invoke(arg1, arg2);
        }

        public void Set(T1 arg1, T2 arg2, K arg3)
        {
            setter.Invoke(arg1, arg2, arg3);
        }
    }
}