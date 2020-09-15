using WebUtil.Common;

namespace Server.Code
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum StockType
    {
        [Description("kosdaqMkt")]
        Kosdaq,

        [Description("stockMkt")]
        Kospi,

        [Description("konexMkt")]
        Konex,

        [Description("")]
        All,
    }
}
