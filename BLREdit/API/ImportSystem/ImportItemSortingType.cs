using System.ComponentModel;

namespace BLREdit
{
    public enum ImportItemSortingType
    {
        [Description("Unsorted")]
        None = 0,
        [Description("By Name Ascending")]
        NameAsc = 1,
        [Description("By Name Descending")]
        NameDesc = 2,
        [Description("By Damage Ascending")]
        DamageAsc = 3,
        [Description("By Damage Descending")]
        DamageDesc = 4,
        [Description("By Spread Ascending")]
        SpreadAsc = 5,
        [Description("By Spread Descending")]
        SpreadDesc = 6,
        [Description("By Recoil Ascending")]
        RecoilAsc = 7,
        [Description("By Recoil Descending")]
        RecoilDesc = 8,
        [Description("By Range Ascending")]
        RangeAsc = 9,
        [Description("By Range Descending")]
        RangeDesc = 10,
    }
}
