using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

namespace SSM.Grid
{
    public static class MGMisc
    {
        public class MicrogridAccessors<T> : Dictionary<MicrogridVar, Accessor<Microgrid, T>> { }

        public static readonly Dictionary<MicrogridVar, string>         enumToHeader;
        public static readonly Dictionary<MicrogridVar, string[]>       enumToMetadata;
        public static readonly Dictionary<MicrogridVar, Color?>         enumToColor;
        public static readonly MicrogridAccessors<List<float>>          accessorLists;
        public static readonly MicrogridAccessors<float[,]>             accessor2DArray;
        public static readonly MicrogridAccessors<float>                accessorFloats;
        public static readonly MicrogridAccessors<bool>                 accessorBools;

        static MGMisc()
        {
            enumToHeader = new Dictionary<MicrogridVar, string>()
            {
                [MicrogridVar.Time]                    = "time",
                [MicrogridVar.TCount]                  = "tcount",
                [MicrogridVar.TSize]                   = "tsize",
                [MicrogridVar.PPV]                     = "ppv",
                [MicrogridVar.PW]                      = "pw",
                [MicrogridVar.PCHPSum]                 = "pmg",
                [MicrogridVar.PBat]                    = "pbat",
                [MicrogridVar.PLoad]                   = "pc",
                [MicrogridVar.PSys]                    = "surplus",
                [MicrogridVar.SoC]                     = "soc",
                [MicrogridVar.CBat]                    = "cbat1",
                [MicrogridVar.EBatMax]                 = "ebat",
                [MicrogridVar.PBatMax]                 = "pbat1",
                [MicrogridVar.PCHPMax]                 = "pmgi",
                [MicrogridVar.MinTimeUp]               = "mintimeup",
                [MicrogridVar.MinTimeDown]             = "mintimedown",
                [MicrogridVar.CBatInitial]             = "cbatinitial",
                [MicrogridVar.SoCInitial]              = "socinitial",
                [MicrogridVar.SoCMin]                  = "socmin",
                [MicrogridVar.SoCMax]                  = "socmax",
                [MicrogridVar.Price]                   = "clearingprice",
                [MicrogridVar.CostFuncA]               = "costa",
                [MicrogridVar.CostFuncB]               = "costb",
                [MicrogridVar.CostFuncC]               = "costc",
                [MicrogridVar.PCHPTotal]               = "PCHPTotal",
                [MicrogridVar.PSysTotal]               = "PSysTotal",
                [MicrogridVar.CHPCount]                = "CHPCount",
                [MicrogridVar.CostSys]                 = "CostSys",
                [MicrogridVar.CostSysTotal]            = "CostSysTotal",
                [MicrogridVar.PCHP]                    = "p_chp",
                [MicrogridVar.CCHP]                    = "c_chp",
                [MicrogridVar.UCHP]                    = "u_chp",
                [MicrogridVar.PTarget]                 = "p_target",
                [MicrogridVar.PRes]                    = "p_res",
                [MicrogridVar.PSum]                    = "p_res",
            };

            enumToMetadata = new Dictionary<MicrogridVar, string[]>()
            {
                [MicrogridVar.Time]        = new string[] { "Time", "Minutes" },
                [MicrogridVar.PPV]         = new string[] { "Power", "MW" },
                [MicrogridVar.PW]          = new string[] { "Power", "MW" },
                [MicrogridVar.PCHPSum]     = new string[] { "Power", "MW" },
                [MicrogridVar.PBat]        = new string[] { "Power", "MW" },
                [MicrogridVar.PLoad]       = new string[] { "Power", "MW" },
                [MicrogridVar.PSys]        = new string[] { "Energy", "MWh" },
                [MicrogridVar.SoC]         = new string[] { "State of Charge", "-" },
                [MicrogridVar.CBat]        = new string[] { "Charging Mode", "-" },
                [MicrogridVar.Price]       = new string[] { "Money", "€" },
                [MicrogridVar.CostSys]     = new string[] { "Power", "MW" },
                [MicrogridVar.PCHP]        = new string[] { "Power", "MW" },
                [MicrogridVar.UCHP]        = new string[] { "CHP State", "-" },
                [MicrogridVar.CCHP]        = new string[] { "Money", "€" },
                [MicrogridVar.PTarget]     = new string[] { "Power", "MW" },
                [MicrogridVar.PRes]        = new string[] { "Power", "MW" },
                [MicrogridVar.PSum]        = new string[] { "Power", "MW" },
            };

            var red     = new Color(0.90f, 0.05f, 0.05f, 1.00f);
            var blue    = new Color(0.05f, 0.05f, 0.90f, 1.00f);
            var orange  = new Color(0.90f, 0.45f, 0.00f, 1.00f);
            var green   = new Color(0.05f, 0.90f, 0.05f, 1.00f);
            var cyan    = new Color(0.05f, 0.90f, 0.90f, 1.00f);
            var magenta = new Color(0.90f, 0.05f, 0.90f, 1.00f);

            enumToColor = new Dictionary<MicrogridVar, Color?>()
            {
                [MicrogridVar.Time]        = Color.black,
                [MicrogridVar.PPV]         = blue,
                [MicrogridVar.PW]          = blue,
                [MicrogridVar.PRes]        = blue,
                [MicrogridVar.PCHPSum]     = orange,
                [MicrogridVar.PBat]        = red,
                [MicrogridVar.PLoad]       = Color.black,
                [MicrogridVar.PSys]        = magenta,
                [MicrogridVar.SoC]         = cyan,
                [MicrogridVar.CBat]        = green,
                [MicrogridVar.PTarget]     = green,
            };

            accessorLists = new MicrogridAccessors<List<float>>()
            {
                [MicrogridVar.PPV] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Input.p_pv),
                    (m, x) => m.Input.p_pv = x.ToArray()),

                [MicrogridVar.PW] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.p_w),
                    (m, x) => m.Input.p_w = x.ToArray()),

                [MicrogridVar.PCHPSum] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Result.p_thr_sum),
                    (m, x) => m.Result.p_thr_sum = x.ToArray()),

                [MicrogridVar.PBat] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Result.p_bat),
                    (m, x) => m.Result.p_bat = x.ToArray()),

                [MicrogridVar.PLoad] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.p_load),
                    (m, x) => m.Input.p_load = x.ToArray()),

                [MicrogridVar.PSys] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Result.p_sys),
                    (m, x) => m.Result.p_sys = x.ToArray()),

                [MicrogridVar.SoC] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Result.soc),
                    (m, x) => m.Result.soc = x.ToArray()),

                [MicrogridVar.CBat] = new Accessor<Microgrid, List<float>>(
                    (m)    => new List<float>(m.Result.c_bat),
                    (m, x) => m.Result.c_bat = x.ToArray()),

                [MicrogridVar.PCHPMax] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.p_thr_max),
                    (m, x) => m.Input.p_thr_max = x.ToArray()),

                [MicrogridVar.MinTimeUp] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.thr_min_utime),
                    (m, x) => m.Input.thr_min_utime = x.ToArray()),

                [MicrogridVar.MinTimeDown] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.thr_min_dtime),
                    (m, x) => m.Input.thr_min_dtime = x.ToArray()),

                [MicrogridVar.CostFuncA] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.thr_c_a),
                    (m, x) => m.Input.thr_c_a = x.ToArray()),

                [MicrogridVar.CostFuncB] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.thr_c_b),
                    (m, x) => m.Input.thr_c_b = x.ToArray()),

                [MicrogridVar.CostFuncC] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.thr_c_c),
                    (m, x) => m.Input.thr_c_c = x.ToArray()),

                [MicrogridVar.Price] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Input.price),
                    (m, x) => m.Input.price= x.ToArray()),

                [MicrogridVar.CostSys] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Result.c_sys),
                    (m, x) => m.Result.c_sys = x.ToArray()),

                [MicrogridVar.PTarget] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Result.p_target),
                    (m, x) => m.Result.p_target = x.ToArray()),

                [MicrogridVar.PRes] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Result.p_res),
                    (m, x) => m.Result.p_res = x.ToArray()),

                [MicrogridVar.PSum] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Result.p_sum),
                    (m, x) => m.Result.p_sum = x.ToArray()),

                [MicrogridVar.PThrSumMax] = new Accessor<Microgrid, List<float>>(
                    (m) => new List<float>(m.Result.p_thr_max_sum),
                    (m, x) => m.Result.p_thr_max_sum = x.ToArray()),
            };

            accessor2DArray = new MicrogridAccessors<float[,]>()
            {
                [MicrogridVar.PCHP] = new Accessor<Microgrid, float[,]>(
                    (m) =>
                    {
                        if (m.Result.p_thr == null)
                        {
                            return new float[0,0];
                        }

                        float[,] arr = new float[m.Result.p_thr.GetLength(0),
                                                 m.Result.p_thr.GetLength(1)];
                        Array.Copy(m.Result.p_thr, arr, m.Result.p_thr.Length);
                        return arr;
                    },
                    (m, x) => Array.Copy(x, m.Result.p_thr, x.Length)),

                [MicrogridVar.UCHP] = new Accessor<Microgrid, float[,]>(
                    (m) =>
                    {
                        if (m.Result.u_thr == null)
                        {
                            return new float[0, 0];
                        }

                        float[,] arr = new float[m.Result.u_thr.GetLength(0),
                                                 m.Result.u_thr.GetLength(1)];
                        Array.Copy(m.Result.u_thr, arr, m.Result.u_thr.Length);
                        return arr;
                    },
                    (m, x) => Array.Copy(x, m.Result.u_thr, x.Length)),

                [MicrogridVar.CCHP] = new Accessor<Microgrid, float[,]>(
                    (m) =>
                    {
                        if (m.Result.c_thr == null)
                        {
                            return new float[0, 0];
                        }

                        float[,] arr = new float[m.Result.c_thr.GetLength(0),
                                                 m.Result.c_thr.GetLength(1)];
                        Array.Copy(m.Result.c_thr, arr, m.Result.c_thr.Length);
                        return arr;
                    },
                    (m, x) => Array.Copy(x, m.Result.c_thr, x.Length))
            };

            accessorFloats = new MicrogridAccessors<float>()
            {
                [MicrogridVar.EBatMax] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.e_bat_max,
                    (m, x) => m.Input.e_bat_max = x),

                [MicrogridVar.PBatMax] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.p_bat_max,
                    (m, x) => m.Input.p_bat_max = x),

                [MicrogridVar.CBatInitial] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.c_bat_init,
                    (m, x) => m.Input.c_bat_init = Mathf.FloorToInt(x)),

                [MicrogridVar.SoCInitial] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.soc_ini,
                    (m, x) => m.Input.soc_ini = x),

                [MicrogridVar.SoCMin] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.soc_min,
                    (m, x) => m.Input.soc_min = x),

                [MicrogridVar.SoCMax] = new Accessor<Microgrid, float>(
                    (m)    => m.Input.soc_max,
                    (m, x) => m.Input.soc_max = x),

                [MicrogridVar.PCHPTotal] = new Accessor<Microgrid, float>(
                    (m) => m.Result.e_thr_total,
                    (m, x) => m.Result.e_thr_total = x),

                [MicrogridVar.PSysTotal] = new Accessor<Microgrid, float>(
                    (m) => m.Result.e_sys_total,
                    (m, x) => m.Result.e_sys_total = x),

                [MicrogridVar.TCount] = new Accessor<Microgrid, float>(
                    (m) => m.Input.tCount,
                    (m, x) => m.Input.tCount = Mathf.FloorToInt(x)),

                [MicrogridVar.CHPCount] = new Accessor<Microgrid, float>(
                    (m) => m.Input.genCount,
                    (m, x) => m.Input.genCount = Mathf.FloorToInt(x)),

                [MicrogridVar.TSize] = new Accessor<Microgrid, float>(
                    (m) => m.Input.tIncrementSize,
                    (m, x) => m.Input.tIncrementSize = Mathf.FloorToInt(x)),

                [MicrogridVar.CostSysTotal] = new Accessor<Microgrid, float>(
                    (m) => m.Result.c_sys_total,
                    (m, x) => m.Result.c_sys_total = x),
            };

            accessorBools = new MicrogridAccessors<bool>();
        }

        public static MicrogridVar[] GetInputVars()
        {
            var mVars = (MicrogridVar[])Enum.GetValues(typeof(MicrogridVar));
            mVars = mVars
                .Where(x => x.HasExpectedFlags(MVarDirection.Input))
                .ToArray();
            return mVars;
        }

        public static MicrogridVar[] GetOutputVars()
        {
            var mVars = (MicrogridVar[])Enum.GetValues(typeof(MicrogridVar));
            mVars = mVars
                .Where(x => x.HasExpectedFlags(MVarDirection.Output))
                .ToArray();
            return mVars;
        }

        public static string LArrayChar => "(";
        public static string RArrayChar => ")";
        public static string ArrayDelimiterChar => ",";

        public static string GetArrayString(MicrogridVar mvar, params int[] count)
        {
            string s = LArrayChar;
            for (int i = 0; i < count.Length; i++)
            {
                s += count[i];

                if (i < count.Length - 1)
                {
                    s += ArrayDelimiterChar;
                }
            }

            return s + RArrayChar;
        }

        public static string MicrogridVarsToString(Microgrid microgrid,
            MicrogridVar[] mVars = null,
            MVarDirection? varFlags = null)
        {
            var dict = new Dictionary<string, List<float>>();
            mVars = mVars ??
                (MicrogridVar[])Enum.GetValues(typeof(MicrogridVar));

            if (varFlags.HasValue)
            {
                mVars = mVars
                    .Where(x => x.HasExpectedFlags(varFlags.Value))
                    .ToArray();
            }

            foreach (MicrogridVar mvar in mVars)
            {
                if (accessorLists.ContainsKey(mvar))
                {
                    var valueList = accessorLists[mvar].Get(microgrid);
                    dict.Add(enumToHeader[mvar], valueList);
                }
                else if (accessorFloats.ContainsKey(mvar))
                {
                    var value = accessorFloats[mvar].Get(microgrid);
                    var valueList = new List<float>() { value };
                    dict.Add(enumToHeader[mvar], valueList);
                }
                else if (accessor2DArray.ContainsKey(mvar))
                {
                    var value = accessor2DArray[mvar].Get(microgrid);
                    for (int i = 0; i < value.GetLength(0); i++)
                    {
                        var valueList = new List<float>(MGHelper.GetRow(value, i));
                        var str = enumToHeader[mvar] + GetArrayString(mvar, i);
                        dict.Add(str, valueList);
                    }
                }
                else if (accessorBools.ContainsKey(mvar))
                {
                    var value = accessorBools[mvar].Get(microgrid);
                    var valueList = new List<float>() { value ? 1.0f : 0.0f };
                    dict.Add(enumToHeader[mvar], valueList);
                }
                else
                {
                    Debug.Log("Microgrid variable " + mvar + "has no " +
                        "associated accesor and cannot be saved to string.");
                }
            }

            var builder = new StringBuilder();
            var lineBuilder = new StringBuilder();
            int max = dict.Values.Max(x => x.Count);

            for (int i = -1; i < max; i++)
            {
                foreach (KeyValuePair<string, List<float>> kvp in dict)
                {
                    if (i == -1)
                    {
                        lineBuilder.Append(kvp.Key).Append(",");
                    }
                    else
                    {
                        if (i < kvp.Value.Count)
                        {
                            lineBuilder.Append(kvp.Value[i]);
                        }
                        lineBuilder.Append(",");
                    }
                }
                builder.AppendLine(lineBuilder.ToString());
                lineBuilder.Clear();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static bool HasExpectedFlags(this MicrogridVar enumValue,
            MVarDirection expectedFlags)
        {
            var actualFlags = GetAttributeOfType<MVarType>(enumValue).flags;
            return expectedFlags == actualFlags;
        }

        public static string GetVarDescription(this MicrogridVar value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes?.Length > 0)
            {
                return attributes[0].Description;
            }

            return value.ToString();
        }

        public static bool IsEditable(this MicrogridVar value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = (MVarType[])fi.GetCustomAttributes(
                typeof(MVarType),
                false);

            if (attributes.Length <= 0)
            {
                return false;
            }

            return attributes[0].flags == MVarDirection.Input;
        }

        public static MicrogridVar[] GetArray(this MicrogridVar value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = (MVarArray[])fi.GetCustomAttributes(
                typeof(MVarArray),
                false);

            if (attributes.Length <= 0)
            {
                return null;
            }

            return attributes[0].arrayCount;
        }

        public static int[] GetArrayFixed(this MicrogridVar value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = (MVarArrayFixed[])fi.GetCustomAttributes(
                typeof(MVarArrayFixed),
                false);

            if (attributes.Length <= 0)
            {
                return null;
            }

            return attributes[0].arrayCount;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class MVarType : Attribute
    {
        public MVarType(MVarDirection t)
        {
            this.flags = t;
        }

        public readonly MVarDirection flags;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class MVarArray : Attribute
    {
        public MVarArray(params MicrogridVar[] mvars)
        {
            arrayCount = mvars;
        }

        public readonly MicrogridVar[] arrayCount;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class MVarArrayFixed : Attribute
    {
        public MVarArrayFixed(params int[] count)
        {
            arrayCount = count;
        }

        public readonly int[] arrayCount;
    }

    public enum MVarDirection
    {
        None = 0,
        Input = 1,
        Output = 2
    }

    public enum MicrogridVar
    {
        [Description("Time Increment Size"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        TSize = 0,

        [Description("Number of Time Increments"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        TCount = 1,

        [Description("Generator Count"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        CHPCount = 2,

        [Description("Photovoltaic Power"),
            MVarType(MVarDirection.Input),
            MVarArray(TCount)]
        PPV = 3,

        [Description("Wind Power"),
            MVarType(MVarDirection.Input),
            MVarArray(TCount)]
        PW = 4,

        [Description("Thermal Power"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PCHPSum = 5,

        [Description("Battery Power"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PBat = 6,

        [Description("Load"),
            MVarType(MVarDirection.Input),
            MVarArray(TCount)]
        PLoad = 7,

        [Description("Energy Exchange"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PSys = 8,

        [Description("State of Charge"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        SoC = 9,

        [Description("Charging Mode"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        CBat = 10,

        [Description("Battery Rated Energy"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        EBatMax = 11,

        [Description("Battery Rated Power"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        PBatMax = 12,

        [Description("Generator Rated Power"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        PCHPMax = 13,

        [Description("Generator Min. Up Time"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        MinTimeUp = 14,

        [Description("Generator Min. Down Time"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        MinTimeDown = 15,

        [Description("Initial Charging Mode"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        CBatInitial = 16,

        [Description("Initial State of Charge"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        SoCInitial = 17,

        [Description("Minimum State of Charge"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        SoCMin = 18,

        [Description("Maximum State of Charge"),
            MVarType(MVarDirection.Input),
            MVarArray(null)]
        SoCMax = 19,

        [Description("Market Price"),
            MVarType(MVarDirection.Input),
            MVarArray(TCount)]
        Price = 20,

        [Description("Generator Cost coefficient A"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        CostFuncA = 21,

        [Description("Generator Cost coefficient B"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        CostFuncB = 22,

        [Description("Generator Cost coefficient C"),
            MVarType(MVarDirection.Input),
            MVarArray(CHPCount)]
        CostFuncC = 23,

        [Description("Thermal Energy"),
            MVarType(MVarDirection.Output),
            MVarArray(null)]
        PCHPTotal = 24,

        [Description("Total Energy Exchange"),
            MVarType(MVarDirection.Output),
            MVarArray(null)]
        PSysTotal = 25,

        [Description("Time"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        Time = 26,

        [Description("Purchase Cost"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        CostSys = 27,

        [Description("Total Purchase Cost"),
            MVarType(MVarDirection.Output),
            MVarArray(null)]
        CostSysTotal = 28,

        [Description("Thermal Power"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount, CHPCount)]
        PCHP = 29,

        [Description("Generator On/Off States"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount, CHPCount)]
        UCHP = 30,

        [Description("Thermal Cost"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount, CHPCount)]
        CCHP = 31,

        [Description("Thermal Power Target"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PTarget = 32,

        [Description("Renewable Power"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PRes = 33,

        [Description("Power Sum"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PSum = 34,

        [Description("Max. Potential Thermal Power"),
            MVarType(MVarDirection.Output),
            MVarArray(TCount)]
        PThrSumMax = 35,
    }
}
