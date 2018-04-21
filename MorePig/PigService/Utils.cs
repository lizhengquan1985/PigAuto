using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigService
{
    public class Utils
    {
        /// <summary>
        /// 通过id获得时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DateTime GetDateById(long id)
        {
            return new DateTime(id * 10000000 + new DateTime(1970, 1, 1, 8, 0, 0).Ticks);
        }
    }
}
