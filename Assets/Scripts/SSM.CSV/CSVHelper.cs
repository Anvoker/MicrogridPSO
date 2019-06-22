using System.Collections.Generic;
using System;
using UnityEngine;
using SSM.Grid;

namespace SSM.CSV
{
    public static class CSVHelper
    {
        private const StringComparison defaultComparison = StringComparison.InvariantCultureIgnoreCase;

        public static void ReadInputFromString(
            string content, 
            Microgrid microgrid, 
            MicrogridVar[] mVars = null)
        {
            List<List<string>> cells = ReadCellsFromString(content);
            mVars = mVars ?? (MicrogridVar[])Enum.GetValues(typeof(MicrogridVar));

            foreach (MicrogridVar mvar in mVars)
            {
                string header = string.Empty;
                MGMisc.enumToHeader.TryGetValue(mvar, out header);

                if (string.IsNullOrEmpty(header))
                {
                    throw new ArgumentException(mvar + " doesn't have a " +
                        "header string defined");
                }

                var listString = GetColumn(cells, header);
                if (listString != null)
                {
                    var listFloats = ParseList(listString);

                    if (listFloats.Count <= 0) { continue; }

                    if (MGMisc.accessorLists.ContainsKey(mvar))
                    {
                        MGMisc.accessorLists[mvar].Set(microgrid, listFloats);
                    }
                    else if (MGMisc.accessorFloats.ContainsKey(mvar))
                    {
                        MGMisc.accessorFloats[mvar].Set(microgrid, listFloats[0]);
                    }
                    else if (MGMisc.accessorBools.ContainsKey(mvar))
                    {
                        bool value = listFloats[0] != 0.0f;
                        MGMisc.accessorBools[mvar].Set(microgrid, value);
                    }
                    else
                    {
                        Debug.Log(mvar + " doesn't have an associated case " +
                            "and cannot be loaded.");
                    }
                }
            }
        }

        public static List<List<string>> ReadCellsFromString(string content)
        {
            var cells = new List<List<string>>();

            void lineReader(int lineIndex, List<string> line)
            {
                cells.Add(new List<string>(line));
            }

            fgCSVReader.LoadFromString(content, lineReader);

            return cells;
        }

        public static int[] FindCell(
            List<List<string>> cells,
            string content,
            StringComparison comparison = defaultComparison)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = 0; j < cells[i].Count; j++)
                {
                    if (String.Compare(cells[i][j], content, comparison) == 0)
                    {
                        return new int[] { i, j };
                    }
                }
            }
            return null;
        }

        public static List<string> GetColumn(
            List<List<string>> cells, 
            string columnName)
        {
            int[] headerPosition = FindCell(cells, columnName);

            //Get the entire column below the position of the header
            if (headerPosition != null)
            {
                int headerX = headerPosition[0];
                int headerY = headerPosition[1];
                var list = new List<string>();

                for (int i = headerX + 1; i < cells.Count; i++)
                {
                    list.Add(cells[i][headerY]);
                }

                return list;
            }
            else
            {
                return null;
            }
        }

        private static List<float> ParseList(List<string> strings)
        {
            var listFloats = new List<float>();
            for (int i = 0; i < strings.Count; i++)
            {
                if (string.IsNullOrEmpty(strings[i])) { break; }
                float f;

                try
                {
                    f = float.Parse(strings[i]);
                }
                catch (FormatException e)
                {
                    Debug.Log(e);
                    break;
                }
                listFloats.Add(f);
            }
            return listFloats;
        }
    }
}
