using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace News.Models
{
    public class CacheData
    {
        public NewsCategory Category { get; set; }
        public DateTime Time { get; set; }
        public News News { get; set; }

        public static void Serialize(string fname, CacheData cachedData)
        {
            var _locker = new object();
            lock (_locker)
            {
                var xs = new XmlSerializer(typeof(CacheData));
                using (Stream s = File.Create(fname))
                    xs.Serialize(s, cachedData);
            }
        }

        public static CacheData Deserialize(string fname)
        {
            var _locker = new object();
            lock (_locker)
            {
                CacheData cachedData;
                var xs = new XmlSerializer(typeof(CacheData));

                using (Stream s = File.OpenRead(fname))
                    cachedData = (CacheData)xs.Deserialize(s);

                return cachedData;
            }
        }
    }
}
