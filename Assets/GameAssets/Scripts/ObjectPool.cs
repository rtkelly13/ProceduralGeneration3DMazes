using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts
{
    //Taken from
    //http://www.gamasutra.com/blogs/WendelinReich/20131127/203843/C_Memory_Management_for_Unity_Developers_part_3_of_3.php
    public class ObjectPool<T> where T : class, new()
    {
        private Stack<T> m_objectStack;

        private Action<T> m_resetAction;
        private Action<T> m_onetimeInitAction;

        public ObjectPool(int initialBufferSize, Action<T>
            ResetAction = null, Action<T> OnetimeInitAction = null)
        {
            m_objectStack = new Stack<T>(initialBufferSize);
            m_resetAction = ResetAction;
            m_onetimeInitAction = OnetimeInitAction;
        }

        public T New()
        {
            if (m_objectStack.Count > 0)
            {
                T t = m_objectStack.Pop();

                if (m_resetAction != null)
                    m_resetAction(t);

                return t;
            }
            else
            {
                T t = new T();

                if (m_onetimeInitAction != null)
                    m_onetimeInitAction(t);

                return t;
            }
        }

        public void Store(T obj)
        {
            m_objectStack.Push(obj);
        }

        public void Store(IEnumerable<T> objs)
        {
            foreach (var obj in objs)
            {
                m_objectStack.Push(obj);
            }
        }

        public IEnumerable<T> New(int length)
        {
            for (var i = 0; i < length; i++)
            {
                yield return New();
            }
        }
    }
}
