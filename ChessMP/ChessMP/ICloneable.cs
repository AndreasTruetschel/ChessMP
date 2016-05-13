using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessMP.Model;

namespace ChessMP
{
    interface ICloneable<T>
    {
        T Clone();

        T Clone(Board board);        
    }
}
