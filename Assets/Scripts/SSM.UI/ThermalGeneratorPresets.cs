using System;
using System.Collections.Generic;
using WindowManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SSM.UI
{
    public static class ThermalGeneratorPresets
    {
        private static Dictionary<string, ThermalGenerator> presets 
            = new Dictionary<string, ThermalGenerator>();

        public static List<List<Tuple<string, int>>> presetGroup = new List<List<Tuple<string, int>>>()
        {
            new List<Tuple<string, int>>()
            {
                Tuple.Create("Large Fast Low-Eff", 1),
                Tuple.Create("Large Slow Low-Eff", 1),
                Tuple.Create("Large Slow Hi-Eff", 1),
                Tuple.Create("Medium Fast Low-Eff", 1),
                Tuple.Create("Medium Fast Hi-Eff", 1),
                Tuple.Create("Medium Slow Low-Eff", 1),
                Tuple.Create("Small Fast Hi-Eff", 2),
                Tuple.Create("Small Slow Hi-Eff", 1),
            }
        };

        static ThermalGeneratorPresets()
        {
            presets.Add("Large Fast Low-Eff", new ThermalGenerator()
            {
                ratedPower = 30.00f,
                minDTime = 60.0f * 4.0f,
                minUTime = 60.0f * 4.0f,
                a = 0.03f,
                b = 9.0f,
                c = 1450.0f,
                // cost at max = 58.23
            });

            presets.Add("Large Fast Hi-Eff", new ThermalGenerator()
            {
                ratedPower = 30.00f,
                minDTime = 60.0f * 4.0f,
                minUTime = 60.0f * 4.0f,
                a = 0.025f,
                b = 8.0f,
                c = 1200.0f,
                // cost at max = 48.75
            });

            presets.Add("Large Slow Low-Eff", new ThermalGenerator()
            {
                ratedPower = 30.00f,
                minDTime = 60.0f * 6.0f,
                minUTime = 60.0f * 6.0f,
                a = 0.03f,
                b = 9.0f,
                c = 1450.0f,
                // cost at max = 58.23
            });

            presets.Add("Large Slow Hi-Eff", new ThermalGenerator()
            {
                ratedPower = 30.00f,
                minDTime = 60.0f * 6.0f,
                minUTime = 60.0f * 6.0f,
                a = 0.025f,
                b = 8.0f,
                c = 1200.0f,
                // cost at max = 48.75
            });

            presets.Add("Medium Fast Low-Eff", new ThermalGenerator()
            {
                ratedPower = 6.00f,
                minDTime = 60.0f * 1.5f,
                minUTime = 60.0f * 1.5f,
                a = 0.04f,
                b = 37.0f,
                c = 125.0f,
                // cost at max = 58.1
            });

            presets.Add("Medium Fast Hi-Eff", new ThermalGenerator()
            {
                ratedPower = 6.00f,
                minDTime = 60.0f * 1.5f,
                minUTime = 60.0f * 1.5f,
                a = 0.04f,
                b = 32.75f,
                c = 90.0f,
                // cost at max = 47.99
            });

            presets.Add("Medium Slow Low-Eff", new ThermalGenerator()
            {
                ratedPower = 6.00f,
                minDTime = 60.0f * 3.0f,
                minUTime = 60.0f * 3.0f,
                a = 0.04f,
                b = 37.0f,
                c = 125.0f,
                // cost at max = 58.1
            });

            presets.Add("Small Fast Low-Eff", new ThermalGenerator()
            {
                ratedPower = 2.00f,
                minDTime = 60.0f * 1.0f,
                minUTime = 60.0f * 1.0f,
                a = 0.003f,
                b = 32.75f,
                c = 55.0f,
                // cost at max = 60.26
            });

            presets.Add("Small Fast Hi-Eff", new ThermalGenerator()
            {
                ratedPower = 2.00f,
                minDTime = 60.0f * 1.0f,
                minUTime = 60.0f * 1.0f,
                a = 0.003f,
                b = 27.75f,
                c = 48.0f,
                // cost at max = 51.76
            });

            presets.Add("Small Slow Low-Eff", new ThermalGenerator()
            {
                ratedPower = 2.00f,
                minDTime = 60.0f * 2.0f,
                minUTime = 60.0f * 2.0f,
                a = 0.003f,
                b = 32.75f,
                c = 55.0f,
                // cost at max = 60.26
            });

            presets.Add("Small Slow Hi-Eff", new ThermalGenerator()
            {
                ratedPower = 2.00f,
                minDTime = 60.0f * 2.0f,
                minUTime = 60.0f * 2.0f,
                a = 0.003f,
                b = 27.75f,
                c = 48.0f,
                // cost at max = 51.76
            });
        }

        public static bool AddPreset(string name, ThermalGenerator generator)
        {
            if (presets.ContainsKey(name))
            {
                return false;
            }
            else
            {
                presets.Add(name, generator);
                return true;
            }
        }

        public static bool TryGetPreset(string name, out ThermalGenerator generator) 
            => presets.TryGetValue(name, out generator);

        public static IEnumerable<KeyValuePair<string, ThermalGenerator>> EnumeratePresets()
        {
            foreach (var kvp in presets)
            {
                yield return kvp;
            }
        }
    }
}
