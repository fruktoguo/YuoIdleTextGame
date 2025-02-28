namespace YuoTools.Extend.Helper
{
    /// <summary>
    /// 正则表达式帮助类
    /// </summary>
    public class RegexHelper
    {
        /// <summary>
        /// 邮箱正则表达式
        /// </summary>
        public const string 邮箱 = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        /// <summary>
        /// 手机号码正则表达式
        /// </summary>
        public const string 手机号码 = @"^1[3|4|5|7|8]\d{9}$";

        /// <summary>
        /// 电话号码正则表达式
        /// </summary>
        public const string 电话号码 = @"^(\(\d{3,4}\)|\d{3,4}-|\s)?\d{7,8}$";

        /// <summary>
        /// URL地址正则表达式
        /// </summary>
        public const string URL地址 = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";

        /// <summary>
        /// 身份证号码正则表达式
        /// </summary>
        public const string 身份证号码 = @"^(\d{15}|\d{18})$";

        /// <summary>
        /// 邮政编码正则表达式
        /// </summary>
        public const string 邮政编码 = @"^\d{6}$";

        /// <summary>
        /// 数字正则表达式
        /// </summary>
        public const string 数字 = @"^\d+$";

        /// <summary>
        /// 整数正则表达式
        /// </summary>
        public const string 整数 = @"^[-\+]?\d+$";

        /// <summary>
        /// 双精度浮点数正则表达式
        /// </summary>
        public const string 双精度浮点数 = @"^[-\+]?\d+(\.[0-9]+)?$";

        /// <summary>
        /// 英文字母正则表达式
        /// </summary>
        public const string 英文字母 = @"^[A-Za-z]+$";

        /// <summary>
        /// 中文字符正则表达式
        /// </summary>
        public const string 中文字符 = @"^[\u4e00-\u9fa5]+$";

        /// <summary>
        /// 中英文字符正则表达式
        /// </summary>
        public const string 中英文字符 = @"^[\u4e00-\u9fa5A-Za-z]+$";

        /// <summary>
        /// 身份证号码正则表达式（多种格式）
        /// </summary>
        public const string 身份证号码多种格式 = @"^([0-9]){7,18}(x|X)?$ 或 ^\d{8,18}|[0-9x]{8,18}|[0-9X]{8,18}?$";

        /// <summary>
        /// 账号正则表达式
        /// </summary>
        public const string 账号 = "^[a-zA-Z][a-zA-Z0-9_]{4,15}$";

        /// <summary>
        /// 密码正则表达式
        /// </summary>
        public const string 密码 = @"^[a-zA-Z]\w{5,17}$";

        /// <summary>
        /// 强密码正则表达式
        /// </summary>
        public const string 强密码 = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,10}$";

        /// <summary>
        /// 日期格式正则表达式
        /// </summary>
        public const string 日期格式 = @"^\d{4}-\d{1,2}-\d{1,2}";

        /// <summary>
        /// 腾讯QQ号正则表达式
        /// </summary>
        public const string 腾讯QQ号 = "[1-9][0-9]{4,} (腾讯QQ号从10000开始)";

        /// <summary>
        /// 中国邮政编码正则表达式
        /// </summary>
        public const string 中国邮政编码 = @"[1-9]\d{5}(?!\d)";

        /// <summary>
        /// IP地址正则表达式
        /// </summary>
        public const string IP地址 = @"\d+\.\d+\.\d+\.\d+";

        /// <summary>
        /// IPv4地址正则表达式
        /// </summary>
        public const string IPv4地址 = "\\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\b";

        /// <summary>
        /// 子网掩码正则表达式
        /// </summary>
        public const string 子网掩码 = "((?:(?:25[0-5]|2[0-4]\\d|[01]?\\d?\\d)\\.){3}(?:25[0-5]|2[0-4]\\d|[01]?\\d?\\d))";

        /// <summary>
        /// 抽取注释正则表达式
        /// </summary>
        public const string 抽取注释 = "<!--(.*?)-->";

        /// <summary>
        /// 判断IE版本正则表达式
        /// </summary>
        public const string 判断IE版本 = "^.*MSIE [5-8](?:\\.[0-9]+)?(?!.*Trident\\/[5-9]\\.0).*$";

        /// <summary>
        /// 匹配HTML标签的正则表达式
        /// </summary>
        public const string HTML标签 = @"<[^>]+>";

        /// <summary>
        /// 匹配空白字符的正则表达式
        /// </summary>
        public const string 空白字符 = @"\s+";

        /// <summary>
        /// 匹配非空白字符的正则表达式
        /// </summary>
        public const string 非空白字符 = @"\S+";

        /// <summary>
        /// 匹配十六进制颜色代码的正则表达式
        /// </summary>
        public const string 十六进制颜色代码 = @"#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})";

        /// <summary>
        /// 匹配时间格式（HH:mm:ss）的正则表达式
        /// </summary>
        public const string 时间格式 = @"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$";

        /// <summary>
        /// 匹配日期时间格式（yyyy-MM-dd HH:mm:ss）的正则表达式
        /// </summary>
        public const string 日期时间格式 = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$";

        /// <summary>
        /// 匹配浮点数的正则表达式
        /// </summary>
        public const string 浮点数 = @"^[-+]?\d*\.?\d+$";

        /// <summary>
        /// 匹配MAC地址的正则表达式
        /// </summary>
        public const string MAC地址 = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";

        /// <summary>
        /// 匹配UUID的正则表达式
        /// </summary>
        public const string UUID = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
    }
}