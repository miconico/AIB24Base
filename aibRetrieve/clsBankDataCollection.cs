using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace aibRetrieve
{
    public class clsBankDataCollection : IEnumerable   
    {
        private ArrayList m_BankDataList;

        public clsBankDataCollection()
        {
            m_BankDataList = new ArrayList();
        }

        public Song this[int index]
        {
            get
            {
                return (clsBankData)m_BankDataList[index];
            }
            set
            {
                m_BankDataList[index] = value;
            }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new BankDataEnumerator(this);
        }

        #endregion

        private class BankDataEnumerator : IEnumerator
        {
            private clsBankDataCollection m_BankDataRef;
            private int m_location;

            public BankDataEnumerator(clsBankDataCollection BankDataRef)
            {
                this.m_BankDataRef = BankDataRef;
                m_location = -1;
            }

            #region Implementation of IEnumerator
            public void Reset()
            {
                m_location = -1;
            }

            public bool MoveNext()
            {
                m_location++;
                return (m_location <= (m_BankDataRef.m_BankDataList.Count - 1));
            }

            public object Current
            {
                get
                {
                    if ((m_location < 0) || (m_location > m_BankDataRef.m_BankDataList.Count)) 
                    {
                        return null;
                    }
                    else
                    {
                        return m_BankDataRef.m_BankDataList[m_location];
                    }
                }


                }
            }
            #endregion

        }


    }

}
