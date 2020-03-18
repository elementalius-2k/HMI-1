using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstProject
{
    public class ToCompare//: IComparable
    {        
        public float First;
        public float Second;
        
        public double CompareTo(object obj)
        {
            ToCompare another = obj as ToCompare;
            return (First == another.First) ? Second - another.Second : First - another.First;
        }

        
    }
}
