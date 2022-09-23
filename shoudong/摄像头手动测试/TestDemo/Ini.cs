using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    public class Ini
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()  
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()         
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern uint GetPrivateProfileStringA(string section, string key,
            string def, Byte[] retVal, int size, string filePath);
        private string sPath = null;
        public Ini(string path)
        {
            this.sPath = path;
        }


        //为某个节写入key
        //如果这个key已存在，那么久是更新此key
        public void Write(string section, string key, string value)
        {

            // section=配置节，key=键名，value=键值，path=路径  

            WritePrivateProfileString(section, key, value, sPath);

        }


        //读取某个key的值
        public string ReadValue(string section, string key)
        {

            // 每次从ini中读取多少字节 （最多2048个字符） 

            StringBuilder temp = new StringBuilder(2048);

            // section=配置节，key=键名，temp=上面，读取该键名的值最大长度，path=路径  

            GetPrivateProfileString(section, key, "", temp, 2048, sPath);


            return temp.ToString();

        }

        //读取所有的节
        public List<string> ReadSections()
        {
            return ReadSections(sPath);
        }

        public List<string> ReadSections(string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(null, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }

        //读取节下所有的keys
        public List<string> ReadKeys(String SectionName)
        {
            return ReadKeys(SectionName, sPath);
        }

        public List<string> ReadKeys(string SectionName, string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(SectionName, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }

        //删除某个节下的key
        public void deleteKey(string section, string key)
        {
            WritePrivateProfileString(section, key, null, sPath);
        }

        //删除某个节
        public void deleteSection(string section)
        {
            WritePrivateProfileString(section, null, null, sPath);
        }
    }


}
