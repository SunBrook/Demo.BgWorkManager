namespace DisposeHub.Con
{
    /// <summary>
    /// 写入和配置基础信息
    /// </summary>
    public class ConfigInfo
    {
        private static ConfigInfo info;
        public static ConfigInfo Instance => GetInstance();
        public static ConfigInfo GetInstance()
        {
            if (info == null)
            {
                info = new ConfigInfo();
            }
            return info;
        }

        string sectionConId = "客户端ID";
        string keyConId = "ConId";

        /// <summary>
        /// 设置客户端ID
        /// </summary>
        public void SetConId(string id) => IniConfig.Instance.Write(sectionConId, keyConId, id);

        /// <summary>
        /// 获取客户端ID
        /// </summary>
        /// <returns></returns>
        public string GetConId() => IniConfig.Instance.Read(sectionConId, keyConId);

        string sectionConName = "客户端名称";
        string keyConName = "ConName";

        /// <summary>
        /// 设置客户端名称
        /// </summary>
        public void SetConName(string name) => IniConfig.Instance.Write(sectionConName, keyConName, name);

        /// <summary>
        /// 获取客户端名称
        /// </summary>
        public string GetConName() => IniConfig.Instance.Read(sectionConName, keyConName);
    }
}
