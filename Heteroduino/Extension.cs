using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Heteroduino
{
       public static class Extensions
    {





        public static int MaxIndex(this IEnumerable<double> a) =>
           a.Select((v, i) => new { value = v, index = i }).Aggregate((max, next)
               => next.value > max.value ? next : max).index;

        public static int MinIndex(this IEnumerable<double> a) =>
            a.Select((v, i) => new { value = v, index = i }).Aggregate((min, next)
                => next.value < min.value ? next : min).index;
        public static int MaxIndex<T>(this IEnumerable<T> a, Func<T, double> f) =>
            a.Select((v, i) => new { value = f(v), index = i }).Aggregate((max, next)
                => next.value > max.value ? next : max).index;

        public static int MinIndex<T>(this IEnumerable<T> a, Func<T, double> f) =>
            a.Select((v, i) => new { value = f(v), index = i }).Aggregate((min, next)
                => next.value < min.value ? next : min).index;


        public static string ToStringChain<T>(this IEnumerable<T> a)
        {
            return string.Join("|", a);
        }

        public static string ToStringChain<T>(this IEnumerable<T> a, string delim) => string.Join(delim, a);

        public static string ToStringChain<T, G>(this IEnumerable<T> a, string delim, Func<T, G> k) => string.Join(delim, a.Select(k));

        public static string ToStringChain<T, G>(this IEnumerable<T> a, Func<T, G> k) => string.Join("|", a.Select(k));

        public static string[] GetNames<T>() => Enum.GetNames(typeof(T)).Select(i => i.Replace("_", " ")).ToArray();

        public static Dictionary<string, T> GetDictionary<T>() =>
            Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(i => i.ToString(), i => i);
        

        public static void Fill(this Param_Integer a, string[] names)
        {
            for (var i = 0; i < names.Length; i++)
                a.AddNamedValue(names[i], i);
        }

        public static string Join(this IEnumerable<string> w) => string.Join("\n", w);
        public static string Join(this IEnumerable<string> w, string a) => string.Join(a, w);
        public static string Join(this IEnumerable<char> w) => new string(w.ToArray());
        public static string JoinFit(this IEnumerable<string> w) => string.Join("", w);




        public static int[][] ToMatrix(this GH_Structure<GH_Integer> a)
        {
            var r = new List<List<int>>();
            var e = a.Paths.Select(i => i.Indices.Last()).Max();
            for (var i = 0; i <= e; i++)
                r.Add(new List<int>());
            foreach (var p in a.Paths)
                r[p.Indices.Last()].AddRange(a[p].Select(j => j.Value));
            return r.Select(i => i.ToArray()).ToArray();
        }
      

        public static double[][][] ToTripleMatrix(this GH_Structure<GH_Number> m)
        {
            if (m.IsEmpty) return null;
            m.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (m.Paths[0].Length == 1) return new[] { m.ToMatrix() };

            var groups = m.Paths.GroupBy(i => i.CullElement().DebuggerDisplay).ToArray();

            //throw new Exception(groups.Select(i=>i.Key).ToStringChain());
            return groups.Select(
                group => group.Select(
                    path => m[path].Select(
                            i => i.Value)
                        .ToArray()).ToArray()).ToArray();
        }
        public static double[][][] To3Matrix(this GH_Structure<GH_Number> m)
        {
            m.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!m.Paths.All(i => i.Length == 2)) return null;
            var groups = m.Paths.GroupBy(i => i.CullElement().CullElement());
            return groups.Select(
                group => group.Select(
                    path => m[path].Select(
                            i => i.Value)
                        .ToArray()).ToArray()).ToArray();
        }

        public static Point3d[][] ToMatrix(this GH_Structure<GH_Point> m)   => 
            m.Branches.Select(i => i.Select(k => k.Value).ToArray()).ToArray();


        public static double[][] ToMatrix(this GH_Structure<GH_Number> m)
        {
            return m.Branches.Select(i => i.Select(k => k.Value).ToArray()).ToArray();
        }

        public static DataTree<Q> ToTree<Q>(this IEnumerable<IEnumerable<Q>> enu, GH_Path prepath)
        {
            var wishlist = enu.ToList();
            var r = new DataTree<Q>();
            if (wishlist.Count > 0)
            {
                var pathindex = 0;
                wishlist.ForEach(b => r.AddRange(b, prepath.AppendElement(pathindex++)));
            }

            return r;
        }

        public static DataTree<Q> ToTree<Q>(this IEnumerable<IEnumerable<Q>> enu, int[] p) =>
            enu.ToTree(new GH_Path(p));
       
        public static DataTree<Q> ToTree<Q>(this IEnumerable<IEnumerable<Q>> enu)
        {
            var wishlist = enu.ToList();
            var r = new DataTree<Q>();
            if (wishlist.Count > 0)
            {
                var pathindex = 0;
                wishlist.ForEach(b => r.AddRange(b, new GH_Path(pathindex++)));
            }

            return r;
        }

        public static GH_Structure<GH_Integer> ToUnSignTree(this IEnumerable<IEnumerable<int>> enu, GH_Path prepath)
        {
            var wishlist = enu.ToList();
            var r = new GH_Structure<GH_Integer>();
            if (wishlist.Count <= 0) return r;
            var pathindex = 0;
            wishlist.ForEach(b =>
                r.AppendRange(b.Select(i => i < 0 ? null : new GH_Integer(i)),
                    prepath.AppendElement(pathindex++))
            );

            return r;
        }

        public static GH_Structure<GH_Integer> ToUnSignTree(this IEnumerable<IEnumerable<int>> enu)
        {
            var wishlist = enu.ToList();
            var r = new GH_Structure<GH_Integer>();
            if (wishlist.Count <= 0) return r;
            var pathindex = 0;
            wishlist.ForEach(b =>
                r.AppendRange(b.Select(i => i < 0 ? null : new GH_Integer(i)),
                    new GH_Path(pathindex++))
            );

            return r;
        }








    }
}